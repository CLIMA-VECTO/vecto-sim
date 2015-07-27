Transient Full Load
===================


VECTO uses a PT1 function to model transient torque build up using this formula:


$P_{fld\ dyn_{i}} = \frac{1}{T(n_{i})+1} \cdot [P_{fld\ stat}(n_{i})+T(n_{i}) \cdot P_{act_{i-1}}]$

where:


|                    |                                                                                          |  
| ------------------ | -----------------------------------------------------------------------------------------| 
| n~i~               | current engine speed                                                                     |  
| T(n~i~)            | PT1 time constant at engine speed n~i~ (col. 4 in [.vfld file](../fileformat/VFLD.html)) |   
| P\_fld\_stat(n~i~) | Static full load at engine speed n~i~ (col. 2 in [.vfld file](../fileformat/VFLD.html))  |
| P\_act~i-1~        | Engine power in previous time step                                                       |