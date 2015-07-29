Fuel Consumption Map (.vmap)
============================

The FC map is used to interpolate the base fuel consumption before corrections are applied. For details see [Fuel Consumption Calculation](#fuel-consumption-calculation).

File Format
-----------

The file uses the [VECTO CSV format](#csv-format).

Format:
: -   Three columns
-   One header line
-   At least four lines with numeric values (below file header)
-   The map must cover the full engine range between full load and motoring curve. **Extrapolation is not possible!**

***Columns:***

| **engine speed [1/min]** | **engine torque [Nm]** | **Fuel Consumption [g/h]** |
| ------------------------ | ---------------------- | -------------------------- |
| ...                      | ...                    | ...                        |
| ...                      | ...                    | ...                        |
| ...                      | ...                    | ...                        |
| ...                      | ...                    | ...                        |
