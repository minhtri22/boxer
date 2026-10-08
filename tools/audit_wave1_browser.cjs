// Real UI/keyboard input only. Read-only Unity snapshot; not a device/Human UAT.
const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');
const suite=process.env.BOXER_WAVE_EVIDENCE==='combat-v3'?'combat-v3':'onboarding-v2';
const evidence = path.resolve(__dirname, '../evidence/wave1/'+suite+'/browser');
fs.mkdirSync(evidence, { recursive: true });
const report = { scope: 'SYNTHETIC_DESKTOP_REAL_UI_NOT_DEVICE_UAT', checks: [], states: {}, errors: [] };
function check(ok, name) { report.checks.push({name, pass: !!ok}); if (!ok) throw new Error(name); }
(async () => {
  const browser = await chromium.launch({executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', headless:true,
    args:['--use-angle=d3d11','--disable-background-timer-throttling','--disable-renderer-backgrounding']});
  try {
    const page = await browser.newPage({viewport:{width:540,height:960},hasTouch:true});
    const pendingAssets=new Set();
    page.on('request',r=>{if(r.url().includes('/Build/'))pendingAssets.add(r);});
    page.on('requestfinished',r=>pendingAssets.delete(r));
    page.on('requestfailed',r=>pendingAssets.delete(r));
    const touch = await page.context().newCDPSession(page);
    async function gesture(x,y,dx=0,dy=0) {
      await touch.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x,y}]});
      await page.waitForTimeout(100);
      if(dx||dy)await touch.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:[{x:x+dx,y:y+dy}]});
      await page.waitForTimeout(200);
      await touch.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});
      await page.waitForTimeout(850);
    }
    page.on('pageerror', e=>report.errors.push(String(e)));
    page.on('console', m=>{if(m.type()==='error')report.errors.push(m.text());});
    page.on('requestfailed', r=>report.errors.push(r.url()+' '+r.failure()?.errorText));
    page.on('response', r=>{if(r.status()>=400)report.errors.push('HTTP '+r.status()+' '+r.url());});
    const snap = () => page.evaluate(()=>window.boxerWave1Snapshot);
    async function state(name, screen) {
      await page.waitForFunction(s=>window.boxerWave1Snapshot?.screen===s,screen,{timeout:150000});
      report.states[name]=await snap();
      await page.screenshot({path:path.join(evidence,name+'.png')});
      console.log(name,JSON.stringify(report.states[name]));
      return report.states[name];
    }
    await page.goto(process.argv[2] || 'http://127.0.0.1:8000/?desktop=1&metrics=1');
    report.productVersion=await page.evaluate(()=>productVersion);
    const iconUrl=await page.locator('link[rel="icon"]').getAttribute('href');
    const iconResponse=await page.request.get(new URL(iconUrl,page.url()).href);
    const iconBytes=await iconResponse.body();
    check(iconResponse.status()===200&&iconBytes.readUInt16LE(0)===0&&iconBytes.readUInt16LE(2)===1&&iconBytes.readUInt16LE(4)===6,'boxer favicon loads as six-frame ICO');
    report.favicon={url:iconUrl,bytes:iconBytes.length,sha256:require('crypto').createHash('sha256').update(iconBytes).digest('hex')};
    await page.locator('#enable').click();
    const home=await state('home','Home');
    check(home.playerHP===100&&!home.gameplayInput&&!home.playerEnabled&&!home.opponentEnabled,'home locks gameplay');
    await page.keyboard.press('k'); await page.waitForTimeout(500);
    check((await snap()).playerCapacity===100,'menu keyboard cannot spend capacity');
    await page.mouse.click(270,674);
    const preview=await state('preview','Preview');
    check(!preview.gameplayInput,'preview locks gameplay');
    await page.mouse.click(270,779);
    const fightStarted=Date.now();
    const initialFight=await state('fight-start','Fight');
    check(Date.now()-fightStarted<5000&&initialFight.result==='IN_PROGRESS'&&!initialFight.trainingStage,'first bout starts without tutorial wait');
    const before=await snap();
    await page.keyboard.down('k'); await page.waitForTimeout(120); await page.keyboard.up('k');
    await page.waitForTimeout(250);
    const after=await snap(); report.states['accepted-cross']=after;
    check(after.playerCapacity<before.playerCapacity&&after.playerStamina<before.playerStamina,'real accepted keyboard punch spends resources');
    await page.screenshot({path:path.join(evidence,'fight-punch.png')});
    for(let i=0;i<12;i++){await page.keyboard.down(i%2?'k':'j');await page.waitForTimeout(100);await page.keyboard.up(i%2?'k':'j');await page.waitForTimeout(600);}
    await state('fight-mid','Fight');
    const result=await state('result','Result');
    check(!result.gameplayInput&&!result.playerEnabled&&!result.opponentEnabled,'result locks both controllers and input');
    check(['KO','POINTS'].includes(result.reason)&&result.result!=='PENDING','result has actual verdict and reason');
    await page.waitForTimeout(750);
    const frozen=await snap();check(frozen.playerHP===result.playerHP&&frozen.playerStamina===result.playerStamina,'resources freeze after result');
    await page.mouse.click(270,779);
    const rematch=await state('rematch','Fight');
    check(rematch.playerHP===100&&rematch.opponentHP===100&&rematch.playerCapacity===100&&rematch.players===1&&rematch.opponents===1,'rematch resets without actor duplication');
    await state('result-rematch','Result');
    await page.mouse.click(270,869); await state('home-return','Home');
    await page.mouse.click(270,759);
    const practice=await state('practice-head','Onboarding');
    check(practice.seconds===0&&practice.playerHP===100&&!practice.opponentEnabled&&!practice.tutorialSeen,'first-user training is separate unscored AI-disabled');
    await page.waitForTimeout(11000);
    check((await snap()).trainingStage==='HEADCONTROL'&&!(await snap()).trainingReady,'idle training does not advance by old timer');
    await page.mouse.click(270,494);await page.waitForTimeout(350);
    check((await snap()).trainingStage==='HEADCONTROL','next requires actual practice');
    const firstGuide=(await snap()).trainingGuide;
    await page.waitForFunction(cue=>window.boxerWave1Snapshot?.trainingGuide!==cue,firstGuide,{timeout:5000});
    check(['HEAD_LEFT','HEAD_RIGHT'].includes(firstGuide)&&['HEAD_LEFT','HEAD_RIGHT'].includes((await snap()).trainingGuide),'head light guide alternates left and right without granting progress');
    await page.screenshot({path:path.join(evidence,'practice-head-light.png')});
    for(const key of ['q','e']){await page.keyboard.down(key);await page.waitForTimeout(600);await page.keyboard.up(key);await page.waitForTimeout(250);}
    check((await snap()).trainingReady,'synthetic head movement completes both directions');
    await state('practice-head-ready','Onboarding');
    await page.mouse.click(270,494);await page.waitForTimeout(400);
    check((await snap()).trainingStage==='FOOTWORK','explicit Next opens movement lesson');
    for(const cue of ['MOVE_UP','MOVE_DOWN','MOVE_LEFT','MOVE_RIGHT']) {
      await page.waitForFunction(c=>window.boxerWave1Snapshot?.trainingGuide===c,cue,{timeout:12000});
      await page.screenshot({path:path.join(evidence,'guide-'+cue.toLowerCase()+'.png')});
    }
    check(!(await snap()).trainingReady,'all four movement light examples remain illustrative');
    // Real browser touch events, not hidden gameplay method calls; still not phone sensor UAT.
    for(const [dx,dy] of [[-100,0],[100,0],[0,-100],[0,100]])await gesture(135,750,dx,dy);
    check((await snap()).trainingReady,'lower-left touch swipes register all four movement directions');
    await state('practice-feet-ready','Onboarding');
    await page.mouse.click(270,354);await page.waitForTimeout(400);
    check((await snap()).trainingStage==='PUNCHES','explicit Next opens punch lesson');
    for(const cue of ['PUNCH_DOWN','PUNCH_UP','PUNCH_RIGHT','PUNCH_LEFT','TAP_REPEAT']) {
      await page.waitForFunction(c=>window.boxerWave1Snapshot?.trainingGuide===c,cue,{timeout:15000});
      await page.screenshot({path:path.join(evidence,'guide-'+cue.toLowerCase()+'.png')});
      if(cue==='TAP_REPEAT') { await page.waitForTimeout(180);await page.screenshot({path:path.join(evidence,'guide-tap-pulse.png')}); }
    }
    check(!(await snap()).trainingReady,'four directional punch guides and repeating tap do not complete practice');
    for(const [dx,dy] of [[0,-100],[0,100],[-100,0],[100,0],[0,0],[0,0]])await gesture(420,750,dx,dy);
    check((await snap()).trainingReady,'lower-right touch up down left right and repeated taps complete punch lesson');
    const practiced=await state('practice-punches-ready','Onboarding');
    check(practiced.playerHP===100&&practiced.opponentHP===100&&practiced.playerStamina===100&&practiced.seconds===0&&!practiced.opponentEnabled,'practice never changes scored HP stamina or timer');
    await page.mouse.click(270,354);
    const trained=await state('practice-complete-home','Home');
    check(trained.tutorialSeen&&!trained.gameplayInput,'completed training returns Home and records completion');
    await page.mouse.click(270,759);await state('practice-repeat','Onboarding');
    await page.mouse.click(270,569);const exited=await state('practice-exit-home','Home');
    check(exited.tutorialSeen&&!exited.gameplayInput&&exited.seconds===0,'practice exit does not auto-start a match');
    // Do not cancel a still-running payload/cache transfer with our own navigation.
    const assetsDeadline=Date.now()+45000;
    while(pendingAssets.size&&Date.now()<assetsDeadline)await page.waitForTimeout(500);
    check(pendingAssets.size===0,'payload transfers finish before deliberate reload');
    await page.waitForTimeout(2500);await page.reload();await page.locator('#enable').click();
    const restored=await state('reload-home','Home');
    check(restored.tutorialSeen&&!restored.gameplayInput,'completed onboarding persists across reload');
    check(!restored.trainingGuide&&!initialFight.trainingGuide,'light guides hidden outside training');
    check(report.errors.length===0,'browser has no JS/load errors');
    report.status='PASS';
  } catch(e) {report.status='FAIL';report.failure=String(e);process.exitCode=1;}
  finally {await browser.close();fs.writeFileSync(path.join(evidence,'report.json'),JSON.stringify(report,null,2));console.log(report.status,report.failure||'');}
})();
