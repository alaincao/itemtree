FROM mcr.microsoft.com/dotnet/sdk:5.0

## Install NodeJS & NPM
ENV DEBIAN_FRONTEND noninteractive
RUN apt-get update \
 && : "Packages that would have been installed by NodeJS's installer" \
 && apt-get install -y apt-utils apt-transport-https lsb-release \
 && : "Install NodeJS" \
 && curl -sL https://deb.nodesource.com/setup_10.x | bash - \
 && apt-get install -y nodejs

## Dependency for EPPlus (Excel library)
RUN apt-get install -y libgdiplus

## Initialize working directory
WORKDIR /itemttt/
COPY . .
RUN : "Restore .Net package & compile application" \
 && dotnet build --configuration=Release

## Initialize startup
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS="http://0.0.0.0:80/"
EXPOSE 80
CMD [ "dotnet", "run", "--no-build", "--configuration=Release" ]
