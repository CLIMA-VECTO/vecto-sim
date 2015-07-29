Engine Only Mode
================


When this mode is enabled in the Job File then VECTO only calculates the fuel consumption based on a load cycle (engine speed and torque). In the [Job File](#job-editor) only the following parameters are needed:

-   Filepath to the Engine File (.veng)
-   Driving Cycles including engine torque (or power) and engine speed

The [driving cylce (.vdri)](#driving-cycle-.vdri) must contain:

-   Engine speed: header: &lt;n&gt;
-   Engine torque &lt;Me&gt; ***or*** engine power &lt;Pe&gt; at clutch. To explicitly define *motoring operation* use the **&lt;DRAG&gt;** keyword,see below. VECTO replaces the keyword with the motoring torque/power from the [.vfld file](../GUI/ENG-Editor.html#fld) during calculation.
-   \[Optional\] Additional power demand (aux) &lt;Padd&gt;

**Note that VECTO adds the engine's inertia to the given power demand!**

 **Example .vdri cycle:**
 ![](pics/VECTO-EngOnlyCycle.svg)
