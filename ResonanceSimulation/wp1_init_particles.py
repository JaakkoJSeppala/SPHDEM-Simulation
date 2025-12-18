# WP1: Geometry and Particle Initialization for SPH dam-break + cube benchmark
# This script initializes fluid, cube (rigid body), and wall particles and visualizes the setup.

import numpy as np
import matplotlib.pyplot as plt

# --- Parameters ---
L, H = 1.0, 0.5           # Tank width and height (meters)
dx = 0.02                 # Particle spacing (meters)
fluid_height = 0.3 * H    # Initial fluid height
cube_size = 0.08          # Cube side length (meters)
cube_x, cube_y = 0.7, 0.1 # Cube bottom-left corner

# --- Fluid particles ---
fluid_particles = []
for i in range(int(L/dx)):
    for j in range(int(fluid_height/dx)):
        x = dx/2 + i*dx
        y = dx/2 + j*dx
        if x < L and y < fluid_height:
            fluid_particles.append([x, y])

# --- Cube (rigid body) particles ---
cube_particles = []
for i in range(int(cube_size/dx)):
    for j in range(int(cube_size/dx)):
        x = cube_x + i*dx
        y = cube_y + j*dx
        if x < cube_x + cube_size and y < cube_y + cube_size:
            cube_particles.append([x, y])

# --- Wall particles (bottom, left, right, top) ---
wall_particles = []
# Bottom
for i in range(int(L/dx)):
    wall_particles.append([dx/2 + i*dx, 0.0])
# Left
for j in range(int(H/dx)):
    wall_particles.append([0.0, dx/2 + j*dx])
# Right
for j in range(int(H/dx)):
    wall_particles.append([L, dx/2 + j*dx])
# Top
for i in range(int(L/dx)):
    wall_particles.append([dx/2 + i*dx, H])

# --- Visualization ---
fluid_x, fluid_y = zip(*fluid_particles)
cube_xs, cube_ys = zip(*cube_particles)
wall_x, wall_y = zip(*wall_particles)
plt.figure(figsize=(8,4))
plt.scatter(fluid_x, fluid_y, s=20, c='b', label='Fluid')
plt.scatter(cube_xs, cube_ys, s=20, c='r', label='Cube')
plt.scatter(wall_x, wall_y, s=20, c='k', label='Wall')
plt.xlabel('x [m]')
plt.ylabel('y [m]')
plt.title('WP1: Initial particle configuration for dam-break + cube')
plt.legend()
plt.axis('equal')
plt.tight_layout()
plt.show()
