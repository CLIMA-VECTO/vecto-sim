Shift Polygons Input File (.vgbs)
=================================

Defines up- and down-shift curves. See [Gear Shift Model](#gear-shift-model) for details.

![](pics/Shift.svg)

File Format
-----------

The file uses the [VECTO CSV format](#csv-format).

Format:
: -   Three columns
-   One header line
-   At least two lines with numeric values (below file header)
  
***Columns:***

| **Engine Torque [Nm]** | **Downshift rpm [1/min]** | **Upshift rpm [1/min]** |
| ---------------------- | ------------------------- | ----------------------- |
| ...                    | ...                       | ...                     |
| ...                    | ...                       | ...                     |
