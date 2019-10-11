cd $PSScriptRoot

# Get SQL files in this directory
$sqlFiles = Get-ChildItem -Path .\ -Filter *.sql -File -Name | ForEach-Object { $_ }

foreach($sqlFile in $sqlFiles) {
    "Executing $PSScriptRoot\$sqlFile"
    Invoke-Sqlcmd -InputFile "$PSScriptRoot\$sqlFile" -ServerInstance $env:COMPUTERNAME
}

