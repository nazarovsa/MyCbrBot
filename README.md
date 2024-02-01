MyCbrBot
=======

[![Build & test](https://github.com/nazarovsa/MyCbrBot/actions/workflows/dotnet.yml/badge.svg)](https://github.com/nazarovsa/MyCbrBot/actions/workflows/dotnet.yml)

Bot can send you information about currency rates according to Central Bank of Russia by date. To get more info [send /start command](https://t.me/MyCbr_Bot?start).

Build docker image
=======

To build docker image run next command from root folder.
```
docker build -t mycbrbot:latest . -f MyCbrBot.Host/Dockerfile
```

Run bot in docker
=======

Bot uses polling.

```
docker run --name mycbrbot \
        -d --restart unless-stopped \
        -e TelegramBotOptions__Token=$MyCbrBotToken \
        mycbrbot:latest
```

Where:
- $MyCbrBotToken - env variable with token of telegram bot.
