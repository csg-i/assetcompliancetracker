FROM microsoft/dotnet:2.1-aspnetcore-runtime AS runtime
WORKDIR /app
COPY publish/ .
CMD ["dotnet", "act.core.web.dll","--urls","https://+443"]
EXPOSE 443
