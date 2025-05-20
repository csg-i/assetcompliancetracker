FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
RUN sed -i 's/TLSv1.2/TLSv1.0/g' /etc/ssl/openssl.cnf
WORKDIR /usr/app
COPY publish/ .
CMD ["dotnet", "act.core.web.dll","--urls","https://+443"]
EXPOSE 443
