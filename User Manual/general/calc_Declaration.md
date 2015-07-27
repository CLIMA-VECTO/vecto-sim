Declaration Mode
================


In Declaration Mode all input parameters that are not user-defined in official certification are locked in the user interface and automatically defined by VECTO during calculation. Calculations will be performed for each mission profile (of the corresponding HDV class) with three different loadings each: Empty, full and with reference loading. 

Declaration Mode can be activated in the [Options Tab](../GUI/mainform.html).


Requirements
------------


-   One or more checked job files in the Job List
-   The job files don't need to include driving cycles. These are automatically assigned.

Results
-------


-   Modal results (.vmod) for each job file and driving cycle. One file for each cycle.
-   Average/sum results (.vsum / .vsum.json). One file in total containing results for each calculation.
-   Results overview (.pdf). One file for each job file.