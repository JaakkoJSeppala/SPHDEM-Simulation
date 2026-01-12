import re
from pathlib import Path

def analyze_latex_log(log_path: str):
    log_file = Path(log_path)
    if not log_file.exists():
        print(f"❌ Lokitiedostoa ei löytynyt: {log_path}")
        return

    text = log_file.read_text(encoding="utf-8", errors="ignore")
    errors = []
    warnings = []

    # Havaitse virheet
    for line in text.splitlines():
        if "! LaTeX Error:" in line or "ERROR" in line:
            errors.append(line.strip())
        elif "Warning" in line or "WARN" in line:
            warnings.append(line.strip())

    # Analysoi tunnettuja virheitä
    explanations = []
    for e in errors:
        if "Missing \\begin{document}" in e:
            explanations.append("⚠️ Puuttuu \\begin{document}. Tarkista, ettei ylimääräistä tekstiä tai merkkejä ole ennen dokumentin alkua.")
        elif "Misplaced alignment tab character &" in e:
            explanations.append("⚠️ Taulukossa tai BibTeXissä on & ilman \\&-merkintää. Käytä \\& kustantajien nimissä kuten 'Taylor \\& Francis'.")
        elif "Undefined control sequence" in e:
            explanations.append("⚠️ Käytetty komento ei ole tunnettu. Voi puuttua paketti tai olla kirjoitusvirhe komennossa.")
        else:
            explanations.append(f"⚠️ Tuntematon virhe: {e}")

    for w in warnings:
        if "Overfull \\hbox" in w:
            explanations.append("ℹ️ Overfull hbox: rivi menee marginaalin yli. Ei estä käännöstä, mutta voit käyttää \\sloppy tai rivinvaihtoa.")
        elif "legacy month field" in w:
            explanations.append("ℹ️ BibTeX-varoitus: käytä `month = 7` tai poista `month`-kenttä. `month = {Jul}` aiheuttaa varoituksen.")
        elif "I didn't find a database entry" in w:
            match = re.search(r"'([^']+)'", w)
            key = match.group(1) if match else "tuntematon"
            explanations.append(f"⚠️ Viitettä '{key}' ei löydy. Lisää se references.bib-tiedostoon.")
        else:
            explanations.append(f"ℹ️ Muu varoitus: {w}")

    # Tulosta tulokset
    print("📘 LaTeX-virheanalyysi")
    print("=" * 40)
    if not errors and not warnings:
        print("✅ Ei virheitä tai varoituksia — käännös kunnossa!")
        return
    for e in explanations:
        print(e)


# Käyttöesimerkki:
# analyze_latex_log("gradu.log")
