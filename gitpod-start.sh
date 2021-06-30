#!/bin/bash

echo 'You must authenticate to the Spawn service to run this example app'
echo ''
echo 'Open the URL presented below and follow the instructions to authenticate'
echo ''
if ! spawnctl auth || ! spawnctl get data-images ; then
  RED='\033[0;31m'
  NC='\033[0m' # No Color
  echo ''
  echo -e "${RED}Error authenticating to Spawn.${NC}"
  echo ''
  echo -e "${RED}Please ensure you've already signed up to Spawn by visiting https://app.spawn.cc/login${NC}"
  echo ''
  echo -e "${RED}Then re-run ./gitpod-start.sh${NC}"
  exit 1
fi


source .env
source spawn.sh

validateImagesExist
setupContainers
migrateDatabases

echo 'Environment set up successfully!'
gp sync-done envsetup
exit