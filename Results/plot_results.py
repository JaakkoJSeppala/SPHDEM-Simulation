#!/usr/bin/env python3
import pandas as pd
import matplotlib.pyplot as plt
import numpy as np

plt.style.use('seaborn-v0_8-darkgrid')

# Figure 1: Damping comparison
fig, axes = plt.subplots(1, 3, figsize=(15, 4))

df_damp = pd.read_csv('damping_comparison.csv')
df_damp['Configuration'] = df_damp['Configuration'].astype(str)
configs = [c.replace('\\%', '%') for c in df_damp['Configuration']]

axes[0].bar(range(len(df_damp)), df_damp['Peak_Reduction_%'], color='steelblue', alpha=0.8)
axes[0].set_xticks(range(len(df_damp)))
axes[0].set_xticklabels(configs, rotation=45, ha='right', fontsize=8)
axes[0].set_ylabel('Peak Pressure Reduction (%)')
axes[0].set_title('Impact Pressure Reduction')
axes[0].grid(axis='y', alpha=0.3)

axes[1].bar(range(len(df_damp)), df_damp['Energy_Dissipation_%'], color='darkorange', alpha=0.8)
axes[1].set_xticks(range(len(df_damp)))
axes[1].set_xticklabels(configs, rotation=45, ha='right', fontsize=8)
axes[1].set_ylabel('Energy Dissipation (%)')
axes[1].set_title('Energy Dissipation Effectiveness')
axes[1].grid(axis='y', alpha=0.3)

axes[2].bar(range(len(df_damp)), df_damp['Effective_Damping'], color='forestgreen', alpha=0.8)
axes[2].set_xticks(range(len(df_damp)))
axes[2].set_xticklabels(configs, rotation=45, ha='right', fontsize=8)
axes[2].set_ylabel('Damping Ratio')
axes[2].set_title('Effective Damping Ratio')
axes[2].grid(axis='y', alpha=0.3)

plt.tight_layout()
plt.savefig('fig_damping_comparison.pdf', dpi=300, bbox_inches='tight')
print('Saved: fig_damping_comparison.pdf')

# Figure 2: Time series decay
fig, axes = plt.subplots(1, 2, figsize=(12, 4))

for name, label, color in [('no_damper', 'No damper', 'red'), 
                             ('medium_damper', 'Medium (5%)', 'blue'),
                             ('heavy_damper', 'Heavy (10%)', 'green')]:
    df = pd.read_csv(f'time_series_{name}.csv')
    axes[0].plot(df['Time_s'], df['Wave_Height_m'], label=label, linewidth=2, color=color, alpha=0.7)
    axes[1].plot(df['Time_s'], df['Kinetic_Energy_J'], label=label, linewidth=2, color=color, alpha=0.7)

axes[0].set_xlabel('Time (s)')
axes[0].set_ylabel('Wave Height (m)')
axes[0].set_title('Sloshing Decay: Wave Height')
axes[0].legend()
axes[0].grid(alpha=0.3)

axes[1].set_xlabel('Time (s)')
axes[1].set_ylabel('Kinetic Energy (J)')
axes[1].set_title('Sloshing Decay: Kinetic Energy')
axes[1].legend()
axes[1].grid(alpha=0.3)
axes[1].set_yscale('log')

plt.tight_layout()
plt.savefig('fig_time_decay.pdf', dpi=300, bbox_inches='tight')
print('Saved: fig_time_decay.pdf')

# Figure 3: Parametric study
fig, axes = plt.subplots(1, 2, figsize=(12, 4))

df_param = pd.read_csv('parametric_study.csv')

# Particle size effect
size_data = df_param[df_param['Particle_Diameter_mm'].notna()]
axes[0].plot(size_data['Particle_Diameter_mm'], size_data['Damping_Ratio'], 
             marker='o', linewidth=2, markersize=8, color='steelblue')
axes[0].axvline(5, color='red', linestyle='--', alpha=0.5, label='Optimal (5mm)')
axes[0].set_xlabel('Particle Diameter (mm)')
axes[0].set_ylabel('Damping Ratio')
axes[0].set_title('Effect of Particle Size')
axes[0].legend()
axes[0].grid(alpha=0.3)

# Mass ratio effect
mass_data = df_param[df_param['Mass_Ratio_%'].notna()]
axes[1].plot(mass_data['Mass_Ratio_%'], mass_data['Damping_Ratio_2'], 
             marker='s', linewidth=2, markersize=8, color='darkorange')
axes[1].axhline(0.15, color='red', linestyle='--', alpha=0.5, label='Practical limit')
axes[1].set_xlabel('Granular Mass Ratio (%)')
axes[1].set_ylabel('Damping Ratio')
axes[1].set_title('Effect of Damper Mass')
axes[1].legend()
axes[1].grid(alpha=0.3)

plt.tight_layout()
plt.savefig('fig_parametric_study.pdf', dpi=300, bbox_inches='tight')
print('Saved: fig_parametric_study.pdf')

# Figure 4: Frequency response
fig, ax = plt.subplots(figsize=(8, 5))

df_freq = pd.read_csv('frequency_response.csv')
ax.plot(df_freq['Frequency_Hz'], df_freq['No_Damper'], label='No damper (ζ=0.01)', 
        linewidth=2, color='red')
ax.plot(df_freq['Frequency_Hz'], df_freq['Medium_Damper'], label='Medium damper (ζ=0.089)', 
        linewidth=2, color='blue')
ax.plot(df_freq['Frequency_Hz'], df_freq['Heavy_Damper'], label='Heavy damper (ζ=0.142)', 
        linewidth=2, color='green')

ax.axvline(0.5, color='black', linestyle='--', alpha=0.3, label='Natural frequency')
ax.set_xlabel('Excitation Frequency (Hz)')
ax.set_ylabel('Amplitude Ratio')
ax.set_title('Frequency Response: Effect of Damping')
ax.legend()
ax.grid(alpha=0.3)
ax.set_ylim(0, 15)

plt.tight_layout()
plt.savefig('fig_frequency_response.pdf', dpi=300, bbox_inches='tight')
print('Saved: fig_frequency_response.pdf')

print('\nAll figures generated successfully!')
