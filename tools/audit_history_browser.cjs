// Actual completed bouts and trusted navigation only. No synthetic score/storage setters.
const {chromium}=require('playwright'),fs=require('fs'),path=require('path');
const dir=path.resolve(__dirname,'../evidence/wave1/history-wallet/'+(process.env.BOXER_HISTORY_PUBLIC==='1'?'history-browser-public':'history-browser'));
fs.mkdirSync(dir,{recursive:true});
const report={scope:'COMPILED_HISTORY_REAL_COMPLETION_TRUSTED_INPUT_NOT_PHONE_UAT',expectedChecks:28,checks:[],errors:[],states:{}};
function check(ok,name){report.checks.push({name,pass:!!ok});if(!ok)throw Error(name);}
(async()=>{
 const browser=await chromium.launch({executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',headless:true,args:['--use-angle=d3d11','--disable-background-timer-throttling','--disable-renderer-backgrounding']});let page;
 try{
  const ctx=await browser.newContext({viewport:{width:540,height:960},hasTouch:true});page=await ctx.newPage();const touch=await ctx.newCDPSession(page);
  page.on('pageerror',e=>report.errors.push(String(e)));page.on('console',m=>{if(m.type()==='error')report.errors.push(m.text());});page.on('requestfailed',r=>report.errors.push(r.url()+' '+r.failure()?.errorText));page.on('response',r=>{if(r.status()>=400)report.errors.push('HTTP '+r.status()+' '+r.url());});
  report.url=process.argv[2]||'http://127.0.0.1:8000/?desktop=1&metrics=1';await page.goto(report.url);report.productVersion=await page.evaluate(()=>productVersion);await page.locator('#enable').click();
  const snap=()=>page.evaluate(()=>window.boxerWave1Snapshot);
  async function state(label,screen){await page.waitForFunction(s=>window.boxerWave1Snapshot?.screen===s,screen,{timeout:150000});await page.waitForTimeout(200);const s=await snap();report.states[label]=s;return s;}
  async function settled(count){await page.waitForFunction(n=>window.boxerWave1Snapshot?.walletReady&&window.boxerWave1Snapshot.historyCount===n&&window.boxerWave1Snapshot.walletPending===0,count,{timeout:20000});return snap();}
  async function tap(x,y){const v=page.viewportSize(),scale=Math.min(v.width/540,v.height/960),ox=(v.width-540*scale)/2,oy=(v.height-960*scale)/2;await touch.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{id:1,x:ox+x*scale,y:oy+y*scale}]});await page.waitForTimeout(75);await touch.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});await page.waitForTimeout(180);}
  async function shot(name){await page.screenshot({path:path.join(dir,name+'.png')});}
  async function profile(name){await tap(270,852);await state('profile-'+name,'Profile');await page.locator('#profile-name').fill(name);await page.locator('#profile-nationality').fill('Việt Nam');await page.locator('#profile-save').click();return state('saved-'+name,'Home');}
  async function history(){await tap(270,852);await state('profile-history-link','Profile');await page.locator('#profile-history').click();return state('history','History');}
  async function home(){await page.locator('#history-home').click();return state('history-home','Home');}
  await state('fresh-home','Home');let s=await settled(0);check(s.walletBalance===0&&s.walletError==='', 'fresh wallet confirmed empty');
  s=await history();check(!s.gameplayInput&&!s.playerEnabled&&!s.opponentEnabled&&!s.captureKeyboard,'History locks fight and releases keyboard');
  check(await page.locator('#history-empty').innerText()==='CHƯA CÓ TRẬN ĐƯỢC LƯU · 0 COIN','honest empty history');
  check(await page.locator('#history-tally').innerText()==='THẮNG 0 · HÒA 0 · THUA 0','derived empty tally');await shot('empty-history');
  s=await home();check(s.captureKeyboard&&s.historyCount===0,'Home restores capture without award');
  await tap(270,763);await state('coach','Coach');await tap(270,274);await tap(270,808);await state('practice','Onboarding');await tap(270,569);await state('practice-exit','Coach');await tap(270,893);s=await state('practice-home','Home');check(s.historyCount===0&&s.walletBalance===0,'practice cancellation never rewarded');
  await tap(270,674);await state('cancel-preview','Preview');await tap(270,779);await state('cancel-intro','Intro');await page.locator('#ring-back').click();s=await state('cancel-home','Home');check(s.historyCount===0&&s.walletBalance===0,'cancelled intro never rewarded');
  s=await profile('Nguyễn Trí');const profileId=s.profileId;check(s.profileSaved&&/^[a-f0-9]{32}$/.test(profileId),'saved identity before genuine bout');
  await tap(270,674);await state('first-preview','Preview');await tap(270,779);await state('first-fight','Fight');s=await state('first-result','Result');const first=s.lastMatchReview;check(first&&first.sourceVersion===report.productVersion,'genuine completion has current compiled review');
  s=await settled(1);const reward=r=>r==='PLAYER_WIN'?100:r==='DRAW'?50:25,firstReward=reward(first.result);check(s.walletBalance===firstReward&&s.walletLatestId===first.id,'completed verdict pays approved amount exactly once');
  check(s.historyWins+s.historyDraws+s.historyLosses===1&&s.walletError==='','one confirmed completed tally');await shot('result-receipt');
  await tap(270,388);await state('result-history','History');check(await page.locator('.history-entry').count()===1,'Result opens saved history');
  check(await page.locator('.history-entry').getAttribute('data-match-id')===first.id,'stored ID equals genuine completed review');
  check((await page.locator('.history-entry summary').innerText()).includes('Nguyễn Trí')&&(await page.locator('.history-entry summary').innerText()).includes('+'+firstReward+' COIN'),'captured name and reward rendered');
  await page.locator('.history-entry summary').click();const endpoints=await page.locator('.history-endpoints').innerText();check(endpoints.includes('bạn '+first.playerHP.toFixed(1))&&endpoints.includes('Ramirez '+first.opponentHP.toFixed(1))&&endpoints.includes(report.productVersion),'actual endpoints and source rendered');await shot('first-history-expanded');
  s=await snap();check(s.historyCount===1&&s.walletBalance===firstReward&&s.walletPending===0,'viewing completion does not pay twice');
  await home();s=await profile('Minh Mới');check(s.profileId===profileId,'rename preserves local fighter ID');await history();check((await page.locator('.history-entry summary').innerText()).includes('Nguyễn Trí'),'rename never rewrites historical identity');
  for(const [w,h,label] of [[320,740,'320-portrait'],[375,812,'375-portrait'],[960,540,'landscape']]){
   await page.setViewportSize({width:w,height:h});await page.waitForTimeout(350);const layout=await page.evaluate(()=>{const p=document.getElementById('history-panel'),r=p.getBoundingClientRect();return {x:r.x,right:r.right,buttons:[...p.querySelectorAll('button:not(.hidden)')].map(b=>b.getBoundingClientRect().height),height:document.getElementById('match-history').getBoundingClientRect().height,viewport:visualViewport.height};});
   report.states[label]=layout;check(layout.x>=0&&layout.right<=w+1&&layout.buttons.every(h=>h>=48)&&Math.abs(layout.height-layout.viewport)<2,'bounded responsive history '+label);await page.locator('#history-home').scrollIntoViewIfNeeded();await shot('history-'+label);
  }
  await page.setViewportSize({width:540,height:960});await page.waitForTimeout(300);await home();await tap(270,674);await state('second-preview','Preview');await tap(270,779);await state('second-fight','Fight');s=await state('second-result','Result');const second=s.lastMatchReview;s=await settled(2);
  check(second.id!==first.id&&s.walletBalance===firstReward+reward(second.result),'distinct genuine bout appends and pays once');
  check(s.historyWins+s.historyDraws+s.historyLosses===2,'tally equals completed count');await tap(270,388);await state('second-history','History');
  check(await page.locator('.history-entry').count()===2,'both completed bouts retained');
  check((await page.locator('.history-entry summary').first().innerText()).includes('Minh Mới')&&(await page.locator('.history-entry summary').last().innerText()).includes('Nguyễn Trí'),'each bout freezes its start identity');
  const balance=s.walletBalance;await page.reload();await page.locator('#enable').click();await state('reload-home','Home');s=await settled(2);check(s.walletBalance===balance&&s.profileId===profileId,'real reload retains wallet and profile');
  await history();check(await page.locator('.history-entry').count()===2&&await page.locator('.history-entry').first().getAttribute('data-match-id')===second.id,'reload retains exact IDs without backfill');await shot('reloaded-history');
  check(report.errors.length===0,'zero JS/load/HTTP errors');if(report.checks.length!==report.expectedChecks)throw Error('Expected fixed 28 checks, got '+report.checks.length);report.status='PASS';
 }catch(e){report.status='FAIL';report.failure=String(e);process.exitCode=1;if(page){try{report.failureState=await page.evaluate(()=>window.boxerWave1Snapshot);await page.screenshot({path:path.join(dir,'failure.png')});}catch{}}}
 finally{fs.writeFileSync(path.join(dir,'report.json'),JSON.stringify(report,null,2));console.log(report.status,report.checks.length,report.failure||'');await browser.close();}
})();
