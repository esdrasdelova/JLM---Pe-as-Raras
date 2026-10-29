# JLM — Loja virtual (ASP.NET Core MVC)

Moda masculina premium (roupas, joias e acessórios) com identidade preto/dourado e mascote João Laumir.

## Requisitos
- .NET SDK 10 (projeto net10.0)
- MySQL/MariaDB (XAMPP) em `localhost:3306`
- Banco `jlm_store` (criado automaticamente na primeira execução)

## Execução
```powershell
cd "JLM"
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --urls "http://localhost:5050"
```
Acesse http://localhost:5050.

## Credenciais de demonstração
- Admin: `admin@jlmstore.com.br` / `Jlm@2026`
- Cliente: `cliente@jlmstore.com.br` / `Jlm@2026`

## Estrutura
- `JLM/` projeto ASP.NET Core MVC
- `JLM/wwwroot/` assets (css, js, images, svg)
- `JLM/Migrations/` EF Core migrations
- Banco: MySQL/MariaDB via Pomelo EF Core

## Observações
- A connection string de desenvolvimento está em `appsettings.Development.json` (root sem senha).
- Migrations são aplicadas automaticamente no startup (`Database.MigrateAsync`).
- Dados iniciais (produtos, coleções, cupons, pedidos demo) são inseridos de forma idempotente.
