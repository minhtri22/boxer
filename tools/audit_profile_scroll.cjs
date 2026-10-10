// Additive mobile-viewport touch scroll probe; never evidence of a real soft keyboard.
const {chromium}=require('playwright'),fs=require('fs'),path=require('path');
const dir=path.resolve(__dirname,'../evidence/wave1/fighter-profile/profile-scroll');fs.mkdirSync(dir,{recursive:true});
const report={scope:'TRUSTED_CDP_TOUCH_SCROLL_MOBILE_EMULATION_NOT_PHONE_KEYBOARD_UAT',checks:[],errors:[]};
function check(ok,name){report.checks.push({name,pass:!!ok});if(!ok)throw Error(name);}
(async()=>{
 const browser=await chromium.launch({executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',headless:true,args:['--use-angle=d3d11']});
 try{
  const ctx=await browser.newContext({viewport:{width:375,height:812},isMobile:true,hasTouch:true}),page=await ctx.newPage(),cdp=await ctx.newCDPSession(page);
  page.on('pageerror',e=>report.errors.push(String(e)));page.on('console',m=>{if(m.type()==='error')report.errors.push(m.text());});
  await page.goto('http://127.0.0.1:8000/?desktop=1&metrics=1');report.productVersion=await page.evaluate(()=>productVersion);await page.locator('#enable').click();
  await page.waitForFunction(()=>window.boxerWave1Snapshot?.screen==='Home');
  const scale=375/540,oy=(812-960*scale)/2;
  await cdp.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{id:1,x:270*scale,y:oy+852*scale}]});await page.waitForTimeout(75);await cdp.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});
  await page.waitForFunction(()=>window.boxerWave1Snapshot?.screen==='Profile');await page.setViewportSize({width:375,height:360});await page.waitForTimeout(500);
  const before=await page.locator('#fighter-profile').evaluate(e=>e.scrollTop);
  await cdp.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{id:2,x:185,y:320}]});
  for(let y=290;y>=60;y-=30){await cdp.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:[{id:2,x:185,y}]});await page.waitForTimeout(35);}
  await cdp.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});await page.waitForTimeout(500);
  const after=await page.locator('#fighter-profile').evaluate(e=>e.scrollTop);report.scroll={before,after};check(after>before+20,'trusted vertical touch scrolls form without programmatic scroll');
  const s=await page.evaluate(()=>window.boxerWave1Snapshot);check(!s.captureKeyboard&&!s.gameplayInput&&s.seconds===0&&s.acceptedPunches===0&&s.audienceEnabledFlashes===0,'scroll cannot trigger gameplay or audience flash');
  await page.locator('#profile-cancel').scrollIntoViewIfNeeded();await page.screenshot({path:path.join(dir,'reduced-viewport-touch-scroll.png')});await page.locator('#profile-cancel').tap();
  await page.waitForFunction(()=>window.boxerWave1Snapshot?.screen==='Home'&&window.boxerWave1Snapshot.captureKeyboard);
  check(await page.locator('#fighter-profile').isHidden(),'trusted Cancel closes form and restores capture');
  check(report.errors.length===0,'no browser JS errors');report.status='PASS';
 }catch(e){report.status='FAIL';report.failure=String(e);process.exitCode=1;}
 finally{fs.writeFileSync(path.join(dir,'report.json'),JSON.stringify(report,null,2));console.log(report.status,report.checks.length,report.failure||'');await browser.close();}
})();
