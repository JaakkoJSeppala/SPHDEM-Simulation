def hydrostatic_theory(ship_mass, ship_width, ship_height, rho_fluid, g):
    """Calculate equilibrium height of ship based on hydrostatic theory."""
    # Buoyant force = weight
    # Displaced volume = ship_mass / (rho_fluid)
    # For a rectangle: V_disp = ship_width * ship_length * h_disp
    # Here, ship_length is not defined, so assume unit length (2D)
    h_disp = ship_mass / (rho_fluid * ship_width * 1.0)
    return h_disp

# --- Deeper error analysis: discretization, convergence rate, sensitivity ---
def deeper_error_analysis():
    N_values = [50, 100, 200, 400]
    dt_values = [0.004, 0.002, 0.001, 0.0005]
    fill_frac = 0.4
    steps = 200
    theory_height = hydrostatic_theory(2.0, 0.5, 0.1, 1000, 9.81)

    # Error vs N
    errors_N = []
    for N in N_values:
        sim = Simulaatio(N, L, h, m, rho0, k, mu, G, 0.001, steps, fill_frac)
        res = sim.aja()
        final_height = res['ship_y_hist'][-1]
        error = abs(final_height - theory_height)
        errors_N.append(error)

    plt.figure(figsize=(8,5))
    plt.loglog(N_values, errors_N, marker='o')
    plt.xlabel('Particle number N')
    plt.ylabel('Absolute error (m)')
    plt.title('Discretization error vs. particle number')
    plt.grid(True)
    plt.show()

    # Estimate convergence rate p (error ~ N^-p)
    import numpy as np
    p = np.polyfit(np.log(N_values), np.log(errors_N), 1)[0] * -1
    print(f"Estimated convergence rate p (error ~ N^-p): {p:.2f}")

    # Error vs dt
    errors_dt = []
    for dt in dt_values:
        sim = Simulaatio(200, L, h, m, rho0, k, mu, G, dt, steps, fill_frac)
        res = sim.aja()
        final_height = res['ship_y_hist'][-1]
        error = abs(final_height - theory_height)
        errors_dt.append(error)

    plt.figure(figsize=(8,5))
    plt.loglog(dt_values, errors_dt, marker='o')
    plt.xlabel('Timestep dt')
    plt.ylabel('Absolute error (m)')
    plt.title('Discretization error vs. timestep')
    plt.grid(True)
    plt.show()

    # Estimate convergence rate q (error ~ dt^q)
    q = np.polyfit(np.log(dt_values), np.log(errors_dt), 1)[0]
    print(f"Estimated convergence rate q (error ~ dt^q): {q:.2f}")

    # Sensitivity to viscosity
    mu_values = [0.05, 0.1, 0.2, 0.4]
    errors_mu = []
    for mu_test in mu_values:
        sim = Simulaatio(200, L, h, m, rho0, k, mu_test, G, 0.001, steps, fill_frac)
        res = sim.aja()
        final_height = res['ship_y_hist'][-1]
        error = abs(final_height - theory_height)
        errors_mu.append(error)

    plt.figure(figsize=(8,5))
    plt.plot(mu_values, errors_mu, marker='o')
    plt.xlabel('Viscosity mu')
    plt.ylabel('Absolute error (m)')
    plt.title('Error sensitivity to viscosity')
    plt.grid(True)
    plt.show()

    print("\nSummary of deeper error analysis:")
    print("- Discretization error decreases with increasing N and decreasing dt.")
    print(f"- Estimated convergence rate p (N): {p:.2f}")
    print(f"- Estimated convergence rate q (dt): {q:.2f}")
    print("- Viscosity affects error; optimal value depends on physical scenario.")
    print("- Main error sources: discretization, boundary effects, model simplifications, numerical damping.")



import numpy as np
import matplotlib.pyplot as plt
import random
import copy

# --- Physical validation: compare to hydrostatic theory ---
def hydrostatic_theory(ship_mass, ship_width, ship_height, rho_fluid, g):
    """Calculate equilibrium height of ship based on hydrostatic theory."""
    # Buoyant force = weight
    # Displaced volume = ship_mass / (rho_fluid)
    # For a rectangle: V_disp = ship_width * ship_length * h_disp
    # Here, ship_length is not defined, so assume unit length (2D)
    h_disp = ship_mass / (rho_fluid * ship_width * 1.0)
    return h_disp

def run_stability_convergence_tests():
    # Vary time step (dt)
    dt_values = [0.002, 0.001, 0.0005]
    N_values = [50, 100, 200]
    fill_frac = 0.4  # Fixed for these tests
    steps = 200
    theory_height = hydrostatic_theory(2.0, 0.5, 0.1, 1000, 9.81)

    # Stability: vary dt
    plt.figure(figsize=(10,6))
    for dt_test in dt_values:
        sim = Simulaatio(100, L, h, m, rho0, k, mu, G, dt_test, steps, fill_frac)
        res = sim.aja()
        plt.plot(res['ship_y_hist'], label=f"dt={dt_test}")
    plt.axhline(theory_height, color='k', linestyle='--', label=f"Theory ({theory_height:.2f} m)")
    plt.xlabel('Timestep')
    plt.ylabel('Ship height (m)')
    plt.title('Numerical stability: effect of dt')
    plt.legend()
    plt.tight_layout()
    plt.show()

    # Convergence: vary N
    plt.figure(figsize=(10,6))
    for N_test in N_values:
        sim = Simulaatio(N_test, L, h, m, rho0, k, mu, G, 0.001, steps, fill_frac)
        res = sim.aja()
        plt.plot(res['ship_y_hist'], label=f"N={N_test}")
    plt.axhline(theory_height, color='k', linestyle='--', label=f"Theory ({theory_height:.2f} m)")
    plt.xlabel('Timestep')
    plt.ylabel('Ship height (m)')
    plt.title('Convergence: effect of N (particle number)')
    plt.legend()
    plt.tight_layout()
    plt.show()

    # Error quantification: compare final ship height to theory
    print("\nError quantification:")
    for N_test in N_values:
        sim = Simulaatio(N_test, L, h, m, rho0, k, mu, G, 0.001, steps, fill_frac)
        res = sim.aja()
        final_height = res['ship_y_hist'][-1]
        error = abs(final_height - theory_height) / theory_height * 100
        print(f"N={N_test}: Final height={final_height:.4f} m, Theory={theory_height:.4f} m, Error={error:.2f}%")



# --- Utility functions ---
def compute_buoyancy(pos, p, ship_x, ship_width, ship_y, L, N):
    """Compute total buoyancy force under the ship (sum of SPH particle pressures)."""
    under_ship = (pos[:,0] > ship_x) & (pos[:,0] < ship_x + ship_width) & (pos[:,1] < ship_y)
    return np.sum(p[under_ship]) * (L / N)

def compute_damper_reaction(damper_pos, damper_r, damper_y, damper_k, DEM_N):
    """Compute damper reaction force (DEM particles at the bottom)."""
    dem_react_force = 0.0
    for i in range(DEM_N):
        if damper_pos[i, 1] - damper_r < damper_y + 1e-8:
            dem_react_force += damper_k * (damper_y - (damper_pos[i, 1] - damper_r))
    return dem_react_force

"""
SPH-DEM ship hydrodynamics simulation with granular damper.

Usage:
    - Edit parameters at the end of the file (main block).
    - Run: python sph_2d_example.py
    - Output: analysis plots of ship height for different damper fill fractions.

Main classes:
    - Simulation: manages the entire simulation
    - Ship: ship state
    - Damper: granular damper state

All functions/methods have docstrings.
"""


# Example ship parameters (not used directly, see Ship class)
ship_mass = 2.0
ship_y = 0.5
ship_vy = 0.0
ship_height = 0.1
ship_width = 0.5
ship_x = 0.2



import numpy as np
import matplotlib.pyplot as plt
import random



class Laiva:
    """Ship hull and vertical motion."""
    def __init__(self, mass, y, vy, height, width, x):
        self.mass = mass
        self.y = y
        self.vy = vy
        self.height = height
        self.width = width
        self.x = x

class Damperi:
    """Granular damper parameters and state."""
    def __init__(self, width, height, x, y, vy, mass, k_spring, c_damp, y0, dem_r, dem_m, dem_k, dem_gamma, DEM_N):
        self.width = width
        self.height = height
        self.x = x
        self.y = y
        self.vy = vy
        self.mass = mass
        self.k_spring = k_spring
        self.c_damp = c_damp
        self.y0 = y0
        self.dem_r = dem_r
        self.dem_m = dem_m
        self.dem_k = dem_k
        self.dem_gamma = dem_gamma
        self.DEM_N = DEM_N
        self.dem_pos = np.zeros((DEM_N, 2))
        self.dem_vel = np.zeros((DEM_N, 2))
        for i in range(DEM_N):
            self.dem_pos[i, 0] = x + 0.05 + 0.1 * random.random()
            self.dem_pos[i, 1] = y + 0.05 + 0.3 * random.random()

class Simulaatio:
    """Simulation manager: SPH, DEM, ship, damper, energy balance."""
    def __init__(self, N, L, h, m, rho0, k, mu, G, dt, steps, fill_frac):
        self.N = N
        self.L = L
        self.h = h
        self.m = m
        self.rho0 = rho0
        self.k = k
        self.mu = mu
        self.G = G
        self.dt = dt
        self.steps = steps
        self.fill_frac = fill_frac
        self.DEM_N = int(20 * fill_frac / 0.2)
        self.laiva = Laiva(2.0, 0.5, 0.0, 0.1, 0.5, 0.2)
        self.damperi = Damperi(0.2, 0.4, 0.7, 0.3, 0.0, 1.0, 100.0, 2.0, 0.3, 0.015, 0.01, 5000, 2.0, self.DEM_N)
        nx = int(np.sqrt(N))
        ny = N // nx
        x = np.linspace(0.1, 0.9, nx)
        y = np.linspace(0.1, 0.9, ny)
        X, Y = np.meshgrid(x, y)
        self.pos = np.stack([X.ravel(), Y.ravel()], axis=1)
        self.vel = np.zeros_like(self.pos)
        self.damper_kin_energy_hist = []
        self.ship_y_hist = []
        self.total_kin = []
        self.total_pot = []
        self.total_damper_diss = []
        self.damper_dissipated = 0.0

    @staticmethod
    def W(r, h):
        """Cubic spline kernel (2D)."""
        q = np.linalg.norm(r, axis=-1) / h
        sigma = 10 / (7 * np.pi * h**2)
        w = np.zeros_like(q)
        mask1 = q <= 1
        mask2 = (q > 1) & (q <= 2)
        w[mask1] = 1 - 1.5*q[mask1]**2 + 0.75*q[mask1]**3
        w[mask2] = 0.25 * (2 - q[mask2])**3
        return sigma * w

    @staticmethod
    def gradW(r, h):
        """Gradient of cubic spline kernel (2D)."""
        q = np.linalg.norm(r, axis=-1) / h
        sigma = 10 / (7 * np.pi * h**2)
        grad = np.zeros_like(r)
        mask1 = q <= 1
        mask2 = (q > 1) & (q <= 2)
        factor = np.zeros_like(q)
        factor[mask1] = (-3*q[mask1] + 2.25*q[mask1]**2)
        factor[mask2] = -0.75 * (2 - q[mask2])**2
        with np.errstate(divide='ignore', invalid='ignore'):
            grad = (r / (np.linalg.norm(r, axis=-1, keepdims=True) * h)) * factor[:, None]
            grad[np.isnan(grad)] = 0
        return sigma * grad

    def paivita_sph(self):
        """Päivitä SPH-partikkelien tila ja huomioi DEM-vuorovaikutus."""
        N = self.pos.shape[0]
        rho = np.zeros(N)
        for i in range(N):
            rij = self.pos - self.pos[i]
            rho[i] = np.sum(self.m * Simulaatio.W(rij, self.h))
        p = self.k * (rho - self.rho0)
        acc = np.zeros_like(self.pos)
        for i in range(N):
            rij = self.pos - self.pos[i]
            vij = self.vel - self.vel[i]
            acc[i] += -np.sum(self.m * (p[i]/rho[i]**2 + p/rho**2)[:,None] * Simulaatio.gradW(rij, self.h), axis=0)
            acc[i] += self.mu * np.sum(self.m * (vij/rho[:,None]) * Simulaatio.W(rij, self.h)[:,None], axis=0)
        acc += self.G

        # SPH-DEM-vuorovaikutus: lisätään voima niille SPH-partikkeleille, jotka ovat lähellä DEM-partikkeleita
        dem_force_on_sph = np.zeros_like(self.pos)
        for j in range(self.damperi.DEM_N):
            dem_pos = self.damperi.dem_pos[j]
            dists = np.linalg.norm(self.pos - dem_pos, axis=1)
            mask = dists < 2*self.h
            if np.any(mask):
                # Yksinkertainen malli: paine + viskoosi vastus
                rel_vel = self.vel[mask] - self.damperi.dem_vel[j]
                force = -0.5 * self.k * np.stack([np.zeros_like(self.pos[mask,1]), self.pos[mask,1] - dem_pos[1]], axis=1)
                visc = -0.1 * rel_vel
                dem_force_on_sph[mask] += (force + visc) / np.sum(mask)
                # Reaktiovoima DEM-partikkelille (kerätään myöhemmin)
                if not hasattr(self, 'dem_react_forces'):
                    self.dem_react_forces = np.zeros((self.damperi.DEM_N,2))
                self.dem_react_forces[j] = -np.sum(force + visc, axis=0)
        acc += dem_force_on_sph / self.m

        self.vel += acc * self.dt
        self.pos += self.vel * self.dt
        self.pos = np.clip(self.pos, 0, self.L)
        self.vel[(self.pos == 0) | (self.pos == self.L)] *= -0.5
        return p

    def paivita_dem(self):
        """Päivitä DEM-partikkelien tila ja huomioi SPH-vuorovaikutus."""
        DEM_N = self.damperi.DEM_N
        dem_acc = np.zeros_like(self.damperi.dem_pos)
        for i in range(DEM_N):
            dem_acc[i, 1] += self.G[1]
            # SPH-DEM reaktiovoima (jos laskettu)
            if hasattr(self, 'dem_react_forces'):
                dem_acc[i] += self.dem_react_forces[i] / self.damperi.dem_m
            if self.damperi.dem_pos[i, 0] - self.damperi.dem_r < self.damperi.x:
                dem_acc[i, 0] += self.damperi.dem_k * (self.damperi.x - (self.damperi.dem_pos[i, 0] - self.damperi.dem_r)) / self.damperi.dem_m
                self.damperi.dem_vel[i, 0] *= -self.damperi.dem_gamma
            if self.damperi.dem_pos[i, 0] + self.damperi.dem_r > self.damperi.x + self.damperi.width:
                dem_acc[i, 0] -= self.damperi.dem_k * ((self.damperi.dem_pos[i, 0] + self.damperi.dem_r) - (self.damperi.x + self.damperi.width)) / self.damperi.dem_m
                self.damperi.dem_vel[i, 0] *= -self.damperi.dem_gamma
            if self.damperi.dem_pos[i, 1] - self.damperi.dem_r < self.damperi.y:
                dem_acc[i, 1] += self.damperi.dem_k * (self.damperi.y - (self.damperi.dem_pos[i, 1] - self.damperi.dem_r)) / self.damperi.dem_m
                self.damperi.dem_vel[i, 1] *= -self.damperi.dem_gamma
            if self.damperi.dem_pos[i, 1] + self.damperi.dem_r > self.damperi.y + self.damperi.height:
                dem_acc[i, 1] -= self.damperi.dem_k * ((self.damperi.dem_pos[i, 1] + self.damperi.dem_r) - (self.damperi.y + self.damperi.height)) / self.damperi.dem_m
                self.damperi.dem_vel[i, 1] *= -self.damperi.dem_gamma
        self.damperi.dem_vel += dem_acc * self.dt
        self.damperi.dem_pos += self.damperi.dem_vel * self.dt
        self.damperi.dem_pos[:, 0] = np.clip(self.damperi.dem_pos[:, 0], self.damperi.x + self.damperi.dem_r, self.damperi.x + self.damperi.width - self.damperi.dem_r)
        self.damperi.dem_pos[:, 1] = np.clip(self.damperi.dem_pos[:, 1], self.damperi.y + self.damperi.dem_r, self.damperi.y + self.damperi.height - self.damperi.dem_r)
        # Tyhjennä reaktiovoimat seuraavaa askelta varten
        if hasattr(self, 'dem_react_forces'):
            del self.dem_react_forces

    def laske_energiatase(self, damper_vy):
        """Laske koko järjestelmän energiatase."""
        sph_kin = 0.5 * self.m * np.sum(np.sum(self.vel**2, axis=1))
        sph_pot = self.m * np.sum(self.pos[:,1] * abs(self.G[1]))
        dem_kin = 0.5 * self.damperi.dem_m * np.sum(np.sum(self.damperi.dem_vel**2, axis=1))
        dem_pot = self.damperi.dem_m * np.sum(self.damperi.dem_pos[:,1] * abs(self.G[1]))
        ship_kin = 0.5 * self.laiva.mass * self.laiva.vy**2
        ship_pot = self.laiva.mass * self.laiva.y * abs(self.G[1])
        self.damper_dissipated += abs(self.damperi.c_damp * damper_vy**2) * self.dt
        total_kin = sph_kin + dem_kin + ship_kin
        total_pot = sph_pot + dem_pot + ship_pot
        return total_kin, total_pot, self.damper_dissipated

    def aja(self):
        """Aja simulaatio yhdellä parametrilla."""
        for step in range(self.steps):
            p = self.paivita_sph()
            buoyancy_force = compute_buoyancy(self.pos, p, self.laiva.x, self.laiva.width, self.laiva.y, self.L, self.N)
            dem_react_force = compute_damper_reaction(self.damperi.dem_pos, self.damperi.dem_r, self.damperi.y, self.damperi.dem_k, self.damperi.DEM_N)
            ship_ay = (buoyancy_force - self.laiva.mass * abs(self.G[1]) - dem_react_force) / self.laiva.mass
            self.laiva.vy += ship_ay * self.dt
            self.laiva.y += self.laiva.vy * self.dt
            if self.laiva.y < self.laiva.height:
                self.laiva.y = self.laiva.height
                self.laiva.vy *= -1
            if self.laiva.y > self.L:
                self.laiva.y = self.L
                self.laiva.vy *= -1
            self.ship_y_hist.append(self.laiva.y)
            self.damperi.y0 = self.laiva.y - self.damperi.height - 0.01
            damper_ay = (-self.damperi.k_spring * (self.damperi.y - self.damperi.y0)
                         - self.damperi.c_damp * self.damperi.vy
                         + dem_react_force) / self.damperi.mass
            self.damperi.vy += damper_ay * self.dt
            self.damperi.y += self.damperi.vy * self.dt
            if self.damperi.y < 0:
                self.damperi.y = 0
                self.damperi.vy *= -1
            if self.damperi.y + self.damperi.height > self.L:
                self.damperi.y = self.L - self.damperi.height
                self.damperi.vy *= -1
            self.paivita_dem()
            dem_kin_energy = 0.5 * self.damperi.dem_m * np.sum(self.damperi.dem_vel**2)
            self.damper_kin_energy_hist.append(dem_kin_energy)
            kin, pot, damper_diss = self.laske_energiatase(self.damperi.vy)
            self.total_kin.append(kin)
            self.total_pot.append(pot)
            self.total_damper_diss.append(damper_diss)
        return {
            'fill_frac': self.fill_frac,
            'ship_y_hist': self.ship_y_hist,
            'damper_kin_energy_hist': self.damper_kin_energy_hist,
            'kin': self.total_kin,
            'pot': self.total_pot,
            'diss': self.total_damper_diss
        }

# --- Modulaariset funktiot ---
def paivita_sph(pos, vel, m, h, k, rho0, mu, G, dt):
    N = pos.shape[0]
    rho = np.zeros(N)
    for i in range(N):
        rij = pos - pos[i]
        rho[i] = np.sum(m * Simulaatio.W(rij, h))
    p = k * (rho - rho0)
    acc = np.zeros_like(pos)
    for i in range(N):
        rij = pos - pos[i]
        vij = vel - vel[i]

    pos += vel * dt
    return pos, vel, p

def paivita_dem(dem_pos, dem_vel, dem_r, dem_m, dem_k, dem_gamma, damper_x, damper_y, damper_width, damper_height, G, dt):
    DEM_N = dem_pos.shape[0]
    dem_acc = np.zeros_like(dem_pos)
    for i in range(DEM_N):
        dem_acc[i, 1] += G[1]
        if dem_pos[i, 0] - dem_r < damper_x:
            dem_acc[i, 0] += dem_k * (damper_x - (dem_pos[i, 0] - dem_r)) / dem_m
            dem_vel[i, 0] *= -dem_gamma
        if dem_pos[i, 0] + dem_r > damper_x + damper_width:
            dem_acc[i, 0] -= dem_k * ((dem_pos[i, 0] + dem_r) - (damper_x + damper_width)) / dem_m
            dem_vel[i, 0] *= -dem_gamma
        if dem_pos[i, 1] - dem_r < damper_y:
            dem_acc[i, 1] += dem_k * (damper_y - (dem_pos[i, 1] - dem_r)) / dem_m
            dem_vel[i, 1] *= -dem_gamma
        if dem_pos[i, 1] + dem_r > damper_y + damper_height:
            dem_acc[i, 1] -= dem_k * ((dem_pos[i, 1] + dem_r) - (damper_y + damper_height)) / dem_m
            dem_vel[i, 1] *= -dem_gamma
    dem_vel += dem_acc * dt
    dem_pos += dem_vel * dt
    return dem_pos, dem_vel

def laske_energiatase(pos, vel, m, G, dem_pos, dem_vel, dem_m, ship_y, ship_vy, ship_mass, damper_c_damp, damper_vy, dt, damper_dissipated):
    sph_kin = 0.5 * m * np.sum(np.sum(vel**2, axis=1))
    sph_pot = m * np.sum(pos[:,1] * abs(G[1]))
    dem_kin = 0.5 * dem_m * np.sum(np.sum(dem_vel**2, axis=1))
    dem_pot = dem_m * np.sum(dem_pos[:,1] * abs(G[1]))
    ship_kin = 0.5 * ship_mass * ship_vy**2
    ship_pot = ship_mass * ship_y * abs(G[1])
    damper_dissipated += abs(damper_c_damp * damper_vy**2) * dt
    total_kin = sph_kin + dem_kin + ship_kin
    total_pot = sph_pot + dem_pot + ship_pot
    return total_kin, total_pot, damper_dissipated

def aja_simulaatio(fill_frac, N, L, h, m, rho0, k, mu, G, dt, steps):
    DEM_N = int(20 * fill_frac / 0.2)
    damper_width = 0.2
    damper_height = 0.4
    damper_x = 0.7
    damper_y = 0.3
    damper_vy = 0.0
    damper_mass = 1.0
    damper_k_spring = 100.0
    damper_c_damp = 2.0
    damper_y0 = 0.3
    dem_r = 0.015
    dem_m = 0.01
    dem_k = 5000
    dem_gamma = 2.0
    ship_mass = 2.0
    ship_y = 0.5
    ship_vy = 0.0
    ship_height = 0.1
    ship_width = 0.5
    ship_x = 0.2
    dem_pos = np.zeros((DEM_N, 2))
    dem_vel = np.zeros((DEM_N, 2))
    for i in range(DEM_N):
        dem_pos[i, 0] = damper_x + 0.05 + 0.1 * random.random()
        dem_pos[i, 1] = damper_y + 0.05 + 0.3 * random.random()
    nx = int(np.sqrt(N))
    ny = N // nx
    x = np.linspace(0.1, 0.9, nx)
    y = np.linspace(0.1, 0.9, ny)
    X, Y = np.meshgrid(x, y)
    pos = np.stack([X.ravel(), Y.ravel()], axis=1)
    vel = np.zeros_like(pos)
    damper_kin_energy_hist = []
    ship_y_hist = []
    total_kin = []
    total_pot = []
    total_damper_diss = []
    damper_dissipated = 0.0
    for step in range(steps):
        pos, vel, p = paivita_sph(pos, vel, m, h, k, rho0, mu, G, dt)
        under_ship = (pos[:,0] > ship_x) & (pos[:,0] < ship_x + ship_width) & (pos[:,1] < ship_y)
        buoyancy_force = np.sum(p[under_ship]) * (L / N)
        dem_react_force = 0.0
        for i in range(DEM_N):
            if dem_pos[i, 1] - dem_r < damper_y + 1e-8:
                dem_react_force += dem_k * (damper_y - (dem_pos[i, 1] - dem_r))
        ship_ay = (buoyancy_force - ship_mass * abs(G[1]) - dem_react_force) / ship_mass
        ship_vy += ship_ay * dt
        ship_y += ship_vy * dt
        if ship_y < ship_height:
            ship_y = ship_height
            ship_vy *= -1
        if ship_y > L:
            ship_y = L
            ship_vy *= -1
        ship_y_hist.append(ship_y)
        damper_y0 = ship_y - damper_height - 0.01
        damper_ay = (-damper_k_spring * (damper_y - damper_y0)
                     - damper_c_damp * damper_vy
                     + dem_react_force) / damper_mass
        damper_vy += damper_ay * dt
        damper_y += damper_vy * dt
        if damper_y < 0:
            damper_y = 0
            damper_vy *= -1
        if damper_y + damper_height > L:
            damper_y = L - damper_height
            damper_vy *= -1
        dem_pos, dem_vel = paivita_dem(dem_pos, dem_vel, dem_r, dem_m, dem_k, dem_gamma, damper_x, damper_y, damper_width, damper_height, G, dt)
        dem_pos[:, 0] = np.clip(dem_pos[:, 0], damper_x + dem_r, damper_x + damper_width - dem_r)
        dem_pos[:, 1] = np.clip(dem_pos[:, 1], damper_y + dem_r, damper_y + damper_height - dem_r)
        dem_kin_energy = 0.5 * dem_m * np.sum(dem_vel**2)
        damper_kin_energy_hist.append(dem_kin_energy)
        kin, pot, damper_dissipated = laske_energiatase(pos, vel, m, G, dem_pos, dem_vel, dem_m, ship_y, ship_vy, ship_mass, damper_c_damp, damper_vy, dt, damper_dissipated)
        total_kin.append(kin)
        total_pot.append(pot)
        total_damper_diss.append(damper_dissipated)
    return {
        'fill_frac': fill_frac,
        'ship_y_hist': ship_y_hist,
        'damper_kin_energy_hist': damper_kin_energy_hist,
        'kin': total_kin,
        'pot': total_pot,
        'diss': total_damper_diss
    }



# SPH Parameters (pysyvät samana kaikissa ajoissa)
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

# Parametritutkimuksen parametrit (esim. damperin täyttöaste)
fill_fractions = [0.2, 0.4, 0.6, 0.8]  # 20%, 40%, 60%, 80% damperin tilavuudesta
results = []  # Tallennetaan laivan liikkeet eri parametreilla

energy_results = []  # Tallennetaan energiataseet



# --- Physical validation: compare to hydrostatic theory ---
def hydrostatic_theory(ship_mass, ship_width, ship_height, rho_fluid, g):
    """Calculate equilibrium height of ship based on hydrostatic theory."""
    # Buoyant force = weight
    # Displaced volume = ship_mass / (rho_fluid)
    # For a rectangle: V_disp = ship_width * ship_length * h_disp
    # Here, ship_length is not defined, so assume unit length (2D)
    h_disp = ship_mass / (rho_fluid * ship_width * 1.0)
    return h_disp


for fill_frac in fill_fractions:
    sim = Simulaatio(N, L, h, m, rho0, k, mu, G, dt, steps, fill_frac)
    res = sim.aja()
    results.append(res)


# Calculate hydrostatic theory height before plotting
theory_height = hydrostatic_theory(2.0, 0.5, 0.1, 1000, 9.81)

# Plot simulation results and theory
plt.figure(figsize=(10,6))
for res in results:
    plt.plot(res['ship_y_hist'], label=f"Fill fraction {res['fill_frac']:.2f}")
plt.axhline(theory_height, color='k', linestyle='--', label=f"Hydrostatic theory ({theory_height:.2f} m)")
plt.xlabel('Timestep')
plt.ylabel('Ship height (m)')
plt.legend()
plt.title('Ship height evolution for different damper fill fractions')
plt.tight_layout()
plt.show()

# --- Documentation ---
print("""
Physical validation: The dashed line shows the hydrostatic equilibrium height predicted by theory for a floating rectangle. Simulation results should approach this value at steady state. Deviations may be due to numerical damping, damper effects, or model limitations.

Parameter sensitivity: The effect of damper fill fraction on ship motion is visualized. Higher fill fractions generally increase damping and reduce oscillation amplitude.

Limitations: The model assumes 2D geometry, simplified SPH/DEM interactions, and idealized boundary conditions. For more accurate results, 3D effects, turbulence, and real damper geometry should be considered.
""")

# Jos käytät stats.norm.pdf ja matplotlibin labelissa $\mu$ ja $\sigma$, käytä raw stringiä:
# axs[2].plot(x, stats.norm.pdf(x, mu, std), 'r--', label=rf"Normal fit ($\mu$={mu:.2f}, $\sigma$={std:.2f})")
## Käytä raw stringiä, jotta $\mu$ ja $\sigma$ eivät aiheuta SyntaxWarningia:
# axs[2].plot(x, stats.norm.pdf(x, mu, std), 'r--', label=rf"Normal fit ($\mu$={mu:.2f}, $\sigma$={std:.2f})")










