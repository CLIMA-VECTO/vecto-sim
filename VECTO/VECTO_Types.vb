Public Enum tPHEMmode As Short
    ModeSTANDARD = 0
    ModeBATCH = 1
End Enum

Public Enum tCalcMode As Short
    cmStandard = 0      '0 Standard (Gesamtes Fzg mit bestehendem Kennfeld)
    cmEngineOnly = 1    '1 Motor alleine (mit bestehendem Kennfeld)
    cmKEmes = 2         '2 Kennfeld erstellen aus Rollenzyklus
    cmEAnpi = 3         '3 Motoranalyse (x-Sek. Mittelwerte Messung-Rechng)
    cmKEnpi = 4         '4 Kennfeld erstellen aus Motormessung
    cmEAmes = 5         '5 Motoranalyse aus Fahrzeugmessung
    cmHEV = 6           '6 Hybridfahrzeug (VKM + Elektro)
    cmEV = 7            '7 Elektrofahrzeug
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
    Undef
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
    RigidTruck = 0
    Tractor = 1
    Citybus = 2
    InterurbanBus = 3
    Coach = 4
    Undef = 99
End Enum

Public Enum tAxleConf As Integer
    a4x2 = 0
    a4x4 = 1
    a6x2 = 2
    a6x4 = 3
    a6x6 = 4
    a8x2 = 5
    a8x4 = 6
    a8x6 = 7
    a8x8 = 8
    Undef = 99
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







