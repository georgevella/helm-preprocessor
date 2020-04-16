#! /bin/bash

dotnet publish ./src/ArgoCdEnvironmentManager --runtime linux-x64 -c Debug /p:PublishSingleFile=true /p:AssemblyName=helm-preprocessor-linux-x64 /p:Version=${CIRCLE_TAG:-"0.1.0"}

install src/ArgoCdEnvironmentManager/bin/Debug/netcoreapp3.1/linux-x64/publish/helm-preprocessor-linux-x64 ~/.local/bin/helm-preprocessor-dev