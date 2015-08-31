Rolling Resistance Coefficient
==============================

The rolling resistance is calculated using a speed-independent rolling resistance coefficient (RRC).
In order to consider that the RRC is dependent from the vehicle weight it is modelled as a function of the total vehicle mass. The total RRC is calculated in VECTO using the following formula. The index i refers to each single vehicle axle (truck and trailer).



$RRC = \sum_{i=1}^{n} s_{(i)} \cdot RRC_{ISO(i)} \cdot \left( \frac{s_{(i)} \cdot m \cdot g }{w_{(i)} \cdot F_{zISO(i)} } \right)^{\beta-1}$



|         |    |             |
| ------- |----| ----------- |
RRC	| [-] | Total rolling resistance coefficient used for calculation | [calculated]
s~(i)~ | [-] | Relative axle load. Defined in the [Vehicle File](#vehicle-editor).| [user input]
RRC~ISO(i)~ | [-]	| ...Tyre RRC according to ISO 28580. Defined in the [Vehicle File](#vehicle-editor). | [user input]
m | [kg] | Vehicle mass plus loading. | [calculated]
g | [m/s²] | Earth gravity acceleration (constant = 9.81) | [constant model parameter]
w~(i)~ | [-] | Number of tyres (4 if Twin Tyres, else 2). Defined in the Vehicle File. | [user input]
F~zISO(i)~ | [N] | Tyre test load according to ISO 28580 (85% of max. load capacity). Defined in the [Vehicle File](#vehicle-editor). | [user input]
β | [-]	| Constant parameter = 0.9 | [constant model parameter]

For each axle the parameters **Relative axle load, RRC~ISO~** and **F~zISO~** have to be defined. Axles with twin tyres have to be marked using the respective checkbox in the [Vehicle-Editor](#vehicle-editor).
