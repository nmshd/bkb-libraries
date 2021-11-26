#!/bin/bash

set -x
set -e

./.ci/find_changed_projects.sh | tee /dev/stderr | while read proj
do 
    proj_path="$proj/$proj"
    dotnet restore $proj_path
    dotnet build $proj_path --no-restore
done
