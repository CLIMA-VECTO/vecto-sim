Main Form
=========


![](pics/mainform.svg)


Description
-----------

The Main Form is loaded when starting VECTO. Closing this form will close VECTO even if other dialogs are still open. In this form all global settings can be controlled and all other application dialogs can be opened.

In order to start a calculation the [Calculation Mode](#calculation-modes) must be set and at least one [Job File (.vecto)](#job-editor) must added to the Job List. After clicking START all checked files in the Job List will be calculated.

The Main Form includes three tabs as described below:
:   -   Job Files Tab
-   Driving Cycles Tab (only if [Batch Mode](#batch-mode) is enabled)
-   Options Tab


Job Files Tab
-------------

###Job Files List#

Job files (.vecto) listed here will be used for calculation. Unchecked files will be ignored!
Doubleclick entries to edit job files with the [VECTO Editor](#job-editor).

![cb](pics/checkbox.png) All
:   (Un-)Check all files in Job List. Only checked files are calculated when clicking START.

![add](pics/plus-circle-icon.png) ***Add files to Job List***

![remove](pics/minus-circle-icon.png) ***Remove selected files from List***

![up](pics/Actions-arrow-up-icon.png)![down](pics/Actions-arrow-down-icon.png) ***Move selected files up or down in list***

####List Options#

- **Save/Load List**
    - Save or load Job List to text file
- **Load Autosave-List**
    - The Autosave-List is saved automatically on application exit and calculation start
- **Clear List**
    - Remove all files from Job List
- **Remove Paths**
    - Remove paths, i.e. only file names remain using the Working Directory as source path.





###![START](pics/Play-icon.png) ***START Button***

Start VECTO in the selected mode (see [Options](#options-tab)).


Driving Cycles Tab
------------------

Driving Cycle List
:   The Driving Cycles List is only used in [Batch Mode](#batch-mode). The same controls are used as in the Job Files List.


Options Tab
-----------

In this tab the global calculation settings can be changed.

![](pics/checkbox.png) Declaration Mode
:   Enable or disable [Declaration Mode](#declaration-mode)


![](pics/checkbox.png) Batch Mode
:   If Declaration Mode is disabled VECTO can be run in [Batch Mode](#batch-mode).


![cb](pics/checkbox.png) Cycle Distance Correction
:   Toggle Cycle Distance Correction. Always ON in Declaration Mode. Cycle Distance Correction monitors the driven distance in each time step and, if necessary, adds or removes time steps in order to keep the original distance given in the driving cycle.
:   -   If **enabled** the vehicle drives the same **distance** as given in the driving cycle
-   If ***disabled*** the vehicle travels the same **time** as given in the driving cycle (Note that distance-based cycles (see [here](#driving-cycle-.vdri)) are always converted to time-based cycles internally)


![cb](pics/checkbox.png) Use gears/rpm's form driving cycle
:   If activated VECTO will use gear and/or engine speed defintions included in the driving cycle (see [here](#driving-cycle-.vdri)).


![cb](pics/checkbox.png) Write modal results
:   Toggle output of modal results (.vmod files). Summary files (.vsum, .vres) are always created.


![cb](pics/checkbox.png) Shutdown system after last job
:   If activated VECTO will shutdown the system after the last job was completed. (Can be aborted during 100 seconds before shutdown.)

Output Path (BATCH Mode only)
:   Select target directory for result files (.vmod, .vres, .vsum)



![cb](pics/checkbox.png) Create Subdirectories for modal results (BATCH Mode only)
:   If activated a subdirectory for each job file will be created inside **Output Path** for modal output.


Controls
--------

![new](pics/blue-document-icon.png) New Job File
: Create a new .vecto file using the [VECTO Editor](#job-editor)


![open](pics/Open-icon.png) Open existing Job or Input File
: Open an existing input file (Job, Engine, etc.)


![tools](pics/Misc-Tools-icon.png) ***Tools***

- **[Job](#job-editor), [Vehicle](#vehicle-editor), [Engine](#engine-editor), [Gearbox](#gearbox-editor) Editor**
    - Opens the respective Editor
- **Graph**
    -   Open a new [Graph Window](#graph-window)
- **Open Log**
    -   Opens the [Log File](#application-files) in the system's default text editor
- **Settings**
    -   Opens the [Settings](#settings) dialog.


![info](pics/Help-icon.png) ***Help***


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

Note that the [message log](#application-files) can be opened in the ![](pics/Misc-Tools-icon.png) Tools menu with **Open Log**.

Statusbar
: Displays current status and progress of running calculations. When no calculation is running the current mode is displayed (Standard, Batch or Declaration Mode).
