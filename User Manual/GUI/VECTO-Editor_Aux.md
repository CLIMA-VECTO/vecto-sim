
Auxiliary Dialog
================


![](pics/VECTO-Editor_Aux.png)


Description
-----------

The Auxiliary Dialog is used to configure auxiliaries. Auxiliary efficieny is defined using an [Auxiliary Input File (.vaux)](../fileformat/VAUX.html). See [Auxiliaries](../general/Auxiliaries.html) for details on how the power demand for each auxiliary is calculated.

In [Declaration Mode](../general/calc_Declaration.html) only the Technology for each auxiliary has to be selected.


Settings
--------

Type
:	String defining type of auxiliary. Click the arrow to load from a predefined list, however It is not required to use a type from the list.

ID
:	The ID string is required to link the auxiliary to the corresponding supply power definition in the driving cycle. The ID must not contain space or special characters text and numbers only). The ID is not case sensitive (e.g. "ALT" will link to "Alt" or "alt", etc.)
***Example*** *: Auxiliary "ALT" is linked to the column "&lt;AUX\_ALT&gt;" in the driving cylce.*
See [Auxiliaries](../general/Auxiliaries.html) for details.

Input File
:	Path to the [Auxiliary File (.vaux)](../fileformat/VAUX.html).

Controls
--------

![ok](../pics/misc/OK.png) ***Save and close***

![cancel](../pics/misc/Cancel.png) ***Close without saving***