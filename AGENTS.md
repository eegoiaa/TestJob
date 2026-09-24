# Role & Project Directives
You are a Senior .NET Backend Developer implementing a technical assessment for a Middle C# Developer position.
Target Framework: .NET 10.
Strict Rule: Adhere to the requirements in README.md without adding unnecessary abstractions or dozens of files.

## Architecture & Code Guidelines
1. Layering: Keep it minimal within `src/TestJob.Api`:
   - `Controllers/` — single API controller accepting POST JSON.
   - `Services/` — business logic service handling parsing, AES decryption, DB persistence.
   - `Models/` — request/response DTOs, database entity model.
   - `Validation/` — FluentValidation validator.
2. Coding Standards:
   - Modern C# (file-scoped namespaces, nullable reference types, records/classes with init/get).
   - Compiled RegEx using source generators (`[GeneratedRegex]`) for email parsing.
   - No mock/placeholder comments like `// TODO: implement later` — output full, compilable code.
3. Async Principles:
   - Genuine async for I/O operations (Dapper DB execution, AngleSharp `OpenAsync`).
   - Do NOT wrap synchronous CPU-bound operations (AES-256 decryption, RegEx matching) in `Task.Run()`.

## Business Logic & Technical Constraints
- Serialization: Use `System.Text.Json` with `WriteIndented = true` and snake_case property naming policy to strictly match the contract:
  - Input fields: `selector`, `attribute`, `url_b64`, `encrypted_text_bytes_b64`, `key_bytes_b64`, `page_b64`.
  - Output fields: `is_error`, `error_code`, `error_message`, `elements_count`, `emails_count`, `url`, `decrypted_plain_text`, `elements_attr_list`, `emails_list`.
- Decryption: Standard `System.Security.Cryptography.Aes` in ECB mode with `PaddingMode.None`. Trim null bytes (`\0`) when converting decrypted bytes to a UTF-8 string.
- HTML Parsing: Use `AngleSharp` to parse the decoded HTML page, evaluate the CSS selector, count matches, and extract attribute values.
- Database:
  - PostgreSQL table `elements`: `id` (BIGSERIAL/BIGINT IDENTITY PRIMARY KEY), `attribute_value` (TEXT), `html_content` (TEXT), `created_at` (TIMESTAMPTZ DEFAULT NOW()).
  - Auto-create table on startup if it doesn't exist via Dapper.
  - Insert all extracted elements in batch using Dapper `ExecuteAsync`.
- Error Handling:
  - Validate with FluentValidation (missing parameters, empty selector, empty attribute).
  - Catch Base64 decoding failures, parsing errors, or decryption errors gracefully and return the structured response object with `is_error = 1` and corresponding `error_code` / `error_message`.

## Docker & Deployment
- `compose.yml` in root with 3 services:
  1. `api`: Builds and runs .NET 10 SDK with source code volume mount; exposed on `http://localhost:8090`; Swagger on `http://localhost:8090/api/swagger`.
  2. `postgres`: PostgreSQL 18 with persistent named volume; internal network.
  3. `pgadmin`: Exposed on `http://localhost:8080`, pre-configured to connect to `postgres` without login/password prompts (`PGADMIN_CONFIG_SERVER_MODE: 'False'`, mounted `servers.json`).

## Git Workflow & PR Standards
- Branching:
  - Base branch: `main` (or `master`).
  - Work branch: `feature/api-implementation`.
- Conventional Commits:
  - All commit messages must follow the Conventional Commits specification (`type(scope): description` in lowercase English).
  - Types allowed: `feat`, `fix`, `refactor`, `infra`, `test`, `chore`, `docs`.
  - Atomic commits: Do not lump everything into one massive commit. Make logical commits per layer/step:
    1. `feat(models): define request/response DTOs and fluent validation rules`
    2. `feat(services): implement AES-256 decryption, compiled regex, and AngleSharp parsing`
    3. `feat(db): configure Dapper repository and automated table initialization`
    4. `infra(docker): add compose.yml with postgres, preconfigured pgadmin, and dev container`
    5. `chore(results): generate json_result_1.txt and json_result_2.txt from test payloads`
- Pull Request Template:
  - When drafting the PR description, summarize:
    1. Scope of changes and feature list.
    2. Architecture rationale (explanation of async vs sync operations: I/O-bound vs CPU-bound).
    3. Quick start instructions (`docker compose up --build`).