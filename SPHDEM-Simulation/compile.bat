@echo off
setlocal
set filename=gradu
copy gradu.tex gradu.txt
echo === Käännetään LaTeX + Biber ===
pdflatex %filename%.tex
biber %filename%
pdflatex %filename%.tex
pdflatex %filename%.tex
echo === Valmis! Tarkista %filename%.pdf ===
endlocal
