import re
import json
import requests
from pathlib import Path

CROSSREF_URL = "https://api.crossref.org/works"

def search_crossref(title: str):
    """Hakee Crossrefistä BibTeX-tietueen otsikon perusteella."""
    params = {"query.title": title, "rows": 1}
    try:
        response = requests.get(CROSSREF_URL, params=params, timeout=10)
        response.raise_for_status()
        data = response.json()
        items = data.get("message", {}).get("items", [])
        if not items:
            return None

        item = items[0]
        doi = item.get("DOI", "")
        year = ""
        if "published-print" in item:
            year = item["published-print"]["date-parts"][0][0]
        elif "issued" in item:
            year = item["issued"]["date-parts"][0][0]

        # Rakennetaan BibTeX
        entry = f"""@article{{{item['DOI'].replace('/', '_')},
  title = {{{item['title'][0]}}},
  author = {{{' and '.join(a['family'] + ', ' + a.get('given', '') for a in item.get('author', []))}}},
  journal = {{{item.get('container-title', [''])[0]}}},
  year = {{{year}}},
  doi = {{{doi}}},
  url = {{{item.get('URL', '')}}}
}}"""
        return entry
    except Exception as e:
        print(f"⚠️ Crossref-haku epäonnistui ({title}): {e}")
        return None


def find_missing_citations(log_path: str):
    """Etsii lokista puuttuvat viitteet."""
    log_text = Path(log_path).read_text(encoding="utf-8", errors="ignore")
    return re.findall(r"I didn't find a database entry for '([^']+)'", log_text)


def complete_missing_references(log_path: str, bib_path: str, output_path: str):
    """Täydentää puuttuvat viitteet Crossrefin avulla."""
    missing = find_missing_citations(log_path)
    if not missing:
        print("✅ Ei puuttuvia viitteitä.")
        return

    print(f"🔍 Etsitään {len(missing)} puuttuvaa viitettä Crossrefistä...")

    completed = []
    for key in missing:
        print(f"  - Haetaan '{key}'...")
        entry = search_crossref(key)
        if entry:
            completed.append(entry)
        else:
            print(f"  ⚠️ Ei löytynyt: {key}")

    if not completed:
        print("❌ Yhtään uutta viitettä ei lisätty.")
        return

    # Tallennetaan uusi tiedosto
    bib_file = Path(bib_path)
    bib_text = bib_file.read_text(encoding="utf-8", errors="ignore")
    bib_text += "\n\n" + "\n\n".join(completed)
    Path(output_path).write_text(bib_text, encoding="utf-8")

    print(f"✅ {len(completed)} uutta viitettä lisätty tiedostoon {output_path}")


# Käyttö:
# complete_missing_references("gradu.log", "references.bib", "references_autofix.bib")
