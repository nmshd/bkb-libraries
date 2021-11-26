#!/bin/bash

git --no-pager diff --name-only HEAD^ | grep csproj\$ | while read -r line
do
    diff=$(git --no-pager diff -U0 --raw HEAD^ $line)

    if [[ $diff =~ \<Version\>.*\<\/Version\> ]]; then
        echo $line | cut -f1 -d"/"
    fi
done

