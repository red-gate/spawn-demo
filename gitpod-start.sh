#!/bin/bash

echo 'You must authenticate to the Spawn service to run this example app'
echo ''
echo 'Open the URL presented below and follow the instructions to authenticate'
echo ''
spawnctl auth

source .env
source spawn.sh

validateImagesExist
setupContainers
migrateDatabases

echo ''
echo 'Building .NET app...'

dotnet build ./api/Spawn.Demo.WebApi

echo ''
echo 'Restoring frontend dependencies...'

pushd ./client
yarn
popd

echo 'Environment set up successfully!'
echo 'In two separate terminals run:

  dotnet run --project ./api/Spawn.Demo.WebApi

  cd client && REACT_APP_SPAWN_DEMO_ENDPOINT=$(gp url 5050) yarn start
  
'