#!/bin/bash

param(
  [Parameter(Mandatory=$true)]
  [string]$todoHost,
  [Parameter(Mandatory=$true)]
  [string]$todoPort,
  [Parameter(Mandatory=$true)]
  [string]$todoUser,
  [Parameter(Mandatory=$true)]
  [string]$todoPassword,
  [Parameter(Mandatory=$true)]
  [string]$accountHost,
  [Parameter(Mandatory=$true)]
  [string]$accountPort,
  [Parameter(Mandatory=$true)]
  [string]$accountUser,
  [Parameter(Mandatory=$true)]
  [string]$accountPassword
)

$accountScriptsDir=(Join-Path $PSScriptRoot "/database/account/sql/")
$todoScriptsDir=(Join-Path $PSScriptRoot "/database/todo/sql/")

function Write-FlywayMessage() {
  param (
    [string]$Message,
    [bool]$Exit
  )
  if ($true -eq $exit) {
    throw "$message"
  }
  Write-Host -ForegroundColor Cyan "$message"
}

function Assert-FlywayInstalled() {
  if (Get-Command "flyway.exe" -ErrorAction SilentlyContinue) {
    return $true
  }
  return $false
}

function Assert-DockerInstalled() {
  if (Get-Command "docker" -ErrorAction SilentlyContinue) {
    return $true
  }
  return $false
}

function Get-FlywayExecutable() {
  param(
    [string]$scriptsPath
  )
  if (Assert-FlywayInstalled){
    return "flyway.exe -locations=filesystem:$scriptsPath"
  }
  return "docker run --net=host --rm -v $($scriptsPath):/flyway/sql flyway/flyway"
}

if (-not ((Assert-FlywayInstalled) -or (Assert-DockerInstalled))) {
  Write-FlywayMessage -Exit $true "You must have Flyway or Docker installed to be able to migrate the database"
}

Write-FlywayMessage "Starting migration of 'Account' database"
$FlywayCommand=(Get-FlywayExecutable $accountScriptsDir)
Invoke-Expression -Command "$FlywayCommand migrate -url=`"jdbc:sqlserver://$($accountHost):$($accountPort);databaseName=spawndemoaccount`" -user=`"$accountUser`" -password=`"$accountPassword`" -mixed=true"
Write-Host
Write-Host

Write-FlywayMessage "Starting migration of 'Todo' database"
$FlywayCommand=(Get-FlywayExecutable $todoScriptsDir)
Invoke-Expression -Command "$FlywayCommand migrate -url=`"jdbc:postgresql://$($todoHost):$($todoPort)/spawndemotodo`" -user=`"$todoUser`" -password=`"$todoPassword`" -mixed=true"
Write-Host
Write-Host

Write-FlywayMessage "Finished migrating both databases"