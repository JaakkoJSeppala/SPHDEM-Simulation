import pandas as pd
import matplotlib.pyplot as plt
import os

# Absolute paths
base = r'C:/Users/jaakk/Desktop/Gradu/ShipHydroDynamics/SPHDEM-Simulation-GIT/Results'
no_damper_path = os.path.join(base, 'time_series_no_damper.csv')
medium_damper_path = os.path.join(base, 'time_series_medium_damper.csv')
heavy_damper_path = os.path.join(base, 'time_series_heavy_damper.csv')
output_path = r'C:/Users/jaakk/Desktop/Gradu/Results/kineettinen_energia_vertailu.png'

# Load data
no_damper = pd.read_csv(no_damper_path)
medium_damper = pd.read_csv(medium_damper_path)
heavy_damper = pd.read_csv(heavy_damper_path)

# Plot kinetic energy
plt.figure(figsize=(8,5))
plt.plot(no_damper['Time_s'], no_damper['Kinetic_Energy_J'], label='No Damper')
plt.plot(medium_damper['Time_s'], medium_damper['Kinetic_Energy_J'], label='Medium Damper')
plt.plot(heavy_damper['Time_s'], heavy_damper['Kinetic_Energy_J'], label='Heavy Damper')
plt.xlabel('Time [s]')
plt.ylabel('Kinetic Energy [J]')
plt.title('Kinetic Energy Evolution: Damper Comparison')
plt.legend()
plt.grid(True)
plt.tight_layout()
plt.savefig(output_path)
plt.show()
