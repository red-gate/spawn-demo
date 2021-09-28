$env:SPAWN_ACCOUNT_IMAGE_NAME="demo-account:latest"
$env:SPAWN_TODO_IMAGE_NAME="demo-todo:latest"

function Get-GitBranchName {
  $branchName=((git rev-parse --abbrev-ref HEAD).Trim() -replace "\W")
  return $branchName
}

function Get-DataContainerName {
  param (
    [Parameter(Mandatory=$true)]
    [string]$database
  )

  $branchName=Get-GitBranchName

  switch ($database) 
  {
    "account" {
      $imageWithoutTag=$env:SPAWN_ACCOUNT_IMAGE_NAME.SubString(0, $env:SPAWN_ACCOUNT_IMAGE_NAME.IndexOf(":"))
      return "$imageWithoutTag-$branchName"
    }
    "todo" {
      $imageWithoutTag=$env:SPAWN_TODO_IMAGE_NAME.SubString(0, $env:SPAWN_TODO_IMAGE_NAME.IndexOf(":"))
      return "$imageWithoutTag-$branchName"
    }
  }
}

function Write-SpawnMessage() {
  param (
    [Parameter(Mandatory=$true)]
    [string]$Message,
    [bool]$Exit
  )
  if ($true -eq $exit) {
    throw "$message"
  }
  Write-Host -ForegroundColor Green "$message"
}

function Assert-DataImagesExist() {
  if ($null -eq $env:SPAWN_TODO_IMAGE_NAME) {
    Write-SpawnMessage -Exit $true "No spawn 'Todo' database image specified in environment variable SPAWN_TODO_IMAGE_NAME. Please specify an image id."
  }
  if ($null -eq $env:SPAWN_ACCOUNT_IMAGE_NAME) {
    Write-SpawnMessage -Exit $true "No spawn 'Account' database image specified in environment variable SPAWN_ACCOUNT_IMAGE_NAME. Please specify an image id."
  }

  Invoke-Expression -Command "spawnctl get data-image $env:SPAWN_TODO_IMAGE_NAME 2>&1" | Out-Null
  if ($LASTEXITCODE -ne 0) {
    Write-SpawnMessage -Exit $true "Could not find spawn image with id '$env:SPAWN_TODO_IMAGE_NAME'. Please ensure you have created the image."
  }

  Invoke-Expression -Command "spawnctl get data-image $env:SPAWN_ACCOUNT_IMAGE_NAME 2>&1" | Out-Null
  if ($LASTEXITCODE -ne 0) {
    Write-SpawnMessage -Exit $true "Could not find spawn image with id '$env:SPAWN_ACCOUNT_IMAGE_NAME'. Please ensure you have created the image."
  }
}

function Assert-DataContainersExist() {
  param(
    [Parameter(Mandatory=$true)]
    [string]$TodoContainer,
    [Parameter(Mandatory=$true)]
    [string]$AccountContainer
  )
  Write-SpawnMessage "Checking if Spawn containers already exist"

  Invoke-Expression -Command "spawnctl get data-container $TodoContainer 2>&1" | Out-Null
  if ($LASTEXITCODE -ne 0) {
    return $false
  }
  
  Invoke-Expression -Command "spawnctl get data-container $AccountContainer 2>&1" | Out-Null
  if ($LASTEXITCODE -ne 0) {
    return $false
  }

  return $true
}

function Edit-AppSettingsDatabaseConnectionStrings () {
  param(
    [Parameter(Mandatory=$true)]
    [string]$todoContainerName,
    [Parameter(Mandatory=$true)]
    [string]$accountcontainerName
  )


  $appSettingsFilePath=(Join-Path $PSScriptRoot "/api/Spawn.Demo.WebApi/appsettings.Development.Database.json")
  Write-SpawnMessage "Updating '$appSettingsFilePath' with data container connection strings"

  $todoContainerDetails=(Invoke-Expression -Command "spawnctl get data-container -o json $todoContainerName" | ConvertFrom-Json)
  $accountContainerDetails=(Invoke-Expression -Command "spawnctl get data-container -o json $accountContainerName" | ConvertFrom-Json)

  $todoHost=$todoContainerDetails.Host
  $todoPort=$todoContainerDetails.Port
  $todoUser=$todoContainerDetails.User
  $todoPassword=$todoContainerDetails.Password

  $accountHost=$accountContainerDetails.Host
  $accountPort=$accountContainerDetails.Port
  $accountUser=$accountContainerDetails.User
  $accountPassword=$accountContainerDetails.Password

  $todoConnString="Host=$todoHost;Port=$todoPort;Database=spawndemotodo;User Id=$todoUser;Password=$todoPassword;"
  $accountConnString="Server=$accountHost,$accountPort;Database=spawndemoaccount;User Id=$accountUser;Password=$accountPassword;"

  $connStringObj=[PSCustomObject]@{
    TodoDatabaseConnectionString = $todoConnString
    AccountDatabaseConnectionString = $accountConnString
  }

  $json=($connStringObj | ConvertTo-Json)

  Set-Content -Path $appSettingsFilePath -Value $json
}

function New-DataContainers() {
  $todoContainerName=(Get-DataContainerName "todo")
  $accountContainerName=(Get-DataContainerName "account")

  if (((Assert-DataContainersExist $todoContainerName $accountContainerName) -eq $true) -and ($null -eq $env:NEW_CONTAINERS)) {
    Write-SpawnMessage "Containers found - reusing existing Spawn containers"
  } else {
    Write-SpawnMessage "No containers found - creating new Spawn containers"
    Write-Host

    Write-SpawnMessage "Creating 'Todo' Spawn container from image '$env:SPAWN_TODO_IMAGE_NAME'"
    Invoke-Expression -Command "spawnctl create data-container --image $env:SPAWN_TODO_IMAGE_NAME --name '$todoContainerName' -q" | Out-Null
    Write-SpawnMessage "Spawn 'Todo' container '$todoContainerName' created from image '$SPAWN_TODO_IMAGE_NAME'"
    Write-Host

    Write-SpawnMessage "Creating 'Account' Spawn container from image '$env:SPAWN_ACCOUNT_IMAGE_NAME'"
    Invoke-Expression -Command "spawnctl create data-container --image $env:SPAWN_ACCOUNT_IMAGE_NAME --name '$accountContainerName' -q" | Out-Null
    Write-SpawnMessage "Spawn 'Account' container '$accountContainerName' created from image '$SPAWN_ACCOUNT_IMAGE_NAME'"
    Write-Host
  }

  Edit-AppSettingsDatabaseConnectionStrings $todoContainerName $accountContainerName

  Write-Host
  Write-Host

  Write-SpawnMessage "Successfully provisioned Spawn containers. Ready to start app"
}

function Sync-DatabasesWithMigrationScripts {
  $todoContainerName=(Get-DataContainerName "todo")
  $accountContainerName=(Get-DataContainerName "account")

  $todoContainerDetails=(Invoke-Expression -Command "spawnctl get data-container -o json $todoContainerName" | ConvertFrom-Json)
  $accountContainerDetails=(Invoke-Expression -Command "spawnctl get data-container -o json $accountContainerName" | ConvertFrom-Json)

  $todoHost=$todoContainerDetails.Host
  $todoPort=$todoContainerDetails.Port
  $todoUser=$todoContainerDetails.User
  $todoPassword=$todoContainerDetails.Password

  $accountHost=$accountContainerDetails.Host
  $accountPort=$accountContainerDetails.Port
  $accountUser=$accountContainerDetails.User
  $accountPassword=$accountContainerDetails.Password

  .\migrate-db.ps1 $todoHost $todoPort $todoUser $todoPassword $accountHost $accountPort $accountUser $accountPassword
}

New-DataContainers
Sync-DatabasesWithMigrationScripts