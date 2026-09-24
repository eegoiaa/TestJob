FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /app

COPY src/TestJob.Api/TestJob.Api.csproj src/TestJob.Api/
RUN dotnet restore src/TestJob.Api/TestJob.Api.csproj

COPY src/ src/

WORKDIR /app/src/TestJob.Api
EXPOSE 8090

CMD ["sh", "-c", "dotnet watch run --no-launch-profile --urls http://0.0.0.0:8090"]
