P~wheel~-Input (SiCo Mode)
==========================

For verification tasks it is possible to manually input the power at wheels (Pwheel) which is normally calculated via longitudinal dynamics. In this case VECTO only calculates the losses between wheels and engine and auxiliary power demand.
This mode is active as soon as Pwheel, Gear and Engine Speed are defined in the driving cycle.

Requirements
------------

- Driving Cycle must include Pwheel, Gear and Engine Speed, see [Driving Cycle (.vdri) format](#driving-cycle-.vdri).
- The driving cycle must be time-based.
- Distance Correction must be disabled ([Options tab in Main Form](#options-tab)).

**Example driving cylce with Pwheel input.**

| \<t\> | \<Pwheel\> | \<Gear\> | \<n\>
|-------|------------|----------|--------
|1|0.0|0|560.0
|2|0.0|0|560.0
|3|14.0|1|593.2
|4|51.9|1|705.5
|5|60.0|2|690.0
|6|85.6|2|868.4
|7|92.0|3|820.0
|8|112.3|3|897.6
|...|...|...|...
