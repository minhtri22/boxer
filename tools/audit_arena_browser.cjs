// Actual compiled UI/trusted touch; diagnostics are strictly read-only. Not physical phone UAT.
const {chromium}=require('playwright'),fs=require('fs'),path=require('path');
const dir=path.resolve(__dirname,'../evidence/wave1/'+(process.env.BOXER_WAVE_EVIDENCE==='fighter-profile'?'fighter-profile':'arena-surround')+'/'+(process.env.BOXER_ARENA_PUBLIC==='1'?'arena-browser-public':'arena-browser'));
fs.mkdirSync(dir,{recursive:true});
const report={scope:'REAL_COMPILED_UI_READONLY_ARENA_OBSERVATION_NOT_PHONE_VISUAL_UAT',checks:[],errors:[],states:{},trace:[]};
function check(ok,name){report.checks.push({name,pass:!!ok});if(!ok)throw Error(name);}
function off(s){return s.audienceEnabledFlashes===0&&s.audienceActiveFlash===-1&&s.audienceFlashStrength===0&&s.audienceObservedSideMask===0;}
(async()=>{
 const browser=await chromium.launch({executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',headless:true,args:['--use-angle=d3d11','--disable-background-timer-throttling','--disable-renderer-backgrounding']});
 try{
  const ctx=await browser.newContext({viewport:{width:540,height:960},hasTouch:true}),page=await ctx.newPage(),touch=await ctx.newCDPSession(page);
  page.on('pageerror',e=>report.errors.push(String(e)));page.on('console',m=>{if(m.type()==='error')report.errors.push(m.text());});
  page.on('requestfailed',r=>report.errors.push(r.url()+' '+r.failure()?.errorText));page.on('response',r=>{if(r.status()>=400)report.errors.push('HTTP '+r.status()+' '+r.url());});
  report.url=process.argv[2]||'http://127.0.0.1:8000/?desktop=1&metrics=1';await page.goto(report.url);report.productVersion=await page.evaluate(()=>productVersion);await page.locator('#enable').click();
  const snap=()=>page.evaluate(()=>window.boxerWave1Snapshot);
  async function state(label,screen){await page.waitForFunction(s=>window.boxerWave1Snapshot?.screen===s,screen,{timeout:150000});await page.waitForTimeout(100);const s=await snap();report.states[label]=s;return s;}
  async function tap(x,y){await touch.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{id:1,x,y}]});await page.waitForTimeout(75);await touch.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});await page.waitForTimeout(150);}
  let s=await state('home','Home');check(s.audienceSides===4&&s.audienceFlashNodes===24,'four compiled scenery sides and bounded twenty-four flash nodes');check(off(s),'Home effect disabled');
  await tap(270,763);s=await state('coach','Coach');check(off(s)&&s.seconds===0&&!s.gameplayInput,'Coach has no flash or scored gameplay');
  await tap(390,274);await tap(270,808);s=await state('practice','Onboarding');await page.waitForTimeout(800);check(off(await snap())&&s.seconds===0&&!s.opponentEnabled,'unscored practice has no fight flash');
  await tap(270,s.trainingStage==='HEADCONTROL'?569:429);await state('coach-return','Coach');await tap(270,893);await state('home-return','Home');
  await tap(270,674);s=await state('preview','Preview');check(off(s)&&s.seconds===0,'Preview effect disabled');
  await tap(270,779);s=await state('intro','Intro');check(off(s)&&s.seconds===0&&!s.opponentEnabled,'ring intro has no flashes or scored clock');
  s=await state('fight','Fight');check(s.audienceSides===4&&s.audienceFlashNodes===24&&s.seconds<1&&s.result==='IN_PROGRESS','actual intro completion starts fresh Fight with scenery');
  const until=Date.now()+7500;while(Date.now()<until){const sample=await snap();report.trace.push({screen:sample.screen,seconds:sample.seconds,index:sample.audienceActiveFlash,strength:sample.audienceFlashStrength,enabled:sample.audienceEnabledFlashes,mask:sample.audienceObservedSideMask});await page.waitForTimeout(25);}
  s=await snap();check(s.screen==='Fight'&&s.audienceObservedSideMask===15,'actual update loop observed localized flashes on all four sides');
  check(report.trace.some(x=>x.enabled===1)&&report.trace.some(x=>x.enabled===0)&&report.trace.every(x=>x.enabled<=1&&x.strength>=0&&x.strength<=.75001&&x.index>=-1&&x.index<24),'actual renderers sparse one-at-time and bounded intensity');
  await page.screenshot({path:path.join(dir,'fight-audience.png')});
  s=await state('result','Result');check(off(s)&&!s.gameplayInput&&!s.playerEnabled&&!s.opponentEnabled,'actual terminal Result clears flash and gameplay input');
  await tap(270,869);s=await state('final-home','Home');check(off(s),'return Home remains reset');
  check(report.errors.length===0,'zero browser JS/load/HTTP errors');report.status='PASS';
 }catch(e){report.status='FAIL';report.failure=String(e);process.exitCode=1;}
 finally{fs.writeFileSync(path.join(dir,'report.json'),JSON.stringify(report,null,2));console.log(report.status,report.checks.length,report.failure||'');await browser.close();}
})();
