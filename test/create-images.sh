#!/bin/bash

set -e

function logSpawnMessage() {
    GREEN='\033[0;32m'
    NC='\033[0m'
    printf "🛸  ${GREEN}$1${NC}\n"
}

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

source $DIR/auth.sh

logSpawnMessage "Creating 'Todo' Spawn data image for testing with name: '${SPAWN_TODO_IMAGE_NAME}:${TAG}'"
spawnctl create data-image -f ./database/todo/spawn/test.yaml --name $SPAWN_TODO_IMAGE_NAME --tag $TAG -q > /dev/null
logSpawnMessage "Successfully created Spawn data image '${SPAWN_TODO_IMAGE_NAME}:${TAG}'"

logSpawnMessage "Creating 'Account' Spawn data image for testing with name: '${SPAWN_ACCOUNT_IMAGE_NAME}:${TAG}'"
spawnctl create data-image -f ./database/account/spawn/test.yaml --name $SPAWN_ACCOUNT_IMAGE_NAME --tag $TAG -q > /dev/null
logSpawnMessage "Successfully created Spawn data image '${SPAWN_ACCOUNT_IMAGE_NAME}:${TAG}'"