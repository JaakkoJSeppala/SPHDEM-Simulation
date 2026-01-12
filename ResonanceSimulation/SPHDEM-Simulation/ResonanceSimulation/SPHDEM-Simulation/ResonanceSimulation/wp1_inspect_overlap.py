# Inspect closest fluid-cube pairs for overlap
import numpy as np

# --- Parameters (must match wp1_init_particles.py) ---
L, H = 1.0, 0.5
fluid_height = 0.3 * H
cube_size = 0.08
cube_x, cube_y = 0.7, 0.1
dx = 0.02
min_dist = dx * 0.9

# --- Recreate particles as in wp1_init_particles.py (with offset cube grid) ---
cube_particles = []
for i in range(int(cube_size/dx)):
    for j in range(int(cube_size/dx)):
        x = cube_x + dx/2 + i*dx
        y = cube_y + dx/2 + j*dx
        if x < cube_x + cube_size and y < cube_y + cube_size:
            cube_particles.append([x, y])

fluid_particles = []
def is_far_from_cube(x, y):
    for cx, cy in cube_particles:
        if np.hypot(x-cx, y-cy) < min_dist:
            return False
    return True
for i in range(int(L/dx)):
    for j in range(int(fluid_height/dx)):
        x = dx/2 + i*dx
        y = dx/2 + j*dx
        if x < L and y < fluid_height and is_far_from_cube(x, y):
            fluid_particles.append([x, y])

# --- Find closest pairs ---
min_actual = float('inf')
closest_pair = None
for f in fluid_particles:
    for c in cube_particles:
        d = np.linalg.norm(np.array(f)-np.array(c))
        if d < min_actual:
            min_actual = d
            closest_pair = (f, c)

print(f"Minimum fluid-cube distance: {min_actual:.5f} m (threshold: {min_dist:.5f} m)")
print(f"Closest fluid: {closest_pair[0]}")
print(f"Closest cube:  {closest_pair[1]}")
if min_actual < min_dist:
    print("--> Overlap detected!")
else:
    print("--> No overlap.")
