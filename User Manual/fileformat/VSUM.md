Summary Results (.vsum)
=======================

The .vsum file includes total / average results for each calculation run in one execution (ie. click of [START Button](../GUI/mainform.html)). The file is located in the directory of the fist run .vecto file.

***Quantities:***
  
|  Name | Unit | Description
| ----- | ---- | -----------------------------------------
|  Job | [-] | Job number. Format is "x-y" with x = file number and y = cycle number
|  Input File | [-] | Name of the input file
|  Cycle | [-] | Name of the cycle file
|  time | [s] | Total simulation time
|  distance | [km] | Total travelled distance
|  speed | [km/h] | Average vehicle speed
|  ∆altitude | [m] | Altitude difference between start and end of cycle
|  Ppos | [kW] | Average positive engine power
|  Pneg | [kW] | Average negative engine power 
|  FC | [g/km] | Average fuel consumption
|  FC-AUXc | [g/km] | Fuel consumption after [Auxiliary-Start/Stop Correction](../general/FC.html). (Based on FC.)
|  FC-WHTCc | [g/km] | Fuel consumption after [WHTC Correction](../general/FC.html). (Based on FC-AUXc.)
|  Pbrake | [kW] | Average brake power (not including engine drag)
|  EposICE | [kWh] | Total positive engine work
|  EnegICE | [kWh] | Total negative engine work (engine brake)
|  Eair | [kWh] | Total work of air resistance
|  Eroll | [kWh] | Total work of rolling resistance
|  Egrad | [kWh] | Total work of gradient resistance
|  Eacc | [kWh] | Total work from accelerations (<0) / decelerations (>0) 
|  Eaux | [kWh] | Total energy demand of auxiliaries
|  Eaux_xxx | [kWh] | Energy demand of auxiliary with ID xxx. See also [Aux Dialog](../GUI/VECTO-Editor_Aux.html) and [Driving Cycle](VDRI.html).
|  Ebrake | [kWh] | Total work dissipated in mechanical braking (sum of service brakes, retader and additional engine exhaust brakes)
|  Etransm | [kWh] | Total work of transmission losses
|  Eretarder | [kWh] | Total retarder losses
|  Mass | [kg] | Vehicle mass (equals **Curb Weight Vehicle** plus **Curb Weight Extra Trailer/Body**, see [Vehicle Editor](../GUI/VEH-Editor.html))
|  Loading | [kg] | Vehicle loading (see [Vehicle Editor](../GUI/VEH-Editor.html))
|  a | [m/s2] | Average acceleration
|  a_pos | [m/s²] | Average acceleration in acceleration phases*
|  a_neg | [m/s²] | Average deceleration in deceleration phases*
|  Acc.Noise | [m/s²] | Acceleration noise
|  pAcc | [%] | Time share of acceleration phases*
|  pDec | [%] | Time share of deceleration phases*
|  pCruise | [%] | Time share of cruise phases*
|  pStop | [%] | Time share of stop phases*

\*Driving conditions:

-	Acceleration: a~3s~ &gt; 0.125 \[m/s^2^\]
-	Deceleration: a~3s~ &lt; -0.125 \[m/s^2^\]
-	Cruise: -0.125 ≤ a~3s~ ≤ 0.125 \[m/s^2^\]
-	Stop: v &lt; 0.1 \[m/s\]\
	*a~3s~ = 3 seconds-averaged acceleration*


Definition of work (E...):
: sign &gt; 0: positive work applied to the vehicle (e.g. from engine, from kinetic energy)
sign &lt; 0: losses
The sum of EposICE, EnegICE, Eair, Eroll, Egrad, Eacc, Eaux, Ebrake, Etransm and Eretarder is zero (besides small rounding error for long driving cycles)
