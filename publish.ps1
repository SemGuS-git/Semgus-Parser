param([string]$ZipSuffix = '')

Remove-Item -path publish -Recurse -Force
New-Item -name publish -path . -itemType directory

dotnet publish SemgusParser -p:PublishProfile=win-x64
dotnet publish SemgusParser -p:PublishProfile=osx-x64
dotnet publish SemgusParser -p:PublishProfile=linux-x64

New-Item -name zip -path publish -itemType directory 

zip -j "publish/zip/SemgusParser-win-x64${ZipSuffix}.zip" publish/win-x64/SemgusParser.exe
zip -j "publish/zip/SemgusParser-osx-x64${ZipSuffix}.zip" publish/osx-x64/SemgusParser
zip -j "publish/zip/SemgusParser-linux-x64${ZipSuffix}.zip" publish/linux-x64/SemgusParser
