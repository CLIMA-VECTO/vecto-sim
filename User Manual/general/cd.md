
Cross Wind Correction
=====================



VECTO offers two different modes to consider cross wind influence on the drag coefficient. It is configured in the [Vehicle File](../GUI/VEH-Editor.html).


 Speed dependent correction
---------------------------

This is the default mode which is used in [Declaration Mode](calc_Declaration.html). The base drag coefficient (see [Vehicle File](../GUI/VEH-Editor.html)) is corrected by a speed dependent scaling function. The input file (.vcdv) format is described [here](../fileformat/VCDV.html).

 ![](pics/VCDV.png)


 Correction using Vair & Beta Input
-----------------------------------

If available the actual (measured) air speed and direction can be used. The input file (.vcdb) defines the drag coefficient scaling factor. The
input file (.vcdb) format is described [here](../fileformat/VCDB.html). The [driving cycle](../fileformat/VDRI.html) must include the air speed relative to vehicle (&lt;vair\_res&gt;) and the wind yaw angle (&lt;vair\_beta&gt;).

 ![](pics/VCDB.png)