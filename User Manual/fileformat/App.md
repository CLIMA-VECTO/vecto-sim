Application Files
=================


VECTO uses a numbers of files to save GUI settings and file lists. All files are text-based and can be changed outside of VECTO ***if VECTO is not running***.


Settings.json
-------------

This file is located in VECTO's **config** folder. Here all parameters of the [Settings Dialog](#settings) are saved. The file uses the [JSON format](http://en.wikipedia.org/wiki/JSON)
![](pics/external-icon%2012x12.png).


Job / Cycle lists
-----------------

The job and cycle lists in the [Main Form](#main-form) are saved in the **joblist.txt** / **cyclelist.txt** files of the **config** folder.

Both files save the full file paths separated by line breaks. Additionally it is saved whether each file's checkbox is checked or not. "?1" after a file path means the file is checked (otherwise "?0"). However, this information can be omitted in which case the file will be loaded in checked state.


LOG.txt
-------

The tabulator-separated log file saves all messages of the [Main Form's Message List](#main-form) and is located in VECTO's program directory. The file is restarted whenever the [Logfile Size Limit](#settings) is reached.One backup is always stored as LOG\_backup.txt.


License file
------------

The license file license.dat is located in VECTO's program directory. Without a valid lisence file VECTO won't run.

It no valid license file is provided with your VECTO version please contact <vecto@jrc.ec.europa.eu>.
