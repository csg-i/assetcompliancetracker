FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
RUN sed -i 's/TLSv1.2/TLSv1.0/g' /etc/ssl/openssl.cnf
ADD CSG.crt /usr/local/share/ca-certificates/CSG.crt
RUN chmod 644 /usr/local/share/ca-certificates/CSG.crt && dpkg-reconfigure -f noninteractive ca-certificates
WORKDIR /usr/app
COPY publish/ .
CMD ["dotnet", "act.core.web.dll","--urls","https://+443"]
EXPOSE 443
