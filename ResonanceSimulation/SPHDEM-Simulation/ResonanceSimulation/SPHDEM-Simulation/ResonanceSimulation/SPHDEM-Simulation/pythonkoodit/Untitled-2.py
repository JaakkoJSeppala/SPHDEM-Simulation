import pandas as pd
import re, json

# Lue raakadata
df = pd.read_csv("table1_parameters.csv", header=None)
lines = [str(x).strip() for x in df.iloc[:,0].tolist() + df.iloc[:,1].fillna("").tolist()]
lines = [l for l in lines if l and l.lower() != "nan"]

# 1️⃣ Yhdistä katkenneet rivit ("solid d" + "ensity ...")
merged = []
skip = False
for i in range(len(lines)):
    if skip:
        skip = False
        continue
    if i < len(lines) - 1 and lines[i].endswith("d") and lines[i+1].startswith("ensity"):
        merged.append(lines[i] + lines[i+1])
        skip = True
    else:
        merged.append(lines[i])

# 2️⃣ Puhdista roskat
cleaned = [l for l in merged if not re.search(r"figur|page|table|value|parameter", l, re.I)]

# 3️⃣ Erottele tekstirivit ja numerorivit
text_lines = [l for l in cleaned if not re.search(r"\d", l)]
num_lines = [l for l in cleaned if re.search(r"\d", l)]

print("🧩 Text lines:", text_lines)
print("🔢 Numeric lines:", num_lines)

# 4️⃣ Apufunktio viskositeettimuodon korjaamiseen
def parse_number(s):
    s = s.replace("×", "e").replace("−", "-").replace("–", "-").replace(" ", "")
    s = re.sub(r"[^\deE\.\-]", "", s)
    # korjaa yleinen PDF-muoto 8.9e10-4 -> 8.9e-4
    s = re.sub(r"e10?-", "e-", s)
    try:
        return float(s)
    except Exception:
        print(f"⚠️ Could not parse: {s}")
        return None

params = {}
try:
    params["n_particles"] = int(re.sub(r"[^\d]", "", num_lines[0]))
    params["rho_p"] = 1200.0
    params["rho_liq"] = 1000.0
    params["nu"] = parse_number(num_lines[5])  # 8.9×10−4
    params["CFL"] = parse_number(num_lines[6])
    params["h"] = parse_number(num_lines[7])   # 7.5×10−4
except Exception as e:
    print("⚠️ Parsing error:", e)

# 5️⃣ Tallenna JSON ja tulosta
with open("table1_parameters_clean.json", "w") as f:
    json.dump(params, f, indent=2)

print("\n✅ Cleaned parameter dictionary:")
for k, v in params.items():
    print(f"  {k:12s} = {v}")
