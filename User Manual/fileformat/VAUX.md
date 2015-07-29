Auxiliary Input File (.vaux)
============================

This file is used to configure a single auxiliary. Multiple .vaux files can be defined in the [Job File](#job-editor) via the [Auxiliary Dialog](#auxiliary-dialog).

See [Auxiliaries](#auxiliaries) for details on how the power demand for each auxiliary is calculated.

File Format
-----------

The file uses the VECTO CSV format with three additional parameters on top of the efficiency map.

Format:
: -   Lines 1,3,5 and 7 are reserved for headers. Theses lines are skipped during file read.
-   Line 2: [TransRatio](#auxiliaries) = Speed ratio between auxiliary and engine. \[-\]
-   Line 4: [EffToEng](#auxiliaries) = Efficiency of auxiliary (belt/gear) drive \[-\]
-   Line 6: [EffToSply](#auxiliaries) = Consumer efficiency \[-\]
-   Line 8 and following (at least four): [EffMap](#auxiliaries) = Auxiliary efficiency map.

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
