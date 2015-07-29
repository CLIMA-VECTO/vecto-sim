Acceleration Limiting Input File (.vacc)
========================================

The file is used for [Acceleration Limiting](#acceleration-limiting). It defines the acceleration and deceleration limits as function of
vehicle speed. The filepath has to be defined in the [Job File](#job-editor).

![](pics/AccLimit.png)

File Format
-----------

The file uses the [VECTO CSV format](#user-interface).

Format:
: -   Three columns
-   One header line
-   At least two lines with numeric values (below file header)

***Columns:***

  | **vehicle speed \[km/h\]** | **Max. acceleration \[m/s^2^\]** | **Max. deceleration \[m/s^2^\]** |
  | -------------------------- | -------------------------------- | -------------------------------- |
  | ...                        | ...                              | ...                              |
  | ...                        | ...                              | ...                              |
