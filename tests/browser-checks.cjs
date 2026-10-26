const { chromium } = require(process.env.HEARTH_PLAYWRIGHT || 'playwright');
const fs = require('node:fs/promises');
const path = require('node:path');
const assert = require('node:assert/strict');

// End-to-end regression for the two-role shop: registers a throwaway buyer, buys a cup, ships the
// order as the manager, and checks the buyer sees the new status. Needs the site running.
(async () => {
 const root = path.resolve(__dirname, '..');
 const out = path.join(root, 'output/playwright'); await fs.mkdir(out, {recursive:true});
 const base = process.env.HEARTH_URL || 'http://127.0.0.1:5188';
 const admin = await fs.readFile(path.join(root, 'src/HearthCup.Web/.local/admin-login.txt'), 'utf8');
 const adminEmail = admin.match(/Email: (\S+)/)[1];
 const adminPassword = admin.match(/Password: (\S+)/)[1];
 const buyerEmail = 'qa' + Date.now() + '@example.com';
 const buyerPassword = '135790';

 const browser = await chromium.launch({channel:'chrome', headless:true});
 const buyerCtx = await browser.newContext({viewport:{width:1440,height:1000}});
 const page = await buyerCtx.newPage();
 const errors=[]; const external=[];
 page.on('pageerror', e => errors.push(e.message));
 page.on('console', m => { if(m.type()==='error' && !m.text().includes('Failed to load resource')) errors.push(m.text()); });
 page.on('request', r => { if(!r.url().startsWith(base) && !r.url().startsWith('data:')) external.push(r.url()); });

 const results=[];
 const check = async (name, fn) => { await fn(); results.push(name); console.log('PASS ' + name); };
 const go = route => page.goto(base + route);
 const main = () => page.locator('main').innerText();

 let orderId = 0;
 try {
  await check('Anonymous catalogue lists eight cups per page', async () => {
   await go('/Shop');
   assert.equal(await page.locator('.product-card').count(), 8);
   assert.match(await main(), /\d+ little reasons/);
   await page.screenshot({path: path.join(out,'shop-desktop.png'), fullPage:true});
  });

  await check('Anonymous visitor is sent to sign in for the basket', async () => {
   await page.goto(base + '/Cart');
   assert.match(new URL(page.url()).pathname, /\/Account\/Login/);
  });

  await check('A buyer registers with an email and six digits', async () => {
   await go('/Account/Register');
   await page.getByLabel('Email', {exact:true}).fill(buyerEmail);
   await page.getByLabel('Six digit password', {exact:true}).fill(buyerPassword);
   await page.getByLabel('Repeat the password', {exact:true}).fill(buyerPassword);
   await page.getByRole('button', {name:/Create account/}).click();
   assert.equal(new URL(page.url()).pathname, '/Shop');
  });

  await check('Basket accepts a cup and the quantity can be raised', async () => {
   await go('/Home/Details/1');
   await page.getByRole('button', {name:/Make it yours/}).click();
   await go('/Cart');
   await page.getByRole('button', {name:/Increase quantity/}).first().click();
   assert.equal((await page.locator('.qty-control span').first().innerText()).trim(), '2');
   await page.screenshot({path: path.join(out,'basket-desktop.png'), fullPage:true});
  });

  await check('Checkout creates a pending order', async () => {
   await page.getByRole('button', {name:/Place the order/}).click();
   const text = await main();
   orderId = Number((text.match(/Order #(\d+)/) || [0,0])[1]);
   assert.ok(orderId > 0, 'order id missing');
   assert.match(text, /Pending/i);
  });

  await check('A buyer cannot open the manager screens', async () => {
   await page.goto(base + '/Manage');
   assert.match(new URL(page.url()).pathname, /\/Account\/Login/);
  });

  const managerCtx = await browser.newContext({viewport:{width:1440,height:1000}});
  const adminPage = await managerCtx.newPage();
  await check('The manager signs in and sees the order with the buyer email', async () => {
   await adminPage.goto(base + '/Account/Login');
   await adminPage.getByLabel('Email', {exact:true}).fill(adminEmail);
   await adminPage.getByLabel('Password', {exact:true}).fill(adminPassword);
   await adminPage.getByRole('button', {name:/Sign in/}).click();
   await adminPage.goto(base + '/Manage/Orders');
   const card = adminPage.locator('.order-card', {hasText: buyerEmail}).first();
   assert.equal(await card.count(), 1);
   assert.match(await card.innerText(), new RegExp('Order #' + orderId));
   await adminPage.screenshot({path: path.join(out,'manage-orders.png'), fullPage:true});
  });

  await check('The manager marks the order as shipped', async () => {
   const card = adminPage.locator('.order-card', {hasText: buyerEmail}).first();
   await card.getByRole('button', {name:/Mark as shipped/}).click();
   const updated = adminPage.locator('.order-card', {hasText: buyerEmail}).first();
   assert.match(await updated.innerText(), /Shipped/i);
  });

  await check('The buyer sees the shipped status', async () => {
   await go('/Orders');
   assert.match(await main(), new RegExp('Order #' + orderId));
   assert.match(await main(), /Shipped/i);
  });

  await check('Delisting a cup hides it from the shop and keeps it in the manager list', async () => {
   await adminPage.goto(base + '/Manage');
   const before = await (await adminPage.request.get(base + '/Shop')).text();
   await adminPage.getByRole('button', {name:'Delist'}).first().click();
   const after = await (await adminPage.request.get(base + '/Shop')).text();
   assert.notEqual(before.match(/(\d+) little reasons/)[1], after.match(/(\d+) little reasons/)[1]);
   await adminPage.getByRole('button', {name:'List'}).first().click();
   await managerCtx.close();
  });

  await check('Mobile layout has no horizontal clipping', async () => {
   await page.setViewportSize({width:390,height:844});
   await go('/');
   assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth > innerWidth), false);
   await page.screenshot({path: path.join(out,'home-mobile.png'), fullPage:true});
  });

  await check('No page errors and no external requests', async () => {
   assert.deepEqual(errors, []);
   assert.deepEqual(external, []);
  });
 } finally {
  await fs.writeFile(path.join(out,'browser-results.json'),
   JSON.stringify({date:new Date().toISOString(), passed:results.length, checks:results}, null, 2));
  await buyerCtx.close(); await browser.close();
 }
 console.log(results.length + ' browser checks passed.');
})().catch(e => { console.error('FAILED: ' + e.message); process.exit(1); });
