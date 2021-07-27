#!/bin/bash
set -x

echo "building api..."
echo $PWD
echo "container workspace folder ${PWD} ..."

cd ${PWD}/api/Spawn.Demo.WebApi
dotnet build
cd -

echo "building client..."
cd ${PWD}/client
yarn
cd -
echo "Codespace environment build successfully!"

exit