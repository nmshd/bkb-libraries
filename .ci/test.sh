#!/bin/bash

set -x
set -e

./.ci/find_changed_projects.sh | tee /dev/stderr | while read proj; do dotnet test "${proj}/${proj}.Tests"; done

#--logger "junit;LogFilePath=test-results/results.xml"
