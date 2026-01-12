# Tietojenkäsittelytieteen (tietotekniikan) LaTeX-tutkielmapohja (Jyväskylän yliopisto)

[![fi](https://img.shields.io/badge/lang-fi-green.svg)](README.md)
[![en](https://img.shields.io/badge/lang-en-green.svg)](README.en.md)

Tämä LaTeX-mallipohja soveltuu käytettäväksi tietojenkäsittelytieteen (aiemmin tietotekniikka) pro gradu- ja kandidaatintutkielmissa. 

Lue ensin `malliopas.tex`. Se auttaa sinua oppimaan LaTeXin ja tämän mallipohjan käyttöä. Jos et aio kehittää LaTeX-luokkaa eteenpäin, käytä yllä olevaa latausnapikkaa, älä turhaan kloonaa projektia.

Wiki tuossa vasemmalla tulee myöhemmin sisältämään hyödyllistä tietoa latexin ja tutkielmapohjan käytöstä.

# Tarvittavat ohjelmistot 

* Joku LaTeX-toteutus, esim: 
  - `lualatex`
  - `xelatex`
  - `pdflatex` (vanhentumassa, kokeile ensin lualatexia)
* `biber`  kirjallisuusviitteiden hallintaan 
* Joitain LaTeX-paketteja 

Nämä löytyvät yleensä LaTeX-järjestelmästä kuten \
[TeX Live](https://www.tug.org/texlive/).

# Dokumentin tuottaminen  

Dokumentin voi tex-lähdetiedostosta tuottaa ja esikatsella automaattisesti komennolla

```
latexmk -pvc somefile
```

Komennolla [`man latexmk`](https://manpages.org/latexmk) voit lukea lisää ohjeita `latexmk`-ohjelman käytöstä ja konfiguroinnista.

# Dokumentin tuottaminen käsin 

```
$ latex somefile.tex
$ biber somefile # if somefile.bib is your bibliography
$ latex somefile.tex
$ latex somefile.tex
```

# Saavutettavuus 

Sisällön saavutettavuuden varmistaminen [on osa Suomen lakia](https://www.finlex.fi/fi/laki/alkup/2019/20190306).
Yksi tunnetuimmista sähköisen tulostuksen arkistoista, arXiv, [suosittelee LaTeX-dokumenttien tekemistä viemällä ne HTML-muotoon](https://info.arxiv.org/about/accessible_HTML.html).
JYU Open Science Center ei kuitenkaan ole päättänyt tukea tätä lähestymistapaa.
[Ohjeidensa mukaan](https://openscience.jyu.fi/en/thesis-tutorial/bachelors-masters-thesis/publishing-your-thesis/thesis-accessibility) he ovat päättäneet tukea tagged PDF:ää, joka on vasta viime aikoina saanut laajaa tukea PDF-tiedostoissa.
LaTeXin tuki tagged PDF:lle on vielä kokeellinen.
Jos sinulla on kuitenkin käytössäsi tarpeeksi uusi LaTeX-versio, (TeXLive 2024+) voit kääntää sen ja lisätä nämä ominaisuudet dokumenttiisi noudattamalla `accessibility.texissä` annettuja ohjeita.

# SPH-DEM Simulation Project

This project provides a modular Python implementation for simulating coupled Smoothed Particle Hydrodynamics (SPH) and Discrete Element Method (DEM) systems. It is designed to answer the following research questions:

1. How does granular material affect free-surface motion and impact pressure in a tank?
2. Which parameters (particle size, fill ratio, damping coefficient) most strongly influence energy dissipation efficiency?
3. How well does the coupled SPH-DEM approach reproduce benchmark results from prior studies?

## Structure
- `sph/` — SPH fluid simulation module
- `dem/` — DEM particle simulation module
- `sim/` — Coupled SPH-DEM simulation runner
- `analysis/` — Parameter sweep automation and result analysis
- `visualization/` — Plotting and reporting tools

## Getting Started
1. Install Python 3.10+ and recommended packages (see requirements.txt)
2. Run example simulations from the `sim/` directory
3. Analyze results using scripts in `analysis/` and `visualization/`

## License
MIT