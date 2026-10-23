const {chromium}=require(process.env.HEARTH_PLAYWRIGHT||'playwright');
const fs=require('node:fs/promises');const path=require('node:path');
(async()=>{
 const browser=await chromium.launch({channel:'chrome',headless:true});const context=await browser.newContext();const page=await context.newPage();const output={capturedAt:new Date().toISOString(),candidates:[]};
 try{for(const [category,q] of [['Mugs','ceramic mugs'],['Glassware','glass drinking tumblers'],['Tumblers','insulated tumbler straw'],['Travel cups','travel mug insulated']]){
 await page.goto('https://www.amazon.co.uk/s?k='+encodeURIComponent(q),{waitUntil:'domcontentloaded',timeout:45000});
 const title=await page.title();if(/robot|captcha/i.test(title))throw new Error('Amazon requires verification; collection stopped.');
 await page.locator('[data-component-type=s-search-result]').first().waitFor({timeout:15000});
 const items=await page.locator('[data-component-type=s-search-result]').evaluateAll(cards=>cards.slice(0,10).map(c=>({asin:c.getAttribute('data-asin'),title:c.querySelector('h2')?.textContent,image:c.querySelector('img.s-image')?.src,price:c.querySelector('.a-price .a-offscreen')?.textContent})));
 output.candidates.push(...items.map(x=>({...x,category,sourceUrl:'https://www.amazon.co.uk/dp/'+x.asin})));console.log(JSON.stringify({category,items}));
 }
 await fs.writeFile(path.resolve('data/amazon-candidates.json'),JSON.stringify(output,null,2));
 }finally{await browser.close();}
})().catch(e=>{console.error(e.message);process.exitCode=1;});
