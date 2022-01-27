#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS publish
WORKDIR /src

COPY ["ts.hermanj.no/ts.hermanj.no.csproj", "ts.hermanj.no/"]
RUN dotnet restore "ts.hermanj.no/ts.hermanj.no.csproj"

COPY . .
RUN dotnet publish "ts.hermanj.no/ts.hermanj.no.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine
WORKDIR /app
COPY --from=publish /app .

RUN apk add --no-cache icu-libs tzdata
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false TZ=Europe/Oslo

ENTRYPOINT ["dotnet", "ts.hermanj.no.dll"]