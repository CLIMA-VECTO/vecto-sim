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

-   The main input files ([Job](../GUI/VECTO-Editor.html), [Vehicle](../GUI/VEH-Editor.html), [Engine](../GUI/ENG-Editor.html) and [Gearbox](../GUI/GBX-Editor.html)) use the [JSON format](http://en.wikipedia.org/wiki/JSON) ![](../pics/misc/external-icon%2012x12.png).
-   The [Auxiliary Input File (.vaux)](VAUX.html) uses a modified format.
-   The [Driving Cycle (.vdri)](VDRI.html) uses keywords to identify columns and therefore the header line must follow certain specifications.