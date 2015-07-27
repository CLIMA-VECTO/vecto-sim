
Main Form
=========


![](pics/mainform.svg)


Description
-----------

The Main Form is loaded when starting VECTO. Closing this form will close VECTO even if other dialogs are still open. In this form all global settings can be controlled and all other application dialogs can be opened.

In order to start a calculation the [Calculation Mode](../general/calc_index.html) must be set and at least one [Job File (.vecto)](VECTO-Editor.html) must added to the Job List. After clicking START all checked files in the Job List will be calculated.

The Main Form includes three tabs as described below:
:   -   Job Files Tab
-   Driving Cycles Tab (only if [Batch Mode](../general/calc_BATCH.html) is enabled)
-   Options Tab


Job Files Tab
-------------

###Job Files List#

Job files (.vecto) listed here will be used for calculation. Unchecked files will be ignored!
Doubleclick entries to edit job files with the [VECTO Editor](VECTO-Editor.html).

![cb](../pics/misc/checkbox.png) All
:   (Un-)Check all files in Job List. Only checked files are calculated when clicking START.

![add](../pics/icons/plus-circle-icon.png) ***Add files to Job List***

![remove](../pics/icons/minus-circle-icon.png) ***Remove selected files from List***

![up](../pics/icons/Actions-arrow-up-icon.png)![down](../pics/icons/Actions-arrow-down-icon.png) ***Move selected files up or down in list***

####List Options#

- **Save/Load List**
    - Save or load Job List to text file
- **Load Autosave-List**
    - The Autosave-List is saved automatically on application exit and calculation start
- **Clear List**
    - Remove all files from Job List
- **Remove Paths**
    - Remove paths, i.e. only file names remain using the Working Directory as source path.









###![START](../pics/icons/Play-icon.png) ***START Button***

Start VECTO in the selected mode (see [Options](mainform_options.html)).


Driving Cycles Tab
------------------

Driving Cycle List
:   The Driving Cycles List is only used in [Batch Mode](../general/calc_BATCH.html). The same controls are used as in the Job Files List.


Options Tab
-----------

In this tab the global calculation settings can be changed.

![](../pics/misc/checkbox.png) Declaration Mode
:   Enable or disable [Declaration Mode](../general/calc_Declaration.html)


![](../pics/misc/checkbox.png) Batch Mode
:   If Declaration Mode is disabled VECTO can be run in [Batch Mode](../general/calc_BATCH.html).


![cb](../pics/misc/checkbox.png) Cycle Distance Correction
:   Toggle Cycle Distance Correction. Always ON in Declaration Mode. Cycle Distance Correction monitors the driven distance in each time step and, if necessary, adds or removes time steps in order to keep the original distance given in the driving cycle.
:   -   If **enabled** the vehicle drives the same **distance** as given in the driving cycle
-   If ***disabled*** the vehicle travels the same **time** as given in the driving cycle (Note that distance-based cycles (see [here](../fileformat/VDRI.html)) are always converted to time-based cycles internally)


![cb](../pics/misc/checkbox.png) Use gears/rpm's form driving cycle
:   If activated VECTO will use gear and/or engine speed defintions included in the driving cycle (see [here](../fileformat/VDRI.html)).


![cb](../pics/misc/checkbox.png) Write modal results
:   Toggle output of modal results (.vmod files). Summary files (.vsum, .vres) are always created.


![cb](../pics/misc/checkbox.png) Shutdown system after last job
:   If activated VECTO will shutdown the system after the last job was completed. (Can be aborted during 100 seconds before shutdown.)

Output Path (BATCH Mode only)
:   Select target directory for result files (.vmod, .vres, .vsum)



![cb](../pics/misc/checkbox.png) Create Subdirectories for modal results (BATCH Mode only)
:   If activated a subdirectory for each job file will be created inside **Output Path** for modal output.


Controls
--------

![new](../pics/icons/blue-document-icon.png) New Job File
: Create a new .vecto file using the [VECTO Editor](VECTO-Editor.html)


![open](../pics/icons/Open-icon.png) Open existing Job or Input File
: Open an existing input file (Job, Engine, etc.)


![tools](../pics/icons/Misc-Tools-icon.png) ***Tools***

- **[Job](VECTO-Editor.html), [Vehicle](VEH-Editor.html), [Engine](ENG-  Editor.html), [Gearbox](GBX-Editor.html) Editor**
    - Opens the respective Editor
- **Graph**
    -   Open a new [Graph Window](Graph.html)
- **Open Log**
    -   Opens the [Log File](../fileformat/App.html) in the system's default text editor
- **Settings**
    -   Opens the [Settings](settings.html) dialog.


![info](../pics/icons/Help-icon.png) ***Help***


- **User Manual**
    - Opens this User Manual
- **Create Activation File**
    - Create an Activation File used for Licensing
- **About VECTO**
    - Information about the software, license and support contact


Message List
: All messages, warnings and errors are displayed here and written to the log file LOG.txt in the VECTO application folder.
Depending on the colour the following message types are displayed:

-   <span style="font-family: Courier New;">Status Messages</span>
-   <span style="font-family: Courier New; background-color: rgb(255, 204, 102);">Warnings</span>
-   <span style="font-family: Courier New; background-color: red; color: white;">Errors</span>
-   <span style="font-family: Courier New; text-decoration: underline; color: rgb(51, 51, 255);">Links</span> - click to open file/user manual/etc.

Note that the [message log](../fileformat/App.html) can be opened in the ![](../pics/icons/Misc-Tools-icon.png) Tools menu with **Open Log**.


Statusbar
: Displays current status and progress of running calculations. When no calculation is running the current mode is displayed (Standard, Batch or Declaration Mode).