[![License](https://img.shields.io/badge/license-view-orange)](https://github.com/MultifactorLab/express-messenger-bot/blob/main/LICENSE.md)

# MF.Express.Bot

_Also available in other languages: [English](README.md)_

**MF.Express.Bot** — это локально развёртываемый компонент на стороне заказчика, предназначенный для интеграции **корпоративного мессенджера Express** (на платформе BotX) с **централизованным сервисом MultiFactor Express API**.

Компонент является частью гибридного 2FA решения сервиса <a href="https://multifactor.ru/" target="_blank">МультiFactor</a>.

* <a href="https://github.com/MultifactorLab/express-messenger-bot" target="_blank">Исходный код</a>
* <a href="https://github.com/MultifactorLab/express-messenger-bot/releases" target="_blank">Сборки</a>

Подробная документация доступна по адресу <https://multifactor.ru/docs/enroll/#express> с инструкциями по интеграции 2FA через Express Messenger.

## Содержание

* [Описание](#описание)
* [Требования](#требования)
* [Быстрый старт](#быстрый-старт)
  * [Регистрация бота в BotX Platform](#регистрация-бота-в-botx-platform)
  * [Сборка Docker образа](#сборка-docker-образа)
  * [Настройка окружения](#настройка-окружения)
  * [Развертывание](#развертывание)
* [Ссылки](#ссылки)
* [Лицензия](#лицензия)

## Описание

MF.Express.Bot выступает в роли прокси между вашим Express Messenger (BotX Platform) и централизованным сервисом аутентификации MultiFactor.

**Архитектура:**
* **На стороне заказчика (локальное развертывание):**
  * BotX Platform (Express Messenger) - корпоративный мессенджер
  * MF.Express.Bot - данный компонент (прокси-сервис)

* **На стороне MultiFactor (централизованно):**
  * MF.Express.API - централизованный backend аутентификации
  * MultiFactor Core Service - основной сервис 2FA

**Основные функции:**
* Прием JWT-защищенных webhook'ов от BotX Platform
* Обработка команд пользователя для аутентификации
* Взаимодействие с централизованным MultiFactor Express API
* Отправка push-уведомлений пользователям через Express Messenger

## Требования

**Системные:**
* Docker Engine 20.10+
* Docker Compose 3.8+ (опционально)
* ОС: Linux, Windows Server 2019+, macOS
* Порт: 8080 (настраиваемый)

**Сетевая связность:**
* Исходящий HTTPS (443) к `express-service.multifactor.ru` (API MultiFactor)
* Исходящий HTTPS (443) к вашему серверу BotX Platform
* Входящий HTTP/HTTPS (8080) от BotX Platform

## Быстрый старт

### Регистрация бота в BotX Platform

1. Войдите в административную панель BotX: `https://ваш-express-сервер/admin/`
2. Перейдите в **Боты** → **Создать нового бота**
3. Настройте:
   - **Bot API URL**: `https://express-bot.ваш-домен.ru/api`
   - **Идентификатор бота**: `mf-auth-bot` (или любое уникальное имя)
   - **Включен**: ✓
   - **Разрешить создавать чаты**: ✓
4. Сохраните и скопируйте сгенерированные учетные данные:
   - **Bot ID** (формат GUID)
   - **Bot Secret Key**

### Сборка Docker образа

```bash
# Клонирование репозитория
git clone https://github.com/MultifactorLab/express-messenger-bot.git
cd express-messenger-bot

# Переключение на стабильную версию (рекомендуется для production)
git checkout tags/v1.0.0

# Сборка образа
cd src
docker build -t mf-express-bot:latest -f Dockerfile .
```

### Настройка окружения

Создайте файл `.env`:

```bash
# Учетные данные BotX Platform (из административной панели)
EXPRESSBOT__BOTID=ваш-bot-id-guid
EXPRESSBOT__BOTSECRETKEY=ваш-секретный-ключ-бота
EXPRESSBOT__BOTXAPIBASEURL=https://ваш-express-сервер
EXPRESSBOT__EXPECTEDISSUER=ваш-express-сервер

# Централизованный API MultiFactor
MFEXPRESSAPI__BASEURL=https://express-service.multifactor.ru

# Окружение
ASPNETCORE_ENVIRONMENT=Production
```

### Развертывание

**С помощью Docker Compose (рекомендуется):**

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
# Запуск сервиса
docker-compose up -d

# Проверка статуса
docker-compose ps

# Просмотр логов
docker-compose logs -f express-bot

# Проверка состояния
curl http://localhost:8080/health/ready
```

**С помощью Docker Run:**

```bash
docker run -d \
  --name mf-express-bot \
  -p 8080:8080 \
  -e EXPRESSBOT__BOTID="ваш-bot-id" \
  -e EXPRESSBOT__BOTSECRETKEY="ваш-секретный-ключ" \
  -e EXPRESSBOT__BOTXAPIBASEURL="https://ваш-express-сервер" \
  -e EXPRESSBOT__EXPECTEDISSUER="ваш-express-сервер" \
  -e MFEXPRESSAPI__BASEURL="https://express-service.multifactor.ru" \
  -v /var/log/mf-express-bot:/app/Logs \
  --restart unless-stopped \
  mf-express-bot:latest
```

## Ссылки

* **Документация:** <https://multifactor.ru/docs/enroll/#express>
* **Поддержка:** support@multifactor.ru
* **GitHub:** <https://github.com/MultifactorLab/express-messenger-bot>

## Лицензия

Обратите внимание на <a href="https://github.com/MultifactorLab/express-messenger-bot/blob/main/LICENSE.ru.md" target="_blank">лицензию</a>. Она не дает вам право вносить изменения в исходный код Компонента и создавать производные продукты на его основе. Исходный код предоставляется в ознакомительных целях.
