# Computer Science master's/bachelor's thesis template (University of Jyväskylä)

[![fi](https://img.shields.io/badge/lang-fi-green.svg)](README.md)
[![en](https://img.shields.io/badge/lang-en-green.svg)](README.en.md)

This LaTeX template is designed for Computer Science theses (in Finnish, tietojenkäsittelytiede; formerly known as tietotekniikka). It is suitable for both bachelor's and master's theses.

Start off by reading `enthesis.tex`, which includes various information on using this template, LaTeX, and writing a thesis in general.

If you just want to use this template in your thesis, use the download button. If you want to develop the template, use clone. 

Wiki on the left will later contain useful information.

# Software you'll need

1. A LaTeX driver program such as
  - `lualatex`
  - `xelatex`
  - `pdflatex`
2. `biber` processing backend
3. Various LaTeX packages

[TeX Live](https://www.tug.org/texlive/) 
or a similar TeX distribution usually includes these.

# Build automatically

```
latexmk -pvc somefile
```

is used to watch the your tex file and continually compile it with preview.

See [`man latexmk`](https://manpages.org/latexmk) for how to configure the driver program and PDF viewer.

# Build manually

```
$ latex somefile.tex
$ biber somefile # if somefile.bib is your bibliography
$ latex somefile.tex
$ latex somefile.tex
```

# Accessibility

Making content accessible [is part of the law in Finland](https://www.finlex.fi/fi/laki/alkup/2019/20190306).
One of the most prominent e-print repositories, arXiv, [recommends making LaTeX documents by exporting to HTML](https://info.arxiv.org/about/accessible_HTML.html).
However, the JYU Open Science Center has chosen not to support this approach.
According [to their guidance](https://openscience.jyu.fi/en/thesis-tutorial/bachelors-masters-thesis/publishing-your-thesis/thesis-accessibility), they have chosen to support tagged PDF, a which has only more recently received widespread support in PDFs.
LaTeX support for tagged PDF is still experimental.
However, if you have access to a new enough version of LaTeX (TeXLive 2024+) you can compile and follow the instructions in `accessibility.tex` to add these features to your document.
