# Hearth & Handle (CUP) — Three-Member Presentation and Documentation Map

**Repository:** https://github.com/ztm667-667/CUP  
**Audited baseline:** `main`, commit `ec7cbc1985d7159ad5cbfe77087d0fd21f24e677` (2 November 2026).  
**Purpose:** Assign three equally sized **presentation and demonstration sections** for an IMAT3701 group presentation. This is not a retrospective record of who actually developed each feature. Each student's claimed contribution, personal reflection, and AI declaration must agree with their actual work.

## 1. Equal division at a glance

| Presentation assignment | Main question answered | Demonstration responsibility | Quality attribute(s) | Proposed allocation |
|---|---|---|---|---|
| **Member 1 — Storefront and User Experience** | How do visitors find and inspect a suitable cup? | Homepage → Shop → search/filter/sort → details → mobile view | **Usability** | 5 slides / ~5 minutes |
| **Member 2 — Accounts, Cart and Order Processing** | How does a customer safely complete a simulated purchase? | Register → add to cart → change quantity → place order → inspect status | **Security, Testability** | 5 slides / ~5 minutes |
| **Member 3 — Management, Database and System Reliability** | How does the store persist and manage data and recover from limited faults? | Manager login → create/edit/delist item → show database/health/CI | **Modifiability, Availability** | 5 slides / ~5 minutes |

**Fairness:** Equal time, five slides each, one live demonstration each, and each member explains a user journey, specific C# / UI implementation, at least one tactic, one test, and one limitation. Equal slides are not proof of equal historical coding effort. Shared code is listed once under a primary presenter, with cross-references where necessary.

## 2. Member 1: Storefront and User Experience (Slides 1–5)

**Responsibility:** Visual design and visitor-facing product browsing; product selection by search, filters, sorting and pagination. Give users straightforward, accessible navigation.

### Source code to demonstrate

- [`src/HearthCup.Web/Controllers/HomeController.cs`, lines 8–23](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.Web/Controllers/HomeController.cs#L8-L23): homepage, `/Shop`, and product details endpoints.
- [`src/HearthCup.Web/Views/Home/Index.cshtml`](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.Web/Views/Home/Index.cshtml): landing page.
- [`src/HearthCup.Web/Views/Home/Shop.cshtml`](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.Web/Views/Home/Shop.cshtml): search, filters, sorting, empty results, and product listing.
- [`src/HearthCup.Web/Views/Home/Details.cshtml`](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.Web/Views/Home/Details.cshtml): product detail display.
- [`src/HearthCup.Web/Views/Shared/_Layout.cshtml`](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.Web/Views/Shared/_Layout.cshtml), [`_ProductCard.cshtml`](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.Web/Views/Shared/_ProductCard.cshtml), [`_Pagination.cshtml`](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.Web/Views/Shared/_Pagination.cshtml): reusable UI elements, global search and accessible navigation.
- [`src/HearthCup.Web/wwwroot/css/site.css`](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.Web/wwwroot/css/site.css), [`shop.css`](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.Web/wwwroot/css/shop.css), [`site.js`](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.Web/wwwroot/js/site.js): styling, responsive design, local page behaviour.
- [`src/HearthCup.Utility/Services/ShopService.cs`, lines 13–26](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.Utility/Services/ShopService.cs#L13-L26): validates search/filter/sort inputs before querying.
- [`src/HearthCup.DataAccess/Repository/ProductRepository.cs`, lines 11–45](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.DataAccess/Repository/ProductRepository.cs#L11-L45): EF Core search, filters, deterministic sort, and eight-per-page pagination. *Member 3 discusses the repository abstraction, not the browsing logic.*

### Existing documentation to read / cite in slides

- [README — Implemented Features, **Buyer / Responsive / Frontend** bullets (lines 30–39)](https://github.com/ztm667-667/CUP/blob/main/README.md#L30-L39)
- [README — Product Source Updates (lines 98–100)](https://github.com/ztm667-667/CUP/blob/main/README.md#L98-L100)
- [README — Homepage and Product Pages (lines 102–104)](https://github.com/ztm667-667/CUP/blob/main/README.md#L102-L104)
- [Assessment map — Product browsing and consistent UI rows (lines 7–13)](https://github.com/ztm667-667/CUP/blob/main/docs/assessment-map.md#L7-L13)
- [Assessment map — **Usability** tactic (line 40)](https://github.com/ztm667-667/CUP/blob/main/docs/assessment-map.md#L40)
- [Validation — Separate landing page and catalogue (lines 27–29)](https://github.com/ztm667-667/CUP/blob/main/docs/validation.md#L27-L29)
- [Validation — Amazon catalogue update (lines 23–25)](https://github.com/ztm667-667/CUP/blob/main/docs/validation.md#L23-L25)

### Five-slide plan

1. **What visitors see:** brand concept, homepage and catalogue screenshots; clarify example store vs real checkout service.
2. **Interface architecture:** `HomeController` → `ShopService.BrowseAsync` → `ProductRepository.SearchAsync` → Razor partials; one compact sequence diagram.
3. **Search and discoverability:** show UI inputs, filtering, eight-product pages, deterministic sort, empty-result recovery.
4. **Quality Attribute — Usability:** tactics *support user initiative* (search/filter/sort) and *prevent user error* (bounded inputs and feedback); evidence: labelled controls, responsive layout, skip-to-content link. Limitation: no formal usability study.
5. **Live demonstration (about 75 seconds):** open `/`, use “Shop”, search e.g. “ceramic”, sort by price, open one product, then switch to a 390px-wide device viewport (or show saved mobile screenshot).

**Presenter explanation (editable):** “My presentation section covers how customers discover products. MVC separates the catalogue page from the query logic. The usability tactic is to help customers take initiative through search, filtering and navigation rather than requiring them to scan all products.”

## 3. Member 2: Accounts, Cart and Order Processing (Slides 6–10)

**Responsibility:** Buyer registration/login, role restrictions, cart behaviour, simulated checkout, order history, and service-level tests.

### Source code to demonstrate

- [`src/HearthCup.Web/Controllers/AccountController.cs`](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.Web/Controllers/AccountController.cs): registration, login, session claims and logout.
- [`src/HearthCup.Utility/Services/AccountService.cs`](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.Utility/Services/AccountService.cs): validation, unique email, password hashing through a port, sign-in rules.
- [`src/HearthCup.Web/Controllers/CartController.cs`](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.Web/Controllers/CartController.cs): buyer-only cart; add, update, remove and checkout actions.
- [`src/HearthCup.Web/Controllers/OrdersController.cs`](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.Web/Controllers/OrdersController.cs): only the buyer's orders.
- [`src/HearthCup.Utility/Services/ShopService.cs`, lines 64–120](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.Utility/Services/ShopService.cs#L64-L120): validate cart quantities; update cart against live availability and price.
- [`src/HearthCup.Utility/Services/OrderService.cs`](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.Utility/Services/OrderService.cs): validation before placing an order, current price snapshot, stock checks, shipping operation.
- [`src/HearthCup.DataAccess/Repository/OrderRepository.cs`, lines 9–77](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.DataAccess/Repository/OrderRepository.cs#L9-L77): actual transactional order persistence. *Member 3 discusses how this relates to EF Core and schema design.*
- [`src/HearthCup.Models/RegisterInput.cs`](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.Models/RegisterInput.cs), [`Roles.cs`](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.Models/Roles.cs): buyer input and role definitions.
- [`tests/HearthCup.Checks/Program.cs`, lines 46–131](https://github.com/ztm667-667/CUP/blob/main/tests/HearthCup.Checks/Program.cs#L46-L131): fake repositories and deterministic tests for cart, registration, checkout and shipping.
- Security configuration reference: [`src/HearthCup.Web/Program.cs`, lines 48–76](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.Web/Program.cs#L48-L76) (cookie auth and rate limiter). Shared infrastructure is explained briefly here; Member 3 owns the general composition root.

### Existing documentation to read / cite in slides

- [README — Accounts and Roles (lines 15–28)](https://github.com/ztm667-667/CUP/blob/main/README.md#L15-L28)
- [README — Implemented Features, **Shopping cart / Checkout / Orders / Security** bullets (lines 34–42)](https://github.com/ztm667-667/CUP/blob/main/README.md#L34-L42)
- [README — Verification (lines 76–92)](https://github.com/ztm667-667/CUP/blob/main/README.md#L76-L92): business checks and browser script (do not conflate them).
- [Assessment map — Basket, identities and orders rows (lines 9–11)](https://github.com/ztm667-667/CUP/blob/main/docs/assessment-map.md#L9-L11)
- [Assessment map — **Testability and Security** tactics (lines 39, 41)](https://github.com/ztm667-667/CUP/blob/main/docs/assessment-map.md#L39-L41)
- [Validation — Buyer and manager accounts, orders and listing (lines 71–86)](https://github.com/ztm667-667/CUP/blob/main/docs/validation.md#L71-L86)

### Five-slide plan

6. **Role separation:** anonymous vs Buyer vs Manager; what each role can reach; registration uses buyer role only.
7. **Cart rules:** quantity changes, total, stock limits, sold-out/delisted items, server-side validation.
8. **Checkout flow:** cart → `OrderService` → transactional `OrderRepository` → `Pending` → Manager ships → Buyer sees `Shipped`. Explicitly say this is **simulated order placement without payment gateway or actual shipping**.
9. **Quality Attributes — Security + Testability:** tactics *authenticate/authorise* using cookies and `[Authorize]`, *limit attacks* with rate-limited login and password hashing; *dependency substitution* through repository ports and test fakes. Limitations: six-digit buyer passwords and no penetration test; business tests do not prove production authentication security.
10. **Live demonstration (about 75 seconds):** register a fresh buyer, add a cup, increase quantity, place the order, show `Pending`. Optionally have Member 3 show the Manager shipping it and return to the Buyer page to show `Shipped` as a coordinated hand-off.

**Presenter explanation (editable):** “My presentation section describes the rules between registration and order placement. Validation occurs on the server, and checkout creates an order transaction rather than accepting prices from the browser. I can test most of these rules without SQL Server by substituting fake repository implementations.”

## 4. Member 3: Management, Database and System Reliability (Slides 11–15)

**Responsibility:** Administrator product management, SQL Server persistence, n-tier assembly, EF migrations, reliability features, and CI pipeline.

### Source code to demonstrate

- [`src/HearthCup.Web/Controllers/ManageController.cs`](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.Web/Controllers/ManageController.cs): Manager-only products; create, edit, delete, list/delist, and view orders. Member 2 owns the buyer-facing order story; Member 3 owns manager operations.
- [`src/HearthCup.Utility/Services/ShopService.cs`, lines 31–62](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.Utility/Services/ShopService.cs#L31-L62): product validation on save, delete, version token.
- [`src/HearthCup.DataAccess/Repository/ProductRepository.cs`, lines 53–113](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.DataAccess/Repository/ProductRepository.cs#L53-L113): EF create/update/delete, optimistic concurrency handling; Member 1 explains `SearchAsync` only.
- [`src/HearthCup.DataAccess/Data/ApplicationDbContext.cs`](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.DataAccess/Data/ApplicationDbContext.cs): five table sets, key constraints, indexes, `rowversion`.
- [`src/HearthCup.DataAccess/Migrations`](https://github.com/ztm667-667/CUP/tree/main/src/HearthCup.DataAccess/Migrations): schema changes, accounts/orders and listing data migration.
- [`src/HearthCup.DataAccess/DbInitializer/DbInitializer.cs`](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.DataAccess/DbInitializer/DbInitializer.cs): migration and initial product/manager seeding.
- [`src/HearthCup.Web/Program.cs`, lines 20–46 and 92–136](https://github.com/ztm667-667/CUP/blob/main/src/HearthCup.Web/Program.cs): interface-to-implementation registration, DB retry, error handler, `/health` endpoint.
- [`.github/workflows/build.yml`](https://github.com/ztm667-667/CUP/blob/main/.github/workflows/build.yml): restore, Release build and business checks on push/PR.

### Existing documentation to read / cite in slides

- [README — Running in Visual Studio (lines 5–13)](https://github.com/ztm667-667/CUP/blob/main/README.md#L5-L13)
- [README — Implemented Features, **Manager** (line 32)](https://github.com/ztm667-667/CUP/blob/main/README.md#L32)
- [README — Solution Structure (lines 43–54)](https://github.com/ztm667-667/CUP/blob/main/README.md#L43-L54)
- [README — Database (lines 56–74)](https://github.com/ztm667-667/CUP/blob/main/README.md#L56-L74)
- [README — Verification (lines 76–92)](https://github.com/ztm667-667/CUP/blob/main/README.md#L76-L92): CI setup and limitations.
- [Assessment map — Product CRUD requirement (line 7)](https://github.com/ztm667-667/CUP/blob/main/docs/assessment-map.md#L7)
- [Assessment map — Architecture and four logical tiers (lines 15–31)](https://github.com/ztm667-667/CUP/blob/main/docs/assessment-map.md#L15-L31)
- [Assessment map — **Availability and Modifiability** tactics (lines 37–38)](https://github.com/ztm667-667/CUP/blob/main/docs/assessment-map.md#L37-L38)
- [Assessment map — Data integrity (lines 43–45)](https://github.com/ztm667-667/CUP/blob/main/docs/assessment-map.md#L43-L45)
- [Validation — N-tier restructure and migrations (lines 31–58)](https://github.com/ztm667-667/CUP/blob/main/docs/validation.md#L31-L58)
- [Validation — CI (lines 59–69)](https://github.com/ztm667-667/CUP/blob/main/docs/validation.md#L59-L69)

### Five-slide plan

11. **Manager workflows:** create, edit, publish/unpublish, delete, view orders; role guard protects `/Manage`.
12. **Database design:** EF Core entities and relations; `Products`, `Users`, `CartItems`, `Orders`, `OrderItems`; rowversion and SQL check constraints.
13. **Quality Attribute — Modifiability:** tactics *separate responsibilities* (Models / Utility / DataAccess / Web), *use abstraction* (repository interfaces) and *defer binding* (dependency injection). Show `IProductRepository` to `ProductRepository` registration.
14. **Quality Attribute — Availability + verification:** tactics *retry failed requests* (`EnableRetryOnFailure(3)`), *detect faults* (`/health` gives 503 when DB unavailable), exception handling; show CI workflow. Clarify: these do not provide failover or an uptime SLA.
15. **Live demonstration (about 75 seconds):** log in as Manager, create or edit a test product, delist it and confirm it vanishes from `/Shop`, re-list it, then open `/health` or the GitHub Actions success run; restore/delete the test product safely.

**Presenter explanation (editable):** “My section explains how data is stored and managed, and why the system is maintainable. The four project tiers separate presentation, business rules, database interfaces and concrete persistence. The application retries some transient SQL failures and exposes database readiness, but it is not a high-availability cluster.”

## 5. Fast navigation by original document

| Existing documentation | Member 1 (Storefront) | Member 2 (Customer transactions) | Member 3 (Management / architecture) |
|---|---|---|---|
| **README.md** | `Implemented Features` (Buyer browse, responsive, frontend); `Product Source Updates`; `Homepage and Product Pages` | `Accounts and Roles` (buyer); `Implemented Features` (cart, checkout, orders, security); business checks in `Verification` | `Running in Visual Studio`; manager role/features; `Solution Structure`; `Database`; CI in `Verification` |
| **docs/assessment-map.md** | Functional rows: browsing and UI; `Usability` row | Functional rows: basket, identity and orders; `Security` and `Testability` rows | Functional CRUD row; `Architecture`; `Availability`, `Modifiability`; `Data integrity` |
| **docs/validation.md** | `Amazon catalogue update`; `Separate landing page and catalogue`; mobile observations | `Buyer and manager accounts, orders and listing` (accounts/order checks) | `N-tier restructure`; `Readable code, schema migrations...`; `Vendor libraries removed, continuous integration added` |
| **docs/amazon-sources.md** | Product images and pricing snapshot | Reference only | Reference only |
| **docs/qa-inventory.md** | UI checks | Buyer/customer test checklist | Database/admin checks — some statements describe older versions |

**Recommended slide footer:** `Evidence: README.md § X; docs/assessment-map.md § Y; src/.../File.cs#Lxx` with a clickable repository link. For a quality attribute, state **Attribute → Tactic → Code proof → Benefit → Limitation → Verification**.

## 6. Five quality attributes: group-wide checklist

| Attribute | Primary presenter | Tactic and source proof | Honest limitation |
|---|---|---|---|
| Usability | Member 1 | User initiative through search/filter/sort and feedback: `Shop.cshtml`, `_Layout.cshtml` | No measured user-study outcomes |
| Security | Member 2 | Authentication/authorisation, password hashing, login rate limiting, antiforgery: `AccountController`, `Program.cs`, `[Authorize]` | Coursework-level security; six-digit passwords; not penetration-tested |
| Testability | Member 2 | Substitute fake repositories for SQL Server: `tests/HearthCup.Checks/Program.cs` | Browser regression is not in CI |
| Modifiability | Member 3 | Separation of concerns, repository interfaces, constructor DI, migrations: `Program.cs`, Models interfaces, and DataAccess repositories | Changing the persistence technology still requires new implementation work |
| Availability | Member 3 | Transient SQL retries, health readiness and exception handling: `Program.cs` | No replication/failover/SLA/load test |

## 7. Live-demo preparation and factual cautions

1. Use a **local development database**, never production data. Start SQL Server Express and run the app using the README instructions.
2. Create a throwaway buyer for Member 2 and a temporary, clearly named test product for Member 3. Prepare the Manager sign-in privately; do not put credentials in slides or screen recordings.
3. Do not demonstrate charging a card or real parcel dispatch: the application performs simulated order placement and changes the shipping status only.
4. **Documentation inconsistency:** `docs/assessment-map.md` under *Data integrity* says the project “does not claim checkout capability,” but current code clearly implements **simulated checkout** in `CartController.Checkout` and `OrderService.PlaceAsync`. Explain “no real payment gateway,” not “no checkout.” The functional map also mentions `HomeController.Index` for catalogue search; current route is `HomeController.Shop` at `/Shop`.
5. `docs/validation.md` is a chronological history. Earlier test counts and statements about an old session basket, fictional products or `EnsureCreated` do **not** describe the latest code. The 26 October section reports **29 business checks**, but says the rewritten Playwright script was not executed on that machine. Do not present browser tests as current CI evidence; CI currently runs the business checks.
6. The latest GitHub Actions run associated with the 2 November README commit succeeded: https://github.com/ztm667-667/CUP/actions/runs/41458712283 . This does not prove that every live demonstration scenario was recently tested.
7. If a slide says “I implemented X,” the speaker should replace X with what they *actually* did. You can use this division to share **presentation responsibility** or to plan further work; don't use it to invent contribution history.

## 8. Suggested hand-off timing for a 15-minute group talk

| Time | Presenter | Segment | Handoff |
|---|---|---|---|
| 00:00–05:00 | Member 1 | Visitor journey, UI, usability, live search | “Now that customers can find a cup, how do they order it?” |
| 05:00–10:00 | Member 2 | Roles, cart, checkout, security/testability, demo | “The order is created. How is it stored and managed?” |
| 10:00–15:00 | Member 3 | Manager operations, database, modifiability/availability, CI, demo | Final recap: five attributes each with concrete tactics |

**Total: 15 slides, ~15 minutes, equal 5-minute presentation blocks.** If the assignment requires a different total time, scale all three blocks proportionally.