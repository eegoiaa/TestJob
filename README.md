# Реализация REST API для обработки HTML-страниц

## 📋 Описание
Реализован REST API сервис для обработки HTML-страниц с функционалом парсинга элементов, извлечения email-адресов и AES-256 дешифрования текста.

## ✨ Реализованный функционал

### API Endpoint
- **POST** `/api/processing` — принимает JSON с параметрами для обработки
- Swagger UI доступен на `/api/swagger`

### Обработка данных
- ✅ Валидация входных данных через FluentValidation
- ✅ Декодирование Base64 (URL, HTML, криптографические данные)
- ✅ Парсинг HTML с помощью AngleSharp и поиск элементов по CSS-селектору
- ✅ Извлечение атрибутов из найденных элементов
- ✅ Поиск email-адресов с помощью скомпилированного Regex
- ✅ AES-256 ECB дешифрование текста с PaddingMode.None
- ✅ Сохранение результатов в PostgreSQL через Dapper

### Инфраструктура
- ✅ Docker Compose с 3 сервисами (API, PostgreSQL 18, pgAdmin)
- ✅ Автоматическое создание таблицы `elements` при старте
- ✅ pgAdmin без пароля на `localhost:8080`
- ✅ API доступен на `localhost:8090`

## 🏗️ Архитектура

### Структура проекта
src/TestJob.Api/
├── Controllers/          # API контроллер
├── Services/             # Бизнес-логика
├── Models/               # DTO и сущности
├── Validation/           # FluentValidation правила
└── Program.cs            # Точка входа и конфигурация

### Асинхронность и производительность
- **I/O-bound операции** (async/await):
  - Парсинг HTML (`OpenAsync`)
  - Работа с БД (`ExecuteAsync`, `OpenAsync`)
  - Валидация (`ValidateAsync`)
  
- **CPU-bound операции** (синхронно):
  - AES-256 дешифрование
  - Regex matching (скомпилированный)
  - Base64 декодирование

### Обработка ошибок
Использован подход с кастомным исключением `ProcessingException`:
- Каждое специфичное исключение преобразуется в структурированный ответ
- Error codes: `BASE64_URL_ERROR`, `BASE64_PAGE_ERROR`, `BASE64_CRYPTO_ERROR`, `DECRYPTION_ERROR`, `VALIDATION_ERROR`, `INTERNAL_ERROR`

## 🚀 Быстрый старт

```bash
# Клонировать репозиторий
git clone <repository-url>
cd TestJob

# Запустить все сервисы
docker compose up --build

# API доступен на http://localhost:8090
# Swagger: http://localhost:8090/api/swagger
# pgAdmin: http://localhost:8080
