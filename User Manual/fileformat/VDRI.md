Driving Cycle (.vdri)
=====================

The Driving Cycle defines vehicle speed, road gradient and other parameters either time- or distance-based. It's open format requires Idenfiers to define the content of each column.

Important Notes
---------------

- For distance-based cycles there is a minimum distance-step of 1[m] required. The calculation will abort if larger steps are used.
- Time-based cycles can be defined in any given time resolution, including variable time steps. If the time identifier "<t>" is not used the data will be interpreted in 1[s] resolution (1Hz).

Supported Identifiers
---------------------

| Identifier | Quantity | Unit   | Description                                     
| ---------- | -------- | ------ | ------------------------------------------------
| \<s\> 		 | Distance	| [m]    | Travelled distance used for distance-based cycles. If \<t\> is also defined this column will be ignored.
| \<t\>        | Time     | [s]    | Used for time-based cycles. If neither this nor the distance \<s\> is defined the data will be interpreted as 1Hz.
| \<v\> | Vehicle Speed   | [km/h] | Required except for [Engine Only Mode](../general/EngOnlyMode.html) calculations.
| **\<grad\>** | Road Gradient | \[%\] | Optional.
| **\<stop\>** | Stopping Time | \[s\] | Required for distance-based cycles. Not used in time based cycles. \<stop\> defines the time the vehicle spends in stop phases.
| **\<Aux_xxx\>** | Auxiliary Supply Power | \[kW\] | Supply Power input for each auxiliary defined in the [.vecto file](../GUI/VECTO-Editor.html) where xxx matches the ID of the corresponding [Auxiliary](../GUI/VECTO-Editor_Aux.html). ID's are not case sensitive and must not contain space or special characters.
| **\<n\>** | Engine Speed | \[rpm\] | If \<n\> is defined VECTO uses that instead of the calculated engine speed value.
| **\<gear\>** | Gear	| \[-\] | Gear input. Overwrites the gear shift model.
| **\<Padd\>** | Additional Aux Power Demand | \[kW\]	| This power input will be directly added to the engine power in addition to possible other auxiliaries. Also used in [Engine Only Mode](../general/EngOnlyMode.html).
| **\<vair_res\>** | Air speed relative to vehicle | \[km/h\] | Only required if [**Cross Wind Correction**](../general/cd.html) is set to **Vair & Beta Input**.
| **\<vair_beta\>** | Wind Yaw Angle | \[°\] | Only required if [**Cross Wind Correction**](../general/cd.html) is set to **Vair & Beta Input**.
| **\<Pe\>** | Engine power | \[kW\] | Effective engine power at clutch. Only required in [Engine Only Mode](../general/EngOnlyMode.html). Alternatively torque <Me> can be defined. Use **\<DRAG>** to define motoring operation.
| **\<Pwheel\>** | Power at wheels | \[kW\] | Overwrites power calculation. Requires Gear and Engine Speed input.

Examples
--------

The demo data provided with VECTO contains several .vdri files that may be used as template.

**Example 1:** Distance-based cycle with **Road Gradient** and two **Auxiliaries**


|\<s\>|\<v\>|\<grad\>|\<stop\>|\<Aux_Alt\>|\<Aux_Demo\>
| - | - | ---- | ---- | ------- | ---------
| 0 | 60 | 2.95016969 | 0 | 0.5 | 4.752
| 1 | 60 | 2.95016969 | 0 | 0.5 | 4.752
| 2 | 60 | 2.95016969 | 0 | 0.5 | 4.752
| 3 | 60 | 2.95016969 | 0 | 0.5 | 4.752
| 4 | 60 | 2.95016969 | 0 | 0.5 | 4.752
| 5 | 60 | 2.95016969 | 0 | 0.5 | 4.752
| 6 | 60 | 2.95016969 | 0 | 0.5 | 4.752
| 7 | 60 | 2.95016969 | 0 | 0.5 | 4.752
| 8 | 60 | 2.95016969 | 0 | 0.5 | 4.752
| 9 | 60 | 3.06801369 | 0 | 0.5 | 4.752
| 10 | 60 | 3.06801369 | 0 | 0.5 | 4.752
| 11 | 60 | 3.06801369 | 0 | 0.5 | 4.752
| 12 | 60 | 3.06801369 | 0 | 0.5 | 4.752
| 13 | 60 | 3.06801369 | 0 | 0.5 | 4.752


**Example 2:** Time-based cycle with Engine Speed, **Vair & Beta Input**, one **Auxiliary** and **Additional Aux Demand**


|\<t\> |\<v\>|\<grad\>|\<n\>  |\<vair_res\>|\<vair_beta\>|\<Aux_Alt\>|\<Padd\>
| --- | -- | ----- | ------- | --------- | ---------- | -------- | --------
|1 |0       |0           |594     | 0     | 0    |0.532     |2.007686806
|2 |0       |0           |602.25  | 0     | 0    |0.588     |3.222867975
|3 |0       |0           |600     | 0     | 0    |0.644     |3.215345965
|4 |0       |0           |598     | 0     | 0    |0.728     |3.208650609
|5 |0       |0           |595.25  | 0     | 0    |0.644     |3.199146758
|6 |0       |0           |602.5   | 0     | 0    |0.588     |2.050366424
|7 |0       |0           |599.25  | 0     | 0    |0.588     |3.212783873
|8 |0       |0           |598     | 0     | 0    |0.644     |3.208568475
|9 |0       |0           |595.75  | 0     | 0    |0.504     |3.201815003
|10|0.3112  |0           |983.75  | 0     | 0    |0.476     |4.532197507
|11|5.2782  |-0.041207832|723.75  | 8.532 | 0    |0.42      |2.453370264
|12|10.5768 |-0.049730127|1223.25 | 12.024| 34   |0.476     |3.520827362
|13|15.66795|-0.05296987 |1737.25 | 14.472| 28   |0.504     |4.880874189
|14|20.80995|-0.05715414 |2238.25 | 21.312| 21   |0.476     |6.648425375
|15|24.1622 |-0.059104326|2428.5  | 21.42 | 5    |0.476     |7.393337294
|16|26.56975|-0.057649533|1709.25 | 22.5  | -8   |0.476     |4.999156225
|17|31.6701 |-0.056915608|1966.75 | 32.22 | -11  |0.504     |5.889710204
|18|36.98445|-0.06826105 |2250    | 38.232| -5   |0.504     |6.917938049


