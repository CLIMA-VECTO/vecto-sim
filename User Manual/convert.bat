@echo off

xcopy pics output\html\pics /Y

setlocal enabledelayedexpansion enableextensions
set LIST=
for /f %%f in (files.txt) do set LIST=!LIST! "%%f"

pandoc %LIST% -s -S --toc --toc-depth=2 --katex=katex/katex.min.js --katex-stylesheet=katex/katex.min.css -c style.css -o output\html\help.html

REM pandoc -s -S --toc --toc-depth=2 -N %LIST% -o help.docx
REM pandoc -s -S --toc --toc-depth=2 -N %LIST% -o help.pdf
REM pandoc -s -S --toc --toc-depth=2 -N %LIST% -o help.latex
