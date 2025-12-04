import re
from pathlib import Path

def parse_tex_citekeys(tex_file):
    """Hae kaikki \cite{...} -viittaukset .tex-tiedostosta."""
    with open(tex_file, 'r', encoding='utf-8') as f:
        content = f.read()
    # Etsi kaikki \cite{...} ja \cite*{...} -merkinnät
    citekeys = re.findall(r'\\(?:cite(?:[^]{]*){([^}]*)}|cite\*[^]{]*{([^}]*)})', content)
    # Flatten tuple-list (re.findall palauttaa tuplen)
    citekeys = [key for pair in citekeys for key in pair if key]
    # Poista tyhjät ja pilko useat avaimet
    citekeys = [key.strip() for cite in citekeys for key in cite.split(',')]
    return set(citekeys)

def parse_bib_keys(bib_file):
    """Hae kaikki @...{-avain,...} -avaimet .bib-tiedostosta."""
    with open(bib_file, 'r', encoding='utf-8') as f:
        content = f.read()
    # Etsi kaikki @...{-avain,...} -merkinnät
    bib_keys = re.findall(r'@\w+\{([^,]+)', content)
    return set(bib_keys)

def find_missing_keys(tex_file, bib_file):
    """Etsi puuttuvat avaimet."""
    tex_keys = parse_tex_citekeys(tex_file)
    bib_keys = parse_bib_keys(bib_file)
    missing = tex_keys - bib_keys
    unused = bib_keys - tex_keys
    return missing, unused

if __name__ == "__main__":
    import sys
    if len(sys.argv) != 3:
        print("Käyttö: python puuttuvatviitteet.py <gradu.tex> <references.bib>")
        sys.exit(1)

    tex_file = sys.argv[1]
    bib_file = sys.argv[2]

    missing, unused = find_missing_keys(tex_file, bib_file)

    if missing:
        print("Puuttuvat avaimet .bib-tiedostosta:")
        for key in sorted(missing):
            print(f"  - {key}")
    else:
        print("Kaikki viittaukset löytyvät .bib-tiedostosta!")

    if unused:
        print("\nKäyttämättömät avaimet .bib-tiedostossa:")
        for key in sorted(unused):
            print(f"  - {key}")
    else:
        print("\nKaikki .bib-tiedoston avaimet ovat käytössä!")
