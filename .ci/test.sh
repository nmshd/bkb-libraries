#!/bin/bash

set -x
set -e


./.ci/find_changed_projects.sh | tee /dev/stderr | while read proj
do 
    test_project_name="${proj}/${proj}.Tests/${proj}.Tests.csproj"
    
    if [ -e "$test_project_name" ]; then
        dotnet test $test_project_name;
    else
        echo "no tests found"
    fi 
done

#--logger "junit;LogFilePath=test-results/results.xml"
