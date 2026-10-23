# QA inventory
- Build all C# and compiled Razor views.
- Catalogue seeded with 18 fictional cups; 8/page; category, material, stock filters, search, sorting and pagination.
- Details image, specs, quantity bounds and sold-out button.
- Basket add/duplicate merge/count/subtotal/remove/empty state. Independent sessions.
- Admin unauthorized redirect, manager sign-in/out, product create/read/update/delete, browser and server validation.
- Off-happy paths: tampered quantity/price/image, missing antiforgery token, stale edit token, deleted/out-of-stock basket product.
- Desktop 1440x1000 and mobile 390x844: initial headline, call to action, images, filter controls and cart usable; no horizontal overflow.
- Original local SVGs load; no external network dependencies.
- DB records survive process restart. No real orders/payments are claimed.
