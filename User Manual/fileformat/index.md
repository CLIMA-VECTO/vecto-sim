CSV Format
==========

The following format applies to all CSV (Comma-separated values) Input Files used in VECTO:

|                     |                                                                                                 |    
| --------------------|-------------------------------------------------------------------------------------------------|
| **List Separator:** | Comma ","                                                                                       |
| **Decimal-Mark:**   | Dot "."                                                                                         |
| **Comments:**       | "\#" at the beginning of the comment line. Number and position of comment lines is not limited. |
| **Header:**         | One header line (***not*** **a comment line**) at the beginning of the file.                    |




Exceptions
----------

-   The main input files ([Job](#job-editor), [Vehicle](#vehicle-editor), [Engine](#engine-editor) and [Gearbox](#gearbox-editor)) use the [JSON format](http://en.wikipedia.org/wiki/JSON) ![](../pics/misc/external-icon%2012x12.png).
-   The [Auxiliary Input File (.vaux)](#auxiliary-input-file-.vaux) uses a modified format.
-   The [Driving Cycle (.vdri)](#driving-cycle-.vdri) uses keywords to identify columns and therefore the header line must follow certain specifications.
