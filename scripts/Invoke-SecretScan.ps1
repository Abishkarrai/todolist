$ErrorActionPreference = 'Stop'

function Get-StagedAddedLines {
    $diffOutput = git diff --cached --unified=0 --no-color --no-ext-diff

    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($diffOutput)) {
        return @()
    }

    return $diffOutput -split "`r?`n" |
        Where-Object { $_ -match '^\+(?!\+\+)' } |
        ForEach-Object { $_.Substring(1) }
}

function Get-SecretPatterns {
    return @(
        @{
            Name = 'MongoDB connection string with embedded credentials'
            Regex = 'mongodb(\+srv)?://(?!<)[^/\s:]+:(?!<)[^@\s]+@'
        },
        @{
            Name = 'Private key block'
            Regex = '-----BEGIN (RSA |EC |DSA |OPENSSH |PGP )?PRIVATE KEY-----'
        },
        @{
            Name = 'GitHub token'
            Regex = 'gh[pousr]_[A-Za-z0-9]{20,}'
        },
        @{
            Name = 'AWS access key'
            Regex = 'AKIA[0-9A-Z]{16}'
        },
        @{
            Name = 'Password assignment'
            Regex = '(?i)(password|pwd)\s*[:=]\s*["'']?(?!<|changeme|example|your-)[^;"'',\s]+'
        }
    )
}

function Find-PotentialSecrets {
    param(
        [string[]]$Lines
    )

    $findings = [System.Collections.Generic.List[string]]::new()
    $patterns = Get-SecretPatterns

    foreach ($line in $Lines) {
        foreach ($pattern in $patterns) {
            if ($line -match $pattern.Regex) {
                $findings.Add(('{0}: {1}' -f $pattern.Name, $line.Trim()))
                break
            }
        }
    }

    return $findings
}

function Invoke-GitleaksScan {
    $gitleaksCommand = Get-Command gitleaks -ErrorAction SilentlyContinue

    if ($null -eq $gitleaksCommand) {
        return $false
    }

    & $gitleaksCommand.Source protect --staged --redact --verbose
    exit $LASTEXITCODE
}

$repoRoot = git rev-parse --show-toplevel 2>$null

if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($repoRoot)) {
    Write-Error 'Unable to determine the git repository root for the secret scan hook.'
}

if (Invoke-GitleaksScan) {
    return
}

$stagedAddedLines = Get-StagedAddedLines

if ($stagedAddedLines.Count -eq 0) {
    exit 0
}

$potentialSecrets = Find-PotentialSecrets -Lines $stagedAddedLines

if ($potentialSecrets.Count -eq 0) {
    Write-Host 'Secret scan passed.'
    exit 0
}

Write-Host 'Commit blocked: potential secrets detected in staged changes.' -ForegroundColor Red
Write-Host ''

foreach ($finding in $potentialSecrets) {
    Write-Host "- $finding"
}

Write-Host ''
Write-Host 'Fix the staged content, or move the value into user-secrets or environment variables.' -ForegroundColor Yellow
Write-Host 'Install gitleaks and keep it on PATH for stronger secret scanning coverage.' -ForegroundColor Yellow
exit 1
