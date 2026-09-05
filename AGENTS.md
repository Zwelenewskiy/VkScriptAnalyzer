# VkScriptAnalyzer — контекст для агента

Читай этот файл в начале работы с репозиторием. Не проси пользователя заново объяснять проект.

## Что это

Интерпретатор **VKScript** — диалекта JavaScript для VK API `execute`. Не полный клон официального VK `execute`.

Рабочая папка: `C:\Users\Alexander\YandexDisk\Programming\C#\VkScriptAnalyzer`.  
Решение: `VkScriptAnalyzer.sln`. Целевой фреймворк: **net10.0**.

Общение с автором — **на русском**. Код и комментарии смешанные (RU/EN).

## Стек интерпретатора

1. **Лексер** — самописный, несколько DFA-автоматов параллельно (`Core.Lexer.Mashines`), сборка токена в `GetToken`.
2. **Парсер** — рекурсивный спуск (LL), ручной, `Core.Parser.SyntacticAnalyzer`.
3. **Исполнение** — обход AST (`Core.Emulator.EmulatorMashine`), не байткод.

Грамматика описана в `Грамматика.md` (EBNF). Код и грамматика **могут расходиться** — не считать файл истиной без сверки с парсером.

Это **не** полный VKScript: нет массивов, нет `@.`, нет методов строк/массивов и т.п. Не предлагать «дописать весь execute», пока автор явно не попросит.

## Слои (Clean Architecture)

| Проект | Роль |
|--------|------|
| **Entities** | Модели: лексер (`Token`, `TokenType`), парсер (AST, `ParseResult`), эмулятор (`Scope`, `Env`, `DataType`, `CalculateResult`, символы). Без бизнес-логики. |
| **Core** | Лексер, парсер, эмулятор, контракт `Core.ApiMethodsExecutor.IApiMethodsExecutor`. Ссылается **только на Entities**. |
| **VkApi** | `ApiMethodsExecutor : IApiMethodsExecutor` на VkNet. Ссылается на Core и Entities. |
| **VkScriptAnalyzer** | Консольный host: `Program.cs`, `appsettings.json`, `input.vkscript`. Composition root. |
| **VkScriptAnalyzerTests** | MSTest. Не трогать тесты, пока автор не попросит. |

### Правило зависимостей

- Core **не** ссылается на VkApi.
- Интерфейс API живёт в **Core**, не в Entities (иначе цикл или лишняя связь Core→инфраструктура).
- Host создаёт `ApiMethodsExecutor` и передаёт в `EmulatorMashine`.

DDD «по учебнику» (агрегаты, репозитории, доменные события) **не нужен**. CA достаточно. Не предлагать раскатывать DDD без запроса.

## Важные типы

**`ParseResult`** (`Entities.Parser`): успех — ctor `(Node node)`, свойство **`Program`**; ошибка — ctor `(string errorMessage)`.

**`CalculateResult`** (`Entities.Emulator`): успех — ctor `(object val, DataType type)` → `IsSuccess=true`; ошибка — ctor `(string errorMessage)` → `IsSuccess=false`. Есть `GetResult()`.

**`SyntacticAnalyzer.Parse()`** возвращает `ParseResult`. Внутренние методы (`Expr`, `If`, …) по-прежнему могут возвращать `Node`/`null` и писать **`ErrorMessage`** на анализаторе (боковой канал ошибок — известный недочёт CA).

**`EmulatorMashine`**: нет поля `ErrorMessage`; ошибки только через `CalculateResult`. `null` из `StartEmulate` = выполнение без `return`. Конструктор `(Node ast, IApiMethodsExecutor api)`. Вызов API: `_api.Execute(...)`.

**`ApiMethodsExecutor`**: ctor `(string login, string password, ulong applicationId)`. Учётные данные **только из конфига**, не хардкодить. Клиент VkNet типизировать как `VkNet.VkApi`, чтобы не конфликтовать с namespace `VkApi`.

## Host

`Program.cs`: читает `appsettings.json` (`vkLogin`, `vkPassword`, `applicationId`), парсит `input.vkscript`, затем:

```text
new EmulatorMashine(parseResult.Program, new ApiMethodsExecutor(...))
```

`appsettings.json` копируется в output. **Не коммитить и не цитировать пароли** в ответах.

## Соглашения по коду

- Поля и локальные переменные: **camelCase**.
- Методы и типы: **PascalCase** (в т.ч. имена тестовых методов).
- Не раздувать scope: делать только то, что попросили.
- Коммиты — только по явной просьбе.

## Архитектурные решения, которые уже приняты

- Слои: Entities / Core / VkApi / host — так и оставлять.
- Интерфейс исполнителя API — в Core.
- Результат парсера и эмулятора — отдельные result-типы, не исключения как основной путь.
- Credentials — `appsettings.json`, не исходники.
