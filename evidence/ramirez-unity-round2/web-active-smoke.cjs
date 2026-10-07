// Evidence-only test harness. Does not alter the locked application or artifact.
const fs=require('fs'),path=require('path'),http=require('http');
const {chromium}=require('playwright');
const root=path.resolve(__dirname,'../..'), output=__dirname;
const roots={baseline:path.resolve(root,'../_ramirez-unity-baseline-bc0b2129'),candidate:path.join(root,'builds/web/boxer-round2')};
const result={scope:'LOCAL_HEADLESS_EDGE_DESKTOP_SYNTHETIC_NOT_HUMAN_UAT',errors:[],console:[],http:[],missingRequests:[],measurements:{}};
const mime={'.html':'text/html','.js':'application/javascript','.wasm':'application/wasm','.data':'application/octet-stream','.css':'text/css'};
const server=http.createServer((req,res)=>{const url=new URL(req.url,'http://localhost');const parts=url.pathname.split('/');const label=parts[1];const base=roots[label];if(!base){result.missingRequests.push(req.url);res.writeHead(404).end();return;}
  const file=path.resolve(base,decodeURIComponent(parts.slice(2).join('/'))||'index.html');
  if(!file.startsWith(base+path.sep)||!fs.existsSync(file)){result.missingRequests.push(req.url);res.writeHead(404).end();return;}
  res.setHeader('Content-Length',fs.statSync(file).size);res.setHeader('Content-Type',mime[path.extname(file)]||'application/octet-stream');res.setHeader('Cross-Origin-Opener-Policy','same-origin');res.setHeader('Cross-Origin-Embedder-Policy','require-corp');res.writeHead(200);fs.createReadStream(file).pipe(res);
});
const wait=ms=>new Promise(resolve=>setTimeout(resolve,ms));
async function main(){
 await new Promise(resolve=>server.listen(0,'127.0.0.1',resolve));const origin='http://127.0.0.1:'+server.address().port;let browser;
 try {
  for(const file of ['index.html','Build/boxer-round2.data','Build/boxer-round2.wasm']){const response=await fetch(origin+'/candidate/'+file);result.http.push({file,status:response.status,bytes:(await response.arrayBuffer()).byteLength});if(response.status!==200)throw Error('HTTP failure');}
  browser=await chromium.launch({executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',headless:true,args:['--disable-background-timer-throttling','--disable-renderer-backgrounding','--disable-backgrounding-occluded-windows']});
  for(const label of ['baseline','candidate']){
   const page=await browser.newPage({viewport:{width:540,height:960}});
   page.on('pageerror',error=>result.errors.push({label,error:String(error)}));page.on('console',m=>result.console.push({label,type:m.type(),text:m.text()}));
   await page.goto(origin+'/'+label+'/?desktop=1',{waitUntil:'domcontentloaded'});await page.locator('#enable').click();
   await page.waitForFunction(()=>document.querySelector('#gate').classList.contains('hidden'),{},{timeout:180000});
   await page.bringToFront();await page.locator('canvas').click({position:{x:270,y:700}});
   result.measurements[label]={focus:await page.evaluate(()=>({hasFocus:document.hasFocus(),visibility:document.visibilityState}))};
   console.log(label+' loaded and focused');
   for(const key of ['q','e','w','s','a','d']){await page.keyboard.down(key);await wait(1000);await page.keyboard.up(key);}
   for(const key of ['j','k','l','u','o']){await page.keyboard.press(key);await wait(1200);}
   await page.screenshot({path:path.join(output,'web-active-'+label+'-training.png')});
   // Frozen onboarding timeout path: 70s total. No hidden StartBout bypass.
   for(let i=0;i<4;i++){await wait(15000);console.log(label+' onboarding wait '+(i+1));}
   await page.screenshot({path:path.join(output,'web-active-'+label+'-bout-before.png')});
   result.measurements[label].frames=await page.evaluate(()=>new Promise(resolve=>{let previous=performance.now(),count=0;const samples=[];function tick(now){const dt=now-previous;previous=now;if(++count>120)samples.push(dt);if(samples.length===600){const sorted=[...samples].sort((a,b)=>a-b),mean=samples.reduce((a,b)=>a+b,0)/samples.length;resolve({samples:600,mean_ms:mean,fps:1000/mean,p95_ms:sorted[570],max_ms:sorted.at(-1)});}else requestAnimationFrame(tick);}requestAnimationFrame(tick);}));
   for(const key of ['j','k','l','u','o']){await page.keyboard.press(key);await wait(600);}
   await page.screenshot({path:path.join(output,'web-active-'+label+'-bout-after.png')});
   await page.keyboard.press('F3');await wait(1000);await page.screenshot({path:path.join(output,'web-active-'+label+'-performance-debug.png')});await page.keyboard.press('F3');
   console.log(label+' measured');
   await Promise.race([page.close(),wait(5000)]);
  }
  const b=result.measurements.baseline.frames,c=result.measurements.candidate.frames;
  result.mean_frame_delta_ms=c.mean_ms-b.mean_ms;result.mean_frame_delta_percent=(c.mean_ms/b.mean_ms-1)*100;
   result.consoleErrors=result.console.filter(m=>m.type==='error');
   result.engineErrors=result.consoleErrors.filter(m=>!m.text.startsWith('Failed to load resource: the server responded with a status of 404'));
   result.unexpectedMissingRequests=result.missingRequests.filter(url=>!new URL(url,'http://localhost').pathname.endsWith('/favicon.ico'));
   result.startup=result.errors.length||result.engineErrors.length||result.unexpectedMissingRequests.length?'FAIL':'PASS';
 }catch(error){result.startup='FAIL';result.failure=String(error);process.exitCode=1;}
 finally{
  // Persist even if browser teardown stalls. Teardown is not application startup evidence.
  fs.writeFileSync(path.join(output,'web-active-smoke.json'),JSON.stringify(result,null,2));console.log(JSON.stringify({...result,console:result.console.length}));
  if(browser)await Promise.race([browser.close(),wait(5000)]);server.close();
 }
}
main().then(()=>process.exit(process.exitCode||0)).catch(error=>{console.error(error);process.exit(1);});
