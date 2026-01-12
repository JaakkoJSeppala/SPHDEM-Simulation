import os
import glob
import pandas as pd
import matplotlib.pyplot as plt

RESULTS_DIR = os.path.join(os.path.dirname(__file__), '..', 'Results')
SUMMARY_DIR = os.path.join(RESULTS_DIR, 'summary')
SWEEP_NODAMPER_DIR = os.path.join(RESULTS_DIR, 'sweep_nodamper')
SWEEP_WITHDAMPER_DIR = os.path.join(RESULTS_DIR, 'sweep_withdamper')

os.makedirs(SUMMARY_DIR, exist_ok=True)
os.makedirs(SWEEP_NODAMPER_DIR, exist_ok=True)
os.makedirs(SWEEP_WITHDAMPER_DIR, exist_ok=True)

def plot_frequency_response():
    csv_path = os.path.join(RESULTS_DIR, 'frequency_response.csv')
    if not os.path.exists(csv_path):
        print(f"Missing: {csv_path}")
        return
    df = pd.read_csv(csv_path)
    plt.figure()
    for col in df.columns:
        if col != 'Frequency_Hz':
            plt.plot(df['Frequency_Hz'], df[col], label=col)
    plt.xlabel('Frequency (Hz)')
    plt.ylabel('Amplitude')
    plt.title('Frequency Response')
    plt.legend()
    plt.savefig(os.path.join(RESULTS_DIR, 'fig_frequency_response.pdf'))
    plt.close()

def plot_damping_comparison():
    csv_path = os.path.join(RESULTS_DIR, 'damping_comparison.csv')
    if not os.path.exists(csv_path):
        print(f"Missing: {csv_path}")
        return
    df = pd.read_csv(csv_path)
    plt.figure(figsize=(10,6))
    x = df['Configuration']
    width = 0.25
    plt.bar(x, df['Peak_Reduction_%'], width, label='Peak Reduction (%)')
    plt.bar(x, df['Energy_Dissipation_%'], width, label='Energy Dissipation (%)', bottom=df['Peak_Reduction_%'])
    plt.bar(x, df['Effective_Damping'], width, label='Effective Damping', bottom=df['Peak_Reduction_%']+df['Energy_Dissipation_%'])
    plt.xlabel('Configuration')
    plt.ylabel('Value')
    plt.title('Damping Comparison')
    plt.xticks(rotation=30, ha='right')
    plt.legend()
    plt.tight_layout()
    plt.savefig(os.path.join(RESULTS_DIR, 'fig_damping_comparison.pdf'))
    plt.close()

def plot_parametric_study():
    csv_path = os.path.join(RESULTS_DIR, 'parametric_study.csv')
    if not os.path.exists(csv_path):
        print(f"Missing: {csv_path}")
        return
    df = pd.read_csv(csv_path)
    plt.figure()
    x = df['Particle_Diameter_mm']
    for col in df.columns:
        if col != 'Particle_Diameter_mm':
            plt.plot(x, df[col], label=col)
    plt.xlabel('Particle Diameter (mm)')
    plt.ylabel('Value')
    plt.title('Parametric Study')
    plt.legend()
    plt.savefig(os.path.join(RESULTS_DIR, 'fig_parametric_study.pdf'))
    plt.close()

def plot_time_series():
    for fname in ['time_series_no_damper.csv', 'time_series_medium_damper.csv', 'time_series_heavy_damper.csv']:
        csv_path = os.path.join(RESULTS_DIR, fname)
        if not os.path.exists(csv_path):
            print(f"Missing: {csv_path}")
            continue
        df = pd.read_csv(csv_path)
        plt.figure()
        x = df['Time_s']
        for col in df.columns:
            if col != 'Time_s':
                plt.plot(x, df[col], label=col)
        plt.xlabel('Time (s)')
        plt.ylabel('Value')
        plt.title(fname.replace('.csv', ''))
        plt.legend()
        plt.savefig(os.path.join(RESULTS_DIR, f"fig_{fname.replace('.csv', '')}.pdf"))
        plt.close()

def main():
    plot_frequency_response()
    plot_damping_comparison()
    plot_parametric_study()
    plot_time_series()
    print("Plotting complete.")

if __name__ == "__main__":
    main()
