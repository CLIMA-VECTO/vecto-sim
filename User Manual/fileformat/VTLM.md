Transmission Loss Map (.vtlm)
=============================


This file defines losses in gearbox and axle transmission and must be provided for each gear in the [Gearbox File](#gearbox-editor).

File Format
-----------

The file uses the [VECTO CSV format](#csv-format).

Format:
:	-   Three columns
-   One header line
-   At least four lines with numeric values (below file header)

***Columns:***

| **Input Speed \[1/min\]** | **Input Torque \[Nm\]** | **Torque Loss** |
| ------------------------- | ----------------------- | --------------- |
| ...                       | ...                     | ...             |
| ...                       | ...                     | ...             |
| ...                       | ...                     | ...             |
| ...                       | ...                     | ...             |




Input Speed & Torque always means **engine-side**.

Sign of torque values:
: -   Input Torque &gt; 0 for normal driving operation
-   Input Torque &lt; 0 for motoring operation. ***The Torque Loss Map must include negative torque values for engine motoring operation!***
-   Torque Loss is always positive!

Calculation of Output Torque:
:	VECTO calculates the output torque using this formula, independent from the current operation mode (driving/braking).

$Output\ Torque = (Input\ Torque-Torque\ Loss) \times GearRatio$
