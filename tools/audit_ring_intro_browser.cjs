// Real compiled Unity UI + rendered browser media; read-only state, no callback/score injection.
const {chromium}=require('playwright');
const fs=require('fs'),path=require('path');
const suite=process.env.BOXER_WAVE_EVIDENCE==='arena-surround'?'arena-surround':process.env.BOXER_WAVE_EVIDENCE==='bell-repair'?'bell-repair':process.env.BOXER_WAVE_EVIDENCE==='coach-analysis'?'coach-analysis':process.env.BOXER_WAVE_EVIDENCE==='coach-ui'?'coach-ui':process.env.BOXER_WAVE_EVIDENCE==='punch-feel'?'punch-feel':process.env.BOXER_WAVE_EVIDENCE==='training-pov'?'training-pov':'ring-intro';
const dir=path.resolve(__dirname,'../evidence/wave1/'+suite+'/'+(process.env.BOXER_RING_PUBLIC==='1'?'media-browser-public':'media-browser'));fs.mkdirSync(dir,{recursive:true});
const report={scope:'REAL_WEBGL_UI_MEDIA_SYNTHETIC_DESKTOP_NOT_PHONE_OR_LISTENING_UAT',checks:[],errors:[],states:{}};
function check(pass,name){report.checks.push({name,pass:!!pass});if(!pass)throw Error(name);}
(async()=>{
  const browser=await chromium.launch({executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',headless:true,
    args:['--use-angle=d3d11','--disable-background-timer-throttling','--disable-renderer-backgrounding']});
  let page;
  try{
    page=await browser.newPage({viewport:{width:540,height:960},hasTouch:true});
    page.on('pageerror',e=>report.errors.push(String(e)));
    page.on('console',m=>{if(m.type()==='error')report.errors.push(m.text());});
    page.on('requestfailed',r=>report.errors.push(r.url()+' '+r.failure()?.errorText));
    page.on('response',r=>{if(r.status()>=400)report.errors.push('HTTP '+r.status()+' '+r.url());});
    report.url=process.argv[2]||'http://127.0.0.1:8000/?desktop=1&metrics=1';
    await page.goto(report.url);
    report.productVersion=await page.evaluate(()=>productVersion);
    const snap=()=>page.evaluate(()=>({native:window.boxerWave1Snapshot,media:window.boxerRingMedia.state,video:{time:document.querySelector('#ring-video').currentTime,width:document.querySelector('#ring-video').videoWidth,muted:document.querySelector('#ring-video').muted}}));
    async function state(name,screen){await page.waitForFunction(s=>window.boxerWave1Snapshot?.screen===s,screen,{timeout:150000});const s=await snap();report.states[name]=s;return s;}
    function frozen(s){return s.native.seconds===0&&s.native.playerHP===100&&s.native.opponentHP===100&&s.native.playerStamina===100&&s.native.opponentStamina===100&&s.native.playerCapacity===100&&s.native.opponentCapacity===100&&!s.native.gameplayInput&&!s.native.playerEnabled&&!s.native.opponentEnabled&&s.native.opponentAttacks===0;}
    const count=(s,n)=>s.media.events.filter(e=>e.name===n).length;
    await page.locator('#enable').click();const home=await state('home','Home');
    check(home.media.phase==='Idle'&&!home.media.crowdActive,'Home does not play ring media/crowd');
    await page.mouse.click(270,674);await state('preview','Preview');await page.mouse.click(270,779);await state('cancel-intro','Intro');
    await page.waitForFunction(()=>window.boxerRingMedia.state.events.some(e=>e.name==='intro-playing'));
    await page.waitForTimeout(900);const intro=await snap();
    check(frozen(intro)&&intro.media.phase==='Intro','Intro locks both actors, input, HP/stamina/capacity and scored clock');
    check(intro.video.width===480&&intro.video.muted&&intro.video.time>0,'owner video decodes and advances, without duplicate embedded audio');
    await page.screenshot({path:path.join(dir,'ring-girl-portrait.png')});
    await page.keyboard.press('k');await page.keyboard.down('w');await page.waitForTimeout(400);await page.keyboard.up('w');
    check(frozen(await snap()),'ordinary punch/footwork keys cannot spend or enable AI in Intro');
    await page.locator('#ring-back').click();const cancelled=await state('cancel-home','Home');await page.waitForTimeout(6200);const after=await snap();
    check(frozen(after)&&after.native.screen==='Home'&&after.media.phase==='Idle'&&count(after,'start-bell')===0,'Back cancels media; old ended event cannot start hidden combat');
    check(cancelled.media.token===intro.media.token,'cancel evidence retains original token');
    await page.mouse.click(270,674);await state('preview-again','Preview');await page.mouse.click(270,779);await state('intro-again','Intro');
    await page.waitForFunction(()=>window.boxerRingMedia.state.events.some(e=>e.name==='intro-playing'&&e.token===window.boxerRingMedia.state.token),null,{timeout:15000});
    await page.waitForTimeout(2200);const middle=await snap();
    report.states['intro-middle']=middle;
    check(frozen(middle)&&middle.video.time>1&&middle.media.token!==intro.media.token,'new intro is fresh, frozen and epoch separated');
    const fight=await state('fight','Fight');
    check(fight.native.result==='IN_PROGRESS'&&fight.native.seconds<1&&fight.native.playerHP===100&&fight.native.opponentHP===100,'fight starts fresh only after video completes');
    const played=fight.media.events.filter(e=>e.token===fight.media.token&&e.name==='intro-playing').at(-1);
    const ended=fight.media.events.find(e=>e.token===fight.media.token&&e.name==='intro-ended');
    check(ended&&ended.videoTime>=5&&ended.at-played.at>=5000&&ended.at-played.at<12000,'ring girl plays at least five real seconds before scored fight');
    check(fight.media.audioState==='running'&&fight.media.buffers.intro>5&&fight.media.buffers.bell>.8&&fight.media.buffers.crowd>4.5,'all source-derived sounds decode with active unlocked AudioContext');
    if(['bell-repair','arena-surround'].includes(suite)){
      const signal=await page.evaluate(async()=>{const c=new (window.AudioContext||window.webkitAudioContext)();try{const response=await fetch('StreamingAssets/RingMedia/bell.mp3?v='+encodeURIComponent(productVersion));if(!response.ok)throw Error('Bell HTTP '+response.status);const b=await c.decodeAudioData(await response.arrayBuffer());let peak=0,sum=0,active=0,total=0;for(let ch=0;ch<b.numberOfChannels;ch++){const data=b.getChannelData(ch);for(const v of data){peak=Math.max(peak,Math.abs(v));sum+=v*v;total++;if(Math.abs(v)>.001)active++;}}return {seconds:b.duration,peak,rms:Math.sqrt(sum/total),activeFraction:active/total};}finally{await c.close();}});
      report.decodedBellSignal=signal;check(signal.seconds>.8&&signal.seconds<1&&signal.peak>.05&&signal.peak<.98&&signal.rms>.003&&signal.activeFraction>.5,'actual browser bell PCM is non-silent and not clipped; duration/events alone are insufficient');
    }
    check(count(fight,'start-bell')===1&&count(fight,'end-bell')===0&&fight.media.crowdActive,'one start bell and one crowd loop, no premature end bell');
    await page.keyboard.press('m');await page.waitForTimeout(250);let muted=await snap();check(!muted.media.sound&&muted.media.masterGain===0,'M mutes intro/bell/crowd together with hit/block audio');
    await page.keyboard.press('m');await page.waitForTimeout(250);check((await snap()).media.masterGain===1,'M restores ring audio');
    await page.screenshot({path:path.join(dir,'fight-after-ring-intro.png')});
    const result=await state('result','Result');
    check(['KO','POINTS'].includes(result.native.reason)&&!result.native.gameplayInput&&!result.native.opponentEnabled,'actual bout completion locks both actors/input');
    check(result.media.phase==='Result'&&!result.media.crowdActive&&count(result,'end-bell')===1,'result stops crowd and schedules exactly one end bell');
    await page.waitForTimeout(1500);const settled=await snap();check(count(settled,'end-bell')===1&&settled.native.playerHP===result.native.playerHP&&settled.native.playerStamina===result.native.playerStamina,'result does not repeat bell or mutate vitals');
    await page.mouse.click(270,779);const rematch=await state('rematch-intro','Intro');check(frozen(rematch),'rematch resets resources but still locks them during its own intro');
    await state('rematch-fight','Fight');const second=await snap();
    check(count(second,'start-bell')===2&&second.media.crowdActive&&second.media.token!==fight.media.token,'rematch gets its own intro/start bell/crowd without duplicated actors');
    check(second.native.players===1&&second.native.opponents===1,'ring presentation does not duplicate boxers');
    check(report.errors.length===0,'media lifecycle has zero JS/load/HTTP errors');report.status='PASS';
  }catch(e){report.status='FAIL';report.failure=String(e);process.exitCode=1;if(page){try{report.failureState=await page.evaluate(()=>({native:window.boxerWave1Snapshot,media:window.boxerRingMedia?.state}));await page.screenshot({path:path.join(dir,'failure.png')});}catch{}}}
  finally{fs.writeFileSync(path.join(dir,'report.json'),JSON.stringify(report,null,2));console.log(report.status,report.checks.length,report.failure||'');await browser.close();}
})();
