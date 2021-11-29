set -x
set -e

./.ci/find_changed_projects.sh | while read project_name; do 
    project_path="$project_name/$project_name"
    dotnet pack $project_path --no-restore --no-build
    dotnet nuget push **/*.nupkg -k $NUGET_API_KEY -s https://api.nuget.org/v3/index.json
done
