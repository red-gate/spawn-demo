#!/bin/bash

set -e

function logSpawnMessage() {
    GREEN='\033[0;32m'
    NC='\033[0m'
    printf "🛸  ${GREEN}$1${NC}\n"
}

if [[ -z $ACCOUNT_TEST_SPAWN_DATA_IMAGE_NAME ]]; then
    logSpawnMessage "Error: you must specify a name for the account spawn data image by setting the 'ACCOUNT_TEST_SPAWN_DATA_IMAGE_NAME' environment variable"
    exit 1
fi

if [[ -z $TODO_TEST_SPAWN_DATA_IMAGE_NAME ]]; then
    logSpawnMessage "Error: you must specify a name for the todo spawn data image by setting the 'TODO_TEST_SPAWN_DATA_IMAGE_NAME' environment variable"
    exit 1
fi

if [[ -z $TAG ]]; then
    logSpawnMessage "Error: you must specify a tag to use for tagging the created data images by setting the 'TAG' environment variable"
    exit 1
fi

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

logSpawnMessage "Creating 'Todo' Spawn data image for testing with name: '${TODO_TEST_SPAWN_DATA_IMAGE_NAME}:${TAG}'"
spawnctl create data-image -f ./database/todo/spawn/test.yaml --name $TODO_TEST_SPAWN_DATA_IMAGE_NAME --tag $TAG -q > /dev/null
logSpawnMessage "Successfully created Spawn data image '${TODO_TEST_SPAWN_DATA_IMAGE_NAME}:${TAG}'"

logSpawnMessage "Creating 'Account' Spawn data image for testing with name: '${ACCOUNT_TEST_SPAWN_DATA_IMAGE_NAME}:${TAG}'"
spawnctl create data-image -f ./database/account/spawn/test.yaml --name $ACCOUNT_TEST_SPAWN_DATA_IMAGE_NAME --tag $TAG -q > /dev/null
logSpawnMessage "Successfully created Spawn data image '${ACCOUNT_TEST_SPAWN_DATA_IMAGE_NAME}:${TAG}'"