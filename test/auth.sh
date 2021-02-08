#!/bin/bash

set -e

if [[ -z "${IN_CI}" ]]; then
  echo "🛸 Generating temporary spawn access token"
  accessTokenPrefixLength=$(echo -n "Access token generated: " | wc -c)
  accessToken=$(spawnctl create access-token --purpose="temporary access token for testing locally")
  export SPAWN_ACCESS_TOKEN=${accessToken:$accessTokenPrefixLength}
fi