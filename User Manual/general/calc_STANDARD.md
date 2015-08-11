Standard Mode
=============


This is the default calculation mode in VECTO. It is active when both [Batch](#batch-mode) and [Declaration](#declaration-mode) Mode are disabled.  In this mode a predefined list of job files (.vecto) is run. Each job file defines a vehicle and a list of driving cycles.

Requirements
------------

-   One or more checked job files in the Job List
-   Each job file must include at least one driving cycle


Results
-------

-   Modal results (.vmod) for each job file and driving cycle. One file for each cycle.
-   Average/sum results (.vsum / .vsum.json). One file in total containing results for each calculation.
