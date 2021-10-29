#old script. need to update and put in psm1
#gives you a grep like cmd in windows shells
Function grep {
    Param(
        [Parameter(Position = 0)]
        [Alias("dir")]
        [String]$d,
        [String]$f,
        [String]$p,
        [Boolean]$r
    )
    
    if (![String]::IsNullOrEmpty($f)) {
        if ($null -eq (Get-ChildItem .\$f)) {
            Write-Host "File not found"
        }
        else {
            if (![String]::IsNullOrEmpty($p)) {
                $data = Get-Content (Get-ChildItem .\$f)
                $lines = $data.split("`n")
                if ($null -eq ($lines | ? { $_ -like "*$p*" })) {
                    Write-Host "Pattern not found in file"
                }
                else {
                    $lines | ? { $_ -like "*$p*" }
                }
            }
            else {
                (Get-ChildItem .\$f)
            }
        }
    }
    
    else {
        if (($null -eq $r) -or ($r -eq $false)) {
            Get-ChildItem $d | Select | ? { $_.Name -like "*$p*" } | Select @{n="File Size";Expression={"$([Math]::Round(($_.Length / 1KB),2)) KB"} }, LastWriteTime,FullName
        }
        else {
            Get-ChildItem $d -Recurse | Select | ? { $_.Name -like "*$p*" } | Select @{n="File Size";Expression={"$([Math]::Round(($_.Length / 1KB),2)) KB"} }, LastWriteTime,FullName
        } 
    }

}
