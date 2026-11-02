# Hearth & Handle Cup Store

An independent foundation project for the IMAT3701 Assessment, built with ASP.NET Core MVC, C#, an n-tier architecture, and SQL Server Express. The brand is fictional. Product information and photos for 18 items were collected from Amazon UK in a one-time snapshot on 2026-10-20; the application does not call external product APIs at runtime. The homepage hero image is a cup lifestyle photograph by Esra Afşar on Unsplash (see `docs/hero-photo-source.md`). The English-language interface uses a cosy cream, terracotta, and sage-green palette.

## Running in Visual Studio

1. Open `HearthCup.sln`.
2. Set `HearthCup.Web` as the startup project and select the `HearthCup` launch profile.
3. Ensure the local `SQL Server (SQLEXPRESS)` service is running.
4. Press F5 or Ctrl+F5; the browser opens at `http://localhost:5188`.
5. On the first run, migrations create the database, seed 18 Amazon-sourced products, and create the sole shop administrator account.

Alternatively, run `powershell -ExecutionPolicy Bypass -File .\Start.ps1` from the project root. The .NET 9 SDK is required, and NuGet packages must be restored on first run.

## Accounts and Roles

Two roles have completely different interfaces after login:

| Role | Creation | Capabilities |
|---|---|---|
| Manager | Seeded at startup using `Admin:Email` and `Admin:Password` from `appsettings.json` | Create, edit, delete, publish and unpublish products; view all orders; mark orders as shipped |
| Buyer | Self-registration at `/Account/Register` | Browse, add to cart, change quantities, check out, and view personal order statuses |

- The default administrator credentials are configured in `appsettings.json`; change them before first use as needed. Changing the configured email and restarting updates the existing administrator record rather than creating a second administrator.
- Only buyers can register. Their email is the login name, and the password must be exactly six digits. Registration automatically signs them in.
- After login, managers go to `/Manage` and buyers to `/Shop`.
- Passwords are stored as hashes using `PasswordHasher`, never as plaintext in the database.
- In development, current administrator credentials are also written to `src/HearthCup.Web/.local/admin-login.txt` for local login and browser scripts. This directory is ignored by Git and must not be committed or included in submissions.

## Implemented Features

- **Manager:** Create, view, edit, and delete products (with deletion confirmation); toggle **published/unpublished** status; edit name, description, price, stock, image, category, and other fields.
- **Buyer:** Browse the catalogue; search names, descriptions, colours, and materials; filter by category, material, and stock; sort by price or name; paginate at 8 products per page.
- **Shopping cart:** Available after login and stored per account. Supports **increasing or decreasing quantities**, removal, subtotals, and totals. Unpublished or out-of-stock items are removed when the cart is viewed, and quantities are capped at current stock.
- **Checkout:** A single transaction validates the order, stores it with price and product-name snapshots, deducts inventory, and empties the cart. Insufficient stock or unpublished products cause rejection with a message.
- **Orders:** Buyers see only their own orders and `Pending` / `Shipped` statuses. Managers see all orders, buyer emails, line items, and totals, and can mark orders as shipped.
- **Security:** Role-based access (buyers cannot access `/Manage`; managers cannot access buyer cart/order pages), cookie authentication, antiforgery tokens, login rate limiting, data annotations and server-side validation, Razor encoding, and CSP.
- **Responsive and accessible UI:** Desktop/mobile layouts, keyboard focus, skip-to-content navigation, semantic markup, and reduced-motion preferences.
- **Frontend:** Only local `site.css`, `shop.css`, and `site.js` are loaded; no third-party CSS/JS frameworks or external page requests. CSP permits same-origin resources only.

There is no payment processing, email, reviews, or real shipping integration; shipping simply changes the order status to `Shipped`. Cart items do not reserve inventory, which is deducted only during checkout.

## Solution Structure (Four-Tier n-Tier Architecture)

```text
src/HearthCup.Models          Product, user, order, and cart entities; DTOs; repository and hashing interfaces
src/HearthCup.Utility         Business services (ShopService / AccountService / OrderService) and SD constants
src/HearthCup.DataAccess      EF Core DbContext, four repositories, PasswordHasherAdapter, DbInitializer, migrations
src/HearthCup.Web             MVC, Razor, ViewComponent, authentication/authorisation, CSS
tests/HearthCup.Checks        29 database-independent business checks
docs/                        Architecture, Assessment mapping, and validation scope
```

Dependencies flow as Models ← Utility ← DataAccess ← Web. Models has no project dependencies; Utility depends only on Models; DataAccess implements repository interfaces and `IPasswordHasher` from Models; Web is the sole composition root. Services depend only on interfaces, so `tests/HearthCup.Checks` references Models and Utility without requiring SQL Server.

## Database

The connection string is `ConnectionStrings:Shop` in `src/HearthCup.Web/appsettings.json`. By default it uses Windows integrated authentication with `.\SQLEXPRESS` and the `HearthCup` database. Adjust the instance name on other machines.

EF Core migrations in `src/HearthCup.DataAccess/Migrations` manage the schema. At startup, `IDbInitializer` calls `Database.MigrateAsync()`. The 18 sample products are seeded **only when the database is initially created**, so deleting them all and restarting will not restore them. The administrator account is checked on every startup. After changing entities or `ApplicationDbContext` mappings, add a migration rather than manually editing the database:

```powershell
dotnet ef migrations add <Name> -p src/HearthCup.DataAccess -s src/HearthCup.Web -o Migrations
```

Databases originally created with `EnsureCreated` lack migration history. Delete only this project's `HearthCup` database to allow migrations to manage its schema; do not delete other databases.

For separate database delivery, use `database/hearthcup-schema-and-data.sql` (5 tables, 18 products, administrator account, and 3 migration records):

```powershell
sqlcmd -S .\SQLEXPRESS -E -i database/hearthcup-schema-and-data.sql
```

The application then recognises the imported schema as current and does not recreate its tables. This importable snapshot does not replace migrations.

## Verification

```powershell
dotnet build HearthCup.sln
dotnet run --project tests/HearthCup.Checks
```

The workflow in `.github/workflows/build.yml` runs these checks on every push and pull request: restore, Release build, and 29 business checks. They do not need SQL Server, so they also run on an Ubuntu runner.

For browser regression tests, start the website and install Node.js and Chrome:

```powershell
npm install
npm run check:browser
```

The script reads administrator credentials from the `.local` file, registers a test buyer, and checks the flow “add to cart → place order → ship → view status”. It is designed to avoid leaving other test data, but **orders cannot be deleted: run it only against a local development database**. Output is written to `output/playwright/`. See `docs/validation.md` for executed tests.

## Demo and Report Preparation

See `docs/assessment-map.md`. These implementation notes help explain the project but do not replace actual group-member contribution records, personal reflections, or formal reports. They are not fabricated evidence of individual work. Declare AI usage truthfully according to course requirements.

## Product Source Updates

A one-time import provided real photos, names, specifications, and reference prices for 18 products, alongside rewritten short descriptions. See `docs/amazon-sources.md` and `data/amazon-catalogue.json`. Package counts remain in product names and source records. Stock is local demonstration data, and prices are a snapshot from 2026-10-20, not live Amazon prices or inventory. Backups of the original fictional catalogue are in `Web/.local/catalogue-backups`. Each sourced product detail page links to Amazon UK for verification.

## Homepage and Product Pages

The homepage (`/`) contains only the brand welcome content. “Find your favourite cup” or the top-navigation “Shop” link opens the separate `/Shop` catalogue. The top search bar goes straight to results from any page and searches names, descriptions, colours, and materials. Category selection, filtering, sorting, and pagination are handled on the product page. Clicking the logo returns home.
