import re
from collections import defaultdict

def parse_bib_file(filename):
    """Palauttaa listan viitteistä (dict), joissa on avain, title, year ja doi."""
    entries = []
    with open(filename, "r", encoding="utf-8") as f:
        content = f.read()

    # Erotellaan BibTeX-entryt
    raw_entries = re.split(r'@(?=\w+{)', content)
    for raw in raw_entries:
        if not raw.strip():
            continue
        # Etsitään avain, title, year ja doi
        key_match = re.search(r'{\s*([^,]+),', raw)
        title_match = re.search(r'title\s*=\s*{([^}]+)}', raw, re.IGNORECASE)
        doi_match = re.search(r'doi\s*=\s*{([^}]+)}', raw, re.IGNORECASE)
        year_match = re.search(r'year\s*=\s*{([^}]+)}', raw, re.IGNORECASE)

        entry = {
            "key": key_match.group(1).strip() if key_match else None,
            "title": title_match.group(1).strip().lower() if title_match else None,
            "doi": doi_match.group(1).strip().lower() if doi_match else None,
            "year": year_match.group(1).strip() if year_match else None,
        }
        entries.append(entry)
    return entries


def find_duplicates(entries):
    """Tulostaa duplikaatit DOI:n ja otsikon perusteella (mutta huomioi vuodet ja DOI:t)."""
    by_doi = defaultdict(list)
    by_title = defaultdict(list)

    for e in entries:
        if e["doi"]:
            by_doi[e["doi"]].append(e["key"])
        if e["title"]:
            by_title[e["title"]].append(e["key"])

    duplicates = []

    # --- DOI-perusteiset duplikaatit ---
    print("\n🔍 DOI-perusteiset duplikaatit:")
    for doi, keys in by_doi.items():
        if len(keys) > 1:
            print(f"  DOI {doi} löytyy {len(keys)} kertaa:", ", ".join(keys))
            duplicates.extend(keys)

    # --- Otsikkoperusteiset duplikaatit ---
    print("\n📚 Otsikkoperusteiset duplikaatit (vain jos DOI ja vuosi samat):")
    for title, keys in by_title.items():
        if len(keys) > 1:
            group = [e for e in entries if e["key"] in keys]
            # tarkistetaan onko sama DOI tai sama vuosi
            unique_pairs = {(e["doi"], e["year"]) for e in group}
            if len(unique_pairs) == 1:  # vain jos molemmat samat
                print(f"  '{title[:80]}...' esiintyy {len(keys)} kertaa:", ", ".join(keys))
                duplicates.extend(keys)

    if not duplicates:
        print("\n✅ Ei duplikaatteja löytynyt!")
    else:
        print(f"\nYhteensä {len(set(duplicates))} duplikaattiavainta löytyi.")

    return duplicates


if __name__ == "__main__":
    filename = "references.bib"  # vaihda tiedoston nimi tarvittaessa
    entries = parse_bib_file(filename)
    find_duplicates(entries)
