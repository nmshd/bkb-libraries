#!/bin/bash

set -x
set -e

./.ci/find_changed_projects.sh | tee /dev/stderr | while read proj
do 
    cd "$proj/$proj"
    dotnet restore
    dotnet build --configuration Release --no-restore
done
