# !/bin/bash

set -e

function logSpawnMessage() {
    GREEN='\033[0;32m'
    NC='\033[0m'
    printf "🛸  ${GREEN}$1${NC}\n"
}

function cleanupContainers() {
    logSpawnMessage "Cleaning up Spawn data containers for data image '$1'"
    imageName=$(spawnctl get data-image $1 | awk 'NR>1 {print $2}')

    if ! [[ -z "${imageName}" ]]; then
        containerNames=$(spawnctl get data-containers | grep $imageName | awk '{print $2}')
    
        for containerName in $containerNames
        do
            logSpawnMessage "Deleting Spawn data container '$containerName'"
            spawnctl delete data-container $containerName -q > /dev/null
        done
    else
        logSpawnMessage "Could not find Spawn data image '$1'"
    fi
}

function cleanupImage() {
  imageName=$(spawnctl get data-image $1 | awk 'NR>1 {print $2}')
  if [[ -z "${imageName}" ]]; then
    logSpawnMessage "Could not find Spawn data image '$1'"
    return
  fi

  cleanupContainers $1
  logSpawnMessage "Deleting Spawn data image '$1'"
  spawnctl delete data-image $1 -q > /dev/null || true
}