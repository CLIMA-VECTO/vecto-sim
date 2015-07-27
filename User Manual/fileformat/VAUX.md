Auxiliary Input File (.vaux)
============================

This file is used to configure a single auxiliary. Multiple .vaux files can be defined in the [Job File](../GUI/VECTO-Editor.html) via the [Auxiliary Dialog](../GUI/VECTO-Editor_Aux.html).

See [Auxiliaries](../general/Auxiliaries.html) for details on how the power demand for each auxiliary is calculated.

File Format
-----------

The file uses the VECTO CSV format with three additional parameters on top of the efficiency map.

Format:
: -   Lines 1,3,5 and 7 are reserved for headers. Theses lines are skipped during file read.
-   Line 2: [TransRatio](../general/Auxiliaries.html) = Speed ratio between auxiliary and engine. \[-\]
-   Line 4: [EffToEng](../general/Auxiliaries.html) = Efficiency of auxiliary (belt/gear) drive \[-\]
-   Line 6: [EffToSply](../general/Auxiliaries.html) = Consumer efficiency \[-\]
-   Line 8 and following (at least four): [EffMap](../general/Auxiliaries.html) = Auxiliary efficiency map.

***Format:***

|                               |                                  |                                  |
| ----------------------------- | -------------------------------- | -------------------------------- |
| **TransRatio \[-\]**          | **Max. acceleration \[m/s^2^\]** | **Max. deceleration \[m/s^2^\]** |
| ...                           |                                  |                                  |
| **EffToEng \[-\]**            |                                  |                                  |
| ...                           |                                  |                                  |
| **EffToSply \[-\]**           |                                  |                                  |
| ...                           |                                  |                                  |
| **Auxiliary speed \[1/min\]** | **Mechanical power \[kW\]**      | **Supply power \[kW\]**          |
| ...                           | ...                              | ...                              |
| ...                           | ...                              | ...                              |