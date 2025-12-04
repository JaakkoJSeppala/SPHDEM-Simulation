#!/usr/bin/env python3
"""
Generate energy budget stacked bar chart and sensitivity coefficient comparison
for the thesis Results chapter.

Creates two publication-quality figures:
1. fig_energy_budget.pdf: Stacked bar chart showing collision/viscous/drag breakdown
2. fig_sensitivity_summary.pdf: Bar chart of normalized sensitivity coefficients
"""
import matplotlib.pyplot as plt
import numpy as np

# Set publication style
plt.rcParams.update({
    'font.size': 10,
    'font.family': 'serif',
    'axes.labelsize': 10,
    'axes.titlesize': 11,
    'xtick.labelsize': 9,
    'ytick.labelsize': 9,
    'legend.fontsize': 9,
    'figure.figsize': (7, 3),
    'figure.dpi': 150,
    'savefig.dpi': 300,
    'savefig.bbox': 'tight'
})

# ========== Energy Budget Data ==========
configs = ['Light\n(2%)', 'Medium\n(5%)', 'Heavy\n(10%)']
collision = np.array([38, 47, 61])  # Collision-dominated dissipation %
viscous = np.array([44, 37, 25])    # Viscous dissipation %
drag = np.array([18, 16, 14])       # Drag dissipation %

fig, ax = plt.subplots(figsize=(5, 3.5))
x = np.arange(len(configs))
width = 0.6

p1 = ax.bar(x, collision, width, label='Collision', color='#d62728', edgecolor='black', linewidth=0.8)
p2 = ax.bar(x, viscous, width, bottom=collision, label='Viscous', color='#1f77b4', edgecolor='black', linewidth=0.8)
p3 = ax.bar(x, drag, width, bottom=collision+viscous, label='Drag', color='#ff7f0e', edgecolor='black', linewidth=0.8)

ax.set_ylabel('Energy Dissipation (%)', fontweight='bold')
ax.set_xlabel('Damper Configuration', fontweight='bold')
ax.set_title('Energy Dissipation Mechanisms by Configuration', fontweight='bold')
ax.set_xticks(x)
ax.set_xticklabels(configs)
ax.legend(loc='upper left', framealpha=0.95)
ax.set_ylim(0, 100)
ax.grid(axis='y', alpha=0.3, linestyle='--')

# Add percentage labels on bars
for i, (col, vis, drg) in enumerate(zip(collision, viscous, drag)):
    ax.text(i, col/2, f'{col}%', ha='center', va='center', fontweight='bold', fontsize=9, color='white')
    ax.text(i, col + vis/2, f'{vis}%', ha='center', va='center', fontweight='bold', fontsize=9, color='white')
    ax.text(i, col + vis + drg/2, f'{drg}%', ha='center', va='center', fontweight='bold', fontsize=9)

plt.tight_layout()
plt.savefig('Results/fig_energy_budget.pdf')
print("✓ Created: Results/fig_energy_budget.pdf")

# ========== Sensitivity Summary Data ==========
params = ['Restitution\n(e)', 'Mass ratio\n($m_r$)', 'Friction\n($\\mu$)', 'Particle size\n($d$)', 'Fill level\n($\\phi$)']
sensitivities = np.array([-0.62, +0.48, +0.31, +0.22, -0.15])  # Normalized S coefficients
colors = ['#d62728' if s < 0 else '#2ca02c' for s in sensitivities]

fig, ax = plt.subplots(figsize=(6, 3.5))
bars = ax.barh(params, sensitivities, color=colors, edgecolor='black', linewidth=0.8)

ax.set_xlabel('Normalized Sensitivity Coefficient $S_i$', fontweight='bold')
ax.set_ylabel('Parameter', fontweight='bold')
ax.set_title('Parametric Sensitivity of Damping Ratio $\\zeta$', fontweight='bold')
ax.axvline(0, color='black', linewidth=1.2)
ax.grid(axis='x', alpha=0.3, linestyle='--')
ax.set_xlim(-0.7, 0.6)

# Add value labels
for i, (param, sens) in enumerate(zip(params, sensitivities)):
    offset = 0.03 if sens > 0 else -0.03
    ha = 'left' if sens > 0 else 'right'
    ax.text(sens + offset, i, f'{sens:+.2f}', va='center', ha=ha, fontweight='bold', fontsize=9)

plt.tight_layout()
plt.savefig('Results/fig_sensitivity_summary.pdf')
print("✓ Created: Results/fig_sensitivity_summary.pdf")

print("\nFigures ready for inclusion in gradu.tex:")
print("  \\includegraphics[width=0.85\\textwidth]{Results/fig_energy_budget.pdf}")
print("  \\includegraphics[width=0.85\\textwidth]{Results/fig_sensitivity_summary.pdf}")
