
import numpy as np

# --- Parameters ---
dx = 0.02  # Particle spacing
h = 2 * dx  # Kernel radius
rho0 = 1000.0  # Reference density
c0 = 20.0  # Artificial speed of sound
g = np.array([0, -9.81])  # Gravity
dt = 0.0005  # Time step
n_steps = 1000  # Number of time steps

# --- SPH particle class (fluid) ---
class SPHParticle:
    def __init__(self, x, v, m):
        self.x = np.array(x, dtype=float)  # Position
        self.v = np.array(v, dtype=float)  # Velocity
        self.m = m  # Mass
        self.rho = rho0  # Density
        self.p = 0.0  # Pressure
        self.f = np.zeros(2)  # Total force

# --- DEM particle class (solid disk) ---
class DEMParticle:
    def __init__(self, X, V, R, M):
        self.X = np.array(X, dtype=float)  # Position
        self.V = np.array(V, dtype=float)  # Velocity
        self.R = R  # Radius
        self.M = M  # Mass
        self.F = np.zeros(2)  # Total force

# --- Cubic spline kernel function ---
def W(r, h):
    q = np.linalg.norm(r) / h
    sigma = 10 / (7 * np.pi * h**2)
    if q < 1:
        return sigma * (1 - 1.5*q**2 + 0.75*q**3)
    elif q < 2:
        return sigma * 0.25 * (2 - q)**3
    else:
        return 0.0

# --- Gradient of cubic spline kernel ---
def gradW(r, h):
    q = np.linalg.norm(r) / h
    sigma = 10 / (7 * np.pi * h**2)
    if q == 0:
        return np.zeros_like(r)
    if q < 1:
        return sigma * (-3*q + 2.25*q**2) * r / (h**2 * q)
    elif q < 2:
        return -sigma * 0.75 * (2 - q)**2 * r / (h**2 * q)
    else:
        return np.zeros_like(r)

# --- Initialize fluid particles (left half of tank) ---
fluid_particles = []
for i in range(10):
    for j in range(10):
        fluid_particles.append(SPHParticle([0.1 + i*dx, 0.1 + j*dx], [0, 0], m=dx*dx*rho0))

print("Starting 2D SPH dam break simulation...")

# --- Main time integration loop ---
for step in range(n_steps):
    # 1) Neighbor search (brute force, small N)
    for pi in fluid_particles:
        pi.rho = 0.0
        for pj in fluid_particles:
            pi.rho += pj.m * W(pi.x - pj.x, h)
    # 2) Equation of state (pressure)
    for pi in fluid_particles:
        pi.p = c0**2 * (pi.rho - rho0)
    # 3) Fluid forces (pressure, gravity)
    for pi in fluid_particles:
        f = np.zeros(2)
        for pj in fluid_particles:
            if pi is pj: continue
            r = pi.x - pj.x
            f -= pj.m * (pi.p/pi.rho**2 + pj.p/pj.rho**2) * gradW(r, h)
        f += pi.m * g
        pi.f = f
    # 4) Update positions and velocities (Euler)
    for pi in fluid_particles:
        pi.v += dt * pi.f / pi.m
        pi.x += dt * pi.v
        # Simple wall at y=0
        if pi.x[1] < 0:
            pi.x[1] = 0
            pi.v[1] *= -0.5
    # Print progress every 100 steps
    if step % 100 == 0:
        y_fluid = np.mean([p.x[1] for p in fluid_particles])
        print(f"Step {step:4d}: mean fluid y = {y_fluid:.3f}")

# --- Visualisointi: piirretään lopputilanne ---
import matplotlib.pyplot as plt
fluid_x = [p.x[0] for p in fluid_particles]
fluid_y = [p.x[1] for p in fluid_particles]
plt.figure(figsize=(6,3))
plt.scatter(fluid_x, fluid_y, s=20, c='b', label='Fluid')
plt.xlabel('x')
plt.ylabel('y')
plt.title('SPH dam-break: fluid particles at final step')
plt.legend()
plt.tight_layout()
plt.show()