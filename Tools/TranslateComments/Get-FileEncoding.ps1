<#
.SYNOPSIS
Gets file encoding.
.DESCRIPTION
The Get-FileEncoding function determines encoding by looking at Byte Order Mark (BOM).
Based on port of C# code from http://www.west-wind.com/Weblog/posts/197245.aspx
.EXAMPLE
Get-ChildItem  *.ps1 | select FullName, @{n='Encoding';e={Get-FileEncoding $_.FullName}} | where {$_.Encoding -ne 'ASCII'}
This command gets ps1 files in current directory where encoding is not ASCII
.EXAMPLE
Get-ChildItem  *.ps1 | select FullName, @{n='Encoding';e={Get-FileEncoding $_.FullName}} | where {$_.Encoding -ne 'ASCII'} | foreach {(get-content $_.FullName) | set-content $_.FullName -Encoding ASCII}
Same as previous example but fixes encoding using set-content
.EXAMPLE 
Do this next line before or add function in Profile.ps1
    Import-Module .\Get-FileEncoding.ps1
.NOTES  
    File Name  : Get-FileEncoding.ps1  
    Author     : <Unknwon>, F.RICHARD, ankostis
    Requires   : PowerShell V2 CTP3   
.LINK 
    http://franckrichard.blogspot.it/2010/08/powershell-get-encoding-file-type.html
    http://unicode.org/faq/utf_bom.html
.LINK 
    http://en.wikipedia.org/wiki/Byte_order_mark
#>
function Get-FileEncoding {

 [CmdletBinding()] 
 Param (
   [Parameter(Position = 0, Mandatory = $True, ValueFromPipelineByPropertyName = $True)]
   [alias(“PSPath”)]
   [alias(“FullName”)]
   [PSObject[]]$Path
 )

PROCESS {
 
    $files = @();
    if($path) {
        $files += $path
    } else {
        $files += @($input | Foreach-Object { $_.FullName })
    }

    #echo "___: $_"
    #echo "PTH: $path"
    #echo "INP: $input"
    #echo "INO: $InputObject"
    #echo "FIL: $files"

    foreach($file ile in $files) {
         [byte[]]$byte = get-content -Encoding byte -ReadCount 4 -TotalCount 4 -Path $File
         #Write-Host Bytes: $byte[0] $byte[1] $byte[2] $byte[3]
         
         # EF BB BF (UTF8)
         if ( $byte[0] -eq 0xef -and $byte[1] -eq 0xbb -and $byte[2] -eq 0xbf )
         { printout 'UTF8' }
         
         # FE FF  (UTF-16 Big-Endian)
         elseif ($byte[0] -eq 0xfe -and $byte[1] -eq 0xff)
         { printout 'Unicode UTF-16 Big-Endian' }
         
         # FF FE  (UTF-16 Little-Endian)
         elseif ($byte[0] -eq 0xff -and $byte[1] -eq 0xfe)
         { printout 'Unicode UTF-16 Little-Endian' }
         
         # 00 00 FE FF (UTF32 Big-Endian)
         elseif ($byte[0] -eq 0 -and $byte[1] -eq 0 -and $byte[2] -eq 0xfe -and $byte[3] -eq 0xff)
         { printout 'UTF32 Big-Endian' }
         
         # FE FF 00 00 (UTF32 Little-Endian)
         elseif ($byte[0] -eq 0xfe -and $byte[1] -eq 0xff -and $byte[2] -eq 0 -and $byte[3] -eq 0)
         { printout 'UTF32 Little-Endian' }
         
         # 2B 2F 76 (38 | 38 | 2B | 2F)
         elseif ($byte[0] -eq 0x2b -and $byte[1] -eq 0x2f -and $byte[2] -eq 0x76 -and ($byte[3] -eq 0x38 -or $byte[3] -eq 0x39 -or $byte[3] -eq 0x2b -or $byte[3] -eq 0x2f) )
         { printout 'UTF7'}
         
         # F7 64 4C (UTF-1)
         elseif ( $byte[0] -eq 0xf7 -and $byte[1] -eq 0x64 -and $byte[2] -eq 0x4c )
         { printout 'UTF-1' }
         
         # DD 73 66 73 (UTF-EBCDIC)
         elseif ($byte[0] -eq 0xdd -and $byte[1] -eq 0x73 -and $byte[2] -eq 0x66 -and $byte[3] -eq 0x73)
         { printout 'UTF-EBCDIC' }
         
         # 0E FE FF (SCSU)
         elseif ( $byte[0] -eq 0x0e -and $byte[1] -eq 0xfe -and $byte[2] -eq 0xff )
         { printout 'SCSU' }
         
         # FB EE 28  (BOCU-1)
         elseif ( $byte[0] -eq 0xfb -and $byte[1] -eq 0xee -and $byte[2] -eq 0x28 )
         { printout 'BOCU-1' }
         
         # 84 31 95 33 (GB-18030)
         elseif ($byte[0] -eq 0x84 -and $byte[1] -eq 0x31 -and $byte[2] -eq 0x95 -and $byte[3] -eq 0x33)
         { printout 'GB-18030' }
         
         else
         { printout 'ASCII' }
        }
    } # End file-loop
}    
    function printout($str) {
        if ($files.length > 1) {
            echo "${file}:$str"
        } else {
            #echo "${file}:$str"
            echo "$str"
        }
    }
