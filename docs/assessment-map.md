# Assessment implementation map

## Functional requirements

| Brief requirement | Implementation | Demonstration |
|---|---|---|
| Create/read/update/delete product records | `ManageController`, `ProductInput`, `ShopService`, `ProductRepository` | Sign in; create a cup, view it in the shop, edit its price and delete it |
| Product browsing, filtering, pagination | `HomeController.Index`, `ProductRepository.SearchAsync`, Home Index and `_Pagination` | Combine category/material/stock, search, change sorting and page |
| Add/view/remove basket items | `CartController`, `ShopService`, `CartRepository` | Add cups, change quantity, view total, remove a cup, show empty state |
| Two identities with different screens | `AccountController`, `AccountService`, `Roles` | Register a buyer, then sign in as the manager and compare the navigation |
| Orders, status and shipping | `OrderService`, `OrderRepository`, `Manage/Orders` | Check out as a buyer, then mark the order shipped as the manager and reload the buyer's page |
| Consistent UI | Shared layout, site.css, local product photos and original hero illustration | Desktop and mobile, focus states and error feedback |
| MVC and n-tier | Models / Utility / DataAccess / Web projects | Explain references, dependency injection and a request from browser to database |

## Architecture

```mermaid
flowchart TD
 Browser[Browser / Razor forms] --> Web[Web: MVC controllers, view models, cart badge component]
 Web --> Shop[Utility: ShopService catalogue and basket rules]
 Web --> Acct[Utility: AccountService registration and sign-in]
 Web --> Ord[Utility: OrderService checkout and shipping]
 Shop --> Ports[Models: IProductRepository, IUserRepository, ICartRepository, IOrderRepository]
 Acct --> Ports
 Ord --> Ports
 Repo[DataAccess: four repositories] -. implements .-> Ports
 Repo --> EF[DataAccess: ApplicationDbContext / EF Core]
 EF --> DB[(SQL Server Express: Users, Products, CartItems, Orders, OrderItems)]
```

The four projects follow the module's n-tier layout: Models holds the data models and shared contracts, Utility holds the application service and constants, DataAccess holds the context, repository and initialiser, and Web holds presentation. Dependencies only ever point downwards (Models <- Utility <- DataAccess <- Web), so nothing in the lower tiers knows about MVC or Razor. The Web composition root registers interfaces and implementations. Business services have no reference to MVC or EF Core. Persistence is isolated behind an interface; test doubles replace it without a running database. MVC separates the HTTP controller, strongly typed view models and Razor views. n-tier adds responsibility boundaries between presentation, business rules and persistence; these are logical layers in a single process, not separately deployed servers.

## Five quality attributes

| Attribute | Actual tactic / code | Evidence and limitation |
|---|---|---|
| Availability | SQL transient-failure retry in `Program.cs`; centralized exception handling, structured logging and `/health` readiness | DB health returns 503 when unavailable; generic user error page. This is limited recovery and diagnostics, NOT replication, failover or guaranteed uptime |
| Modifiability | Separation of concerns, `IProductRepository`, constructor injection, EF Core migrations, shared layout and reusable partials | Replace persistence behind the interface or add a filter without moving query logic into Razor; no external feed coupling |
| Testability | Repository port, deterministic business service and dedicated Checks project | Stock, malformed input, money and current-product basket rules run without SQL or a web server; browser checks cover integrated MVC/SQL flows |
| Usability | Search/filter/pagination, labelled forms, stock states, delete confirmation, success/error messages, responsive CSS | Keyboard focus, skip link and reduced-motion support. Photos are sourced from Amazon UK and prices are dated snapshots; no user-study results are claimed |
| Security | Role separation (Manager versus Buyer), cookie authentication, hashed passwords, rate-limited login, global antiforgery validation, data annotations and application validation, Razor encoding and CSP | Unauthorized admin blocked; POST needs token; no arbitrary external image paths or client-supplied prices. This is a local coursework authentication boundary, not a complete production identity system |

## Data integrity

SQL rowversion is sent back as a hidden token on edit/delete. EF compares the original version, so a stale editor cannot silently overwrite another change. SQL check constraints back up price, capacity and stock validation. Product queries use EF parameters; no raw user-built SQL is executed. Stable secondary sort by ID avoids ambiguous ordering. Cart rows and orders store product IDs and quantities; the basket price is read afresh from the database at every visit, and an order copies name and unit price at checkout so later edits cannot rewrite history. Basket contents do not reserve stock and this project does not claim checkout capability.

## Report and demo scope

Use actual code and observed checks as evidence. Explain the advantages, trade-offs and current limitations of each tactic. Each member must supply their own true reflection and contribution; the project does not generate that evidence. The final coursework additionally needs a demonstration/video, final report, and the course AI checklist/declaration. None is replaced by this source project.
