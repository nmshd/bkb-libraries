#!/bin/bash

set -x
set -e

root_dir=$(pwd)

./.ci/find_changed_projects.sh | tee /dev/stderr | while read project_name
do 
    cd "$root_dir/$project_name/$project_name"
    dotnet restore
    dotnet build --configuration Release --no-restore
done
