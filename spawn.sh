#!/bin/bash

set -e

export PATH=$HOME/.spawnctl/bin:$PATH

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

source $DIR/.env

function getGitBranchName(){
    if [[ "$OSTYPE" == "darwin"* ]]; then
        branchName=$(git rev-parse --abbrev-ref HEAD | tr -cd \[:alnum:\])
        echo $branchName
    else
        # workaround for dev containers... sometimes the .git folder isn't readable so `git rev-parse` won't work
        gitHEADContents=$(cat $DIR/.git/HEAD)
        branchName=$(echo $gitHEADContents | sed -e 's/^ref:\srefs\/heads\///' | tr -cd \[:alnum:\])
        echo $branchName
    fi    
}

GIT_BRANCH=$(getGitBranchName)

function logSpawnMessage() {
    GREEN='\033[0;32m'
    NC='\033[0m'
    printf "🛸  ${GREEN}$1${NC}\n"
}

function error() {
    logSpawnMessage "Error during spawn initialisation on line $1"
}

trap 'error $LINENO' ERR

function validateImagesExist() {
    if [[ -z $SPAWN_TODO_IMAGE_NAME ]]; then
        logSpawnMessage "No spawn 'Todo' database image specified in environment variable SPAWN_TODO_IMAGE_NAME. Please specify an image id."
        exit 1
    fi

    if [[ -z $SPAWN_ACCOUNT_IMAGE_NAME ]]; then
        logSpawnMessage "No spawn 'Account' database image specified in environment variable SPAWN_ACCOUNT_IMAGE_NAME. Please specify an image id."
        exit 1
    fi

    if ! spawnctl get data-image $SPAWN_TODO_IMAGE_NAME &> /dev/null ; then
        logSpawnMessage "Could not find spawn image with id '$SPAWN_TODO_IMAGE_NAME'. Please ensure you have created the image."
        exit 1
    fi

    if ! spawnctl get data-image $SPAWN_ACCOUNT_IMAGE_NAME &> /dev/null ; then
        logSpawnMessage "Could not find spawn image with id '$SPAWN_ACCOUNT_IMAGE_NAME'. Please ensure you have created the image."
        exit 1
    fi
}

function containersExist() {
    logSpawnMessage "Checking if Spawn containers already exist"

    if ! spawnctl get data-container "$1" &> /dev/null ; then
        return 1
    fi

    if ! spawnctl get data-container "$2" &> /dev/null ; then
        return 1
    fi

    return 0
}

function setupContainers() {
    todoContainerName="$(echo $SPAWN_TODO_IMAGE_NAME | cut -f1 -d":")-$GIT_BRANCH"
    accountContainerName="$(echo $SPAWN_ACCOUNT_IMAGE_NAME | cut -f1 -d":")-$GIT_BRANCH"

    set +e
    containersExist "$todoContainerName" "$accountContainerName"
    local doContainersExist=$?
    set -e

    if [[ -z $NEW_CONTAINERS ]] && [[ $doContainersExist == 0 ]]; then
        logSpawnMessage "Containers found - reusing existing Spawn containers"
    else
        logSpawnMessage "No containers found - creating new Spawn containers"

        echo

        logSpawnMessage "Creating 'Todo' Spawn container"
        spawnctl create data-container --image $SPAWN_TODO_IMAGE_NAME --name "$todoContainerName" -q > /dev/null
        logSpawnMessage "Spawn 'Todo' container '$todoContainerName' created from image '$SPAWN_TODO_IMAGE_NAME'"
        
        echo

        logSpawnMessage "Creating 'Account' Spawn container"
        spawnctl create data-container --image $SPAWN_ACCOUNT_IMAGE_NAME --name "$accountContainerName" -q > /dev/null
        logSpawnMessage "Spawn 'Account' container '$accountContainerName' created from image '$SPAWN_ACCOUNT_IMAGE_NAME'"
    fi

    updateDatabaseAppSettings "$todoContainerName" "$accountContainerName"

    echo
    echo

    migrateDatabases "$todoContainerName" "$accountContainerName"

    echo
    echo

    logSpawnMessage "Successfully provisioned Spawn containers. Ready to start app"
}

function migrateDatabases {
    todoDataContainerName=$1
    accountDataContainerName=$2

    todoDataContainerJson=$(spawnctl get data-container $todoDataContainerName -o json)
    accountDataContainerJson=$(spawnctl get data-container $accountDataContainerName -o json)

    todoPort=$(echo $todoDataContainerJson | jq -r .port)
    todoHost=$(echo $todoDataContainerJson | jq -r .host)
    todoUser=$(echo $todoDataContainerJson | jq -r .user)
    todoPassword=$(echo $todoDataContainerJson | jq -r .password)

    accountPort=$(echo $accountDataContainerJson | jq -r .port)
    accountHost=$(echo $accountDataContainerJson | jq -r .host)
    accountUser=$(echo $accountDataContainerJson | jq -r .user)
    accountPassword=$(echo $accountDataContainerJson | jq -r .password)

    $DIR/migrate-db.sh $accountHost $accountPort $accountUser $accountPassword $todoHost $todoPort $todoUser $todoPassword
}

function updateDatabaseAppSettings {
    todoDataContainerName=$1
    accountDataContainerName=$2

    appSettingsFilePath=$DIR/api/Spawn.Demo.WebApi/appsettings.Development.Database.json

    logSpawnMessage "Updating '$appSettingsFilePath' with data container connection strings"

    todoDataContainerJson=$(spawnctl get data-container $todoDataContainerName -o json)
    accountDataContainerJson=$(spawnctl get data-container $accountDataContainerName -o json)

    todoPort=$(echo $todoDataContainerJson | jq -r .port)
    todoHost=$(echo $todoDataContainerJson | jq -r .host)
    todoPassword=$(echo $todoDataContainerJson | jq -r .password)

    accountPort=$(echo $accountDataContainerJson | jq -r .port)
    accountHost=$(echo $accountDataContainerJson | jq -r .host)
    accountPassword=$(echo $accountDataContainerJson | jq -r .password)

    todoConnString="Host=$todoHost;Port=$todoPort;Database=spawndemotodo;User Id=spawn_demo_user;Password=$todoPassword;"
    accountConnString="Server=$accountHost,$accountPort;Database=spawndemoaccount;User Id=spawn_demo_user;Password=$accountPassword;"

    jq -n "{\"TodoDatabaseConnectionString\": \"$todoConnString\", \"AccountDatabaseConnectionString\": \"$accountConnString\"}" > $appSettingsFilePath

    logSpawnMessage "'$appSettingsFilePath' successfully updated with data container connection string"
}

validateImagesExist

setupContainers