#!/bin/bash

set -e

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

accountScriptsDir=$DIR/database/account/sql/
todoScriptsDir=$DIR/database/todo/sql/

accountHost=$1
accountPort=$2
accountUser=$3
accountPassword=$4

shift 4

todoHost=$1
todoPort=$2
todoUser=$3
todoPassword=$4

shift 4

function logFlywayMessage() {
    CYAN='\033[0;36m'
    NC='\033[0m'
    printf "📝  ${CYAN}$1${NC}\n"
}

function flywayInstalled() {
    command -v flyway > /dev/null
    return $?
}

function dockerInstalled() {
    command -v docker > /dev/null
    return $?
}

LOCAL_BINARY=1
DOCKER_BINARY=2
function getMode() {
    flywayInstalled
    isFlywayInstalled=$?

    if [[ $isFlywayInstalled == 0 ]]; then
        return $LOCAL_BINARY
    fi

    return $DOCKER_BINARY
}

function getBinaryToExecute() {
    scriptsPath=$1

    getMode
    mode=$?
    if [[ $mode == $LOCAL_BINARY ]]; then
        echo "flyway -locations=filesystem:$scriptsPath"
    else
        echo "docker run --net=host --rm -v $scriptsPath:/flyway/sql boxfuse/flyway"
    fi
}

function translateHost() {
    if [ "$(uname -s)" = "Darwin" ]; then
        # running in MacOS, localhost translation needs to happen
        if [[ "$1" == "localhost" || "$1" == "127.0.0.1" ]]; then
            echo "host.docker.internal"
        else
            echo "$1"
        fi
    else
        # Linux does not need localhost translation
        echo "$1"   
    fi    
}

set +e
if ! flywayInstalled && ! dockerInstalled; then
  logFlywayMessage "You must have Flyway or Docker installed to be able to migrate the database"
  exit 1
fi

getMode
mode=$?
if [[ $mode == $DOCKER_BINARY ]]; then
    accountHost=$(translateHost $accountHost)
    todoHost=$(translateHost $todoHost)
    docker pull boxfuse/flyway > /dev/null 2>&1
fi
set -e

echo
echo
logFlywayMessage "Starting migration of 'Account' database"
flyway=$(getBinaryToExecute $accountScriptsDir)
eval "$flyway migrate -url=\"jdbc:sqlserver://$accountHost:$accountPort;databaseName=spawndemoaccount\" -user=\"$accountUser\" -password=\"$accountPassword\" -mixed=true"

echo
echo
logFlywayMessage "Starting migration of 'Todo' database"
flyway=$(getBinaryToExecute $todoScriptsDir)
eval "$flyway migrate -url=\"jdbc:postgresql://$todoHost:$todoPort/spawndemotodo\" -user=\"$todoUser\" -password=\"$todoPassword\""

echo
echo

logFlywayMessage "Finished migrating both databases"