// Read-only snapshots + genuine browser touch/keyboard. No native setters or score injection.
const {chromium}=require('playwright'),fs=require('fs'),path=require('path');
const dir=path.resolve(__dirname,'../evidence/wave1/punch-feel/feel-browser');fs.mkdirSync(dir,{recursive:true});
const report={scope:'COMPILED_WEBGL_TOUCH_PROFILE_AB_NOT_PHONE_UAT',checks:[],errors:[],modes:{}};
function check(ok,name){report.checks.push({name,pass:!!ok});if(!ok)throw Error(name);}
(async()=>{
 const browser=await chromium.launch({executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',headless:true,
  args:['--use-angle=d3d11','--disable-background-timer-throttling','--disable-renderer-backgrounding']});
 try{
  for(const mode of ['legacy','curve','fast']){
   const context=await browser.newContext({viewport:{width:540,height:960},hasTouch:true});
   const page=await context.newPage(),touch=await context.newCDPSession(page),trace=report.modes[mode]={punches:[],balance:[]};
   page.on('pageerror',e=>report.errors.push(String(e)));page.on('console',m=>{if(m.type()==='error')report.errors.push(m.text());});
   page.on('response',r=>{if(r.status()>=400)report.errors.push('HTTP '+r.status()+' '+r.url());});
   const url=new URL(process.argv[2]||'http://127.0.0.1:8000/?desktop=1&metrics=1');url.searchParams.set('punchFeel',mode);
   await page.goto(url.href);report.productVersion=await page.evaluate(()=>productVersion);
   await page.locator('#enable').click();await page.waitForFunction(()=>window.boxerWave1Snapshot?.screen==='Home',null,{timeout:150000});
   const snap=()=>page.evaluate(()=>window.boxerWave1Snapshot);
   check((await snap()).punchFeel.toLowerCase()===mode,mode+' explicit profile selected');
   await page.mouse.click(270,759);await page.waitForFunction(()=>window.boxerWave1Snapshot?.trainingStage==='HEADCONTROL');
   for(const key of ['q','e']){await page.keyboard.down(key);await page.waitForTimeout(500);await page.keyboard.up(key);}
   await page.waitForFunction(()=>window.boxerWave1Snapshot.trainingReady);await page.mouse.click(270,494);
   await page.waitForFunction(()=>window.boxerWave1Snapshot.trainingStage==='FOOTWORK');
   for(const key of ['a','d','w','s']){await page.keyboard.down(key);await page.waitForTimeout(160);await page.keyboard.up(key);}
   await page.waitForFunction(()=>window.boxerWave1Snapshot.trainingReady);await page.mouse.click(270,354);
   await page.waitForFunction(()=>window.boxerWave1Snapshot.trainingStage==='PUNCHES');await page.waitForTimeout(250);
   const strokes=[['Straight',0,0],['Straight',0,0],['LeadHook',120,0],['RearHook',-120,0],['Uppercut',0,-120],['Overhand',0,120]];
   for(const [name,dx,dy] of strokes){
    const before=await snap();const now=Date.now();
    await touch.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{id:2,x:400,y:750}]});await page.waitForTimeout(40);
    if(dx||dy)await touch.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:[{id:2,x:400+dx,y:750+dy}]});
    await page.waitForTimeout(40);await touch.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});
    await page.waitForFunction(n=>window.boxerWave1Snapshot.acceptedPunches===n,before.acceptedPunches+1,{timeout:3000});
    await page.waitForFunction(()=>window.boxerWave1Snapshot.playerPhase==='Guard',null,{timeout:3000});
    const after=await snap();trace.punches.push({name,wallMs:Date.now()-now,after});
    check(after.acceptedPunches===before.acceptedPunches+1,mode+' '+name+' touch accepted once');
   }
   let s=await snap();check(s.trainingReady&&s.playerHP===100&&s.opponentHP===100&&s.impactHits===0&&s.impactBlocks===0&&s.swings===6,
    mode+' unscored training has all families and air sound but no fake HIT/BLOCK');
   const quick=trace.punches.filter(p=>p.name!=='Straight');
   check(quick.some(p=>p.after.gestureDurationMs>=60&&p.after.gestureDurationMs<120),mode+' measured decisive swipe below old 120ms threshold');
   check(quick.every(p=>p.after.releaseToAcceptMs>=0&&p.after.releaseToAcceptMs<34),mode+' accepted release latency under two 60Hz frames, desktop only');
   const expected=mode==='fast'?495:510;check(Math.abs(s.recoveryAgeMs-expected)<2,mode+' overhand full cycle matches phase timeline');
   await page.screenshot({path:path.join(dir,mode+'-training.png')});
   // Browser cancel must not be converted into a punch.
   const accepted=s.acceptedPunches;
   await touch.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{id:2,x:420,y:750}]});await page.waitForTimeout(80);
   await touch.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:[{id:2,x:420,y:630}]});
   await touch.send('Input.dispatchTouchEvent',{type:'touchCancel',touchPoints:[]});await page.waitForTimeout(200);
   check((await snap()).acceptedPunches===accepted,mode+' cancelled touch does not punch');
   await page.mouse.click(270,429);await page.waitForFunction(()=>window.boxerWave1Snapshot.screen==='Home');
   await page.mouse.click(270,674);await page.waitForFunction(()=>window.boxerWave1Snapshot.screen==='Preview');
   await page.mouse.click(270,779);await page.waitForFunction(()=>window.boxerWave1Snapshot.screen==='Fight');await page.waitForTimeout(250);
   for(let i=0;i<10;i++){
    await page.waitForFunction(()=>window.boxerWave1Snapshot.playerPhase==='Guard');
    await page.keyboard.press(i%2?'k':'j');
    await page.waitForFunction(n=>window.boxerWave1Snapshot.acceptedPunches===n,i+1);
    await page.waitForFunction(()=>window.boxerWave1Snapshot.playerPhase==='Guard');trace.balance.push(await snap());
   }
   s=await snap();check(s.acceptedPunches===10&&s.impactHits===s.playerHits+s.opponentHits&&s.impactBlocks===s.playerBlocks+s.opponentBlocks,
    mode+' scored real-input receipts and HIT/BLOCK feedback match authoritative counters');
   check(s.impactMisses===s.playerMisses+s.opponentMisses,mode+' MISS has no false HIT feedback');
   await page.screenshot({path:path.join(dir,mode+'-fight.png')});await context.close();
  }
  report.status=report.errors.length?'FAIL':'PASS';
 }catch(e){report.status='FAIL';report.errors.push(String(e));}
 finally{fs.writeFileSync(path.join(dir,'report.json'),JSON.stringify(report,null,2));console.log('PUNCH_FEEL_BROWSER',report.status,report.checks.length,report.errors);await browser.close();}
 if(report.status!=='PASS')process.exitCode=1;
})().catch(e=>{console.error(e);process.exitCode=1;});
