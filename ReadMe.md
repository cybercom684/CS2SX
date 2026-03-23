### Bei Update
## Erst Version hochsetzen, dann packen und danach global installieren
dotnet pack -c Release
dotnet tool update --global --add-source ./bin/Release CS2SX