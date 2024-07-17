FROM mcr.microsoft.com/dotnet/runtime:8.0

WORKDIR /app

COPY ./app /app/

VOLUME [ "/app/config.json" ]

ENTRYPOINT ["./CatMessenger.Matrix"]