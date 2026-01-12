import re
from collections import defaultdict

def load_bibtex(filename):
    with open(filename, encoding="utf-8") as f:
        return f.read()

def extract_entries(bibtex):
    """Palauttaa listan sanakirjoja, joissa on kentät ja tyyppi."""
    entries = re.findall(r'@(\w+)\s*{\s*([^,]+),([\s\S]*?)\n}', bibtex)
    result = []
    for entry_type, key, body in entries:
        fields = dict(re.findall(r'(\w+)\s*=\s*[{"]([^"}]+)[}"]', body))
        result.append({"type": entry_type, "key": key, **fields})
    return result

def check_entry(entry):
    """Palauttaa listan huomautuksista yhdestä viitteestä."""
    warnings = []
    key = entry.get("key", "")
    t = entry.get("type", "")
    title = entry.get("title", "")
    doi = entry.get("doi", "")
    year = entry.get("year", "")
    journal = entry.get("journal", "")
    author = entry.get("author", "")
    number = entry.get("number", "")
    issue = entry.get("issue", "")
    publisher = entry.get("publisher", "")

    # --- puuttuvat pakolliset kentät ---
    required = ["author", "title", "year"]
    if t in {"article", "inproceedings"}:
        required += ["journal"]
    elif t in {"book", "incollection"}:
        required += ["publisher"]

    for r in required:
        if not entry.get(r):
            warnings.append(f"⚠️ Missing field '{r}'")

    # --- DOI-muodon tarkistus ---
    if doi and not doi.startswith("10."):
        warnings.append(f"⚠️ Suspicious DOI format: {doi}")
    if not doi and t == "article":
        warnings.append("⚠️ No DOI for an @article entry")

    # --- Year-muoto ---
    if year and not re.match(r"^\d{4}$", year.strip()):
        warnings.append(f"⚠️ Year not 4 digits: {year}")

    # --- Issue/number epäyhtenäisyys ---
    if issue and number:
        warnings.append("⚠️ Both 'issue' and 'number' present — choose one")

    # --- Erikoismerkit (LaTeX-ongelmat) ---
    if re.search(r"[äöåÄÖÅßéèëáàüñ]", title + author):
        warnings.append("⚠️ Non-ASCII characters detected (use LaTeX encoding like {\\\"o})")

    # --- Mahdollinen vanha DOI-virheellinen kenttä ---
    if doi and " " in doi:
        warnings.append("⚠️ DOI contains spaces")

    # --- Publisher puuttuu kirjasta ---
    if t == "book" and not publisher:
        warnings.append("⚠️ Book without publisher")

    return warnings

def main():
    filename = "references.bib"
    bibtex = load_bibtex(filename)
    entries = extract_entries(bibtex)

    print(f"🔍 Checking {len(entries)} entries in {filename}...\n")

    total_warnings = 0
    for i, e in enumerate(entries, 1):
        warnings = check_entry(e)
        if warnings:
            total_warnings += len(warnings)
            print(f"{i:03d}. {e['key']} ({e['type']}):")
            for w in warnings:
                print(f"   {w}")
            print()

    if total_warnings == 0:
        print("✅ No issues found — perfect BibTeX file!")
    else:
        print(f"⚠️ Total {total_warnings} warnings found across {len(entries)} entries.")

if __name__ == "__main__":
    main()
