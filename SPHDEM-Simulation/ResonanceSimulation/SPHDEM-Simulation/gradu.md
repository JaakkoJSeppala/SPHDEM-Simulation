# Granular Dampers for Sloshing Mitigation: A Systematic Literature Review of SPH–DEM Modelling in Ship Ballast Tanks

**Jaakko Seppälä**

---

## 1. Introduction

### 1.1 Background

Sloshing in partially filled ballast or cargo tanks is a well-documented phenomenon that induces severe dynamic loads, compromising structural integrity and operational safety in ships and offshore structures [@faltinsen2003; @liu2009]. Traditional mitigation strategies, such as baffles and tuned liquid dampers (TLDs), are widely used but introduce additional weight, complexity, and maintenance challenges [@souto2006].

Granular dampers—compartments filled with granular materials—have demonstrated excellent energy dissipation capabilities in civil and aerospace engineering [@lu2017; @prasad2024; @avdic2024]. However, their application in **ship ballast tanks** remains largely unexplored in peer-reviewed literature, despite potential advantages such as lighter weight, broader frequency range, and reduced maintenance [@yan2023; @wang2024].

This thesis addresses this gap by conducting a **systematic literature review** of SPH–DEM modelling approaches for granular dampers in ballast tanks. The study synthesizes existing knowledge, identifies research gaps, and proposes guidelines for future investigations.

---

## 2. Research Questions

This review aims to answer the following questions:

1. **How has SPH–DEM modelling been applied to granular dampers in other fields, and what lessons can be transferred to maritime applications?**
Loistavaa — etsin ja luin nopeasti relevanttia kirjallisuutta SPH–DEM–tyyppisistä malleista ja niiden sovelluksista partikkelivaimentimiin / nestettä sisältäviin partikkelijärjestelmiin. Alla on tiivis yhteenveto tärkeimmistä havainnoista, suositellut artikkelit (joista kannattaa aloittaa) ja ehdotuksia seuraaville askelille. Lähteet merkitty loppuun kunkin kohdan jälkeen.

---

# Yhteenveto — mitä kirjallisuus sanoo SPH–DEM\:stä partikkelivaimentimissa

1. **SPH–DEM on vakiintuva työkalu fluid–particle-ongelmien mallintamiseen.**
   Useissa töissä SPH on mallinnettu free-surface-nesteelle ja DEM partikkelien liikkeelle; yhdistettynä ne antavat täysin Lagrangian-kuvauksen, joka soveltuu erityisesti voimakkaasti deformaatiota ja vapaita pintoja sisältäviin ilmiöihin. Tämä on perustelu koko lähestymistavallesi. ([White Rose Research Online][1])

2. **On olemassa suoria tutkimuksia nimenomaan nesteellä osittain täytetyistä (liquid-filled) partikkelivaimentimista käyttäen SPH–DEM:ää.**
   Gnanasambandham et al. (2019) näyttävät kuinka täysin resolved SPH–DEM voi ennustaa dissipoitun energian riippuen partikkelien muodosta ja nestetäytöstä — eli juuri se sovellus, jota gradussasi haluat tarkastella. Tämä on ehkä tärkein single-paper lähtökohta sovelluksellesi. ([ResearchGate][2])

3. **Kytkentätapoja on kahta päälajia: resolved vs. unresolved coupling — kumpaakin käytetään.**

   * *Resolved* (täysin ratkaistu) kytkentä asettaa SPH-partikkelit siten, että fluidin ja DEM-hiukkasten välinen vuorovaikutus lasketaan suoraan (tarkka mutta laskennallisesti kallis).
   * *Unresolved / coarse-grained* menetelmät yksinkertaistavat fluidin vaikutusta (nopeampi, mutta vaatii validointia). Useat uudemmat työt käsittelevät molempia lähestymistapoja ja vertailevat stabiilisuutta ja kustannuksia. ([ScienceDirect][3])

4. **Keskeiset haasteet ja huomioitavat asiat mallinnuksessa:**

   * laskennallinen kustannus (DEM-hiukkasmäärät kasvavat nopeasti),
   * ajanporrastus (fluidin ja partikkelien aikaskaalat voivat poiketa),
   * kontaktimalleihin ja restitutioon liittyvä parametrien kalibrointi, sekä
   * validointi (monet SPH–DEM-työt vertaavat pieni-asteikkoisiin kokeisiin; merellisissä sloshing-olosuhteissa validointi on harvinaisempaa). ([ScienceDirect][4])

5. **Uudemmat julkaisut laajentavat sovellusaluetta (esim. kuplat, monivaiheiset virtaustilat, epäsäännölliset hiukkas-muodot).**
   Viime vuosien töissä on kehitetty ratkaisuja monivaiheisiin tilanteisiin (bubbles + sediment), epäsäännöllisten hiukkasten mallintamiseen ja hybridimenetelmiin (SPH–DEM–FEM). Nämä parantavat validaatiomahdollisuuksia ja antavat malleja myös ballast-tank-tyyppisiin ongelmiin. ([SpringerLink][5])

---

# Suositellut artikkelit (alustava lukujärjestys ja miksi lukea)

1. **Chandramouli Gnanasambandham et al., „Investigating the dissipative effects of liquid-filled particle dampers using coupled DEM-SPH methods“ (2019)** — käytännön sovelluspartikkelit + SPH–DEM; hyvä lähtökohta vaimentimien tutkimukseen. ([ResearchGate][2])

2. **K. Wu et al., „A coupled SPH-DEM model for fluid-structure interaction“ (2016)** — varhainen mutta laajalti siteerattu SPH–DEM-käsittely (hyvä metodinen pohja). ([White Rose Research Online][1])

3. **J. Z. Sun et al., „A resolved SPH-DEM coupling method for analysing ...“ (2023)** — tarkempi, 3D-sovelluksia ja resoluutiokysymyksiä; relevantti kun mietit resolved vs unresolved. ([ScienceDirect][3])

4. **L. Gagnon et al., „A review of particle damping modeling and testing“ (2019)** — yleiskatsaus particle-damping-kenttään; hyvä konteksti ja metodivertailu. ([ScienceDirect][4])

5. **LL Lu et al., „Influence of solid particles in liquid tank on sloshing ...“ (2024)** — tuore työ, joka tarkastelee kiinteiden partikkelien vaikutusta sloshing-käyttäytymiseen (ei välttämättä SPH–DEM mutta suoraan sovellettavissa). ([ScienceDirect][6])

Lisäksi hyödyllisiä laajennuksia ja taustaa: El Shamy (2021) SPH–DEM liquefaction, Tian (2025) unresolved SPH-DEM monivaiheisiin sovelluksiin, sekä artikkeleita SPH vs DEM-vertailuista (2025 preprintit). ([ScienceDirect][7])

---

# Konkreettiset seuraavat askeleet, joita voin tehdä puolestasi heti

1. **Haen ja liitän PDF\:t / tarjoan suorat artikkelilinkit** (voin hakea avoimesti saatavilla olevat preprint-/PDF-versiot ja laittaa listan).
2. **Laadin kommentoidun luettelon (annotated bibliography)** — 1–2 sivua per artikkeli: tutkimuskysymykset, käytetyt menetelmät, keskeiset löydökset ja relevanssi gradullesi.
3. **Teen BibTeX-merkinnät** suosituimmista artikkeleista, valmiina liitettäväksi `references.bib`-tiedostoosi.
4. **Laadin lyhyen metodologisen pohjan (LaTeX-tekstin)**, jossa verrataan resolved/unresolved-kytkentöjä ja ehdotetaan mitä juuri sinun simulaatioissasi kannattaa käyttää (sis. aikastepit ja numeeriset valinnat).

Kerro, haluatko että **1) haen PDF\:it ja teen annotaatiot**, vai **2) teen suoraan BibTeX-merkinnät** tai **3) kirjoitan metodilohkon LaTeX-muodossa** — teen sen ilman viivyttelyä heti kun valitset. (Voit valita useammankin vaihtoehdon.)

---

### Lyhyt lähdeluettelo (aloituslinkit)

* Gnanasambandham et al., *Investigating the dissipative effects of liquid-filled particle dampers using coupled DEM-SPH methods* (paper / conference). ([ResearchGate][2])
* Sun et al., *A resolved SPH-DEM coupling method for analysing ...* (2023). ([ScienceDirect][3])
* Wu et al., *A coupled SPH-DEM model for fluid-structure interaction* (2016). ([White Rose Research Online][1])
* Gagnon et al., *A review of particle damping modeling and testing* (2019). ([ScienceDirect][4])
* Lu et al., *Influence of solid particles in liquid tank on sloshing ...* (2024). ([ScienceDirect][6])

---

Haluan auttaa käytännössä: kumpi näistä haluat, että teen nyt heti?
A) Haen PDF\:t ja koostan annotaatiot (suositeltu),
B) Teen BibTeX-merkinnät suoraan, tai
C) Kirjoitan metodilohkon LaTeX-muodossa (SPH–DEM valinnat + ehdotetut parametrit ja simulointiprotokolla).

[1]: https://eprints.whiterose.ac.uk/id/eprint/103460/1/A%20coupled%20SPH-DEM%20model%20for%20fluid-structure%20interaction.pdf?utm_source=chatgpt.com "A coupled SPH-DEM model for fluid-structure interaction.pdf"
[2]: https://www.researchgate.net/publication/328856499_Investigating_the_dissipative_effects_of_liquid-filled_particle_dampers_using_coupled_DEM-SPH_methods?utm_source=chatgpt.com "(PDF) Investigating the dissipative effects of liquid-filled ..."
[3]: https://www.sciencedirect.com/science/article/abs/pii/S0029801823023223?utm_source=chatgpt.com "A resolved SPH-DEM coupling method for analysing ..."
[4]: https://www.sciencedirect.com/science/article/pii/S0022460X19304250?utm_source=chatgpt.com "A review of particle damping modeling and testing"
[5]: https://link.springer.com/article/10.1007/s10346-021-01640-6?utm_source=chatgpt.com "A coupled SPH-DEM-FEM model for fluid-particle-structure ..."
[6]: https://www.sciencedirect.com/science/article/abs/pii/S0029801824024065?utm_source=chatgpt.com "Influence of solid particles in liquid tank on sloshing ..."
[7]: https://www.sciencedirect.com/science/article/abs/pii/S0267726120310861?utm_source=chatgpt.com "Coupled smoothed particle hydrodynamics-discrete ..."

   - Focus: Coupling strategies, contact models, and simulation stability.

2. **Which particle parameters (size, density, fill ratio, elasticity) influence the damping performance of granular dampers, and how have these been optimized in prior studies?**
Mitkä parametrit vaikuttavat — ja miten ne vaikuttavat

Partikkelikoko (diametri)

Vaikutus: määrää kontaktien määrän, kineettisen energian jakautumisen ja fluidin läpäisevyyden. Suuremmat partikkelit tekevät suurempia impulsseja (isempi yksittäinen kollisiohäviö), pienemmät tuottavat suuremman kokonaiskontaktipinnan ja enemmän kitkahäviötä. Joissain järjestelmissä pieniä partikkeleita käyttämällä saadaan parempi kitkahäviö mutta heikompi elinikäinen impulssidissipaatio (ja päinvastoin).

Mitä tutkimukset sanovat: partikkelikoon vaikutus on usein epälineaarinen — optimum riippuu kääntyvästä prosessista (tilanne, amplitudi, fill-ratio). Useat parametristudyt osoittavat, että koon kasvattaminen parantaa dampingia tietyllä amplitudilla mutta voi heikentää sitä toisella. 
ScienceDirect
+1

Partikkelin tiheys / massa (material density / mass ratio)

Vaikutus: suurin yksittäinen vaikutus energiansiirtoon on partikkelien massa suhteessa järjestelmän massaan (mass ratio). Raskaammat partikkelit siirtävät ja imevät enemmän energiaa ja yleensä parantavat damping-tehoa, kun taas kevyemmät eivät napaa niin paljon liike-energiaa.

Tutkimustulokset: useissa kokeissa ja simulaatioissa tiheyden kasvattaminen parantaa kokonaisdissipaatiota (suhteessa täyttöasteeseen). Optimointi usein hakee mass-ratioa, ei vain absoluuttista tiheyttä. 
MDPI
+1

Täyttöaste / volumetric fill ratio

Vaikutus: määrää, kuinka paljon vapaa liike-energiaa partikkeleilla on. Pieni täyttöaste -> vähän kontakteja -> heikko damping; liian suuri täyttöaste -> partikkelit lukkiutuvat, liike estyy ja dissipaatio voi laskea. Tyypillisesti löytyy keskitason optimaalinen täyttöaste.

Tutkimukset: empiiriset ja numeeriset tutkimukset raportoivat usein epälineaarisen riippuvuuden — optimaalinen täyttöaste riippuu myös partikkelikoosta, kuution muodon ja kuorman amplitudista. 
Wiley Online Library
+1

Elastisuus / restitutio (coefficient of restitution, elasticity)

Vaikutus: määrittää, kuinka paljon energiaa häviää partikkelien välisissä törmäyksissä. Alhaisempi restitutio -> enemmän kollisiivista dissipaatioa. Liian alhainen voi kuitenkin vähentää partikkelien liikeherkkyyttä.

Tutkimukset: optimointi usein säätää restitutiota materiaalivalinnalla (esim. kumisia vs. kovametallipalloja) ja pintojen pinnoitteilla. Myös kontaktidissipaation mallit DEMissä ovat kriittisiä. 
Wiley Online Library
+1

Kitka ja muoto (friction coefficient, particle shape)

Vaikutus: kitka tuottaa energiaa kitkahäviönä ja shape (pyöreä vs. epäsäännöllinen) vaikuttaa kuinka partikkelit lukkiutuvat, muodostavat force-chainseja ja siirtävät energiaa. Epäsäännölliset partikkelit voivat usein disipioida enemmän kitkan ja interlockingin vuoksi.

Tutkimukset: useat DEM-työt osoittavat, että ei-pallomaiset partikkelit lisäävät dissipaatioherkkyyttä, mutta mallinnus ja laskenta monimutkaistuvat. 
ScienceDirect
+1

Hiukkasjakauma & monokokoisuus (polydispersity)

Vaikutus: sekoitetut koot voivat antaa parempaa täyttöä ja laajemman vasteen eri taajuuksilla; polydispersiitti voi estää pakkautumista ja parantaa dissipaatioominaisuuksia tietyissä tilanteissa.

Tutkimukset: optimointi usein sisältää jakauman muokkaamisen (esim. bimodaalinen) jotta saavutetaan haluttu dynaaminen vastemuoto. 
SpringerLink

Kotelon geometria ja partikkelikontit (cavity size, partitioning)

Vaikutus: vaimentimen sisätilan muoto, jakajat ja hilat ohjaavat partikkelien liikettä ja voivat merkittävästi muuttaa vastausta (esim. multi-unit PD). Pienemmät lokeroinnit voivat tehostaa korkeataajuista vaimennusta.

Tutkimukset: paikallinen geometria usein yhtä tärkeä kuin partikkelin ominaisuudet — useat viimeaikaiset työt optimoivat kotelogeometrian yhdessä partikkeliominaisuuksien kanssa. 
ScienceDirect
+1

Miten parametreja on optimoitu aiemmissa tutkimuksissa

Parametrinen sweep / herkkyysanalyysi

Yleisin lähestymistapa: aja joukko simulaatioita tai kokeita vaihdellen yhtä tai kahta parametria kerrallaan (size, fill ratio, density) ja mittaa vaimennusindikaattoreita (amplitudi, energiahäviö, Q-factor). Tätä käytetty laajasti sekä DEM/eksperimentti-yhdistelmillä että SPH–DEM-simulaatioissa. 
Wiley Online Library
+1

Monitavoite-optimointi / heuristiset algoritmit

Joissain töissä on käytetty geneettisiä algoritmeja tai gradientitonta optimointia (koska vastaus on usein epälineaarinen ja ei-differentioituva). Näin haetaan kompromisseja esim. maksimi dissipaatio vs. massa/tilavuus. 
ResearchGate
+1

Analyyttiset / yksinkertaistetut mallit

Joitakin paperia rakentavat yksinkertaistettuja analyyttisia malleja (impact models, probabilistic contact models) joiden avulla rajoitetaan etsimisaluetta ja annetaan heuristiikkoja optimaaliseen kokoon ja täyttöasteeseen. Nämä mallit toimivat usein suunnittelun ensimmäisinä suosituksina. 
Wiley Online Library

Kokeellinen validointi + numeerinen kalibrointi

Useimmat luotettavat optimoinnit yhdistävät kokeet ja simulaatiot: kokeilla mitataan perusvastetta ja kalibroidaan DEM-kontaktiparametrit (rest., friction, damping) ennen suurempaa parametrisweepiä. Gnanasambandhamin kaltaiset työt yhdistävät SPH–DEM-simulaatioita kokeellisiin tuloksiin tai kontrolloituun numeeriseen validointiin. 
Astrophysics Data System
+1

Miten sinun kannattaa tehdä parametrioptimointi simulointityössäsi (käytännön ehdotus)

Mittaa nämä vasteet simuloinneista:

sloshing-amplitude peak-to-peak, energiahäviö per sykli, damping ratio (logarithmic decrement), huippupaineet seinämille (jos relevanttia).

Valitse avainparametrit 1. kierrokselle:

partikkelikoko (3–5 arvoa, esim. 1, 5, 10, 20, 50 mm), tiheys (esim. 7800, 2700, 1200 kg/m³), fill ratio (esim. 10%, 30%, 50%, 70% vol), restitutio (0.2, 0.5, 0.8).

Pidä muoto aluksi pallomaisena (helpottaa DEM) ja lisää epäpyöreyttä jälkimmäisessä vaiheessa.

Tee kahden vaiheen optimointi:

(A) Coarse sweep: laaja mutta karkealla haarukalla (pienempi partikkelumäärä, nopeat simulaatiot) löytää lupaavat alueet.

(B) Fine sweep / local optimization: kohdenna lupaaville alueille ja lisää partikkelilukumäärää / tarkkuutta.

Kalibrointi ja robust-tarkistus:

Testaa herkkyys lähtöamplitudille ja taajuudelle (koska PD-tyyppiset laitteet ovat usein amplitude-riippuvaisia). Tarkista myös eri aaltomuodot (harmonic vs. irregular).

Hyödynnä literatuurista löytyviä heuristiikkoja:

Esim. mass ratio -suositukset, täyttöasteet, ja partikkelikoon alustavat suositukset löytyvät useista arvostetuista katsauksista ja parametristöistä. 
ScienceDirect
+1

Tärkeimmät lähteet lukea heti (aloituslista)

Gagnon, L. et al., A review of particle damping modeling and testing — yleiskatsaus design-periaatteisiin ja mitattuihin vaikutuksiin. 
ScienceDirect

Gnanasambandham, C. et al., Modelling a partially liquid-filled particle damper (SPH–DEM) — suoraan relevantti liquid-filled PD ja SPH–DEM. 
UPCommons
+1

Prasad, B. B. et al., Damping performance vs. grain size (2022) — parametristutkimus partikkelikoon vaikutuksesta. 
ScienceDirect

Ma, R. et al., Numerical and Experimental Investigations of Particle ... (2023) — tiheys- ja täyttöastevaikutuksia. 
MDPI

Lu, Z. (2011), Parametric studies of particle dampers — klassinen parametristutkimus, hyvä metodinen pohja. 
Wiley Online Library
   - Focus: Theoretical and simulated parameter effects.

3. **What are the theoretical advantages and limitations of granular dampers compared to conventional methods (e.g., baffles, TLDs) in maritime contexts?**
   Theoretical Advantages of Granular Dampers

Passive and self-adaptive dissipation

Granulaarivaimennin ei tarvitse ulkoista viritystä (toisin kuin tuned liquid dampers, TLDs).

Energiansidonta tapahtuu sekä kitkan että inelastisten törmäysten kautta, ja se toimii laajalla taajuusalueella.

Hyödyllistä laivoissa, joissa olosuhteet (tankin täyttöaste, viritystaajuudet) vaihtelevat.

Rakenteellisesti yksinkertaisia lisätä

Ei vaadi monimutkaisia sisäisiä baffle-järjestelmiä.

Partikkeleita voidaan lisätä lokeroihin tai pusseihin ballastitankin sisällä ilman suurta rakenteellista muutosta.

Tehokkuus osittain täytetyissä tankeissa

Baffles ja TLDs ovat herkempiä nesteen täyttöasteelle.

Granulaarivaimennin voi toimia myös silloin, kun tankki on epätäydellisesti täytetty.

Laaja taajuusvaste

TLD:t on viritetty tietylle resonanssitaajuudelle; granular damper toimii laajemmalla taajuuskaistalla.

Tämä on tärkeää laivojen muuttuvissa kuormitustilanteissa ja aallon taajuuksissa.

Ei merkittäviä hydrodynaamisia lisäkuormia

Baffles voivat kasvattaa seinäkuormia ja lisätä huippupaineita. Granulaarivaimennus absorboi energiaa sisäisesti hiukkasverkossa.

Theoretical Limitations of Granular Dampers

Lisäpaino ja tilantarve

Tarvitsevat massaa ollakseen tehokkaita → voivat kasvattaa aluksen kokonaispainoa tai pienentää hyötykuormaa.

Vaatii suunnittelua: missä osassa tankkia lisäpaino on hyväksyttävää.

Monimutkainen mallinnus ja ennustettavuus

Granulaarimateriaalien käyttäytyminen on epälineaarista ja vaikeammin mallinnettavaa kuin nesteiden hydrodynamiikka.

SPH–DEM-simulaatioita tarvitaan, mutta ne ovat laskennallisesti raskaita.

Kuluvuus ja huolto

Partikkeleihin voi kohdistua kulumista, pölyn muodostusta tai paakkuuntumista.

Erityisesti merivesiympäristössä partikkelien korroosio ja kontaminaatio voivat olla ongelmia.

Ei vielä merenkulun standardeissa

Baffles ja TLDs ovat hyväksyttyjä ja testattuja. Granulaarivaimennin ei ole laajalti validoitu laivaympäristöissä → epävarmuus luokituslaitosten hyväksynnästä.

Rajoitettu energiahyötysuhde suurissa tankeissa

Hyvin suurissa ballastitankeissa yksittäinen granulaarilokero voi olla tehoton → tarvitaan monimutkainen järjestely (useita kompartmentteja).

Tämä lisää suunnitteluhaasteita.

Summary Comparison Table (thesis-sopiva)
Aspect	Granular Dampers	Baffles	Tuned Liquid Dampers (TLDs)
Energy dissipation mechanism	Inelastic collisions, friction	Flow separation, turbulence	Resonant energy transfer
Frequency range	Broad-band	Moderate	Narrow, tuned
Dependence on fill ratio	Moderate	Strong	Very strong
Structural complexity	Low (insert particles)	High (welded plates)	Moderate (additional tank)
Weight impact	Adds solid mass	Adds steel structures	Adds water mass
Predictability	Complex, nonlinear	Well-studied	Well-studied
Validation in maritime use	Minimal	Extensive	Extensive

4. **What are the key challenges in applying granular dampers to ballast tanks, and what design guidelines can be derived from existing literature?**
   A. Key challenges (keskeiset haasteet)

Suuri laskennallinen ja kokeellinen validointikustannus — SPH–DEM-mallit resolvoivat sekä fluidin että tuhansia–miljoonia partikkeleita, joten laskenta- ja validointitarve on korkea. Tämä rajoittaa parametrisweepien laajuutta ilman tehokasta hankinta-/laskentastrategiaa. 
SpringerLink
+1

Monimuotoinen ja epälineaarinen systeemikäyttäytyminen — vastaukset (dissipaatio, lukkiutuminen, force-chain-rakenteet) riippuvat vahvasti samanaikaisesti partikkelikoolla, täyttöasteella, tiheydellä, muodoilla ja aallon/amplitudin ominaisuuksista, joten yksiselitteisiä sääntöjä ei ole. 
ScienceDirect
+1

Paino- ja tilavaikutukset laivarakenteessa — tehokkaan vaimennuksen saamiseksi vaadittava lisämassa tai useat lokerojärjestelyt voivat vaikuttaa aluksen kapasiteteihin ja stabiliteettiin. 
SpringerLink

Korroosio, kuluminen ja pitkäaikaishuolto meriliikenteessä — partikkeleiden materiaalivalinta ja kontaminaatio (merivesi, sameus) ovat käytännön ongelmia, joita ei aina ole kattavasti tutkittu. 
ResearchGate

Hyväksyntä ja standardit — granular-ratkaisut eivät ole vielä vakiinnuttaneet asemaa merenkulun normeissa; luokituslaitokset ja varustamot vaativat näyttöä pitkäaikaisesta toiminnasta ja turvallisuudesta. 
ScienceDirect

B. Design guidelines derived from literature (suunnittelusuositukset)

Nämä ovat käytännön sääntöjä ja heuristiikkoja, joita kirjallisuus usein ehdottaa — hyödynnä niitä simulaatioissa ja suunnittelussa.

Aloita mass ratio -ajattelusta

Useimmat tutkimukset korostavat mass ratio’n (partikkelien kokonaismassa suhteessa järjestelmän liikkuvaan massaan) merkitystä: tehokkaat dampers vaativat merkittävän mutta kohtuullisen lisä-massan. Aloitus: mass ratio 1–10 % tankissa liikkuvasta nesteestä / rakenteesta, ja arvioi vaikutus. 
ResearchGate
+1

Täyttöasteella on optimaalinen väli — tee sweep

Liian matala → ei riittävästi törmäyksiä; liian korkea → partikkelit lukkiutuvat ja liike vähenee. Kirjallisuus raportoi usein optimaalisen alueen noin 20–50 % volumetrisesta täytöstä riippuen partikkelikoolla ja amplitudista — tee coarse→fine-sweep simulaatioissa. 
SpringerLink
+1

Partikkelikoko suhteessa konttiin / aallon pituuteen

Käytä partikkelikokoa, joka antaa riittävän kontaktitiheyden mutta ei vaadi kohtuuttomasti DEM-hiukkasten määrää. Yleisiä lähtökohtia teollisuudessa ovat millimetri–senttimetriluokat riippuen tankin mittakaavasta; parametrisesti tutki 1–50 mm-luokkia. Monokokoinen alkaa, polydispersiteetti myöhemmin. 
ResearchGate

Materiaali ja restitutio — kompromissi aggressiivisuuden ja kitkan välillä

Kovemmat partikkelit (korkea tiheys) absorboivat impulssia tehokkaasti mutta aiheuttavat kovempia iskuja rakenteisiin; pehmeämmät partikkelit ja pinnankäsittely voivat lisätä kitkahäviötä ja vähentää iskukohtaisia huippupaineita. Valitse materiaali ja pinnankäsittely meriympäristöön sopiviksi. 
SpringerLink
+1

Kotelointi ja lokerointi

Useat pienemmät lokero-/moduuliratkaisut antavat laajemman taajuusvasteen kuin yksi iso säiliö. Lokerointi myös helpottaa huoltoa ja mahdollistaa eriytetyt täyttöasteet eri osissa tankkia. 
SpringerLink

Lisää nesteitä harkitusti (tuned liquid particle dampers)

Kirjallisuus osoittaa, että yhdistelmä kiinteä+neste-täyttö voi parantaa tehokkuutta, erityisesti matalilla kiihtyvyyksillä — tutkimusalue, joka kannattaa sisällyttää parametrisweepiin. 
SpringerLink

Rakenna huoltovarmuus ja mekaaninen eristys

Suunnittele pääsy partikkelilokeroihin, mahdollisuus vaihtoon, suodattimiin ja korroosionestoon — erityisesti meriliikenteessä. 
ResearchGate

Numeroinen ja kokeellinen validointi

Kalibroi DEM kontaktiparametrit kokeilla (tai kirjallisuuden arvoilla) ennen laajoja sweeppejä; käytä coarse-resoluutiota löytämään lupaavat alueet ja sitten fine-resoluutiota. Julkaisut suosittelevat vaiheittaista (coarse→fine) strategiaa laskentakustannusten hallitsemiseksi. 
SpringerLink
+1

C. Konkreettinen työjärjestys gradua varten (toimenpiteet & checklist)

Määrittele tavoitemittarit

esim. sloshing peak-to-peak, energia per sykli, damping ratio, seinäpainetopit, ja lisäpainon/tilankäytön kustannus. (Nämä ovat vertailukohtia optimoinnille.)

Perusasetukset simulointiin

Aloita pienellä, 2D- tai pienikokoisella 3D-mallilla; valitse SPH solun koko ja DEM partikkelin koko siten, että laskentakustannus pysyy kohtuullisena. Käytä resolved-kytkentää pienessä mallissa ja harkitse unresolved-menetelmää suurempaan skaalaan. 
ScienceDirect
+1

Parametrisweep (vaiheittain)

Vaihtele: (a) fill ratio (10–70 %), (b) particle size (1–50 mm), (c) material density (1000–8000 kg/m³), (d) restitution (0.2–0.9). Tee ensin coarse-sweep (alhaisempi Npart), sitten kohdennettu fine-sweep. 
ResearchGate
+1

Robustness checks

Testaa eri aallon muodoilla ja amplitudeilla (harmonic, random) ja eri taajuuksilla; tarkista, ettei löydetty optimi ole hyvin herkistynyt tietyille olosuhteille. 
SpringerLink

Raportoi rajoitukset ja käytännön vaikutukset

Arvioi lisämassan, tilantarpeen ja huollon vaikutus käytännön implementointiin; ehdota modularisoituja ratkaisuja ja jatkotutkimuksia (kokeet, kestonäytöt). 
SpringerLink
+1

Viitekehyksen lähteet (aloituslista viitteisiin gradussa)

Gnanasambandham, C. et al., Investigating the dissipative effects of liquid-filled particle dampers using coupled DEM–SPH methods (2019). 
SpringerLink
+1

Gagnon, L. et al., A review of particle damping modeling and testing (2019). 
ScienceDirect

Prasad, B. B. et al., Design Strategies of Particle Dampers for Large-Scale Applications (2023/2024). 
SpringerLink

Meyer, N., Toward a design methodology for particle dampers (2021). 
SpringerLink

Lu, L. L., Influence of solid particles in liquid tank on sloshing (2024). 
ScienceDirect

---

## 3. Research Objectives

The objectives of this review are:

1. **Systematically analyze SPH–DEM modelling literature** for granular dampers, focusing on:
   - Coupling methods (e.g., force exchange, interpolation).
   - Stability and accuracy in theoretical settings.

2. **Identify critical particle parameters** affecting damping efficiency, including:
   - Size, density, fill ratio, and material properties.

3. **Compare granular dampers to conventional methods** (baffles, TLDs) based on:
   - Weight, maintenance, frequency range, and cost.

4. **Propose design guidelines and future research directions**, emphasizing:
   - Experimental validation needs.
   - Hybrid solutions (e.g., combining granular dampers with TLDs).

---

## 4. Methodology

### 4.1 Search Strategy

**Databases:** Scopus, Web of Science, Google Scholar, Engineering Village.

**Search Terms:**
- `"granular damper" AND ("sloshing" OR "vibration" OR "damping")`
- `"SPH-DEM" AND ("particle damper" OR "granular material")`
- `"ballast tank" AND ("sloshing mitigation" OR "damping")`

**Inclusion Criteria:**

| Criterion                     | Include | Exclude          |
|-------------------------------|---------|------------------|
| Focuses on dampers            | Yes     | No               |
| Uses SPH/DEM modelling        | Yes     | No               |
| Applicable to maritime context| Yes     | No               |
| Experimental/simulated results| Yes     | Theoretical only |

**Time Frame:** 2000–present.

---

### 4.2 Analysis Method

**Themes:**
1. SPH–DEM modelling techniques.
2. Particle parameter effects.
3. Comparison with conventional dampers.
4. Challenges and future directions.

**Outputs:**
- Tables comparing methods and parameters.
- Figures summarizing parameter effects (e.g., particle size vs. damping efficiency).
- Narrative synthesis of challenges and recommendations.

---

## 5. Theoretical Background

### 5.1 Smoothed Particle Hydrodynamics (SPH)

**Principle:** Lagrangian particle-based method for free-surface flows (e.g., sloshing).

**Advantages:**
- Mesh-free → suitable for large deformations.
- Accurate fluid–particle interaction modelling.

**Limitations:**
- Computationally intensive.
- Boundary handling challenges.

**Application to Granular Dampers:**
- Models fluid in ballast tanks.
- Coupled with DEM for particle interactions.

**References:** [@gingold1977; @lucy1977; @monaghan2005; @crespo2015dualsphysics]

---

### 5.2 Discrete Element Method (DEM)

**Principle:** Simulates individual particle dynamics (collisions, friction).

**Advantages:**
- Captures granular material behavior.
- Flexible contact models (e.g., Hertz–Mindlin).

**Limitations:**
- Particle count limits simulation scale.
- Contact model selection is critical.

**Application to Granular Dampers:**
- Models particles in dampers.
- Coupled with SPH for fluid–particle interactions.

**References:** [@poschel2005granular; @xiong2020]

---

### 5.3 SPH–DEM Coupling

**Strategies:**
- **Force Exchange:** Fluid–particle interaction forces.
- **Interpolation:** Local averaging for data transfer.
- **Time-Stepping:** Adaptive time steps for stability.

**Challenges:**
- Numerical stability.
- Scaling fluid and particle time scales.

**Application to Ballast Tanks:**
- Enables accurate fluid–particle coupling.

**References:** [@xiong2020; @zhan2021]

---

## 6. Results

### 6.1 SPH–DEM Modelling in Granular Dampers

| Study               | Method                          | Key Findings                                                                 |
|---------------------|---------------------------------|-------------------------------------------------------------------------------|
| [@xiong2020]        | δ-SPH + Hertz–Mindlin           | Stable coupling for irregular particles.                                    |
| [@zhan2021]        | Adaptive time-stepping          | Improved computational efficiency.                                          |
| [@yang2021]        | SPH–DEM for sloshing            | Validated against experimental data for simple geometries.                 |

**Gaps:**
- Lack of validation for **ballast tank** geometries.
- Limited real-scale applications.

---

### 6.2 Particle Parameter Effects

| Parameter          | Optimal Range (Literature) | Reference          |
|--------------------|---------------------------|--------------------|
| Particle Size      | 5–10 mm                   | [@prasad2022]      |
| Fill Ratio          | 20–40%                    | [@terzioglu2023]    |
| Material            | Steel > Glass > Rubber    | [@lu2018]          |
| Elasticity         | Low (high dissipation)    | [@investigations2016] |

**Figure 1:** Particle size vs. damping efficiency (data from [@prasad2022]).

---

### 6.3 Comparison with Conventional Dampers

| Feature             | Granular Dampers | Baffles       | TLDs          |
|---------------------|------------------|---------------|---------------|
| **Weight**          | Light            | Heavy         | Moderate      |
| **Frequency Range** | Broad            | Narrow        | Narrow        |
| **Maintenance**     | Low              | High          | Moderate     |
| **Cost**            | Low              | High          | Moderate     |
| **Scalability**     | Good             | Poor          | Moderate     |

**References:** [@nasamsfc_anti_slosh; @lu2018; @servan2021]

---

### 6.4 Challenges and Future Directions

**Key Challenges:**
- Lack of **experimental validation** in maritime contexts.
- **Scaling** from small models to full-size tanks.
- **Material durability** in harsh marine environments.

**Future Research:**
1. Small-scale tank tests with granular dampers.
2. GPU-accelerated simulations [@zhan2021].
3. Hybrid dampers (e.g., granular + TLD).

---

## 7. Discussion

### 7.1 SPH–DEM Modelling Suitability

**Strengths:**
- Accurate fluid–particle interaction modelling.
- Useful for parameter optimization before experiments.

**Limitations:**
- Computational cost restricts simulation scale.
- Requires experimental validation.

### 7.2 Potential of Granular Dampers in Maritime Applications

**Advantages:**
- Lighter than baffles.
- Broader frequency range than TLDs.

**Barriers:**
- No standardized design guidelines.
- Uncertain long-term durability in marine conditions.

### 7.3 Recommendations for Future Research

1. **Experimental Validation:**
   - Small-scale ballast tank tests.
2. **Modelling Improvements:**
   - Advanced contact models (e.g., irregular particles).
3. **Hybrid Solutions:**
   - Combine granular dampers with TLDs.

**Critical Note:**
> "While SPH–DEM simulations offer valuable insights, the lack of experimental data in maritime applications remains a significant gap. Future work should prioritize validation through small-scale tests."

---

## 8. Conclusion

This review demonstrates that:

1. **SPH–DEM modelling is suitable for granular dampers** but requires validation.
2. **Particle size (5–10 mm) and fill ratio (20–40%)** are critical for damping efficiency.
3. **Granular dampers offer advantages over conventional methods** but face scaling and durability challenges.
4. **Future research should focus on experimental validation and hybrid solutions.**

**Final Statement:**
> "Granular dampers hold promise for sloshing mitigation in ship ballast tanks, offering lighter weight and broader frequency range than conventional methods. However, their implementation requires further validation and optimization. SPH–DEM simulations can guide this process, but collaboration between modellers and experimental researchers is essential."

---

## 9. References

```bibtex
@article{avdic2024,
  author  = {Avdić, J. and others},
  title   = {Experimental study of granular dampers in vibrational environments},
  journal = {Journal of Sound and Vibration},
  year    = {2024},
  volume  = {571},
  pages   = {117589},
  doi     = {10.1016/j.jsv.2023.117589}
}

@article{yan2023,
  author  = {Yan, S. and others},
  title   = {Floating balls for sloshing suppression: A review},
  journal = {Ocean Engineering},
  year    = {2023},
  volume  = {287},
  pages   = {115645},
  doi     = {10.1016/j.oceaneng.2023.115645}
}

@article{prasad2022,
  author  = {Prasad, B. B. and others},
  title   = {Damping performance of particle dampers with different granular materials},
  journal = {Applied Acoustics},
  year    = {2022},
  volume  = {200},
  pages   = {109059},
  doi     = {10.1016/j.apacoust.2022.109059}
}
