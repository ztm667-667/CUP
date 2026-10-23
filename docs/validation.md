# Validation

2026-10-20. Local environment: .NET SDK 9.0.318, ASP.NET Core 9.0.20, EF Core SQL Server 9.0.20, Windows, SQL Server Express and installed Chrome.

## Completed

- `dotnet build HearthCup.sln`: initial build passed, 0 warnings and 0 errors.
- `dotnet run --project tests/HearthCup.Checks`: 17 business checks passed.
- `/health`: HTTP 200 Healthy; independent HearthCup database initialized.
- `node tests/browser-checks.cjs` using Playwright: 19 checks passed; detailed names in `output/playwright/browser-results.json`.
- Covered actual SQL-backed create/read/update/delete, search/category/material/stock filters, price sorting, pagination, sold-out/empty states, cart totals and independent sessions, fresh database prices and stock changes, manager login/logout, missing antiforgery token rejection, negative price rejection and rowversion conflict.
- Desktop 1440x1000 and mobile 390x844; screenshots captured and visually inspected for home, detail and management. Mobile home and basket have no horizontal overflow. No page JavaScript/CSP errors or external network dependencies in the regression run.
- Temporary QA products were deleted. Existing 18 sample products remain; the schema and data already survived multiple development process restarts.

## Pre-delivery review

Reviewed the new C# services/repository/controllers, Razor contracts and configuration, not changes to an existing repository. Corrected nullable create concurrency token, generated validation markup violating CSP, and seed lifecycle so intentionally deleting all products will not recreate them on restart. Final `dotnet build HearthCup.sln -c Release --nologo`: passed with 0 warnings and 0 errors. The complete 19-check browser regression was repeated against that Release build on port 5189 and passed.

## Limits

Not a penetration test, load test, availability SLA or formal user study. Tests use the manager account against this development database. There is no actual checkout or payment. Basket session is process-local and is lost when the application restarts. SQL connection and Windows permissions must be configured on other computers. Database initialization uses EnsureCreated, not a migration history. The seed set is fictional, with 8 original cup illustration variants across 18 product records.

## Amazon catalogue update

2026-10-20: 18 local product JPEGs and source manifest imported with backup. Final source build passed with 0 warnings/errors after stopping the old preview process. All 17 business and 19 browser checks passed against the updated catalogue. Desktop/mobile screenshots were re-inspected; no broken images or runtime external network requests. Original illustration assets remain available for locally created products. Source-price snapshots, pack counts and verification discrepancies are documented separately in amazon-sources.md.

## Separate landing page and catalogue

2026-10-20 user feedback: remove redundant top links; make Find navigate to products rather than scroll; add search. Home `/` now has no product grid, Shop `/Shop` owns filter/sort/pagination, and global search submits there. Old root query links redirect to Shop. One Shop navigation link replaces two old links. Search includes colour/material as well as name/description. Release build passed with 0 warnings/errors; full browser regression increased to 20 checks and passed. Additional mobile catalogue/header-search checks passed; desktop/home/mobile screenshots visually inspected.
