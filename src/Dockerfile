FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:5.0
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT Development
ENV IS_DOCKER 1
WORKDIR /app
COPY --from=build-env  /app/out/ .

ENTRYPOINT ["dotnet", "SalveElas.Api.dll"]