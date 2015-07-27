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
:   Used in [Declaration Mode](../general/calc_Declaration.html) to calculate inertia.

Inertia including Flywheel \[kgm²\]
:   Inertia for rotating parts including engine flywheel. In [Declaration Mode](../general/calc_Declaration.html) the inertia is calculated automatically.

Full Load and Drag Curves
-------------------------


The [Full Load and Drag Curves (.vfld)](../fileformat/VFLD.html) Note that gear-specific full load curves can be defined in the [Gearbox File](../GUI/GBX-Editor.html) to limit the maximum gearbox input torque.

The input file (.vfld) file format is described
[here](../fileformat/VFLD.html).

Fuel Consumption Map
--------------------

The [Fuel Consumption Map](../fileformat/VMAP.html) is used to calculate the base FC value. See [Fuel Consumption Calculation](../general/FC.html) for details.

The input file (.vmap) file format is described [here](../fileformat/VMAP.html).

WHTC Correction Factors
-----------------------

The WHTC Corretion Factors are required in [Declaration Mode](../general/calc_Declaration.html) for the [WHTC FC Correction](../general/FC.html).


Chart Area
----------

The Chart Area shows the fuel consumption map and the selected full load curve.

Controls
--------

![new](../pics/icons/blue-document-icon.png)New file
:   Create a new empty .veng file

![open](../pics/icons/Open-icon.png)Open existing file
:   Open an existing .veng file

![save](../pics/icons/Actions-document-save-icon.png)Save current file
:   

![SaveAs](../pics/icons/Actions-document-save-as-icon.png)Save file as...
:   

![sendto](../pics/icons/export-icon.png)Send current file to the [VECTO Editor](VECTO-Editor.html)
:   **Note:** If the current file was opened via the [VECTO Editor](VECTO-Editor.html) the file will be sent automatically when saved.

![](../pics/misc/OpenFile.PNG)***Open file*** (see [File Open Command)](settings.html#opencmd).

![OK](../pics/misc/OK.png)Save and close file
:   If necessary the file path in the [VECTO Editor](VECTO-Editor.html) will be updated.

![Cancel](../pics/misc/Cancel.png)***Cancel without saving***