set AppName=ProbeVst3
set Config=Release
set NetVersion=net8.0
set MacArchitect=osx-arm64
set MacPublishFolder=bin\%Config%\%NetVersion%\%MacArchitect%\publish
set InfoPlistPathSource=Info.plist
set InfoPlistPathDest=%MacPublishFolder%\%AppName%.app\Contents\Info.plist


dotnet publish -c %Config% -r %MacArchitect% -p:UseAppHost=true -p:SelfContained=false -p:PublishSingleFile=true -p:PublishDir="%MacPublishFolder%\"