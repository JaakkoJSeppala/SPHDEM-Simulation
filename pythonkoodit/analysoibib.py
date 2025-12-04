import re
import sys

def read_file(path):
    """Lue tiedoston sisältö"""
    try:
        with open(path, "r", encoding="utf-8") as f:
            return f.read()
    except FileNotFoundError:
        print(f"Virhe: Tiedostoa '{path}' ei löytynyt!")
        sys.exit(1)

def find_cites(tex_content):
    """Etsi kaikki \cite-komennot LaTeX-tiedostosta"""
    cites = re.findall(r'\\cite\{(.*?)\}', tex_content)
    # Tukee useita viittauksia samassa komennossa \cite{a,b}
    all_cites = []
    for c in cites:
        all_cites.extend([v.strip() for v in c.split(",")])
    return all_cites

def find_bib_keys(bib_content):
    """Etsi kaikki BibTeX-avaimet"""
    return re.findall(r'@\w+\{(.*?),', bib_content)

def main():
    tex_file = "gradu.tex"  # Vaihda omaksi tiedostoksesi
    bib_file = "references.bib"     # Vaihda omaksi tiedostoksesi

    tex_content = read_file(tex_file)
    bib_content = read_file(bib_file)

    cites = find_cites(tex_content)
    bib_keys = find_bib_keys(bib_content)

    print("Löydetyt \cite-viittaukset:", cites)
    print("Löydetyt BibTeX-avaimet:", bib_keys)
    print("\n--- Tarkistus ---")

    # Viitteet, joita ei löydy BibTeXistä
    missing = [c for c in cites if c not in bib_keys]
    if missing:
        print("Varoitus! Seuraavia viitteitä ei löydy BibTeXistä:")
        for m in missing:
            print("  -", m)
    else:
        print("Kaikki viitteet löytyvät BibTeXistä ✅")

    # Mahdolliset ylimääräiset avaimet BibTeXissä, joita ei käytetä
    unused = [k for k in bib_keys if k not in cites]
    if unused:
        print("\nHuomio: Seuraavia BibTeX-avaimia ei käytetä dokumentissa:")
        for u in unused:
            print("  -", u)
    else:
        print("\nKaikki BibTeX-avaimet käytössä ✅")

if __name__ == "__main__":
    main()
