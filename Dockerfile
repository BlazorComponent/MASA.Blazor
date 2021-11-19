FROM registry.cn-hangzhou.aliyuncs.com/masa/dotnet_sdk:6.0.100-preview.7-nodejs14.16.1

WORKDIR /app
COPY . .
RUN dotnet build src/Doc/MASA.Blazor.Doc.Server -c Release

ENV LANG="zh_CN.UTF-8"
ENV LANGUAGE="zh_CN:zh"
ENV ASPNETCORE_URLS=http://0.0.0.0:5000

ENTRYPOINT ["dotnet","./src/Doc/MASA.Blazor.Doc.Server/bin/Release/net6.0/MASA.Blazor.Doc.Server.dll"]
