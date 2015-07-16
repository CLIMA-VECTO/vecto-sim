#!/usr/bin/env python
"""
1) Copies the declaration files into the project resource directory.
2) Renames the mission files accordingly
3) Scales Pneumatic System from KW to W.
4) Renames the VCDV paramerters.csv to parameters.csv
5) Scales the WHTC-Weighting percentage values from 0-100 to 0-1

Prerequisites:
  * Original declaration files accessible in "root/Declaration"

Usage:
  python DeclarationConverter.py
"""

import os
import shutil

__author__ = "Michael Krisper"
__email__ = "michael.krisper@tugraz.at"
__date__ = "2015-07-15"
__version__ = "1.0.0"

SOURCE = "../Declaration"
DESTINATION = os.path.abspath("../VectoCore/Resources/Declaration")


def main(source_path, destination_path):
    # Copy files from source to resource dir
    shutil.rmtree(destination_path, onerror=lambda dir, path, err: print(dir, path, err))
    shutil.copytree(source_path, destination_path, ignore=lambda src, names: names if "Reports" in src else [])

    # Rename mission cycles
    os.chdir(os.path.join(destination_path, "MissionCycles"))
    for file in os.listdir():
        os.rename(file, file.replace("_", "").replace(" ", "").replace("Citybus", "").replace("Bus", ""))

    # Adjust PS table
    os.chdir(os.path.join(destination_path, "VAUX"))
    with open("PS-Table.csv", "r") as f:
        lines = f.readlines()

    with open("PS-Table.csv", "w") as f:
        f.write(lines[0])
        for line in lines[1:]:
            values = line.split(",")
            f.write("{},{}\n".format(values[0], ",".join(str(float(v)*1000) for v in values[1:])))

    #VCDV
    os.chdir(os.path.join(destination_path, "VCDV"))
    os.rename("paramerters.csv", "parameters.csv")


    # WHTC Weighting Factors
    os.chdir(os.path.join(destination_path))
    with open("WHTC-Weighting-Factors.csv", "r") as f:
        lines = f.readlines()

    with open("WHTC-Weighting-Factors.csv", "w") as f:
        f.write(lines[0])
        for line in lines[1:]:
            values = line.split(",")
            f.write("{},{}\n".format(values[0], ",".join(str(float(v) / 100) for v in values[1:])))


if __name__ == "__main__":
    main(SOURCE, DESTINATION)