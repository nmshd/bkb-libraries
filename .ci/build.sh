#!/bin/bash

set -x
set -e

root_dir=$(pwd)

./.ci/find_changed_projects.sh | tee /dev/stderr | while read library_name
do 
    cd "$root_dir/${library_name}/${library_name}"
    dotnet restore
    dotnet build --configuration Release --no-restore
done
