# sweep.py
import os, json, yaml, itertools, subprocess, time
import numpy as np
import pandas as pd
from pathlib import Path

# --- 1) parametrit ---
N_PARTICLES = [0, 30, 60]
D_MM        = [6, 8, 10]               # mm
MU_SCALE    = [0.5, 1.0, 2.0]           # x 8.9e-4 Pa s
HFILL_SCALE = [0.75, 1.0, 1.25]         # x 19.7 mm

BASE = yaml.safe_load(open("base_config.yaml"))

# --- 2) apufunktiot ---
def case_id(n, dmm, mus, hfs):
    return f"np{n}_d{dmm}mm_mu{mus}x_h{hfs}x"

def make_config(n, dmm, mus, hfs):
    cfg = json.loads(json.dumps(BASE))  # deep copy
    cfg["dem"]["n_particles"] = int(n)
    cfg["dem"]["particle_size_m"] = dmm * 1e-3
    cfg["fluid"]["mu"] = mus * 8.9e-4
    cfg["tank"]["fill_height_m"] = hfs * 19.7e-3

    # skaalaa DEM-partikkelimassa ja tarvittaessa SPH-resoluutio
    # vinkki: pidä h/Δx samana -> Δx = h/1.3; älä muuta h, ellei pakko
    # jos muutat d, skaalaa paino ja inertiakertoimet vastaavasti

    return cfg

def run_sim(config_path, out_dir):
    out_dir.mkdir(parents=True, exist_ok=True)
    # Korvaa alla oleva komento omallasi (DualSPHysics, Chrono, tms.)
    # Esim:
    # cmd = ["DualSPHysics", "-in", str(config_path), "-out", str(out_dir)]
    cmd = ["python", "dummy_sim.py", str(config_path), str(out_dir)]  # placeholder
    subprocess.run(cmd, check=True)

def parse_results(out_dir):
    # Oleta CSV "time, velocity". Tee samat analyysit kuin liitteessäsi
    dat = np.loadtxt(out_dir / "velocity_uniform.txt", delimiter=",", skiprows=1)
    t, v = dat[:,0], dat[:,1]
    # --- huiput ja taajuus ---
    from scipy.signal import find_peaks
    peaks, _ = find_peaks(v, height=np.mean(v))
    A = v[peaks]
    T = np.diff(t[peaks]) if len(peaks) > 1 else np.array([np.nan])
    f = 1.0 / np.mean(T) if np.isfinite(T).all() else np.nan
    f_std = np.std(1.0 / T) if len(T) > 1 else np.nan
    # --- log. dekrementti δ ---
    deltas = np.log(A[:-1] / A[1:]) if len(A) > 1 else np.array([np.nan])
    delta = float(np.nanmean(deltas))
    delta_ci = 1.96 * np.nanstd(deltas, ddof=1) / np.sqrt(max(len(deltas),1))
    # --- energia ---
    m_eq = 1.0
    E = 0.5 * m_eq * v**2
    E_ratio = E / E[0] if E[0] != 0 else np.full_like(E, np.nan)
    # yksinkertainen “dissipaatio/jakso” arvio:
    Ediss_cycle = np.nan
    if len(peaks) > 2:
        E_peaks = E[peaks]
        Ediss_cycle = float(np.mean(E_peaks[:-1] - E_peaks[1:]))

    return dict(f=f, f_std=f_std, delta=delta, delta_ci=delta_ci,
                E_diss_cycle=Ediss_cycle, t_end=t[-1])

def main():
    configs_dir = Path("sweeps/configs")
    runs_dir    = Path("runs")
    results = []
    for n, dmm, mus, hfs in itertools.product(N_PARTICLES, D_MM, MU_SCALE, HFILL_SCALE):
        # tee halutessasi vain OAT: vaihda itertools.product -> listatavien yhdistelmien läpikäynti
        # tai lisää suodatin:
        if sum([dmm!=8, mus!=1.0, hfs!=1.0]) > 0 and n not in (0,60):
            # esimerkkinä: pidä “täysfaktoriaali” vain n=0 ja n=60, muuten OAT
            continue

        cid = case_id(n, dmm, mus, hfs)
        cfg = make_config(n, dmm, mus, hfs)
        cfg_path = configs_dir / f"{cid}.yaml"
        cfg_path.parent.mkdir(parents=True, exist_ok=True)
        yaml.safe_dump(cfg, open(cfg_path, "w"))

        out_dir = runs_dir / cid
        try:
            run_sim(cfg_path, out_dir)
        except subprocess.CalledProcessError as e:
            print("Simulation failed:", cid, e)
            continue

        metrics = parse_results(out_dir)
        row = dict(case=cid, n_p=n, d_mm=dmm, mu_scale=mus, hfill_scale=hfs, **metrics)
        results.append(row)
        print("Done:", cid, {k:round(v,3) for k,v in metrics.items() if isinstance(v,(int,float))})

    df = pd.DataFrame(results)
    df.to_csv("analysis/metrics.csv", index=False)
    print("Saved -> analysis/metrics.csv")

if __name__ == "__main__":
    main()
