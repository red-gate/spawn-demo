#!/bin/bash
RED='\033[0;31m'
NC='\033[0m' # No Color
if ! spawnctl get data-images > /dev/null 2>&1 ; then
  echo -e "$RED You must authenticate to the Spawn service before using this codespace environment $NC"
  echo ''
  echo -e "$RED Open the https://spawn.cc URL and follow the instructions to authenticate and create an access token $NC"
  echo -e "$RED Then create an access token and add it to your github codespace secrets as SPAWNCTL_ACCESS_TOKEN $NC"
  echo ''

  exit 1
fi

source .env
source spawn.sh

validateImagesExist
setupContainers
migrateDatabases

echo 'Codespace environment set up successfully!'

exit
