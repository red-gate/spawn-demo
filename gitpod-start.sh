#!/bin/bash

if ! spawnctl get data-images > /dev/null 2>&1 ; then
  RED='\033[0;31m'
  NC='\033[0m' # No Color
  echo -e "${RED}You must authenticate to the Spawn service to run this example app${NC}"
  echo ''
  echo -e "${RED}Open the URL presented below and follow the instructions to authenticate${NC}"
  echo ''

  if ! spawnctl auth ; then
    echo -e "${RED}Unexpected error authenticating to Spawn.${NC}"
    echo -e "${RED}Please re-run ./gitpod-start.sh "
    exit 1
  fi

  if ! spawnctl get data-images > /dev/null 2>&1; then
    echo ''
    echo -e "Creating Spawn account - please follow the prompts below."
    echo ''
    if ! spawnctl onboard ; then
      echo -e "${RED}Onboarding failed.${NC}"
      echo -e "${RED}Please re-run ./gitpod-start.sh or contact spawn@red-gate.com for assistance."
      exit 1
    fi
    if ! spawnctl get data-images > /dev/null 2>&1; then
        # check we can now get images after onboard
        echo -e "${RED}Onboarding failed.${NC}"
        echo -e "${RED}Please re-run ./gitpod-start.sh or contact spawn@red-gate.com for assistance."
        exit 1
    fi
  fi
fi

source .env
source spawn.sh

validateImagesExist
setupContainers
migrateDatabases

echo 'Environment set up successfully!'
gp sync-done envsetup
exit