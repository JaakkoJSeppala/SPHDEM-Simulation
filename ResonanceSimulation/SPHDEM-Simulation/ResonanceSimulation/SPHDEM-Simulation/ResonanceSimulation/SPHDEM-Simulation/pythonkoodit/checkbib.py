import re
import requests
import time

CROSSREF_URL = "https://api.crossref.org/works/"
REQUEST_DELAY = 1.0  # seconds between requests to respect rate limits

def load_bibtex(filename):
    with open(filename, encoding="utf-8") as f:
        return f.read()

def extract_entries(bibtex):
    """Palauttaa listan BibTeX-viitteitä dict-muodossa."""
    entries = re.findall(r'@(\w+)\s*{\s*([^,]+),([\s\S]*?)\n}', bibtex)
    result = []
    for entry_type, key, body in entries:
        fields = dict(re.findall(r'(\w+)\s*=\s*[{"]([^"}]+)[}"]', body))
        result.append({
            "type": entry_type.lower(),
            "key": key.strip(),
            **{k.lower(): v.strip() for k, v in fields.items()}
        })
    return result

def check_doi(entry):
    doi = entry.get("doi", "").lower().strip()
    title = entry.get("title", "").lower().strip()
    if not doi:
        return "❌ No DOI", None, None, None

    url = CROSSREF_URL + doi
    try:
        r = requests.get(url, timeout=10)
        if r.status_code != 200:
            return f"❌ DOI not found ({doi})", None, None, None

        data = r.json()["message"]
        crossref_title = data["title"][0].lower()
        journal = data.get("container-title", ["?"])[0]
        year = data.get("issued", {}).get("date-parts", [[None]])[0][0]

        if title in crossref_title or crossref_title in title:
            return "✅ DOI and title match", crossref_title, journal, year
        else:
            return "⚠️ Title mismatch", crossref_title, journal, year

    except Exception as e:
        return f"❌ Error: {e}", None, None, None

def main():
    filename = "references.bib"
    bibtex = load_bibtex(filename)
    entries = extract_entries(bibtex)
    print(f"🔍 Checking {len(entries)} entries in {filename}...\n")

    for i, entry in enumerate(entries, 1):
        key = entry["key"]
        result, crossref_title, journal, year = check_doi(entry)

        print(f"{i:03d}. {result} — {key}")
        if crossref_title and "⚠️" in result:
            print(f"    Your title:   {entry.get('title', '')}")
            print(f"    CrossRef:     {crossref_title}")
        if journal or year:
            print(f"    Journal: {journal}, Year: {year}")
        time.sleep(REQUEST_DELAY)

if __name__ == "__main__":
    main()
