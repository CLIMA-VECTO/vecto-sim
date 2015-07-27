Acceleration Limiting
=====================

VECTO limits the vehicle acceleration and deceleration according to speed-dependent limits. These limits are defined in the [Acceleration Limiting Input File](../fileformat/VACC.html) (.vacc). Note that the full load curve also limits acceleration. If the engine cannot provide the required power the vehicle might accelerate below the defined acceleration limit.

This function cannot be disabled. If acceleration and/or deceleration should not be limited during calculation the values in the Acceleration Limiting file (.vacc) have to be changed accordingly.

Parameters in [Job File](../GUI/VECTO-Editor.html):
:	-   Filepath of the [Acceleration Limiting Input File](../fileformat/VACC.html) (.vacc)

 The input file format is described [here](../fileformat/VACC.html).