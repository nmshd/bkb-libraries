#!/bin/bash
echo "x"
echo "y"
# git --no-pager diff --name-only HEAD^ | grep csproj\$ | while read -r line # find all csproj files in diff
# do
#     diff=$(git --no-pager diff -U0 --raw HEAD^ $line) # get diff for file

#     if [[ $diff =~ \<Version\>.*\<\/Version\> ]]; then # if file has a version tag
#         echo $line | cut -f1 -d"/" # return the filename
#     fi
# done
