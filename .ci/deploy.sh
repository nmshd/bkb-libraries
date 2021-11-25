#!/bin/bash

set -x
set -e

./.ci/find_changed_projects.sh | while read proj; do 
    proj_path="$proj/$proj"
    dotnet restore $proj_path
    dotnet build $proj_path --no-restore
    dotnet pack $proj_path --no-restore --no-build
    dotnet nuget push **/*.nupkg -k $NUGET_PUBLISHER_API_KEY -s https://api.nuget.org/v3/index.json
done

