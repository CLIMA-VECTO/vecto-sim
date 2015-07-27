Engine Start/Stop
=================

If enabled the engine will be turned off after the set **Activation Delay \[s\] **if the following conditions apply:

-   Power demand ≤ 0
-   Vehicle speed is below **Max Speed \[km/h\]**
-   Engine was running for at least **Min ICE-On Time \[s\]**



Parameters in [Job File](../GUI/VECTO-Editor.html):
: -   **Max speed \[km/h\]**.
-   **Min ICE-On Time \[s\]**
-   **Activation Delay \[s\]**

If Start/Stop is enabled the fuel consumption is corrected for not-considered auxiliary energy consumption during engine stop. See [Start/Stop FC Correction](FC.html).