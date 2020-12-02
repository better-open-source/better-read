#!/usr/bin/env bash

docker build . -t build-image -f src/BetterRead.Api/Dockerfile --build-arg CI_BUILDID=$1 --build-arg CI_PRERELEASE=$2
docker create --name build-container build-image
docker cp build-container:./app/out ./out
ls -R | grep ":$" | sed -e 's/:$//' -e 's/[^-][^\/]*\//--/g' -e 's/^/   /' -e 's/-/|/'
docker rm -fv build-container
