$buildid = $args[0]
$prerelease = $args[1]

docker build . -t build-image -f Dockerfile --build-arg CI_BUILDID=$buildid --build-arg CI_PRERELEASE=$prerelease
docker create --name build-container build-image
docker cp build-container:./app ./out
docker rm -fv build-container