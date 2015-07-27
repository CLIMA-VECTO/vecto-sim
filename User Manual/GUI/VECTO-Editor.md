Job Editor
==========

![](pics/VECTO-Editor.svg)


Description
-----------

The job file (.vecto) includes all informations to run a VECTO calculation. It defines the vehicle and the driving cycle(s) to be used for calculation. In summary it defines:

-   Filepath to the [Vehicle File (.vveh)](VEH-Editor.html) which defines the not-engine/gearbox-related vehicle parameters
-   Filepath to the [Engine File (.veng)](ENG-Editor.html) which includes full load curve(s) and the fuel consumption map
-   Filepath ot the [Gearbox File (.vgbx)](GBX-Editor.html) which defines gear ratios and transmission losses
-   Auxiliaries
-   Driver Assist parameters
-   Driving Cycles (not used in Batch Mode)


 Relatve File Paths
-------------------

It is recommended to define relative filepaths. This way the Job File and all input files can be moved without having to update the paths. Example: "Vehicles\\Vehicle1.vveh" points to the "Vehicles" subdirectory of the Job File's directoy.

VECTO automatically uses relative paths if the input file (e.g. Vehicle File) is in the same directory as the Job File. (The Job File must be saved before browsing for input files.)


 General Settings
-----------------

![](../pics/misc/checkbox.png) Engine Only Mode
:	Enables [Engine Only Mode](../general/EngOnlyMode.html). Only the following parameters are needed for this mode:

-   Filepath to the [Engine File (.veng)](ENG-Editor.html)
-   [Driving Cycles](../fileformat/VDRI.html) including engine torque (or power) and engine speed


Filepath to the Vehicle File (.vveh)
:	Files can be created and edited using the [Vehicle Editor](VEH-Editor.html).

Filepath to the Engine File (.veng)
:	Files can be created and edited using the [Engine Editor](ENG-Editor.html).

Filepath ot the Gearbox File(.vgbx)
:	Files can be created and edited using the [Gearbox Editor](GBX-Editor.html).

Auxiliaries
:	This list contains all auxiliaries used for calculation. The auxiliaries are configured using the [Auxiliary Dialog](VECTO-Editor_Aux.html). For each auxiliary an [Auxiliary Input File (.vaux)](../fileformat/VAUX.html) must be provided and the [driving cycle](../fileformat/VDRI.html) must include the corresponding supply power.
**Double-click** entries to edit with the [Auxiliary Dialog](VECTO-Editor_Aux.html).

: ![addaux](../pics/icons/plus-circle-icon.png) Add new Auxiliary
: ![remaux](../pics/icons/minus-circle-icon.png) Remove the selected Auxiliary from the list

: See [Auxiliaries](../general/Auxiliaries.html) for details.

Cycles
:	List of cycles used for calculation. The .vdri format is described [here](../fileformat/VDRI.html).
**Double-click** an entry to open the file (see [File Open Command](settings.html#opencmd)).
**Click** selected items to edit file paths.

: ![addcycle](../pics/icons/plus-circle-icon.png) Add cycle (.vdri)
: ![remcycle](../pics/icons/minus-circle-icon.png) Remove the selected cycle from the list


 Driver Assist Tab
------------------

In this tab the driver assistance functions are enabled and parameterised.

Engine Start/Stop
:	See [Engine Start/Stop](../general/StartStop.html) for details.

Overspeed / Eco-Roll
:	See [Overspeed / Eco-Roll](../general/EcoRoll.html) for details.

Look-Ahead Coasting
:	See [Look-Ahead Coasting](../general/LAC.html) for details.

Acceleration Limiting
:	See [Acceleration Limiting](../general/AccLimit.html) for details.


 Chart Area
-----------

If a valid [Vehicle File](VEH-Editor.html), [Engine File](ENG-Editor.html) and [Gearbox File](GBX-Editor.html) is loaded into the Editor the main vehicle parameters like HDV class and axle configuration are shown here. The plot shows the full load curve(s) and shift polygons. In [Declaration Mode](../general/calc_Declaration.html) the **generic**  shift polygons are shown, not the ones from the Gearbox File.

Controls
--------

![new](../pics/icons/blue-document-icon.png) New Job File
:	Create a new empty .vecto file

![open](../pics/icons/Open-icon.png) Open existing Job File
:	Open an existing .vecto file

![save](../pics/icons/Actions-document-save-icon.png) ***Save current Job File***

![SaveAs](../pics/icons/Actions-document-save-as-icon.png) ***Save Job File as...***

![sendto](../pics/icons/export-icon.png) Send current file to Job List in [Main Form](mainform.html)
:	**Note:** The file will be sent to the Job List automatically when saved.

![veh](pics/VECTO/Veh.png) ***Open [Vehicle Editor](VEH-Editor.html)***

![eng](pics/VECTO/Eng.png) ***Open [Engine Editor](ENG-Editor.html)***

![gbx](pics/VECTO/Gbx.png) ***Open [Gearbox Editor](GBX-Editor.html)***

![](../pics/misc/browse.png) ***Browse for vehicle/engine/gearbox files***

![OK](../pics/misc/OK.png) Save and close file
:	File will be added to Job List in the [Main Form](mainform.html).

![Cancel](../pics/misc/Cancel.png) ***Cancel without saving***