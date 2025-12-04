#!/usr/bin/env python3
"""
Dummy simulator for SPH–DEM parameter sweep testing.
Generates a synthetic velocity decay signal resembling ring-down sloshing.
"""

import sys, os, numpy as np
from pathlib import Path

# --- arguments ---
if len(sys.argv) < 3:
    print("Usage: dummy_sim.py <config.yaml> <output_dir>")
    sys.exit(1)

config_file = Path(sys.argv[1])
output_dir  = Path(sys.argv[2])
output_dir.mkdir(parents=True, exist_ok=True)

# --- parse minimal config values ---
import yaml
cfg = yaml.safe_load(open(config_file))
np_ = cfg["dem"]["n_particles"]
d_  = cfg["dem"]["particle_size_m"]
mu_ = cfg["fluid"]["mu"]
hf_ = cfg["tank"]["fill_height_m"]

# --- derive pseudo-physical parameters ---
# base case reference
f0 = 5.0                    # [Hz] experimental reference
delta0 = 0.01               # baseline damping ratio (log decrement)
# simple trends: more particles and viscosity increase damping, reduce freq
f = f0 * (1.0 - 0.001*np_) * (1.0 - 0.1*(mu_/8.9e-4 - 1))
delta = delta0 * (1.0 + 0.015*np_ + 5.0*(mu_/8.9e-4 - 1))

# --- generate synthetic ring-down ---
t_end = cfg["simulation"]["time"]["t_end"]
dt = cfg["simulation"]["time"]["output_interval"]
t = np.arange(0, t_end, dt)
A0 = 0.1
v = A0 * np.exp(-delta * 2*np.pi*f * t) * np.sin(2*np.pi*f*t)

# --- save output file ---
np.savetxt(
    output_dir / "velocity_uniform.txt",
    np.column_stack([t, v]),
    header="time[s], velocity[m/s]",
    fmt="%.6e",
)

print(f"[dummy_sim] Wrote synthetic velocity data to {output_dir}")
print(f"   f={f:.2f} Hz, δ={delta:.3f}, n_p={np_}, μ={mu_:.1e}")
