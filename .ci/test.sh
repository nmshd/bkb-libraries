#!/bin/bash

set -x
set -e


./.ci/find_changed_projects.sh | tee /dev/stderr | while read proj
do 
    test_project_path="${proj}/${proj}.Tests/${proj}.Tests.csproj"

    if [ -e "$test_project_path" ]; then # check if test project exists
        dotnet test --no-build --no-restore $test_project_path;
    else
        echo "no tests found"
    fi 
done

#--logger "junit;LogFilePath=test-results/results.xml"
