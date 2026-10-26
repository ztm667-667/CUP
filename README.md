# Hearth & Handle 水杯商城

一个独立的 IMAT3701 Assessment 基础项目。使用 ASP.NET Core MVC、C#、n-tier 分层与 SQL Server Express。品牌为虚构示例；18 款商品资料和照片来自亚马逊英国站 2026-10-20 的一次性采集，运行时不调用外部商品 API。首页主图使用 Esra Afşar 在 Unsplash 发布的独立杯子生活摄影，来源见 docs/hero-photo-source.md。界面为英文，采用奶油白、陶土色与鼠尾草绿的温馨居家风格。

## 在 Visual Studio 中运行

1. 打开 `HearthCup.sln`。
2. 将 `HearthCup.Web` 设为启动项目，选择 `HearthCup` 启动配置。
3. 确认本机 `SQL Server (SQLEXPRESS)` 服务已运行。
4. 按 F5 或 Ctrl+F5。浏览器打开 `http://localhost:5188`。
5. 首次运行由迁移建库，写入 18 条亚马逊来源商品，并创建唯一的店铺管理员账号。

也可在项目根目录运行 `powershell -ExecutionPolicy Bypass -File .\Start.ps1`。需要 .NET 9 SDK；首次运行需还原 NuGet 包。

## 账号与角色

两种身份，登录后进入的界面完全不同：

| 身份 | 来源 | 能做什么 |
|---|---|---|
| 管理员（Manager） | 由 `appsettings.json` 的 `Admin:Email` / `Admin:Password` 在首次启动时写入数据库 | 商品增删改、上下架、查看全部订单、点“Mark as shipped”发货 |
| 买家（Buyer） | `/Account/Register` 自助注册 | 浏览、加购、改数量、下单、查看自己的订单状态 |

- 管理员账号默认 `mmx6671280@gmail.com` / `123456`，请在首次运行前按需修改配置。改邮箱后重启即可：种子会把已有的管理员行改名为新邮箱，不会产生第二个管理员。
- 注册只开放买家：邮箱作为登录名，密码必须是 6 位数字，注册成功后自动登录。
- 登录后按角色跳转：管理员进 `/Manage`，买家进 `/Shop`。
- 所有密码只以哈希形式存库（`PasswordHasher`），数据库里没有明文。
- 开发环境同时把当前管理员凭据写到 `src/HearthCup.Web/.local/admin-login.txt`，供本地登录与浏览器脚本读取；该目录被 Git 忽略，不要放进提交包。

## 已实现范围

- 管理员：商品新增、查看、修改、删除（删除前确认）；**上架/下架**切换；商品名称、介绍、价格、库存、图片、分类等全部可编辑。
- 买家：商品目录、名称/描述/颜色/材质搜索、分类与材质筛选、库存筛选、价格/名称排序、分页（每页 8 件）。
- 购物车：登录后可见，按账号存库；支持**加减数量**、移除、小计与合计；下架或售罄的商品在查看时自动清理并收敛到当前库存。
- 下单：一次事务内完成校验、写订单（价格与名称快照）、扣库存、清空购物车；库存不足或已下架会被拒绝并给出提示。
- 订单：买家只看自己的订单与状态（Pending / Shipped）；管理员看全部订单、买家邮箱、明细与金额，并可点发货。
- 安全：角色授权（买家进不了 `/Manage`，管理员进不了购物车与订单页）、Cookie 认证、防伪令牌、登录限流、数据注解与服务端校验、Razor 编码、CSP。
- 适配桌面和手机、键盘焦点、跳转正文、语义化标签、减少动态效果偏好。
- 前端只加载本地 `site.css`、`shop.css` 与 `site.js`，不引用第三方 CSS/JS 框架；页面不发起任何外部网络请求，CSP 也只允许同源资源。

没有支付、邮件、评论与真实物流；发货只是把订单状态改为 Shipped。商品加入购物车不预占库存，库存在下单事务内扣减。

## 解决方案结构（n-tier 四层）

```text
src/HearthCup.Models          商品与用户、订单、购物车实体，DTO、仓储与哈希端口
src/HearthCup.Utility         业务规则服务（ShopService / AccountService / OrderService）与常量 SD
src/HearthCup.DataAccess      EF Core DbContext、四个仓储、PasswordHasherAdapter、DbInitializer、迁移
src/HearthCup.Web             MVC、Razor、ViewComponent、认证与授权、CSS
tests/HearthCup.Checks        不依赖数据库的业务检查（29 项）
docs/                        架构说明、Assessment 对应与验证范围
```

依赖方向为 Models ← Utility ← DataAccess ← Web。Models 不含工程依赖；Utility 只依赖 Models；DataAccess 实现 Models 里的仓储端口与 `IPasswordHasher`；Web 是唯一组合根。业务服务只依赖端口，所以 `tests/HearthCup.Checks` 仅引用 Models 与 Utility 即可运行，不需要 SQL Server。

## 数据库

连接字符串在 `src/HearthCup.Web/appsettings.json` 的 `ConnectionStrings:Shop`。默认 Windows 集成认证连接 `.\SQLEXPRESS`，数据库名为 `HearthCup`。换电脑时按实际实例名调整。

架构由 EF Core 迁移管理，迁移文件位于 `src/HearthCup.DataAccess/Migrations`。启动时 `IDbInitializer` 调用 `Database.MigrateAsync()` 建库或升级，**只有数据库首次创建时**才写入 18 条示例商品——管理员把商品全部删光后重启不会自动恢复；管理员账号则在每次启动时确保存在。修改实体或 `ApplicationDbContext` 的映射后必须新增迁移，不要手改数据库：

```powershell
dotnet ef migrations add <Name> -p src/HearthCup.DataAccess -s src/HearthCup.Web -o Migrations
```

用早期 `EnsureCreated` 建过库的机器没有迁移历史表，需要先删除该项目专属的 `HearthCup` 数据库，让迁移接管架构；不要删除其他数据库。

## 验证

```powershell
dotnet build HearthCup.sln
dotnet run --project tests/HearthCup.Checks
```

同一组命令在 `.github/workflows/build.yml` 中随每次 push / PR 执行：还原、Release 构建、跑 29 项业务检查。检查工程不依赖 SQL Server，所以在 Ubuntu runner 上也能通过。

浏览器回归（需先启动网站，安装 Node.js 和 Chrome）：

```powershell
npm install
npm run check:browser
```

脚本从 `.local` 文件读取管理员凭据，自行注册一个测试买家走完“加购 → 下单 → 发货 → 查看状态”，之后不再残留测试数据（订单无法删除，请只在本地开发库运行）。输出在 `output/playwright/`。已执行情况见 `docs/validation.md`。

## 演示与报告准备

见 `docs/assessment-map.md`。这些是辅助理解项目的实现说明，不代替小组成员的真实贡献记录、个人反思或正式报告，也不是虚构的个人工作证明。AI 使用情况应按课程要求如实申报。

## 商品来源更新

已一次性导入 18 款真实商品照片、名称、规格和参考价格，并重写简短介绍。见 `docs/amazon-sources.md` 与 `data/amazon-catalogue.json`。包装件数保留在名称和来源资料中。店铺库存为本地演示数量，价格为 2026-10-20 的查询快照；不是实时亚马逊价格或库存。原虚构数据的备份在 Web/.local/catalogue-backups。每个来源商品详情页可点击 Amazon UK 链接核对。

## 首页与商品页

首页 `/` 仅展示品牌欢迎内容。点击 Find your favourite cup 或顶部 Shop 进入独立 `/Shop` 商品列表；顶部搜索从任意页面直接进入商品结果，支持名称、描述、颜色和材质。分类、筛选、排序和分页均在商品页完成；Logo 返回首页。
