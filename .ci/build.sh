#!/bin/bash

set -x
set -e

./.ci/find_changed_projects.sh | tee /dev/stderr | while read proj
do 
    project_path="$proj/$proj"
    dotnet restore $project_path
    dotnet build $project_path --no-restore
done
