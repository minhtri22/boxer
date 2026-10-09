// Real browser keyboard/touch input and shared rendered sweep. No score/outcome injection.
const {chromium}=require('playwright');
const fs=require('fs'),path=require('path');
const probe=process.env.COMBAT_TACTIC_PROBE==='1';
const suite=process.env.BOXER_WAVE_EVIDENCE==='ring-intro'?'ring-intro':'combat-v3';
const evidence=path.resolve(__dirname,'../evidence/wave1/'+suite+'/'+(probe?'combat-tactic-probe':'combat-browser'));
fs.mkdirSync(evidence,{recursive:true});
const report={scope:'REAL_WEBGL_INPUT_AND_SOLVED_CONTACT_SYNTHETIC_DESKTOP_NOT_PHONE_UAT',checks:[],errors:[],scenarios:{}};
const activePages=[];
function check(ok,name){report.checks.push({name,pass:!!ok});if(!ok)throw Error(name);}
(async()=>{
  const browser=await chromium.launch({executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',headless:true,
    args:['--use-angle=d3d11','--disable-background-timer-throttling','--disable-renderer-backgrounding']});
  try{
    async function match(name){
      const context=await browser.newContext({viewport:{width:540,height:960},hasTouch:true});
      const page=await context.newPage(),touch=await context.newCDPSession(page);
      await page.addInitScript(()=>{
        window.boxerAuditTouchEvents=[];
        for(const type of ['touchstart','touchmove','touchend','touchcancel'])document.addEventListener(type,e=>{
          window.boxerAuditTouchEvents.push({type,trusted:e.isTrusted,
            active:Array.from(e.touches,t=>({id:t.identifier,x:t.clientX,y:t.clientY})),
            changed:Array.from(e.changedTouches,t=>t.identifier)});
        },{passive:true});
      });
      activePages.push({name,page});
      const scenario=report.scenarios[name]={trace:[],choices:[]};
      page.on('pageerror',e=>report.errors.push(String(e)));
      page.on('console',m=>{if(m.type()==='error')report.errors.push(m.text());});
      page.on('requestfailed',r=>report.errors.push(r.url()+' '+r.failure()?.errorText));
      page.on('response',r=>{if(r.status()>=400)report.errors.push('HTTP '+r.status()+' '+r.url());});
      await page.goto(process.argv[2]||'http://127.0.0.1:8000/?desktop=1&metrics=1');
      report.productVersion=await page.evaluate(()=>productVersion);
      await page.locator('#enable').click();
      await page.waitForFunction(()=>window.boxerWave1Snapshot?.screen==='Home',null,{timeout:150000});
      await page.mouse.click(270,674);await page.waitForFunction(()=>window.boxerWave1Snapshot?.screen==='Preview');
      await page.mouse.click(270,779);await page.waitForFunction(()=>window.boxerWave1Snapshot?.screen==='Fight');
      // The normal menu-release barrier must see a released frame before a held swipe.
      await page.waitForTimeout(350);
      const snap=()=>page.evaluate(()=>window.boxerWave1Snapshot);
      let leftTouch=null;
      const points=right=>[...(leftTouch?[leftTouch]:[]),...(right?[right]:[])];
      let lastAttack=0;
      async function record(){const s=await snap();scenario.trace.push(s);
        if(s.opponentAttacks>lastAttack&&s.opponentIntent!=='None') {lastAttack=s.opponentAttacks;scenario.choices.push(s.opponentBody?'CrossBody':s.opponentIntent);}
        if(s.playerHits>0&&!scenario.firstHit){scenario.firstHit=s;await picture('first-hit');}
        return s;
      }
      async function pause(ms){const end=Date.now()+ms;while(Date.now()<end){await page.waitForTimeout(80);await record();}}
      async function gesture(x,y,dx=0,dy=0,hold=100){
        await touch.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:points({id:2,x,y})});await pause(100);
        if(dx||dy)await touch.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:points({id:2,x:x+dx,y:y+dy})});
        // Edge's verified partial-end behavior takes the ENDED point, not the
        // remaining active list. Empty means lift all; touchMove does not lift.
        await pause(hold);await touch.send('Input.dispatchTouchEvent',{type:'touchEnd',
          touchPoints:leftTouch?[{id:2,x:x+dx,y:y+dy}]:[]});
      }
      async function advanceHeld(active){
        if(active){leftTouch={id:1,x:135,y:750};await touch.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:points(null)});await pause(100);
          leftTouch={id:1,x:135,y:650};await touch.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:points(null)});}
        else if(leftTouch){leftTouch=null;await touch.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});}
      }
      async function timedOverhand(){
        // Prepare an ordinary down-swipe, watch the visible opponent windup,
        // step to pocket range, then release. Only browser input; no native setters.
        leftTouch={id:1,x:135,y:750};
        const right={id:2,x:420,y:700};
        await touch.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:points(right)});await pause(100);
        right.y=805;await touch.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:points(right)});await pause(100);
        let s=await snap();const deadline=Date.now()+5000;
        while(s.screen==='Fight'&&s.opponentPhase!=='Commit'&&Date.now()<deadline){await pause(40);s=await snap();}
        if(s.screen==='Fight'){
          // Observe movement instead of assuming wall-clock hold == simulation
          // travel: frame/event coalescing made the first timed probe inaccurate.
          const pocketDeadline=Date.now()+900;
          while(s.screen==='Fight'&&(s.distance<.715||s.distance>.755)&&Date.now()<pocketDeadline){
            leftTouch.y=750+(s.distance>.735?-20:20);
            await touch.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:points(right)});
            await page.waitForTimeout(40);s=await record();
          }
          leftTouch.y=750;await touch.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:points(right)});
          await page.waitForTimeout(40);
        }
        await touch.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[right]});
        await pause(80);await advanceHeld(false);
      }
      async function picture(label){await page.screenshot({path:path.join(evidence,name+'-'+label+'.png')});}
      return {page,context,snap,record,pause,gesture,advanceHeld,timedOverhand,picture,scenario};
    }
    if(!probe){
    const guard=await match('idle-guard');
    const guardStart=Date.now();
    while((await guard.snap()).screen==='Fight'&&Date.now()-guardStart<65000)await guard.pause(100);
    guard.scenario.final=await guard.record();await guard.picture('result');
    check(guard.scenario.final.screen==='Result','idle guard reaches actual result without forced finish');
    check(guard.scenario.final.playerBlocks>0,'actual enemy punches intersect idle player guard');
    check(guard.scenario.final.opponentHP===100,'idle player cannot deal unearned damage');
    check(guard.scenario.final.playerHP>0,'idle high guard survives default bout; exposed body may take damage');
    check(['Jab','Cross','LeadHook','CrossBody'].every(c=>guard.scenario.choices.includes(c)),'real AI uses all four choices including head/body instead of old Cross loop');
    console.log('IDLE_GUARD',JSON.stringify(guard.scenario.final));
    await guard.context.close();
    const bodyGuard=await match('body-guard');await bodyGuard.advanceHeld(true);
    const bodyDeadline=Date.now()+20000;
    while(Date.now()<bodyDeadline){
      const s=await bodyGuard.record();
      if(s.screen==='Fight'&&s.opponentPhase==='Commit'&&s.opponentBody&&s.distance<.7)break;
      await bodyGuard.pause(80);
    }
    const bodyReady=await bodyGuard.snap();
    check(bodyReady.opponentPhase==='Commit'&&bodyReady.opponentBody&&bodyReady.distance<.7,'close body guard contact precondition reached with real touch input');
    const bodyBefore=await bodyGuard.snap();await bodyGuard.pause(650);const bodyAfter=await bodyGuard.record();
    bodyGuard.scenario.contact={before:bodyBefore,after:bodyAfter};await bodyGuard.picture('contact');
    console.log('BODY_GUARD',JSON.stringify(bodyGuard.scenario.contact));
    check(bodyAfter.playerBlocks>bodyBefore.playerBlocks&&bodyAfter.playerHP===bodyBefore.playerHP,'actual body attack intersecting close high guard BLOCKS without HP loss');
    check(bodyAfter.distance<.7,'lower-left forward touch can follow retreating AI into real close range');
    await bodyGuard.advanceHeld(false);
    await bodyGuard.context.close();
    }
    const fight=await match('touch-attack');
    if(!probe){
    // Move away using ordinary input so the first shot tests genuine long-range MISS.
    await fight.page.keyboard.down('s');await fight.pause(350);await fight.page.keyboard.up('s');await fight.pause(100);
    let before=await fight.snap();
    await fight.gesture(420,750);await fight.pause(700);
    let after=await fight.record();fight.scenario.longRange={before,after};await fight.picture('long-miss');
    console.log('LONG_RANGE',JSON.stringify(fight.scenario.longRange));
    check(after.playerMisses>before.playerMisses&&after.opponentHP===before.opponentHP,'real long-range touch punch resolves MISS and zero HP');
    check(after.playerCapacity<before.playerCapacity,'accepted MISS still spends real resources');
    await fight.gesture(135,750,0,-110,450);await fight.pause(250);
    await fight.page.waitForFunction(()=>{const s=window.boxerWave1Snapshot;return s?.screen==='Fight'&&s.distance<.9&&s.opponentPhase==='Guard'&&s.playerPhase==='Guard';},null,{timeout:15000});
    before=await fight.snap();await fight.gesture(420,750);await fight.pause(650);after=await fight.record();
    fight.scenario.closedGuard={before,after};await fight.picture('closed-guard');
    console.log('CLOSED_GUARD',JSON.stringify(fight.scenario.closedGuard));
    check(after.opponentBlocks>before.opponentBlocks&&after.opponentHP===before.opponentHP,'real touch straight is BLOCKED by opponent glove, zero HP');
    }
    const attackStart=Date.now();
    let attempts=0;
    while((await fight.snap()).screen==='Fight'&&Date.now()-attackStart<60000){
      const current=await fight.snap();
      if(current.playerPhase==='Guard'){
        await fight.timedOverhand();attempts++;
        if(attempts===1)check(await fight.page.evaluate(()=>window.boxerAuditTouchEvents.some(e=>
          e.type==='touchend'&&e.trusted&&e.changed.includes(2)&&e.active.length===1&&e.active[0].id===1)),
          'trusted right-finger release leaves left movement touch active');
        await fight.pause(900);
        if(attempts%4===0)console.log('ATTACK_PROGRESS',JSON.stringify(await fight.snap()));
      }else await fight.pause(100);
    }
    fight.scenario.final=await fight.record();fight.scenario.swipeAttempts=attempts;await fight.picture('result');
    fight.scenario.touchEvents=await fight.page.evaluate(()=>window.boxerAuditTouchEvents);
    await fight.advanceHeld(false);
    const final=fight.scenario.final;
    check(final.screen==='Result'&&final.opponentHP<100&&final.playerHits>0,'real directional touch punches geometrically HIT and reduce opponent HP');
    check(final.result==='PLAYER_WIN','patient real touch attack can win default Ramirez bout');
    check(final.reason==='KO'&&final.opponentHP===0&&!final.playerEnabled&&!final.opponentEnabled,'physical punch HP zero produces KO and locks both actors');
    if(suite==='ring-intro'){
      const media=await fight.page.evaluate(()=>window.boxerRingMedia.state);fight.scenario.mediaAtKO=media;
      check(media.phase==='Result'&&!media.crowdActive&&media.events.filter(e=>e.name==='end-bell').length===1,'real physical-input KO stops crowd and plays end bell exactly once');
    }
    check(final.playerStamina>=0&&final.playerStamina<=100&&final.playerCapacity>=0&&final.playerCapacity<=100,'physical combat keeps stamina/capacity within exact model bounds');
    console.log('TOUCH_ATTACK',JSON.stringify(final));
    check(report.errors.length===0,'physical WebGL combat has zero JS/load/HTTP errors');report.status=probe?'PROBE_PASS_NOT_RELEASE_GATE':'PASS';
  }catch(e){report.status='FAIL';report.failure=String(e);process.exitCode=1;
    for(const {name,page} of activePages)if(!page.isClosed()){
      try{report.scenarios[name].failureState=await page.evaluate(()=>window.boxerWave1Snapshot);
        report.scenarios[name].touchEvents=await page.evaluate(()=>window.boxerAuditTouchEvents);
        await page.screenshot({path:path.join(evidence,name+'-failure.png')});}catch{}
    }
  }
  finally{
    // Persist evidence before shutdown; closing Edge must not hide a completed/failing audit.
    fs.writeFileSync(path.join(evidence,'report.json'),JSON.stringify(report,null,2));
    console.log(report.status,report.failure||'');await browser.close();
  }
})();
