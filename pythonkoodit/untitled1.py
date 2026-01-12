import bibtexparser
import requests
import unicodedata
import re

# Lataa BibTeX-tiedosto
with open("references.bib", encoding="utf-8") as bibfile:
    bib_database = bibtexparser.load(bibfile)

def clean_text(s):
    """Poistaa LaTeX-koodit ja normalisoi unicode-merkit."""
    if not s:
        return ""
    s = re.sub(r"[{}]", "", s)
    s = re.sub(r"\\[a-zA-Z]+\{?\\?.\}?", "", s)
    s = unicodedata.normalize('NFKD', s)
    s = s.encode('ascii', 'ignore').decode('ascii')
    return " ".join(s.lower().split())

def normalize_name(name):
    """Palauttaa sukunimen pienillä kirjaimilla."""
    name = clean_text(name)
    if "," in name:
        name = name.split(",")[0]
    else:
        name = name.split()[-1]
    return name

def extract_year(data):
    """Hakee CrossRefin datasta julkaisuvuoden."""
    for field in ["published-print", "published-online", "issued"]:
        year = data.get(field, {}).get("date-parts", [[None]])[0][0]
        if year:
            return str(year)
    return None

def check_crossref(entry):
    """Vertaa BibTeX-merkintää CrossRefin virallisiin tietoihin."""
    doi = entry.get("doi")
    if not doi:
        return f"{entry.get('ID')} - DOI puuttuu"

    url = f"https://api.crossref.org/works/{doi}"
    try:
        resp = requests.get(url, timeout=10)
        if resp.status_code != 200:
            return f"{entry.get('ID')} - DOI ei löydy CrossRefista"

        data = resp.json()["message"]
        issues = []

        # Otsikko
        my_title = clean_text(entry.get("title", ""))
        cr_title = clean_text(data.get("title", [""])[0])
        if my_title and my_title not in cr_title:
            issues.append("Otsikko ei täsmää")

        # Kirjoittajat
        entry_authors_raw = entry.get("author", "")
        entry_authors = [normalize_name(a) for a in re.split(r"\band\b|,", entry_authors_raw) if a.strip()]
        cr_authors = [normalize_name(a.get("family", "")) for a in data.get("author", [])]

        if entry_authors and cr_authors:
            # Verrataan vain ensimmäistä kirjoittajaa, mikäli useita
            if entry_authors[0] != cr_authors[0]:
                issues.append(f"Ensimmäinen kirjoittaja ei täsmää ({entry_authors[0]} ≠ {cr_authors[0]})")

        # Vuosi
        cr_year = extract_year(data)
        if entry.get("year") and cr_year and entry["year"] != cr_year:
            issues.append(f"Vuosi ei täsmää ({entry['year']} ≠ {cr_year})")

        if issues:
            return f"{entry.get('ID')} - " + ", ".join(issues)
        return None

    except Exception as e:
        return f"{entry.get('ID')} - Virhe: {e}"

# Käy läpi kaikki merkinnät ja tulosta vain ongelmalliset
for entry in bib_database.entries:
    result = check_crossref(entry)
    if result:
        print(result)
