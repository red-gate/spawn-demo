#!/bin/bash

set -e

function logSpawnMessage() {
    GREEN='\033[0;32m'
    NC='\033[0m'
    printf "🛸  ${GREEN}$1${NC}\n"
}

TEST_TO_RUN=$1
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

todoContainerName=""
accountContainerName=""
dotnetLogFile="$DIR/dotnetlog.txt"


pushd $DIR

yarn

popd

echo "Starting '$TEST_TO_RUN' tests..."

function cleanup {
  logSpawnMessage "Deleting existing 'Todo' Spawn data container '$todoContainerName'"
  spawnctl delete data-container $todoContainerName -q > /dev/null
  logSpawnMessage "'$todoContainerName' deleted successfully"

  logSpawnMessage "Deleting existing 'Account' Spawn data container '$accountContainerName'"
  spawnctl delete data-container $accountContainerName -q > /dev/null
  logSpawnMessage "'$accountContainerName' deleted successfully"

  rm -rf $dotnetLogFile

  if [[ -z "${IN_CI}" ]]; then
    echo "Not running in CI, so attempting to kill jobs..."
    jobs -p | while read pid; do kill -15 -- -$(ps -o pgid= $pid | grep -o [0-9]*); done
    echo "An access token was generated for this test - run 'spawnctl get access-tokens' view it and delete it manually"
  fi
}

trap cleanup EXIT

logSpawnMessage "Creating 'Todo' Spawn data container from image '${SPAWN_TODO_IMAGE_NAME}:${TAG}'"
todoContainerName=$(spawnctl create data-container --image ${SPAWN_TODO_IMAGE_NAME}:${TAG} -q)
todoJson=$(spawnctl get data-container $todoContainerName -o json)
todoHost=$(echo $todoJson | jq -r '.host')
todoPort=$(echo $todoJson | jq -r '.port')
logSpawnMessage "Successfully created Spawn data container '$todoContainerName'"

echo

logSpawnMessage "Creating 'Account' Spawn data container from image '${SPAWN_ACCOUNT_IMAGE_NAME}:${TAG}'"
accountContainerName=$(spawnctl create data-container --image ${SPAWN_ACCOUNT_IMAGE_NAME}:${TAG} -q)
accountJson=$(spawnctl get data-container $accountContainerName -o json)
accountHost=$(echo $accountJson | jq -r '.host')
accountPort=$(echo $accountJson | jq -r '.port')
logSpawnMessage "Successfully created Spawn data container '$accountContainerName'"

echo

pushd api/Spawn.Demo.WebApi > /dev/null

appSettingsFilePath=$DIR/../api/Spawn.Demo.WebApi/appsettings.Development.Database.json

logSpawnMessage "Updating '$appSettingsFilePath' with data container connection strings"

todoDataContainerJson=$(spawnctl get data-container $todoContainerName -o json)
accountDataContainerJson=$(spawnctl get data-container $accountContainerName -o json)

todoPort=$(echo $todoDataContainerJson | jq -r .port)
todoHost=$(echo $todoDataContainerJson | jq -r .host)
todoPassword=$(echo $todoDataContainerJson | jq -r .password)
todoUser=$(echo $todoDataContainerJson | jq -r .user)

accountPort=$(echo $accountDataContainerJson | jq -r .port)
accountHost=$(echo $accountDataContainerJson | jq -r .host)
accountPassword=$(echo $accountDataContainerJson | jq -r .password)
accountUser=$(echo $accountDataContainerJson | jq -r .user)

todoConnString="Host=$todoHost;Port=$todoPort;Database=spawndemotodo;User Id=$todoUser;Password=$todoPassword;"
accountConnString="Server=$accountHost,$accountPort;Database=spawndemoaccount;User Id=$accountUser;Password=$accountPassword;"

jq -n "{\"TodoDatabaseConnectionString\": \"$todoConnString\", \"AccountDatabaseConnectionString\": \"$accountConnString\"}" > $appSettingsFilePath

logSpawnMessage "'$appSettingsFilePath' successfully updated with data container connection string"

dotnet run &> $dotnetLogFile &
popd > /dev/null

if ! $DIR/wait-for-it.sh -t 180 localhost:5050 > /dev/null 2>&1 ; then
    echo "ERROR: API was not available after 180 seconds."
    exit 1
fi

export todoContainerName
export accountContainerName

echo "Starting tests for '$TEST_TO_RUN'"

$DIR/node_modules/.bin/mocha --grep $TEST_TO_RUN || :
MOCHA_EXIT_CODE=$?

echo "Test run complete."

if [[ ! $MOCHA_EXIT_CODE ]]; then
  echo "Non-zero mocha exit code. Dotnet logs below:"
  cat $dotnetLogFile
fi

exit $MOCHA_EXIT_CODE