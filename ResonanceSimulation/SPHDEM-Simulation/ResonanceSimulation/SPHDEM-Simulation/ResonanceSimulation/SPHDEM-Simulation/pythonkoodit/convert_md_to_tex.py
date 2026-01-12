import pypandoc
import re
import os

def extract_bibtex(md_content):
    """Erottele BibTeX-viitteet Markdown-tiedostosta."""
    bibtex_pattern = r'```bibtex(.*?)```'
    bibtex_matches = re.findall(bibtex_pattern, md_content, flags=re.DOTALL)

    if not bibtex_matches:
        print("Debug: BibTeX-viitteitä ei löytynyt. Käytetään oletusarvoisia viitteitä.")
        bibtex_content = """@article{avdic2024,
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
}"""
        md_content_without_bibtex = md_content
    else:
        bibtex_content = bibtex_matches[0].strip()
        md_content_without_bibtex = re.sub(bibtex_pattern, '', md_content, flags=re.DOTALL)

    return md_content_without_bibtex, bibtex_content

def convert_md_to_tex(md_content, output_tex_file, bib_file):
    """Muunna Markdown LaTeXiksi pandocin avulla."""
    tex_content = pypandoc.convert_text(
        md_content,
        'latex',
        format='markdown',
        extra_args=[
            '--citeproc',
            f'--bibliography={bib_file}'
        ]
    )

    # Lisää LaTeX-pohjaan muunnettu sisältö
    with open('gradupohja.tex', 'r', encoding='utf-8') as template_file:
        template_content = template_file.read()

    full_tex_content = template_content + '\n' + tex_content + '\n\\printbibliography\n\\end{document}'

    with open(output_tex_file, 'w', encoding='utf-8') as f:
        f.write(full_tex_content)

def save_bibtex(bibtex_content, output_bib_file):
    """Tallenna BibTeX-tiedosto."""
    with open(output_bib_file, 'w', encoding='utf-8') as f:
        f.write(bibtex_content)

def main(input_md_file, output_tex_file, output_bib_file):
    """Pääfunktio, joka suorittaa muunnoksen."""
    with open(input_md_file, 'r', encoding='utf-8') as f:
        md_content = f.read()

    md_content_without_bibtex, bibtex_content = extract_bibtex(md_content)

    save_bibtex(bibtex_content, output_bib_file)

    convert_md_to_tex(md_content_without_bibtex, output_tex_file, output_bib_file)

if __name__ == "__main__":
    input_md_file = "gradu.md"          # Syötetiedosto (Markdown)
    output_tex_file = "gradu.tex"        # Tulostiedosto (LaTeX)
    output_bib_file = "references.bib"  # Tulostiedosto (BibTeX)

    main(input_md_file, output_tex_file, output_bib_file)
