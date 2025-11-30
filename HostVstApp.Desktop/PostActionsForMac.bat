@echo off
set DebugOrRelease=Debug
set MacConfig=osx-arm64
set AppName=HostVstApp.Desktop
set NetVersion=net8.0
set MacPublishFolder=bin\%DebugOrRelease%\%NetVersion%\%MacConfig%\publish
set IconDestFolder=%MacPublishFolder%\%AppName%.app\Contents\Resources

dotnet restore -r %MacConfig%
dotnet msbuild -t:BundleApp -p:RuntimeIdentifier=%MacConfig% -p:UseAppHost=true -p:DefineConstants="MACOS" -p:Configuration=%DebugOrRelease%

REM dotnet publish -r %MacConfig% -c %DebugOrRelease% /p:UseAppHost=true /p:DefineConstants="MACOS"

REM Required for HostVstApp.Desktop because project is referenced instead of nuget package -
copy "bin\%DebugOrRelease%\%NetVersion%\libs\Vst3Pont.dylib" "%MacPublishFolder%\%AppName%.app\Contents\MacOS"

if exist "%MacPublishFolder%\%AppName%.app\Contents\MacOS\libsndfile-1.dll" (
    del "%MacPublishFolder%\%AppName%.app\Contents\MacOS\libsndfile-1.dll"
)
if exist "%MacPublishFolder%\%AppName%.app\Contents\MacOS\libs\Windows" (
    rmdir /S /Q "%MacPublishFolder%\%AppName%.app\Contents\MacOS\libs\Windows"
)
if exist "%MacPublishFolder%\%AppName%.app\Contents\MacOS\libs" (
    rmdir /S /Q "%MacPublishFolder%\%AppName%.app\Contents\MacOS\libs"
)

echo Done!
