#!/usr/bin/env bash

docker build . -t build-image -f Dockerfile --build-arg CI_BUILDID=$1 --build-arg CI_PRERELEASE=$2
docker create --name build-container build-image
docker cp build-container:./app/out ./out
docker rm -fv build-container