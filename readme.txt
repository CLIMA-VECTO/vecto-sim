~ Before compiling you need to add references to: ~
    - vectolic.dll
    - itextsharp.dll
    - Newtonsoft.Json.dll

~ The following directories/files must be provided in the application folder (e.g. ..\bin\Release): ~
    - User Manual
    - User Manual\Release Notes.pdf (provided with the VECTO release)
    - GRAPHi (provided with the VECTO release)
    - vectolic.dll (should be placed there automatically when compiling)
    - license.dat (provided by EC/JRC)

~ Release Checklist ~
    - Make  zip-folder named with the "Semantic-version", ie: 2014_15_5-VECTO-2.0.1-beta1.
    - Copy into:
    -- VECTO: executable
    -- VECTO: itextsharp.dll, Newtonsoft.Json.dll, vectolic.dll (check for right version!! Source is currently in beta for file signing features)
    -- VECTO: User Manual (dir)
    -- VECTO: User Manual\Release Notes.pdf
    -- VECTO: Declaration
    -- <root>: Generic Vehicles (dir)
    - ZIP.
    - Upload into CITNet's SVN:
        https://webgate.ec.europa.eu/CITnet/svn/VECTO/trunk/Share/
      and link from: 
        https://webgate.ec.europa.eu/CITnet/confluence/display/VECTO/Releases    
    - Make licenses and Update private pages.    
    - Send announcment email.
    - Tag repos.
