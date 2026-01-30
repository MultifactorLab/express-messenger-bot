[![License](https://img.shields.io/badge/license-view-orange)](https://github.com/MultifactorLab/mf-express-bot/blob/main/LICENSE.md)

# MF.Express.Bot

_Also available in other languages: [Русский](README.ru.md)_

**MF.Express.Bot** is a locally deployed component on the customer side designed to integrate **Express corporate messenger** (on BotX platform) with **centralized MultiFactor Express API service**.

The component is a part of <a href="https://multifactor.pro/" target="_blank">MultiFactor</a> 2FA hybrid solution.

* <a href="https://github.com/MultifactorLab/mf-express-bot" target="_blank">Source code</a>
* <a href="https://github.com/MultifactorLab/mf-express-bot/releases" target="_blank">Releases</a>

See detailed documentation at <https://multifactor.ru/docs/enroll/#express> for additional guidance on integrating 2FA through Express Messenger.

## Table of Contents

* [Overview](#overview)
* [Requirements](#requirements)
* [Quick Start](#quick-start)
  * [Register Bot in BotX Platform](#register-bot-in-botx-platform)
  * [Build Docker Image](#build-docker-image)
  * [Configure Environment](#configure-environment)
  * [Deploy](#deploy)
* [Links](#links)
* [License](#license)

## Overview

MF.Express.Bot acts as a proxy between your Express Messenger (BotX Platform) and the centralized MultiFactor authentication service.

**Architecture:**
* **Customer side (deployed locally):**
  * BotX Platform (Express Messenger) - your corporate messenger
  * MF.Express.Bot - this component (proxy service)

* **MultiFactor side (centralized):**
  * MF.Express.API - centralized authentication backend
  * MultiFactor Core Service - main 2FA service

**Key features:**
* Receives JWT-protected webhooks from BotX Platform
* Processes user authentication commands
* Communicates with centralized MultiFactor Express API
* Sends push notifications to users via Express Messenger

## Requirements

**System:**
* Docker Engine 20.10+
* Docker Compose 3.8+ (optional)
* OS: Linux, Windows Server 2019+, macOS
* Port: 8080 (configurable)

**Network connectivity:**
* Outbound HTTPS (443) to `express-service.multifactor.ru` (MultiFactor API)
* Outbound HTTPS (443) to your BotX Platform server
* Inbound HTTP/HTTPS (8080) from BotX Platform

## Quick Start

### Register Bot in BotX Platform

1. Login to BotX admin panel: `https://your-express-server/admin/`
2. Navigate to **Bots** → **Create new bot**
3. Configure:
   - **Bot API URL**: `https://express-bot.your-domain.com/api`
   - **Bot Identifier**: `mf-auth-bot` (or any unique name)
   - **Enabled**: ✓
   - **Allow creating chats**: ✓
4. Save and copy generated credentials:
   - **Bot ID** (GUID format)
   - **Bot Secret Key**

### Build Docker Image

```bash
# Clone repository
git clone https://github.com/MultifactorLab/mf-express-bot.git
cd mf-express-bot

# Checkout stable version (recommended for production)
git checkout tags/v1.0.0

# Build image
cd src
docker build -t mf-express-bot:latest -f Dockerfile .
```

### Configure Environment

Create `.env` file:

```bash
# BotX Platform credentials (from admin panel)
EXPRESSBOT__BOTID=your-bot-id-guid
EXPRESSBOT__BOTSECRETKEY=your-bot-secret-key
EXPRESSBOT__BOTXAPIBASEURL=https://your-express-server
EXPRESSBOT__EXPECTEDISSUER=your-express-server

# MultiFactor centralized API
MFEXPRESSAPI__BASEURL=https://express-service.multifactor.ru

# Environment
ASPNETCORE_ENVIRONMENT=Production
```

### Deploy

**Using Docker Compose (recommended):**

```yaml
# docker-compose.yml
version: '3.8'

services:
  express-bot:
    image: mf-express-bot:latest
    container_name: mf-express-bot
    env_file:
      - .env
    ports:
      - "8080:8080"
    volumes:
      - ./logs:/app/Logs
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health/live"]
      interval: 30s
      timeout: 3s
      start_period: 15s
      retries: 3
```

```bash
# Start service
docker-compose up -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f express-bot

# Verify health
curl http://localhost:8080/health/ready
```

**Using Docker Run:**

```bash
docker run -d \
  --name mf-express-bot \
  -p 8080:8080 \
  -e EXPRESSBOT__BOTID="your-bot-id" \
  -e EXPRESSBOT__BOTSECRETKEY="your-secret-key" \
  -e EXPRESSBOT__BOTXAPIBASEURL="https://your-express-server" \
  -e EXPRESSBOT__EXPECTEDISSUER="your-express-server" \
  -e MFEXPRESSAPI__BASEURL="https://express-service.multifactor.ru" \
  -v /var/log/mf-express-bot:/app/Logs \
  --restart unless-stopped \
  mf-express-bot:latest
```

## Links

* **Documentation:** <https://multifactor.ru/docs/enroll/#express>
* **Support:** support@multifactor.ru
* **GitHub:** <https://github.com/MultifactorLab/mf-express-bot>

## License

Please note, the <a href="https://github.com/MultifactorLab/mf-express-bot/blob/main/LICENSE.md" target="_blank">license</a> does not entitle you to modify the source code of the Component or create derivative products based on it. The source code is provided as-is for evaluation purposes.
