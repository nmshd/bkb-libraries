set -x
set -e

./.ci/find_changed_projects.sh | while read library_name; do 
    cd "$library_name/$library_name"
    dotnet pack --configuration Release --no-restore --no-build
    dotnet nuget push bin/Release/*.nupkg -k $NUGET_API_KEY -s https://api.nuget.org/v3/index.json
done
