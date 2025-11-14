import matplotlib.pyplot as plt
import numpy as np

# Validation test results
tests = ['Kernel Normalization (∫W dV = 1)', 'Hydrostatic Pressure Formula', 'Archimedes Buoyancy Principle', 'Terminal Velocity (Stokes)', 'Wave Dispersion (Deep Water)', 'Newton\'s 3rd Law (F + (-F) = 0)', 'Drag Force Formula', 'Quaternion Normalization (|q| = 1)'
]
errors = [0,0000, 0,0000, 0,0000, 0,0000, 0,0000, epäluku, 0,0000, 0,0000
]

fig, ax = plt.subplots(figsize=(10, 6))
bars = ax.bar(range(len(tests)), errors, color='steelblue', alpha=0.8)
ax.set_xticks(range(len(tests)))
ax.set_xticklabels(tests, rotation=45, ha='right')
ax.set_ylabel('Relative Error (%)')
ax.set_title('Validation Test Results: Relative Error')
ax.axhline(y=1.0, color='red', linestyle='--', label='1% threshold')
ax.legend()
ax.grid(axis='y', alpha=0.3)
plt.tight_layout()
plt.savefig('validation_results.pdf', dpi=300, bbox_inches='tight')
plt.show()
