FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
RUN adduser -u 1000 --disabled-password --gecos "" appuser && chown -R appuser /app
ENV ASPNETCORE_URLS http://*:5008
RUN apk add --no-cache icu-libs krb5-libs libgcc libintl libssl3 libstdc++ zlib tzdata
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
EXPOSE 5008
ADD --chown=1000:1000 . .
USER appuser
# In some of our container environments, we start with a soft limit
# of 1024 open files and a hard limit of 524288. 1024 is quite low.
# When a .NET process starts, it attempts to increase the soft limit
# to the hard limit, but for some reason this fails in our environment,
# leaving the soft limit at 1024 which we can easily hit, resulting in
# errors like (because sockets are files):
# System.Net.Sockets.SocketException (23): Too many open files in system
# It seems that manually upping the soft limit before starting the process
# does work though.
ENTRYPOINT ["/bin/sh", "-c", "ulimit -S -n 500000; dotnet PlatformPortalXL.dll"]
