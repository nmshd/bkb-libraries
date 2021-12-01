#!/bin/bash

set -x
set -e

root_dir=$(pwd)

./.ci/find_changed_projects.sh | tee /dev/stderr | while read library_name; do 
    test_project_path="${root_dir}/${library_name}/${library_name}.Tests"

    if [ -e "$test_project_path" ]; then # check if test project exists
        cd "${test_project_path}"
        dotnet test --no-build --no-restore
    else
        echo "no tests found"
    fi 
done

#--logger "junit;LogFilePath=test-results/results.xml"
