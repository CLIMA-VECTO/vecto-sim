' Copyright 2014 European Union.
' Licensed under the EUPL (the 'Licence');
'
' * You may not use this work except in compliance with the Licence.
' * You may obtain a copy of the Licence at: http://ec.europa.eu/idabc/eupl
' * Unless required by applicable law or agreed to in writing,
'   software distributed under the Licence is distributed on an "AS IS" basis,
'   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'
' See the LICENSE.txt for the specific language governing permissions and limitations.

''' <summary>
''' Determines how file extensions are set in the File Browser
''' </summary>
''' <remarks></remarks>
Public Enum tFbExtMode As Integer
    ForceExt = 0
    MultiExt = 1
    SingleExt = 2
End Enum

Public Enum tWorkMsgType
    StatusBar
    StatusListBox
    ProgBars
    JobStatus
    CycleStatus
    InitProgBar
    Abort
End Enum

Public Enum tMsgID
    NewJob
    Normal
    Warn
    Err
End Enum


Public Enum tCalcResult
    Err
    Abort
    Done
End Enum

Public Enum tJobStatus
    Running
    Queued
    OK
    Err
    Warn
    Undef
End Enum

Public Enum tDriComp
    t
    V
    Grad
    Alt
    nU
    Gears
    Padd
    Pe
    VairVres
    VairBeta
    Undefined
    s
    StopTime
    Torque
    Pwheel
End Enum

Public Enum tVehState
    Cruise
    Acc
    Dec
    Stopped
End Enum

Public Enum tEngState
    Idle
    Drag
    FullDrag
    Load
    FullLoad
    Stopped
    Undef
End Enum

Public Enum tEngClutch
    Closed
    Opened
    Slipping
End Enum

Public Enum tAuxComp
    Psupply
    Undefined
End Enum

Public Enum tCdMode
    ConstCd0 = 0
    CdOfV = 1
    CdOfBeta = 2
End Enum

Public Enum tRtType
    None = 0
    Primary = 1
    Secondary = 2
End Enum

Public Enum tGearbox
    Manual = 0
    SemiAutomatic = 1
    Automatic = 2
    Custom = 3
End Enum

Public Enum tVehCat As Integer
    Undef = 0
    RigidTruck = 1
    Tractor = 2
    Citybus = 3
    InterurbanBus = 4
    Coach = 5
End Enum

Public Enum tAxleConf As Integer
    Undef = 0
    a4x2 = 1
    a4x4 = 2
    a6x2 = 3
    a6x4 = 4
    a6x6 = 5
    a8x2 = 6
    a8x4 = 7
    a8x6 = 8
    a8x8 = 9
End Enum

Public Enum tLoading
    FullLoaded
    EmptyLoaded
    RefLoaded
    UserDefLoaded
End Enum

Public Enum tMission
    LongHaul
    RegionalDelivery
    UrbanDelivery
    MunicipalUtility
    Construction
    HeavyUrban
    Urban
    Suburban
    Interurban
    Coach
    Undef
End Enum

Public Enum tWHTCpart
    Urban
    Rural
    Motorway
End Enum

Public Enum tAux
    Fan
    SteerPump
    HVAC
    ElectricSys
    PneumSys
End Enum







