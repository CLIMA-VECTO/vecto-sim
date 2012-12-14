﻿
## 1. Gather all single-line comments.
##
filter extract-comments {
    if ($_.Path -ne $last.Path) {
        $file = $_.Filename;
        echo ">>> $file";
    }
    $lno = $_.LineNumber;
    $line = $_.Line;
    echo "${lno}:$line";
    $last = $_
}
Select-String -Path *.vb,*/*vb -Pattern "^\s*'.*[a-z]" -Encoding UTF8 |
    extract-comments | 
    Out-File -Encoding UTF8 ../comments.txt


## 2. (MANUAL)Inspect and discard any comments .


## 3. Isolate text to translate.
##
filter isolate-text() {
    if ($_ -match "\d+:\s*'(C )?\W*(\w.*)$") {
        echo $Matches[2];
    } else {
        echo "";
    }
}
cat comments2.txt | 
    isolate-text |
    Out-File -Encoding UTF8  ../translate_from.txt


## 4a. (MANUAL)Inspect translation and go back to 1 or 4a in case of problems.
## 5b. (MANUAL) Store translated-comments into ../translate_to.txt

$coms=cat comments2.txt
$from=cat translate_from.txt
$to=cat translate_to.txt
$coms.length, $from.length, $to.length



function isolate-untranslated($coms, $from, $to) {
    for($i=0; $i -lt $coms.length; $i++) {
        $cline = $coms[$i];
        $fline = $from[$i];
        $tline = $to[$i];
        $tline = $tline.trim();
        if ($cline.startsWith('>>> ')) {
            echo "$cline" | Out-File -Encoding UTF8  comments_untrans.txt -Append;
        } elseif ($tline -and !($tline.startsWith('@'))) {
            echo "$fline" | Out-File -Encoding UTF8  comments_untrans.txt -Append;
        }
    } 
}

## 5. Merge translated comment-lines with original ones.
##
function merge-translated($coms, $from, $to) {
$r=for($i=0; $i -lt $coms.length; $i++) {
        $cline = $coms[$i];
        $fline = $from[$i];
        $tline = $to[$i];
        $tline = $tline.trim();
        if ($cline.startsWith('>>> ')) {
            echo "$cline"
        } else {
            $m = [regex]::Matches($cline, "(\d+):\s*'(C )?\W*(\w.*)$", "IgnoreCase");
            if ($m) {
                $ccm = $m[0].Groups[3];
                $ocom = $ccm.Value;
                $ocomi = $ccm.Index;
                $ln = $m[0].Groups[1].Value;
            } else {
                $ocom = "";
                $ln = "<?>"
            }
            
            if ($ocom -ne $fline) {
                echo "Unmtached with Original(parsed) line ${ln}:`n  Parsed: ${ocom}`n  TrFrom: $fline";
                return;
            }
            
            if (!$tline) {
                #$nline = $cline;
                continue;
            } elseif ($tline.startsWith('@')) {
                $tline = $tline.Substring(1);
                $nline = $cline.Substring(0, $ocomi) + "$tline";
            } else {
                $nline = $cline.Substring(0, $ocomi) + "${fline} |@@| ${tline}";
            }
            echo "$nline"
        }
    } 
    
    Set-Content -Path comments2-trans.txt -Value $r -Encoding UTF8
}
 


        

## 7. PATCH files
$coms=cat comments2.txt
filter Patch-Comments() {
BEGIN {
    $basepath = '../../VECTO/';
    $i = -1;
}
PROCESS {
    $i++;
    if ($_.startsWith('>>> ')) {
        $file = $_.Substring(4);
        echo "Merging: $basepath$file";
    } else {
        $expline = $coms[$i];
        $m = [regex]::Matches($_, "(\d+):(.*)");
        if ($m) {
            $mm = $m[0];
            $lnum = $mm.Groups[1].Value;
            $newline = $mm.Groups[2].Value;
            
            if ($expline -ne $_) {
                echo "Unexpected line $lnum:`n  EXP:$expline`n  GOT:$_"
            }
        } else {
                echo "Bad comment line $lnum:`n  GOT:$_"
        }
    }
}## End proc-loop
}
cat comments2-trans.txt | Merge-Comments


## DONE




## OTHER
filter clusterize-comments {
    if ($_.Path -eq $last.Path) {
        if ($_.LineNumber -ne ($Last.LineNumber + 1)) {
            $lno = $_.LineNumber;
            echo "@@$lno";
        }
    } else {
        $file = $_.Filename;
        echo "--- $file";
        echo "+++ $file";
        $lno = $_.LineNumber;
        echo "@@$lno";
    }
    $line = $_.Line;
    echo " $line";
    $last = $_
}

filter ec {
    if ($_.Path -ne $last.Path) {
        $file = $_.Filename;
        echo ">>> $file";
    }
    $lno = $_.LineNumber;
    $line = $_.Line;
    echo "${lno}:$line";
    $last = $_
}

Select-String -Encoding Default -Path ../../*.vb,../../*/*vb -Pattern "^\s*'"|
    select Path,LineNumber,Line|
    Export-Clixml -Path ../comments.xml
    
    
## Invocted from with from within 'VectoSource/Vecto' folder.
Select-String -Encoding Default -Path ../../*.vb,../../*/*vb -Pattern "^\s*'.*[a-z]"|
    select -Property LineNumber,Line |
    Export-Csv -Encoding UTF8 -NoTypeInformation -Delimiter ';' -Path comments.csv

