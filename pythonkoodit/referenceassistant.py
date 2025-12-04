import requests
import csv

def hae_crossref(aihe, max_results=5):
    """Hakee artikkeleita Crossrefistä"""
    url = "https://api.crossref.org/works"
    params = {"query": aihe, "rows": max_results}
    response = requests.get(url, params=params)
    data = response.json()

    tulokset = []
    for item in data["message"]["items"]:
        otsikko = item.get("title", ["Ei otsikkoa"])[0]
        tekijat = ", ".join([a.get("family", "") for a in item.get("author", [])]) if "author" in item else "Ei tietoa"
        vuosi = item.get("created", {}).get("date-parts", [[None]])[0][0]
        doi = item.get("DOI", "")
        tulokset.append({
            "Lähde": "Crossref",
            "Otsikko": otsikko,
            "Tekijät": tekijat,
            "Vuosi": vuosi,
            "DOI": f"https://doi.org/{doi}"
        })
    return tulokset


def hae_semanticscholar(aihe, max_results=5):
    """Hakee artikkeleita Semantic Scholarista"""
    url = f"https://api.semanticscholar.org/graph/v1/paper/search"
    params = {
        "query": aihe,
        "limit": max_results,
        "fields": "title,authors,year,url,citationCount"
    }
    response = requests.get(url, params=params)
    data = response.json()

    tulokset = []
    for item in data.get("data", []):
        otsikko = item.get("title", "Ei otsikkoa")
        tekijat = ", ".join([a.get("name", "") for a in item.get("authors", [])])
        vuosi = item.get("year", "Ei tietoa")
        url = item.get("url", "")
        viittaukset = item.get("citationCount", 0)
        tulokset.append({
            "Lähde": "Semantic Scholar",
            "Otsikko": otsikko,
            "Tekijät": tekijat,
            "Vuosi": vuosi,
            "DOI": url,
            "Viittaukset": viittaukset
        })
    return tulokset


def tallenna_csv(tulokset, tiedostonimi="artikkelit.csv"):
    """Tallentaa tulokset CSV-tiedostoon"""
    if not tulokset:
        print("Ei tallennettavaa.")
        return

    # kerätään kaikki mahdolliset kentät
    kaikki_kentat = set()
    for rivi in tulokset:
        kaikki_kentat.update(rivi.keys())
    kentat = list(kaikki_kentat)

    with open(tiedostonimi, "w", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(f, fieldnames=kentat)
        writer.writeheader()
        for rivi in tulokset:
            # täytetään puuttuvat kentät tyhjällä
            writer.writerow({kentta: rivi.get(kentta, "") for kentta in kentat})

    print(f"Tulokset tallennettu tiedostoon {tiedostonimi}")


# -------------------------------
# Käyttöesimerkki
# -------------------------------
if __name__ == "__main__":
    aihe = input("Anna hakutermi: ")

    crossref_tulokset = hae_crossref(aihe, 5)
    semanticscholar_tulokset = hae_semanticscholar(aihe, 5)

    kaikki = crossref_tulokset + semanticscholar_tulokset

    for art in kaikki:
        print(f"{art['Lähde']}: {art['Otsikko']} ({art['Vuosi']})")
        print(f"  Tekijät: {art['Tekijät']}")
        print(f"  Linkki: {art['DOI']}")
        if 'Viittaukset' in art:
            print(f"  Viittaukset: {art['Viittaukset']}")
        print()

    # Tallennus CSV:ksi
    tallenna_csv(kaikki)
