Modal Results (.vmod)
=====================

Modal results are only created if enabled in the [Options](#main-form) tab. One file is created for each calculation and stored in the same directory as the .vecto file.

***Quantities:***

| **Name**        | **Unit** | **Description** |
| --------------- | ---------------------- | -------------------------- |
| time            | [s]      | Time step. |
| dist            | [km]     | Travelled distance. |
| v_act           | [km/h]   | Actual vehicle speed. |
| v_targ          | [km/h]   | Target vehicle speed. |
| acc             | [m/s²]   | Vehicle acceleration. |
| grad            | [%]      | Road gradient. |
| n               | [1/min]  | Engine speed. |
| Tq_eng          | [Nm]     | Engine torque. |
| Tq_clutch       | [Nm]     | Torque at clutch (before clutch, engine-side) |
| Tq_full         | [Nm]     | Full load torque |
| Tq_drag         | [Nm]     | Motoring torque |
| Pe_eng          | [kW]     | Engine power. |
| Pe_full         | [kW]     | Engine full load power. |
| Pe_drag         | [kW]     | Engine drag power. |
| Pe_clutch       | [kW]     | Engine power at clutch (equals **Pe** minus loss due to rotational inertia **Pa Eng**). |
| Gear            | [-]      | Gear. "0" = clutch opened / neutral.|
| Ploss GB        | [kW]     | Gearbox losses. |
| Ploss Diff      | [kW]     | Losses in differential / axle transmission. |
| Ploss Retrader  | [kW]     | Retarder losses. |
| Pa Eng          | [kW]     | Rotational acceleration power: Engine. |
| Pa GB           | [kW]     | Rotational acceleration power: Gearbox. |
| Pa Veh          | [kW]     | Vehicle acceleration power. |
| Proll           | [kW]     | Rolling resistance power demand. |
| Pair            | [kW]     | Air resistance power demand. |
| Pgrad           | [kW]     | Power demand due to road gradient. |
| Paux            | [kW]     | Total auxiliary power demand. |
| Pwheel          | [kW]     | Total power demand at wheel = sum of rolling, air, acceleration and road gradient resistance. |
| Pbrake          | [kW]     | Brake power. Drag power is included in **Pe**. |
| Paux_xxx        | [kW]     | Power demand of Auxiliary with ID xxx. See also [Aux Dialog](#auxiliary-dialog) and [Driving Cycle](#driving-cycle-.vdri). |
| FC-Map            | [g/h]    | Fuel consumption from FC map. |
| FC-AUXc         | [g/h]    | Fuel consumption after [Auxiliary-Start/Stop Correction](#fuel-consumption-calculation). (Based on FC.) |
| FC-WHTCc        | [g/h]    | Fuel consumption after [WHTC Correction](#fuel-consumption-calculation). (Based on FC-AUXc.) |
| TCv             | [-]      | Torque converter speed ratio |
| TCµ             | [-]      | Torque converter torque ratio |
| TC_M_Out        | [Nm]     | Torque converter output torque |
| TC_n_Out        | [1/min]  | Torque converter output speed |
