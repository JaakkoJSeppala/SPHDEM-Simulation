# Tarkista, onko g++ jo asennettu
$compiler = Get-Command g++.exe -ErrorAction SilentlyContinue

if ($compiler) {
    Write-Host "✅ g++ on jo asennettu: $($compiler.Source)" -ForegroundColor Green
} else {
    Write-Host "🔧 Asennetaan MinGW-w64 (GCC)..." -ForegroundColor Cyan
    
    # Asenna MinGW-w64 Wingetillä
    winget install --id=GCC.Mingw-w64 -e --source=winget

    # Etsi MinGW:n asennuspolku (yleisimmät sijainnit)
    $possiblePaths = @(
        "C:\Program Files\mingw-w64",
        "C:\ProgramData\mingw-w64",
        "$env:ProgramFiles\mingw-w64"
    )

    $mingwPath = $null
    foreach ($path in $possiblePaths) {
        if (Test-Path $path) {
            $mingwPath = Get-ChildItem $path -Directory -Recurse -ErrorAction SilentlyContinue |
                         Where-Object { Test-Path "$($_.FullName)\bin\g++.exe" } |
                         Select-Object -First 1 -ExpandProperty FullName
            if ($mingwPath) { break }
        }
    }

    if ($mingwPath) {
        $binPath = "$mingwPath\bin"
        Write-Host "📁 Löydetty MinGW-polku: $binPath" -ForegroundColor Yellow

        # Lisää PATH-muuttujaan, jos ei vielä ole
        $cur
