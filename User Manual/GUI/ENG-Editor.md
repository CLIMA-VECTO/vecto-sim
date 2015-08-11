Engine Editor
=============
![](pics/ENG-Editor.svg)

Description
-----------
The Engine File (.veng) defines all engine-related parameters and input files like Fuel Consumption Map and Full Load Curves.

Relative File Paths
-------------------
It is recommended to define relative filepaths. This way the Job File and all input files can be moved without having to update the paths.

Example: "Demo\\FLD1.vfld" points to the "Demo" subdirectory of the Engine File's directory.

VECTO automatically uses relative paths if the input file (e.g. FC Map) is in the same directory as the Engine File. The Engine File must be saved before browsing for input files.)

Main Engine Parameters
----------------------
Make and Model
:   Free text defining the engine model, type, etc.

Idling Engine Speed \[rpm\]
:   Low idle, applied in simulation for vehicle standstill in neutral gear position.

Displacement \[ccm\]
:   Used in [Declaration Mode](#declaration-mode) to calculate inertia.

Inertia including Flywheel \[kgm²\]
:   Inertia for rotating parts including engine flywheel. In [Declaration Mode](#declaration-mode) the inertia is calculated automatically.

Full Load and Drag Curves
-------------------------


The [Full Load and Drag Curves (.vfld)](#full-load-and-drag-curves-.vfld) Note that gear-specific full load curves can be defined in the [Gearbox File](#gearbox-editor) to limit the maximum gearbox input torque.

The input file (.vfld) file format is described
[here](#full-load-and-drag-curves-.vfld).

Fuel Consumption Map
--------------------

The [Fuel Consumption Map](#fuel-consumption-map-.vmap) is used to calculate the base FC value. See [Fuel Consumption Calculation](#fuel-consumption-calculation) for details.

The input file (.vmap) file format is described [here](#fuel-consumption-map-.vmap).

WHTC Correction Factors
-----------------------

The WHTC Corretion Factors are required in [Declaration Mode](#declaration-mode) for the [WHTC FC Correction](#fuel-consumption-calculation).


Chart Area
----------

The Chart Area shows the fuel consumption map and the selected full load curve.

Controls
--------

![new](pics/blue-document-icon.png)New file
:   Create a new empty .veng file

![open](pics/Open-icon.png)Open existing file
:   Open an existing .veng file

![save](pics/Actions-document-save-icon.png)***Save current file*** \
   

![SaveAs](pics/Actions-document-save-as-icon.png)***Save file as...*** \
   

![sendto](pics/export-icon.png)Send current file to the [VECTO Editor](#job-editor)
:   **Note:** If the current file was opened via the [VECTO Editor](#job-editor) the file will be sent automatically when saved.

![](pics/OpenFile.PNG)***Open file*** (see [File Open Command)](#settings).

![OK](pics/OK.png)Save and close file
:   If necessary the file path in the [VECTO Editor](#job-editor) will be updated.

![Cancel](pics/Cancel.png)***Cancel without saving***
