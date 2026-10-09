// Actual keyboard/menu input into compiled WebGL. No score or pose setters.
const {chromium}=require('playwright'),fs=require('fs'),path=require('path');
const label=process.argv[2]||'baseline';
if(!/^[a-z0-9-]+$/.test(label))throw Error('Invalid capture label');
const dir=path.resolve(__dirname,'../evidence/wave1/punch-feel/clips/'+label);
fs.mkdirSync(dir,{recursive:true});
(async()=>{
 const browser=await chromium.launch({executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',headless:true,
  args:['--use-angle=d3d11','--disable-background-timer-throttling','--disable-renderer-backgrounding']});
 const context=await browser.newContext({viewport:{width:540,height:960},recordVideo:{dir,size:{width:540,height:960}}});
 const page=await context.newPage();const trace={scope:'COMPILED_WEBGL_KEYBOARD_NOT_PHONE_UAT',label,errors:[],events:[]};
 page.on('pageerror',e=>trace.errors.push(String(e)));
 try{
  await page.goto(process.argv[3]||'http://127.0.0.1:8000/?desktop=1&metrics=1');
  trace.productVersion=await page.evaluate(()=>productVersion);
  await page.locator('#enable').click();
  await page.waitForFunction(()=>window.boxerWave1Snapshot?.screen==='Home',null,{timeout:150000});
  await page.mouse.click(270,674);await page.waitForFunction(()=>window.boxerWave1Snapshot?.screen==='Preview');
  await page.mouse.click(270,779);await page.waitForFunction(()=>window.boxerWave1Snapshot?.screen==='Fight');
  await page.waitForTimeout(350);
  for(const [name,key] of [['jab','j'],['cross','k'],['hook','l'],['uppercut','u'],['overhand','o']]){
   trace.events.push({name,at:Date.now(),before:await page.evaluate(()=>window.boxerWave1Snapshot)});
   await page.keyboard.press(key);await page.waitForTimeout(1100);
   trace.events.push({name,after:await page.evaluate(()=>window.boxerWave1Snapshot)});
  }
  await page.keyboard.down('w');await page.waitForTimeout(280);await page.keyboard.up('w');
  for(const key of ['k','o','j','l','u']){await page.keyboard.press(key);await page.waitForTimeout(1000);}
  await page.screenshot({path:path.join(dir,'pov.png')});
 }finally{
  fs.writeFileSync(path.join(dir,'trace.json'),JSON.stringify(trace,null,2));
  await context.close();console.log('VIDEO',await page.video().path());await browser.close();
 }
})().catch(e=>{console.error(e);process.exitCode=1;});
