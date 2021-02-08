#!/bin/bash

if ! [ -x "$(command -v jq)" ]; then
  echo 'Error: jq is not installed. Install it from here: https://stedolan.github.io/jq/' >&2
  exit 1
fi

if ! [ -x "$(command -v yarn)" ]; then
  echo 'Error: yarn is not installed. Install it from here: https://yarnpkg.com/en/' >&2
  exit 1
fi

if ! [ -x "$(command -v node)" ]; then
  echo 'Error: node is not installed. Install it from here: https://nodejs.org/en/' >&2
  exit 1
fi

if ! [ -x "$(command -v dotnet)" ]; then
  echo 'Error: dotnet is not installed. Install it from here: https://dotnet.microsoft.com/download' >&2
  exit 1
fi

if ! [ -x "$(command -v spawnctl)" ]; then
  echo 'Error: spawnctl is not installed. Install it from here: https://run.spawn.cc/' >&2
  exit 1
fi

if ! [ -x "$(command -v docker)" ] && ! [ -x "$(command -v flyway)" ]; then
  echo 'Error: Docker or Flyway is not installed.
       You must have one of these installed.
       Install Docker here: https://www.docker.com/get-started
       Install Flyway here: https://flywaydb.org/download/' >&2
  exit 1
fi

echo 'All prerequisites installed and available.'
echo
echo