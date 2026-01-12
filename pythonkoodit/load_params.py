import json
import os

def load_params(filename=r"C:\Users\jaakk\Desktop\Gradu\table1_parameters_clean.json"):
    """Load SPH–DEM parameters from cleaned JSON."""
    if not os.path.exists(filename):
        raise FileNotFoundError(f"❌ File not found: {filename}")
    with open(filename, "r") as f:
        params = json.load(f)
    print("✅ SPH–DEM parameters loaded:")
    for k, v in params.items():
        print(f"  {k:12s} = {v}")
    return params
