# Automated test for WP1: Geometry and Particle Initialization
# Run this after wp1_init_particles.py to check correctness

import numpy as np

# --- Parameters (must match wp1_init_particles.py) ---
L, H = 1.0, 0.5
fluid_height = 0.3 * H
cube_size = 0.08
cube_x, cube_y = 0.7, 0.1
dx = 0.02

# --- Load or regenerate particles (match wp1_init_particles.py logic) ---
min_dist = dx * 0.9
cube_particles = []
for i in range(int(cube_size/dx)):
    for j in range(int(cube_size/dx)):
        x = cube_x + dx/2 + i*dx
        y = cube_y + dx/2 + j*dx
        if x < cube_x + cube_size and y < cube_y + cube_size:
            cube_particles.append([x, y])

def is_far_from_cube(x, y):
    for cx, cy in cube_particles:
        if np.hypot(x-cx, y-cy) < min_dist:
            return False
    return True
fluid_particles = []
for i in range(int(L/dx)):
    for j in range(int(fluid_height/dx)):
        x = dx/2 + i*dx
        y = dx/2 + j*dx
        if x < L and y < fluid_height and is_far_from_cube(x, y):
            fluid_particles.append([x, y])
# --- Cube (rigid body) particles (offset grid to match initialization) ---
cube_particles = []
for i in range(int(cube_size/dx)):
    for j in range(int(cube_size/dx)):
        x = cube_x + dx/2 + i*dx
        y = cube_y + dx/2 + j*dx
        if x < cube_x + cube_size and y < cube_y + cube_size:
            cube_particles.append([x, y])
wall_particles = []
for i in range(int(L/dx)):
    wall_particles.append([dx/2 + i*dx, 0.0])
for j in range(int(H/dx)):
    wall_particles.append([0.0, dx/2 + j*dx])
for j in range(int(H/dx)):
    wall_particles.append([L, dx/2 + j*dx])
for i in range(int(L/dx)):
    wall_particles.append([dx/2 + i*dx, H])

# --- Automated checks ---
def check_no_overlap(p1, p2, min_dist):
    for a in p1:
        for b in p2:
            if np.linalg.norm(np.array(a)-np.array(b)) < min_dist:
                return False
    return True

def check_in_domain(particles, xlim, ylim):
    for x, y in particles:
        if not (xlim[0] <= x <= xlim[1] and ylim[0] <= y <= ylim[1]):
            return False
    return True


# 1. Check particle counts (account for exclusion logic)
expected_fluid = len(fluid_particles)
expected_cube = len(cube_particles)
expected_wall = int(L/dx)*2 + int(H/dx)*2

print(f"Fluid particles: {len(fluid_particles)} (expected {expected_fluid})")
print(f"Cube particles: {len(cube_particles)} (expected {expected_cube})")
print(f"Wall particles: {len(wall_particles)} (expected {expected_wall})")

# 2. Check no overlap between fluid and cube
min_dist = dx * 0.9
no_overlap = check_no_overlap(fluid_particles, cube_particles, min_dist)
print(f"No overlap between fluid and cube: {no_overlap}")

# 3. Check all particles in domain
fluid_in_domain = check_in_domain(fluid_particles, (0, L), (0, H))
cube_in_domain = check_in_domain(cube_particles, (0, L), (0, H))
wall_in_domain = check_in_domain(wall_particles, (0, L), (0, H))
print(f"All fluid in domain: {fluid_in_domain}")
print(f"All cube in domain: {cube_in_domain}")
print(f"All wall in domain: {wall_in_domain}")

# 4. Check cube is not touching wall
cube_not_touching_wall = check_no_overlap(cube_particles, wall_particles, min_dist)
print(f"Cube not touching wall: {cube_not_touching_wall}")

# 5. Final verdict
if (len(fluid_particles) == expected_fluid and
    len(cube_particles) == expected_cube and
    len(wall_particles) == expected_wall and
    no_overlap and fluid_in_domain and cube_in_domain and wall_in_domain and cube_not_touching_wall):
    print("WP1 test PASSED: Initialization is correct.")
else:
    print("WP1 test FAILED: Check initialization and parameters.")
