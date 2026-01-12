@echo off
setlocal enabledelayedexpansion

:: Tiedostonimi parametrista tai oletus
if "%1"=="" (
    set "filename=gradu"
) else (
    set "filename=%1"
)

echo ===========================================
echo Käännetään LaTeX: %filename%.tex
set /a run=1

:latexloop
echo --- Käännös %run% ---
pdflatex -interaction=nonstopmode "%filename%.tex"
if errorlevel 1 (
    echo Virhe pdflatex-vaiheessa!
    pause
    exit /b 1
)

:: Tarkista onko Biber tarpeen
if exist "%filename%.bcf" (
    echo --- Ajetaan Biber ---
    biber "%filename%"
    if errorlevel 1 (
        echo Virhe Biber-vaiheessa!
        pause
        exit /b 1
    )
)

:: Tarkista logista, tarvitaanko uusi ajo
set "rerun=0"
findstr /C:"Rerun to get cross-references right" "%filename%.log" >nul && set "rerun=1"
findstr /C:"LaTeX Warning: There were undefined references" "%filename%.log" >nul && set "rerun=1"
findstr /C:"LaTeX Warning: Citation" "%filename%.log" >nul && set "rerun=1"

if %rerun%==1 (
    if !run! lss 5 (
        set /a run+=1
        echo --- Uusi ajo tarvitaan, jatketaan ---
        goto latexloop
    ) else (
        echo Liian monta ajoa, lopetetaan.
    )
)

echo ===========================================
echo Valmis! Tarkista "%filename%.pdf"

:: Siivoa väliaikaiset tiedostot
del /q "%filename%.aux" "%filename%.log" "%filename%.bbl" "%filename%.blg" "%filename%.toc" "%filename%.out" 2>nul

pause
endlocal
