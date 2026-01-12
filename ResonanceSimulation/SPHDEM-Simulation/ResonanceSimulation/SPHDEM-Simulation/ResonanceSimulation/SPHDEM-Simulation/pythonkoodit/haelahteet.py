import requests
import html
import re
from pybtex.database import parse_file

CROSSREF_API = "https://api.crossref.org/works/"

def normalize_text(s):
    """Poistaa LaTeX-koodit, dekoodaa HTML-entiteetit, normalisoi välimerkit ja muuttaa pieniksi."""
    if not s:
        return ""
    s = html.unescape(s)
    s = re.sub(r"\\&", "&", s)
    s = re.sub(r"[{}]", "", s)
    s = s.replace("–", "-").replace("—", "-")
    return " ".join(s.lower().split())

def get_year(data):
    """Hakee julkaisuvuoden: ensin print, sitten online, lopuksi issued."""
    def extract_year(field):
        return data.get(field, {}).get("date-parts", [[None]])[0][0]
    return extract_year("published-print") or extract_year("published-online") or extract_year("issued")

def find_doi_by_title(title):
    """Hakee CrossRefista DOI:n otsikon perusteella."""
    params = {"query.title": title, "rows": 1}
    r = requests.get(CROSSREF_API, params=params)
    if r.status_code == 200:
        items = r.json().get("message", {}).get("items", [])
        if items:
            return items[0].get("DOI")
    return None

def check_entry(entry):
    doi = entry.fields.get("doi")
    title = entry.fields.get("title")

    if doi:
        doi = doi.strip().replace("https://doi.org/", "")
        url = CROSSREF_API + doi
        r = requests.get(url)
        if r.status_code == 200:
            data = r.json().get("message", {})
        else:
            return [f"DOI {doi} ei löytynyt CrossRefista."]
    else:
        # Yritetään hakea DOI otsikolla
        found_doi = find_doi_by_title(title)
        if found_doi:
            doi = found_doi
            print(f"  - Huom! DOI puuttui, mutta löytyi otsikolla: {doi}")
            url = CROSSREF_API + doi
            r = requests.get(url)
            if r.status_code == 200:
                data = r.json().get("message", {})
            else:
                return [f"DOI {doi} löytyi otsikolla mutta CrossRef ei vastannut."]
        else:
            return ["DOI puuttuu – ei löytynyt otsikon perusteella."]

    errors = []

    # Otsikko
    cr_title_list = data.get("title", [])
    cr_title_raw = cr_title_list[0] if cr_title_list else ""
    cr_title = normalize_text(cr_title_raw)
    my_title = normalize_text(title)
    if my_title and my_title not in cr_title:
        errors.append(f"Otsikko ei täsmää: {title} ≠ {cr_title_raw}")

    # Vuosi
    cr_year = get_year(data)
    if "year" in entry.fields and cr_year != int(entry.fields["year"]):
        errors.append(f"Vuosi ei täsmää: {entry.fields['year']} ≠ {cr_year}")

    # Lehti
    container_list = data.get("container-title", [])
    cr_journal_raw = container_list[0] if container_list else ""
    cr_journal = normalize_text(cr_journal_raw)
    my_journal = normalize_text(entry.fields.get("journal", ""))
    if my_journal and my_journal not in cr_journal:
        errors.append(f"Lehti ei täsmää: {entry.fields.get('journal','')} ≠ {cr_journal_raw}")

    return errors or ["OK"]

def check_bib_file(filename):
    bib_data = parse_file(filename)
    for key, entry in bib_data.entries.items():
        print(f"\n== {key} ==")
        results = check_entry(entry)
        for res in results:
            print("  -", res)

# Käyttö
if __name__ == "__main__":
    check_bib_file("references.bib")
