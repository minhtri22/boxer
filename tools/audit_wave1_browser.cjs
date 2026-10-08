// Real UI/keyboard input only. Read-only Unity snapshot; not a device/Human UAT.
const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');
const evidence = path.resolve(__dirname, '../evidence/wave1/browser');
fs.mkdirSync(evidence, { recursive: true });
const report = { scope: 'SYNTHETIC_DESKTOP_REAL_UI_NOT_DEVICE_UAT', checks: [], states: {}, errors: [] };
function check(ok, name) { report.checks.push({name, pass: !!ok}); if (!ok) throw new Error(name); }
(async () => {
  const browser = await chromium.launch({executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', headless:true,
    args:['--use-angle=d3d11','--disable-background-timer-throttling','--disable-renderer-backgrounding']});
  try {
    const page = await browser.newPage({viewport:{width:540,height:960}});
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
    await page.locator('#enable').click();
    const home=await state('home','Home');
    check(home.playerHP===100&&!home.gameplayInput&&!home.playerEnabled&&!home.opponentEnabled,'home locks gameplay');
    await page.keyboard.press('k'); await page.waitForTimeout(500);
    check((await snap()).playerCapacity===100,'menu keyboard cannot spend capacity');
    await page.mouse.click(270,674);
    const preview=await state('preview','Preview');
    check(!preview.gameplayInput,'preview locks gameplay');
    await page.mouse.click(270,779);
    const training=await state('tutorial','Onboarding');
    check(training.playerHP===100&&training.seconds===0,'tutorial unscored');
    // Let the existing tutorial timers advance; no hidden StartBout injection.
    await state('fight-start','Fight');
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
    check(report.errors.length===0,'browser has no JS/load errors');
    report.status='PASS';
  } catch(e) {report.status='FAIL';report.failure=String(e);process.exitCode=1;}
  finally {await browser.close();fs.writeFileSync(path.join(evidence,'report.json'),JSON.stringify(report,null,2));console.log(report.status,report.failure||'');}
})();
