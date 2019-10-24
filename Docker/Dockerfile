FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS runtime
WORKDIR /usr/app
COPY publish/ .
CMD ["dotnet", "act.core.web.dll","--urls","https://+443"]
EXPOSE 443
