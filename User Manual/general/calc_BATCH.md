
Batch Mode
==========

In Batch Mode a list of vehicles is run with a list of driving cycles. Each vehicle defined in the Job List is calculated with each driving cycle defined in the Driving Cycle List. Note that the Driving Cycle List is only visible if Batch Mode is enabled in the Main Form / Options Tab.

Requirements
------------

-   One or more checked job files in the Job List. The job files don't need to include driving cycles. These are ignored in Batch mode.
-   One or more checked driving cycles in the Dricing Cycle List

Results
-------

-   Modal results (.vmod) for each job file and driving cycle. One file for each vehicle/cycle combination.
-   Average/sum results (.vsum / .vsum.json). One file in total containing results for each vehicle/cycle combination.