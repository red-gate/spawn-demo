#!/bin/bash

set -e

function logSpawnMessage() {
    GREEN='\033[0;32m'
    NC='\033[0m'
    printf "🛸  ${GREEN}$1${NC}\n"
}

TEST_TO_RUN=$1

if [[ "$TEST_TO_RUN" = "" ]]; then
    echo "Error: No test to run was specified. You must specify a test to run by passing the tests to run as the first argument"
    exit 1
fi

if [[ -z $ACCOUNT_TEST_SPAWN_DATA_IMAGE_NAME ]]; then
    logSpawnMessage "Error: you must specify a name for the account spawn data image for this test by setting the 'ACCOUNT_TEST_SPAWN_DATA_IMAGE_NAME' environment variable"
    exit 1
fi

if [[ -z $TODO_TEST_SPAWN_DATA_IMAGE_NAME ]]; then
    logSpawnMessage "Error: you must specify a name for the todo spawn data image for this test by setting the 'TODO_TEST_SPAWN_DATA_IMAGE_NAME' environment variable"
    exit 1
fi

if [[ -z $TAG ]]; then
    logSpawnMessage "Error: you must specify the tag that was used for the test data images by setting the 'TAG' environment variable"
    exit 1
fi

if [[ -z $ACCOUNT_SPAWN_DATA_CONTAINER_NAME ]]; then
    logSpawnMessage "Error: you must specify a name for the account spawn data container for this test by setting the 'ACCOUNT_SPAWN_DATA_CONTAINER_NAME' environment variable"
    exit 1
fi

if [[ -z $TODO_SPAWN_DATA_CONTAINER_NAME ]]; then
    logSpawnMessage "Error: you must specify a name for the account spawn data container for this test by setting the 'TODO_SPAWN_DATA_CONTAINER_NAME' environment variable"
    exit 1
fi

TEST_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

source $TEST_DIR/../spawn.sh
export SPAWN_ACCOUNT_IMAGE_NAME="${ACCOUNT_TEST_SPAWN_DATA_IMAGE_NAME}:${TAG}"
export SPAWN_TODO_IMAGE_NAME="${TODO_TEST_SPAWN_DATA_IMAGE_NAME}:${TAG}"
export SPAWN_ACCOUNT_CONTAINER_NAME_OVERRIDE=$ACCOUNT_SPAWN_DATA_CONTAINER_NAME
export SPAWN_TODO_CONTAINER_NAME_OVERRIDE=$TODO_SPAWN_DATA_CONTAINER_NAME
validateImagesExist
setupContainers
disablePooling=true
updateDatabaseAppSettings $SPAWN_TODO_CONTAINER_NAME_OVERRIDE $SPAWN_ACCOUNT_CONTAINER_NAME_OVERRIDE $disablePooling

echo "Starting the API as a docker container..."

apiContainerId=$(docker run -p 5050:8080 -d -v $TEST_DIR/../api/Spawn.Demo.WebApi/appsettings.Development.Database.json:/app/appsettings.Development.Database.json redgatefoundry/spawn-demo-api)

if ! $TEST_DIR/wait-for-it.sh -t 180 localhost:5050 > /dev/null 2>&1 ; then
    echo "ERROR: API was not available after 180 seconds."
    exit 1
fi

echo "Starting tests for '$TEST_TO_RUN'"

pushd $TEST_DIR

echo "Installing test dependencies..."
yarn
popd

export todoContainerName=$TODO_SPAWN_DATA_CONTAINER_NAME
export accountContainerName=$ACCOUNT_SPAWN_DATA_CONTAINER_NAME

$TEST_DIR/node_modules/.bin/mocha --grep $TEST_TO_RUN || :
MOCHA_EXIT_CODE=$?

echo "Test run complete."

if [[ $MOCHA_EXIT_CODE != 0 ]]; then
  echo "Non-zero mocha exit code. Dotnet logs below:"
  docker logs $apiContainerId
fi

echo "Cleaning up"

spawnctl delete data-container -q $ACCOUNT_SPAWN_DATA_CONTAINER_NAME
spawnctl delete data-container -q $TODO_SPAWN_DATA_CONTAINER_NAME
docker rm -f $apiContainerId

exit $MOCHA_EXIT_CODE