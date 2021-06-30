#!/bin/bash

if ! spawnctl get data-images > /dev/null 2>&1 ; then
  RED='\033[0;31m'
  NC='\033[0m' # No Color
  echo -e "${RED}You must authenticate to the Spawn service to run this example app${NC}"
  echo ''
  echo -e "${RED}Open the URL presented below and follow the instructions to authenticate${NC}"
  echo ''
  if ! spawnctl auth || ! spawnctl get data-images > /dev/null 2>&1 ; then
    echo ''
    echo -e "${RED}Error authenticating to Spawn.${NC}"
    echo ''
    echo -e "${RED}Please ensure you've already signed up to Spawn by running 'spawnctl onboard'${NC}"
    echo ''
    echo -e "${RED}Then re-run ./gitpod-start.sh${NC}"
    exit 1
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