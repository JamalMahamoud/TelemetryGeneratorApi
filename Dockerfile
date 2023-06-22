FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 5253

RUN apt-get update && apt-get install -y crun
ENV ASPNETCORE_URLS=http://+:5253


FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["OTelOnOpenShiftDemo/OTelOnOpenShiftDemo.csproj", "OTelOnOpenShiftDemo/"]
RUN dotnet restore "OTelOnOpenShiftDemo/OTelOnOpenShiftDemo.csproj"
COPY . .
WORKDIR "/src/OTelOnOpenShiftDemo"
RUN dotnet build "OTelOnOpenShiftDemo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OTelOnOpenShiftDemo.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OTelOnOpenShiftDemo.dll"]
