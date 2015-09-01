Acceleration Limiting
=====================

VECTO limits the vehicle acceleration and deceleration according to speed-dependent limits. These limits are defined in the [Acceleration Limiting Input File](#acceleration-limiting-input-file-.vacc) (.vacc). Note that the full load curve also limits acceleration. If the engine cannot provide the required power the vehicle might accelerate below the defined acceleration limit.

This function cannot be disabled. If acceleration and/or deceleration should not be limited during calculation the values in the Acceleration Limiting file (.vacc) have to be changed accordingly.

Parameters in [Job File](#job-editor):
:	-   Filepath of the [Acceleration Limiting Input File](#acceleration-limiting-input-file-.vacc) (.vacc)

 The input file format is described [here](#acceleration-limiting-input-file-.vacc).
