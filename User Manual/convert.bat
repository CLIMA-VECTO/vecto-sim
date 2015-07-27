REM for /R "." %%f in (*.md) do pandoc -s -f markdown -t latex "%%f" -o "%%~dpf%%~nf.pdf"
REM for /R "." %%f in (*.md) do pandoc -s -f markdown -t docbook "%%f" -o "%%~dpf%%~nf.db"
for /R "." %%f in (*.md) do pandoc -f markdown --mathjax -t html -H header.txt "%%f" -o "%%~dpf%%~nf.html"
REM for /R "." %%f in (*.md) do pandoc -s -f markdown -t docx "%%f" -o "%%~dpf%%~nf.docx"
REM for /R "." %%f in (*.md) do pandoc -s -f markdown -t asciidoc "%%f" -o "%%~dpf%%~nf.adoc"
REM for /R "." %%f in (*.md) do pandoc -s -f markdown -t latex "%%f" -o "%%~dpf%%~nf.tex"
REM for /R "." %%f in (*.md) do pandoc -s -f markdown -t mediawiki "%%f" -o "%%~dpf%%~nf.mw"