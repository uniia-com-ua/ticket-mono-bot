FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TelegramEventBot/TelegramEventBot.csproj", "TelegramEventBot/"]
RUN dotnet restore "TelegramEventBot/TelegramEventBot.csproj"
COPY . .
WORKDIR "/src/TelegramEventBot"
RUN dotnet build "TelegramEventBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TelegramEventBot.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TelegramEventBot.dll"] 