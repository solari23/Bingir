@echo off

SET repoDir=%~dp0..

dotnet publish ^
    %repoDir%/src/Bingir.sln ^
    -c Release ^
    -r osx-x64 ^
    -p:PublishReadyToRun=true ^
    -p:PublishSingleFile=true ^
    --self-contained ^
    --output %repoDir%/out/Mac