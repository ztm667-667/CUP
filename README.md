# Hearth & Handle 水杯商城

一个独立的 IMAT3701 Assessment 基础项目。使用 ASP.NET Core MVC、C#、n-tier 分层与 SQL Server Express。品牌为虚构示例；18 款商品资料和照片来自亚马逊英国站 2026-10-20 的一次性采集，运行时不调用外部商品 API。首页主图使用 Esra Afşar 在 Unsplash 发布的独立杯子生活摄影，来源见 docs/hero-photo-source.md。界面为英文，采用奶油白、陶土色与鼠尾草绿的温馨居家风格。

## 在 Visual Studio 中运行

1. 打开 `HearthCup.sln`。
2. 将 `HearthCup.Web` 设为启动项目，选择 `HearthCup` 启动配置。
3. 确认本机 `SQL Server (SQLEXPRESS)` 服务已运行。
4. 按 F5 或 Ctrl+F5。浏览器打开 `http://localhost:5188`。
5. 首次运行创建独立 `HearthCup` 数据库及 18 条亚马逊来源商品。不会修改课堂 Bulky 数据库。

也可在项目根目录运行 `powershell -ExecutionPolicy Bypass -File .\Start.ps1`。需要 .NET 9 SDK；首次运行需还原 NuGet 包。

## 管理端

入口：页脚 **Shop manager**，或 `/Manage`。

首次开发启动自动生成随机的本地管理密码，用户名为 `manager`。在 `src/HearthCup.Web/.local/admin-login.txt` 查看。`.local` 不位于静态文件目录，且被 Git 忽略。不要将该文件放进提交包或公开仓库。实际验证用的是哈希密码；本地文本仅供开发者取用。生产环境不会自动生成账号，需设置 `Admin:PasswordHash` 并部署 HTTPS。

## 已实现范围

- 商品新增、查看、修改、删除；删除前确认。
- 商品目录、名称/描述搜索、类型/材质/库存筛选、价格/名称排序、分页（每页 8 件）。
- 商品详情、价格、库存、容量、材质、颜色及本地商品照片。
- 无需买家注册的购物车：添加、查看、移除、合并同款、数量角标和小计。
- 服务端读取当前价格；加入时检查库存；查看时清理删除/售罄商品并收敛到当前库存。
- 最小管理登录保护，防伪令牌、参数校验、错误处理、数据库健康检查、编辑并发检测。
- 适配桌面和手机、键盘焦点、跳转正文、语义化标签、减少动态效果偏好。

购物车是内存 Session，空闲四小时过期，应用重启后丢失；商品持久存入数据库。没有订单、支付、注册、邮件、评论和物流功能，避免扩展 Assessment 范围。商品加入购物车不会预留或扣减库存。

## 解决方案结构

```text
src/HearthCup.Domain          商品实体和允许的分类/材质等选项
src/HearthCup.Application     业务规则、输入验证、DTO 和数据访问接口
src/HearthCup.Infrastructure  EF Core、SQL Server、仓储实现、初始数据
src/HearthCup.Web             MVC、Razor、HTTP/权限/会话、CSS 和本地 SVG
 tests/HearthCup.Checks       不依赖数据库的业务检查
 docs/                       架构说明、Assessment 对应与验证范围
```

数据访问依赖应用层接口，控制器调用业务服务；业务服务可替换仓储进行测试。

## 数据库

连接字符串在 `src/HearthCup.Web/appsettings.json` 的 `ConnectionStrings:Shop`。默认 Windows 集成认证连接 `.\SQLEXPRESS`，数据库名为 `HearthCup`。换电脑时按实际实例名调整。示例使用 EF Core `EnsureCreated` 初始化，不包含迁移链；后续更改表结构应引入 migrations 或对本项目专属开发数据库进行受控重建，不要直接删除其他数据库。

## 验证

```powershell
dotnet build HearthCup.sln
dotnet run --project tests/HearthCup.Checks
```

浏览器回归（需先启动网站，安装 Node.js 和 Chrome）：

```powershell
npm install
npm run check:browser
```

浏览器脚本从本地 `.local` 文件读取密码，不硬编码凭据。它创建一个 `QA Family Cup` 测试商品并在测试完成时删除；请只在本地开发数据库运行。输出在 `output/playwright/`。已执行情况见 `docs/validation.md`。

## 演示与报告准备

见 `docs/assessment-map.md`。这些是辅助理解项目的实现说明，不代替小组成员的真实贡献记录、个人反思或正式报告，也不是虚构的个人工作证明。AI 使用情况应按课程要求如实申报。

## 商品来源更新

已一次性导入 18 款真实商品照片、名称、规格和参考价格，并重写简短介绍。见 `docs/amazon-sources.md` 与 `data/amazon-catalogue.json`。包装件数保留在名称和来源资料中。店铺库存为本地演示数量，价格为 2026-10-20 的查询快照；不是实时亚马逊价格或库存。原虚构数据的备份在 Web/.local/catalogue-backups。每个来源商品详情页可点击 Amazon UK 链接核对。

## 首页与商品页

首页 `/` 仅展示品牌欢迎内容。点击 Find your favourite cup 或顶部 Shop 进入独立 `/Shop` 商品列表；顶部搜索从任意页面直接进入商品结果，支持名称、描述、颜色和材质。分类、筛选、排序和分页均在商品页完成；Logo 返回首页。
