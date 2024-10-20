FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
RUN adduser -u 1000 --disabled-password --gecos "" appuser && chown -R appuser /app
ENV ASPNETCORE_URLS http://*:5008
RUN apk add --no-cache icu-libs krb5-libs libgcc libintl libssl3 libstdc++ zlib tzdata
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
EXPOSE 5008
ADD --chown=1000:1000 . .
USER appuser
ENTRYPOINT ["dotnet", "InsightCore.dll"]
