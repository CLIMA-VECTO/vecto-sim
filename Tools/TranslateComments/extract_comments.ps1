## 0. Transform all ACII files to UTF
$basepath = '../../VECTO';
ls "*/*/*/*.vb","*/*/*.vb","*/*.vb","*.vb" | 
    %{
        $f=$_; 
        if ((Get-FileEncoding $f) -ne UTF8) {
            echo "Re-encoding $f";
            (cat $f) | 
            Out-File -FilePath $f -Encoding UTF8
        } else {
            echo "Skipping $f";
        }
    }
    
    

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

## 5.a. Merge translated comment-lines with original ones.
##
$r=for($i=0; $i -lt $coms.length; $i++) {
    $cline = $coms[$i];
    $fline = $from[$i];
    $tline = $to[$i];
    $tline = $tline.trim();
    if ($cline.startsWith('>>> ')) {
        echo "$cline"
    } else {
        $m = [regex]::Matches($cline, "(\d+):\s*'(C )?\W*(\w.*)$", "IgnoreCase");
        if ($m[0]) {
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
 
## 5.b Manually remove empty filepaths (those without translated lines)


## 6.a comments2-orig.txt: Created by runing the above code slightly modified
##   so as to remove those non-translated lines from the verbatim-comments, and then 
## 6.b  Manually remove empty filepaths (those without translated lines)
        


## 7. PATCH files
function matchTransLine($line) {
    [hashtable]$res = @{};
    
    $m = [regex]::Matches($line, "^(\d+):(.*)");
    if ($m[0]) {
        $mm = $m[0];
        $res.lnum = $mm.Groups[1].Value;
        $res.line = $mm.Groups[2].Value;
        return $res;
    } else {
        echo "Bad comment line: `n$line"
        return $Null;
    }
}

$coms=cat comments2-orig.txt
filter Patch-Comments() {
BEGIN {
    $basepath = '../../VECTO';
    $i = -1;
    $file = $Null;
    $isFileOK = $true;
}
PROCESS {
    $i++;
    if ($_.startsWith('>>> ')) {
        if ($file) {
            if ($isFileOK) {
                echo $file | Out-File -FilePath $fname -Encoding UTF8
                echo "Merged $fname";
            } else {
                echo "FAILED $fname";
            }
        }
        $isFileOK = $true;
        
        $fname = "$basepath/" + $_.Substring(4);
        echo "Merging: ${fname}";
        $file = cat "$fname";
    } else {
        $m = matchTransLine($coms[$i]);
        if (!$m) {
            $isFileOK = $false;
            return;
        }
        $expline = $m.line;
        $explnum = $m.lnum;

        $m = matchTransLine($_);
        if (!$m) {
            $isFileOK = $false;
            return;
        }
        $trnline = $m.line;
        $trnlnum = $m.lnum;

        if ($explnum -ne $trnlnum) {
            $isFileOK = $false;
            echo "Mismatch in line-nums:`n  EXP($explnum):$expline`n  TRN($trnlnum):$trnline"
        } else {
            $orgline = $file[($trnlnum - 1)];
            
            if ($orgline -ne $expline) {
                $isFileOK = $false;
                echo "Unexpected line $lnum:`n  ORG:$orgline`n  EXP:$expline`n  TRN:$trnline"
            } else {
                $file[($trnlnum - 1)] = $trnline;
            }
        }
    }
}## End proc-loop

END {
    if ($file) {
        if ($isFileOK) {
            echo $file | Out-File -FilePath $fname -Encoding UTF8
            echo "Merged $fname";
        } else {
            echo "FAILED $fname";
        }
    }
}
}
cat comments2-trans.txt | Patch-Comments


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

