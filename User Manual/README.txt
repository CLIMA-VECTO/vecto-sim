README
======
In this readme the generation of the user documentation for VECTO is described.

Requirements
------------
* pandoc: http://pandoc.org/

Structure
---------
* README.txt: this readme file.
* convert.bat: the script to convert the markdown files into the required formats.
* files.txt: the list of files which should be used (also defines the order).
* fileformat: contains the .md files regarding the input/output files
* general: contains the .md files regarding general concepts
* GUI: contains the .md files regarding the graphical user interface
* pics: contains all pictures
* output: directory for the generated files.

The documents are written in Markdown (pandoc markdown, Extension: .md). To generate the final documentation execute "convert.bat". The resulting files are stored in the "output"-directory. The included markdown files and their order is defined in the file "files.txt".

HTML
----
* katex: javascript framework for formulas
* style.css: stylesheet


PDF
---
