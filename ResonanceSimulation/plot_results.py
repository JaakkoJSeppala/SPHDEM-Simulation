#!/usr/bin/env python3
"""
Post-processing skripti resonanssisimulaation tuloksille.
Piirtää resonanssikäyrän, paine-etenemän, vapaan pinnan ja energian.
"""

import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from pathlib import Path

# LaTeX-tyyli kuvioille (graduun)
plt.rcParams.update({
    'font.size': 11,
    'font.family': 'serif',
    'text.usetex': False,  # Aseta True jos LaTeX asennettu
    'axes.labelsize': 12,
    'axes.titlesize': 12,
    'legend.fontsize': 10,
    'xtick.labelsize': 10,
    'ytick.labelsize': 10,
    'figure.figsize': (8, 5),
    'figure.dpi': 150
})

def plot_resonance_curve(results_dir='results'):
    """Piirrä resonanssikäyrä: max(p) vs f."""
    
    summary_file = Path(results_dir) / 'summary' / 'resonance_curve.csv'
    if not summary_file.exists():
        print(f"Error: {summary_file} not found!")
        return
    
    df = pd.read_csv(summary_file)
    
    plt.figure(figsize=(8, 5))
    plt.plot(df['Frequency'], df['MaxPressure_NoDamper'], 'o-', 
             label='No damper', color='#CC6677', linewidth=2, markersize=6)
    plt.plot(df['Frequency'], df['MaxPressure_WithDamper'], 's-', 
             label='With damper', color='#117733', linewidth=2, markersize=6)
    
    plt.xlabel('Excitation frequency $f$ [Hz]')
    plt.ylabel('Maximum wall pressure $p_{\\mathrm{max}}$ [Pa]')
    plt.title('Resonance curve: 1:50 ballast tank')
    plt.legend(frameon=True, fancybox=False, edgecolor='black')
    plt.grid(True, alpha=0.3, linestyle='--')
    plt.tight_layout()
    
    output_file = Path(results_dir) / 'summary' / 'fig_resonance_curve.pdf'
    plt.savefig(output_file, bbox_inches='tight')
    print(f"Saved: {output_file}")
    plt.close()
    
    # Laske piikkivaimennus
    idx_peak_nodamper = df['MaxPressure_NoDamper'].idxmax()
    p_peak_nodamper = df.loc[idx_peak_nodamper, 'MaxPressure_NoDamper']
    p_peak_withdamper = df.loc[idx_peak_nodamper, 'MaxPressure_WithDamper']
    reduction = (1 - p_peak_withdamper / p_peak_nodamper) * 100
    
    print(f"\nResonance peak reduction: {reduction:.1f}%")
    print(f"  Without damper: {p_peak_nodamper:.1f} Pa")
    print(f"  With damper: {p_peak_withdamper:.1f} Pa")


def plot_pressure_time_history(results_dir='results', frequency=0.6):
    """Piirrä paine-etenemä p(t) resonanssitaajuudella."""
    
    nodamper_file = Path(results_dir) / 'sweep_nodamper' / f'f_{frequency:.2f}Hz.csv'
    withdamper_file = Path(results_dir) / 'sweep_withdamper' / f'f_{frequency:.2f}Hz.csv'
    
    if not nodamper_file.exists() or not withdamper_file.exists():
        print(f"Error: Time history files not found for f={frequency} Hz")
        return
    
    df_nodamper = pd.read_csv(nodamper_file)
    df_withdamper = pd.read_csv(withdamper_file)
    
    plt.figure(figsize=(10, 4))
    plt.plot(df_nodamper['Time'], df_nodamper['WallPressure'], 
             label='No damper', color='#CC6677', linewidth=1.2, alpha=0.8)
    plt.plot(df_withdamper['Time'], df_withdamper['WallPressure'], 
             label='With damper', color='#117733', linewidth=1.2, alpha=0.8)
    
    plt.xlabel('Time $t$ [s]')
    plt.ylabel('Wall pressure $p(t)$ [Pa]')
    plt.title(f'Pressure time history at resonance ($f={frequency}$ Hz)')
    plt.legend(frameon=True, fancybox=False, edgecolor='black')
    plt.grid(True, alpha=0.3, linestyle='--')
    plt.xlim(0, min(30, df_nodamper['Time'].max()))
    plt.tight_layout()
    
    output_file = Path(results_dir) / 'summary' / f'fig_pressure_time_f{frequency:.2f}Hz.pdf'
    plt.savefig(output_file, bbox_inches='tight')
    print(f"Saved: {output_file}")
    plt.close()


def plot_energy_decay(results_dir='results', frequency=0.6):
    """Piirrä energian vaimeneminen Ek(t)."""
    
    nodamper_file = Path(results_dir) / 'sweep_nodamper' / f'f_{frequency:.2f}Hz.csv'
    withdamper_file = Path(results_dir) / 'sweep_withdamper' / f'f_{frequency:.2f}Hz.csv'
    
    if not nodamper_file.exists() or not withdamper_file.exists():
        print(f"Error: Energy files not found for f={frequency} Hz")
        return
    
    df_nodamper = pd.read_csv(nodamper_file)
    df_withdamper = pd.read_csv(withdamper_file)
    
    plt.figure(figsize=(8, 5))
    plt.semilogy(df_nodamper['Time'], df_nodamper['KineticEnergy'], 
                 label='No damper', color='#CC6677', linewidth=1.5)
    plt.semilogy(df_withdamper['Time'], df_withdamper['KineticEnergy'], 
                 label='With damper', color='#117733', linewidth=1.5)
    
    plt.xlabel('Time $t$ [s]')
    plt.ylabel('Kinetic energy $E_k(t)$ [J]')
    plt.title(f'Energy decay at resonance ($f={frequency}$ Hz)')
    plt.legend(frameon=True, fancybox=False, edgecolor='black')
    plt.grid(True, which='both', alpha=0.3, linestyle='--')
    plt.tight_layout()
    
    output_file = Path(results_dir) / 'summary' / f'fig_energy_decay_f{frequency:.2f}Hz.pdf'
    plt.savefig(output_file, bbox_inches='tight')
    print(f"Saved: {output_file}")
    plt.close()


def plot_damping_ratios(results_dir='results'):
    """Piirrä vaimennussuhde ζ vs taajuus."""
    
    summary_file = Path(results_dir) / 'summary' / 'damping_ratios.csv'
    if not summary_file.exists():
        print(f"Error: {summary_file} not found!")
        return
    
    df = pd.read_csv(summary_file)
    
    plt.figure(figsize=(8, 5))
    plt.plot(df['Frequency'], df['DampingRatio_NoDamper'], 'o-', 
             label='No damper', color='#CC6677', linewidth=2, markersize=6)
    plt.plot(df['Frequency'], df['DampingRatio_WithDamper'], 's-', 
             label='With damper', color='#117733', linewidth=2, markersize=6)
    
    plt.xlabel('Excitation frequency $f$ [Hz]')
    plt.ylabel('Damping ratio $\\zeta$ [-]')
    plt.title('Damping ratio vs frequency')
    plt.legend(frameon=True, fancybox=False, edgecolor='black')
    plt.grid(True, alpha=0.3, linestyle='--')
    plt.tight_layout()
    
    output_file = Path(results_dir) / 'summary' / 'fig_damping_ratios.pdf'
    plt.savefig(output_file, bbox_inches='tight')
    print(f"Saved: {output_file}")
    plt.close()


def main():
    """Suorita kaikki post-processing vaiheet."""
    
    results_dir = 'results'
    
    print("=== Post-processing Resonance Simulation ===\n")
    
    print("1. Plotting resonance curve...")
    plot_resonance_curve(results_dir)
    
    print("\n2. Plotting pressure time history...")
    plot_pressure_time_history(results_dir, frequency=0.6)
    
    print("\n3. Plotting energy decay...")
    plot_energy_decay(results_dir, frequency=0.6)
    
    print("\n4. Plotting damping ratios...")
    plot_damping_ratios(results_dir)
    
    print("\n=== All plots saved to results/summary/ ===")


if __name__ == '__main__':
    main()
