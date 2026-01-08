
import numpy as np
import matplotlib.pyplot as plt
import random


# SPH Parameters
N = 100                # Number of SPH particles
L = 1.0                # Domain size
h = 0.08               # Smoothing length
m = 0.02               # Particle mass
rho0 = 1000            # Reference density
k = 2000               # Bulk modulus
mu = 0.1               # Viscosity
G = np.array([0, -9.81]) # Gravity

dt = 0.001             # Time step
steps = 200            # Number of steps

# DEM & Granular Damper Parameters
DEM_N = 20             # Number of DEM particles (granular damper)
damper_width = 0.2
damper_height = 0.4
damper_x = 0.7         # Damper box x-position (left)
damper_y = 0.3         # Damper box y-position (bottom, will move)
damper_vy = 0.0        # Damper vertical velocity
damper_mass = 1.0
dem_r = 0.015          # DEM particle radius
dem_m = 0.01           # DEM particle mass
dem_k = 5000           # DEM contact stiffness
dem_gamma = 2.0        # DEM damping

# Initialize DEM particles inside damper
dem_pos = np.zeros((DEM_N, 2))
dem_vel = np.zeros((DEM_N, 2))
for i in range(DEM_N):
    # Randomize initial positions inside the damper box
    dem_pos[i, 0] = damper_x + 0.05 + 0.1 * random.random()
    dem_pos[i, 1] = damper_y + 0.05 + 0.3 * random.random()

# Initialize particles in a grid
nx = int(np.sqrt(N))
ny = N // nx
x = np.linspace(0.1, 0.9, nx)
y = np.linspace(0.1, 0.9, ny)
X, Y = np.meshgrid(x, y)
pos = np.stack([X.ravel(), Y.ravel()], axis=1)
vel = np.zeros_like(pos)
rho = np.ones(N) * rho0
p = np.zeros(N)

# Cubic spline kernel (2D)
def W(r, h):
    q = np.linalg.norm(r, axis=-1) / h
    sigma = 10 / (7 * np.pi * h**2)
    w = np.zeros_like(q)
    mask1 = q <= 1
    mask2 = (q > 1) & (q <= 2)
    w[mask1] = 1 - 1.5*q[mask1]**2 + 0.75*q[mask1]**3
    w[mask2] = 0.25 * (2 - q[mask2])**3
    return sigma * w

def gradW(r, h):
    q = np.linalg.norm(r, axis=-1) / h
    sigma = 10 / (7 * np.pi * h**2)
    grad = np.zeros_like(r)
    mask1 = q <= 1
    mask2 = (q > 1) & (q <= 2)
    factor = np.zeros_like(q)
    factor[mask1] = (-3*q[mask1] + 2.25*q[mask1]**2)
    factor[mask2] = -0.75 * (2 - q[mask2])**2
    # Avoid division by zero
    with np.errstate(divide='ignore', invalid='ignore'):
        grad = (r / (np.linalg.norm(r, axis=-1, keepdims=True) * h)) * factor[:, None]
        grad[np.isnan(grad)] = 0
    return sigma * grad


# Main loop
for step in range(steps):
    # --- SPH ---
    rho = np.zeros(N)
    for i in range(N):
        rij = pos - pos[i]
        rho[i] = np.sum(m * W(rij, h))
    p = k * (rho - rho0)
    acc = np.zeros_like(pos)
    for i in range(N):
        rij = pos - pos[i]
        vij = vel - vel[i]
        acc[i] += -np.sum(m * (p[i]/rho[i]**2 + p/rho**2)[:,None] * gradW(rij, h), axis=0)
        acc[i] += mu * np.sum(m * (vij/rho[:,None]) * W(rij, h)[:,None], axis=0)
    acc += G
    vel += acc * dt
    pos += vel * dt
    pos = np.clip(pos, 0, L)
    vel[(pos == 0) | (pos == L)] *= -0.5

    # --- DEM (Granular damper) ---
    # Satunnainen damperin pystysuuntainen liike (esim. "värähtely")
    if step % 10 == 0:
        damper_vy = 0.05 * (2 * random.random() - 1)  # satunnainen nopeus [-0.05, 0.05]
    damper_y += damper_vy * dt
    # Pidä damper laatikko kentän sisällä
    if damper_y < 0:
        damper_y = 0
        damper_vy *= -1
    if damper_y + damper_height > L:
        damper_y = L - damper_height
        damper_vy *= -1

    # DEM partikkelien voimat
    dem_acc = np.zeros_like(dem_pos)
    for i in range(DEM_N):
        # Painovoima
        dem_acc[i, 1] += G[1]
        # Seinät (laatikko)
        # Vasen
        if dem_pos[i, 0] - dem_r < damper_x:
            dem_acc[i, 0] += dem_k * (damper_x - (dem_pos[i, 0] - dem_r)) / dem_m
            dem_vel[i, 0] *= -dem_gamma
        # Oikea
        if dem_pos[i, 0] + dem_r > damper_x + damper_width:
            dem_acc[i, 0] -= dem_k * ((dem_pos[i, 0] + dem_r) - (damper_x + damper_width)) / dem_m
            dem_vel[i, 0] *= -dem_gamma
        # Ala
        if dem_pos[i, 1] - dem_r < damper_y:
            dem_acc[i, 1] += dem_k * (damper_y - (dem_pos[i, 1] - dem_r)) / dem_m
            dem_vel[i, 1] *= -dem_gamma
        # Ylä
        if dem_pos[i, 1] + dem_r > damper_y + damper_height:
            dem_acc[i, 1] -= dem_k * ((dem_pos[i, 1] + dem_r) - (damper_y + damper_height)) / dem_m
            dem_vel[i, 1] *= -dem_gamma
        # DEM-DEM törmäykset
        for j in range(i+1, DEM_N):
            rij = dem_pos[j] - dem_pos[i]
            dist = np.linalg.norm(rij)
            if dist < 2*dem_r and dist > 1e-8:
                n = rij / dist
                overlap = 2*dem_r - dist
                force = dem_k * overlap
                # Damping
                rel_vel = dem_vel[j] - dem_vel[i]
                damp = dem_gamma * np.dot(rel_vel, n)
                f_total = (force + damp)
                dem_acc[i] -= f_total * n / dem_m
                dem_acc[j] += f_total * n / dem_m

    # Päivitä DEM partikkelien liike
    dem_vel += dem_acc * dt
    dem_pos += dem_vel * dt
    # Pidä partikkelit damperin sisällä (ylimääräinen varmistus)
    dem_pos[:, 0] = np.clip(dem_pos[:, 0], damper_x + dem_r, damper_x + damper_width - dem_r)
    dem_pos[:, 1] = np.clip(dem_pos[:, 1], damper_y + dem_r, damper_y + damper_height - dem_r)

    # --- Visualisointi ---
    if step % 20 == 0:
        plt.clf()
        # SPH-neste
        plt.scatter(pos[:,0], pos[:,1], s=10, label="SPH")
        # Damperin laatikko
        rect = plt.Rectangle((damper_x, damper_y), damper_width, damper_height, fill=False, color='k', lw=2)
        plt.gca().add_patch(rect)
        # DEM-partikkelit
        plt.scatter(dem_pos[:,0], dem_pos[:,1], s=50, c='orange', label="DEM")
        plt.xlim(0, L)
        plt.ylim(0, L)
        plt.title(f"Step {step}")
        plt.legend()
        plt.pause(0.01)
plt.show()

# --- Analysis ---
import scipy.stats as stats

# Particle heights (y-coordinates)
heights = pos[:,1]
mean_height = np.mean(heights)
std_height = np.std(heights)
print(f"Mean height: {mean_height:.4f}, Std: {std_height:.4f}")

# Histogram of heights
plt.figure()
plt.hist(heights, bins=15, density=True, alpha=0.7, label="Particle heights")
# Fit normal distribution for comparison
mu, std = stats.norm.fit(heights)
x = np.linspace(0, 1, 100)
plt.plot(x, stats.norm.pdf(x, mu, std), 'r--', label=f"Normal fit ($\mu$={mu:.2f}, $\sigma$={std:.2f})")
plt.xlabel("Height (y)")
plt.ylabel("Density")
plt.title("Histogram of Particle Heights")
plt.legend()
plt.show()

# Pressure profile vs. hydrostatic theory
plt.figure()
plt.scatter(pos[:,1], p, s=10, label="Simulated pressure")
# Theoretical hydrostatic pressure: p(y) = rho0 * g * (y0 - y)
y0 = np.max(pos[:,1])
pth = rho0 * abs(G[1]) * (y0 - x)
plt.plot(x, pth, 'k-', label="Hydrostatic theory")
plt.xlabel("Height (y)")
plt.ylabel("Pressure")
plt.title("Pressure Profile vs. Hydrostatic Theory")
plt.legend()
plt.tight_layout()
import os
plt.savefig(r'C:/Users/jaakk/Desktop/Gradu/hydrostatic_pressure.png', dpi=300)
plt.show()
