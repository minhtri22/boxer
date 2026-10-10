// Real compiled navigation + browser text entry. Read-only snapshots, no gameplay setters.
const {chromium}=require('playwright'),fs=require('fs'),path=require('path');
const dir=path.resolve(__dirname,'../evidence/wave1/fighter-profile/'+(process.env.BOXER_PROFILE_PUBLIC==='1'?'profile-browser-public':'profile-browser'));
fs.mkdirSync(dir,{recursive:true});
const report={scope:'COMPILED_PROFILE_TRUSTED_INPUT_SYNTHETIC_VIEWPORT_NOT_PHONE_KEYBOARD_UAT',checks:[],errors:[],states:{}};
function check(ok,name){report.checks.push({name,pass:!!ok});if(!ok)throw Error(name);}
function frozen(s){return !s.gameplayInput&&!s.playerEnabled&&!s.opponentEnabled&&s.seconds===0&&s.playerHP===100&&s.opponentHP===100&&s.playerStamina===100&&s.playerCapacity===100&&s.acceptedPunches===0&&s.audienceEnabledFlashes===0;}
(async()=>{
 const browser=await chromium.launch({executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',headless:true,args:['--use-angle=d3d11','--disable-background-timer-throttling','--disable-renderer-backgrounding']});let page;
 try{
  const ctx=await browser.newContext({viewport:{width:540,height:960},hasTouch:true});page=await ctx.newPage();const touch=await ctx.newCDPSession(page);
  page.on('pageerror',e=>report.errors.push(String(e)));page.on('console',m=>{if(m.type()==='error')report.errors.push(m.text());});
  page.on('requestfailed',r=>report.errors.push(r.url()+' '+r.failure()?.errorText));page.on('response',r=>{if(r.status()>=400)report.errors.push('HTTP '+r.status()+' '+r.url());});
  report.url=process.argv[2]||'http://127.0.0.1:8000/?desktop=1&metrics=1';await page.goto(report.url);report.productVersion=await page.evaluate(()=>productVersion);await page.locator('#enable').click();
  const snap=()=>page.evaluate(()=>window.boxerWave1Snapshot);
  async function state(label,screen){await page.waitForFunction(s=>window.boxerWave1Snapshot?.screen===s,screen,{timeout:150000});await page.waitForTimeout(120);const s=await snap();report.states[label]=s;return s;}
  async function tap(x,y){const v=page.viewportSize(),scale=Math.min(v.width/540,v.height/960),ox=(v.width-540*scale)/2,oy=(v.height-960*scale)/2;
   await touch.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{id:1,x:ox+x*scale,y:oy+y*scale}]});await page.waitForTimeout(75);await touch.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});await page.waitForTimeout(150);}
  async function type(id,text){const input=page.locator('#profile-'+id);await input.click();await input.press('ControlOrMeta+A');await page.keyboard.insertText(text);}
  async function shot(name){await page.screenshot({path:path.join(dir,name+'.png')});}
  let s=await state('fresh-home','Home');check(!s.profileSaved&&s.profileName==='BOXER'&&s.profileNationality===''&&s.profileId===''&&s.captureKeyboard,'fresh honest default and keyboard capture');
  await tap(270,852);s=await state('fresh-profile','Profile');check(frozen(s)&&!s.captureKeyboard,'Profile freezes gameplay and releases keyboard capture');
  check(await page.locator('#profile-review').isDisabled(),'empty completed-device record cannot navigate');
  check((await page.locator('#profile-name').inputValue())===''&&(await page.locator('#profile-nationality').inputValue())==='','no assigned identity or nationality');await shot('fresh-profile');
  await type('name','Draft');await type('nationality','Việt Nam');await page.locator('#profile-cancel').click();s=await state('draft-cancel-home','Home');check(!s.profileSaved&&s.captureKeyboard&&frozen(s),'cancel draft no save restores keyboard');
  await tap(270,852);await state('invalid-profile','Profile');check((await page.locator('#profile-name').inputValue())==='','reopen discards draft');
  await page.locator('#profile-save').click();await page.waitForTimeout(250);s=await snap();check(s.screen==='Profile'&&!s.profileSaved&&(await page.locator('#profile-error').innerText()).length>0,'empty submission keeps error and never persists');
  await type('name','<script>');await type('nationality','Việt Nam');await page.locator('#profile-save').click();await page.waitForTimeout(200);check(!(await snap()).profileSaved&&(await page.locator('#profile-name').inputValue())==='<script>','markup rejected without injection or erasing retry draft');
  await type('name','  Nguyễn   Trí  ');await type('nationality','Việt Nam');await page.keyboard.press('j');await page.waitForTimeout(200);check(frozen(await snap()),'HTML typing cannot punch or spend');
  // j belongs to the focused input. Explicitly correct the intended nationality before saving.
  await type('nationality','Việt Nam');await page.locator('#profile-save').click();s=await state('saved-home','Home');
  const id=s.profileId;check(s.profileSaved&&s.profileName==='Nguyễn Trí'&&s.profileNationality==='Việt Nam'&&/^[a-f0-9]{32}$/.test(id),'explicit save normalizes Unicode and assigns local ID');
  check(s.captureKeyboard&&frozen(s),'save returns Home capture without leaked gameplay');await shot('saved-home');
  await tap(270,852);await state('edit-profile','Profile');check((await page.locator('#profile-name').inputValue())==='Nguyễn Trí','saved text restored into form');
  await type('name','Hủy đổi tên');await page.locator('#profile-cancel').click();s=await state('edit-cancel-home','Home');check(s.profileId===id&&s.profileName==='Nguyễn Trí','cancel rename preserves saved identity');
  await tap(270,852);await state('rename-profile','Profile');await type('name','Ramírez Trí');await type('nationality','Côte d’Ivoire');await page.locator('#profile-save').click();s=await state('renamed-home','Home');check(s.profileId===id&&s.profileName==='Ramírez Trí'&&s.profileNationality==='Côte d’Ivoire','rename stable ID and self-reported non-suggestion country');
  await page.waitForTimeout(1500);await page.reload();await page.locator('#enable').click();s=await state('reloaded-home','Home');check(s.profileId===id&&s.profileName==='Ramírez Trí'&&s.profileNationality==='Côte d’Ivoire'&&s.profileSaved,'real page reload retains identity');
  await tap(270,852);await state('responsive-profile','Profile');
  for(const [w,h,label] of [[320,740,'320-portrait'],[375,812,'375-portrait'],[540,960,'540-portrait'],[960,540,'landscape'],[375,360,'reduced-viewport-not-physical-keyboard']]){
   await page.setViewportSize({width:w,height:h});await page.waitForTimeout(500);
   const layout=await page.evaluate(()=>{const form=document.getElementById('profile-form'),root=document.getElementById('fighter-profile'),r=form.getBoundingClientRect();return {x:r.x,right:r.right,width:innerWidth,inputFont:parseFloat(getComputedStyle(document.getElementById('profile-name')).fontSize),buttons:[...form.querySelectorAll('button')].map(b=>b.getBoundingClientRect().height),height:root.getBoundingClientRect().height,viewport:visualViewport.height};});
   report.states[label]=layout;check(layout.x>=0&&layout.right<=w+1&&layout.inputFont>=16&&layout.buttons.every(v=>v>=44)&&Math.abs(layout.height-layout.viewport)<2,'bounded form and minimum pixel targets '+label);
   await page.locator('#profile-name').click();check(await page.locator('#profile-name').evaluate(e=>e===document.activeElement),'native HTML focus reachable '+label);
   await page.locator('#profile-cancel').scrollIntoViewIfNeeded();await shot('profile-'+label);check(frozen(await snap())&&!s.gameplayInput,'resize stays locked '+label);
  }
  await page.locator('#profile-cancel').click();s=await state('resize-cancel-home','Home');check(s.captureKeyboard&&s.profileId===id,'scroll/reduced viewport Cancel restores capture');
  await page.setViewportSize({width:540,height:960});await page.waitForTimeout(400);
  await tap(270,674);await state('profile-preview','Preview');await tap(270,779);s=await state('profile-fight','Fight');check(s.captureKeyboard&&s.gameplayInput&&s.profileId===id,'normal intro starts Fight with restored keyboard and identity');
  await page.keyboard.press('j');await page.waitForTimeout(300);s=await snap();check(s.acceptedPunches>0&&s.playerCapacity<100,'ordinary Fight keyboard attack remains functional');
  s=await state('profile-result','Result');check(s.hasMatchReview&&s.lastMatchReview.sourceVersion===report.productVersion,'genuine completed match captured not fabricated profile stats');const record=JSON.stringify(s.lastMatchReview);
  await tap(270,869);await state('result-home','Home');await tap(270,852);s=await state('profile-with-record','Profile');
  check(!await page.locator('#profile-review').isDisabled()&&JSON.stringify(s.lastMatchReview)===record,'completed-device record enables link without rewrite');await shot('profile-with-record');
  await type('name','Unsaved linked draft');await page.locator('#profile-review').click();s=await state('profile-linked-review','CoachReview');
  check(s.captureKeyboard&&frozen(s)&&JSON.stringify(s.lastMatchReview)===record&&s.profileName==='Ramírez Trí'&&s.profileId===id,'real profile link discards draft opens existing frozen review');
  await tap(270,893);await state('review-coach','Coach');await tap(270,893);s=await state('final-home','Home');check(s.players===1&&s.opponents===1&&s.captureKeyboard,'no duplicate actors and capture restored after linked Coach');
  check(report.errors.length===0,'zero JS/load/HTTP errors');report.status='PASS';
 }catch(e){report.status='FAIL';report.failure=String(e);process.exitCode=1;if(page){try{report.failureState=await page.evaluate(()=>window.boxerWave1Snapshot);await page.screenshot({path:path.join(dir,'failure.png')});}catch{}}}
 finally{fs.writeFileSync(path.join(dir,'report.json'),JSON.stringify(report,null,2));console.log(report.status,report.checks.length,report.failure||'');await browser.close();}
})();
