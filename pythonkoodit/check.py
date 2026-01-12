import re
from pathlib import Path

def parse_bib_keys(bibfile):
    """Palauttaa kaikki BibTeX-avaimet tiedostosta."""
    if not Path(bibfile).exists():
        print(f"⚠️  Tiedostoa '{bibfile}' ei löydy.")
        return set()
    content = Path(bibfile).read_text(encoding='utf-8', errors='ignore')
    keys = set(re.findall(r'@\w+\{([^,]+),', content))
    return keys

def parse_cite_keys(texfile):
    """Palauttaa kaikki \cite-komentojen avaimet tiedostosta."""
    if not Path(texfile).exists():
        print(f"⚠️  Tiedostoa '{texfile}' ei löydy.")
        return set()
    content = Path(texfile).read_text(encoding='utf-8', errors='ignore')
    cites = re.findall(r'\\cite[tp]?\*?\{([^}]+)\}', content)
    keys = set()
    for group in cites:
        for key in group.split(','):
            keys.add(key.strip())
    return keys

def detect_bib_system(texfile):
    """Tunnistaa, käytetäänkö biblatex vai bibtex -järjestelmää."""
    text = Path(texfile).read_text(encoding='utf-8', errors='ignore')
    if 'biblatex' in text:
        return 'biblatex'
    elif '\\bibliography' in text:
        return 'bibtex'
    else:
        return 'unknown'

def main(texfile='gradu.tex', bibfile='references.bib'):
    print(f"🔍 Tarkistetaan {texfile} ja {bibfile}...\n")

    # 1. Etsi käytetty viitejärjestelmä
    system = detect_bib_system(texfile)
    if system == 'biblatex':
        print("📘 Käytät biblatexia → käytä käännöksiä:\n"
              "   pdflatex gradu.tex\n   biber gradu\n   pdflatex gradu.tex\n   pdflatex gradu.tex\n")
    elif system == 'bibtex':
        print("📗 Käytät bibtexiä → käytä käännöksiä:\n"
              "   pdflatex gradu.tex\n   bibtex gradu\n   pdflatex gradu.tex\n   pdflatex gradu.tex\n")
    else:
        print("⚠️  Viitejärjestelmää ei tunnistettu (ei löytynyt biblatex eikä bibliographia-komentoa).\n")

    # 2. Lue avaimet
    bib_keys = parse_bib_keys(bibfile)
    cite_keys = parse_cite_keys(texfile)

    print(f"📄 Löydetty {len(bib_keys)} viitettä BibTeX-tiedostosta ja {len(cite_keys)} cite-komentoa LaTeXissa.\n")

    # 3. Tarkista puuttuvat avaimet
    missing = cite_keys - bib_keys
    unused = bib_keys - cite_keys

    if missing:
        print("❌ Seuraavia viitteitä käytetään, mutta niitä ei löydy .bib-tiedostosta:")
        for key in sorted(missing):
            print("   -", key)
        print()
    else:
        print("✅ Kaikki cite-komennot löytyvät .bib-tiedostosta!\n")

    if unused:
        print("ℹ️  Nämä BibTeX-viitteet eivät ole käytössä LaTeXissa:")
        for key in sorted(unused):
            print("   -", key)
    else:
        print("✅ Kaikki BibTeX-viitteet ovat käytössä.\n")

    print("\n🔧 Tarkistus valmis.")

if __name__ == "__main__":
    main()
