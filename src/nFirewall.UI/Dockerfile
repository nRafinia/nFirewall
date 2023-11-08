FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Firewall.UI/Firewall.UI.csproj", "Firewall.UI/"]
RUN dotnet restore "Firewall.UI/Firewall.UI.csproj"
COPY . .
WORKDIR "/src/Firewall.UI"
RUN dotnet build "Firewall.UI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Firewall.UI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Firewall.UI.dll"]
