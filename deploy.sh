#!/bin/bash

dotnet build OBase.Pazaryeri.Api/OBase.Pazaryeri.Api.csproj \
  -p:DeployOnBuild=true \
  -p:PublishProfile=FolderProfile