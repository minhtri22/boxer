// Local-only synthetic WebGL smoke and matched baseline frame sampling.
const fs=require('fs'), path=require('path'), http=require('http');
const {chromium}=require('playwright');
const root=path.resolve(__dirname,'..');
const evidence=path.join(root,'evidence','ramirez-unity-round2');
const candidate=path.join(root,'builds','web','boxer-round2');
const baseline=path.resolve(root,'..','_ramirez-unity-baseline-bc0b2129');
const errors=[], consoleLines=[], statuses=[];
const mime={'.html':'text/html','.js':'application/javascript','.wasm':'application/wasm','.data':'application/octet-stream','.css':'text/css','.png':'image/png'};
const server=http.createServer((req,res)=>{
  const parsed=new URL(req.url,'http://localhost');
  const isBaseline=parsed.pathname.startsWith('/baseline/');
  const base=isBaseline?baseline:candidate;
  let relative=decodeURIComponent(isBaseline?parsed.pathname.slice(10):parsed.pathname.slice(1));
  if(!relative) relative='index.html';
  const file=path.resolve(base,relative);
  if(!file.startsWith(base+path.sep)) {res.writeHead(403);res.end();return;}
  if(!fs.existsSync(file)) {res.writeHead(404);res.end();return;}
  res.setHeader('Content-Type',mime[path.extname(file)]||'application/octet-stream');
  res.setHeader('Cross-Origin-Opener-Policy','same-origin');
  res.setHeader('Cross-Origin-Embedder-Policy','require-corp');
  res.writeHead(200);fs.createReadStream(file).pipe(res);
});
async function main(){
  await new Promise(resolve=>server.listen(0,'127.0.0.1',resolve));
  const origin='http://127.0.0.1:'+server.address().port;
  let browser;
  const result={scope:'LOCAL_DESKTOP_SYNTHETIC_NOT_IPHONE_NOT_HUMAN_UAT',statuses,errors,consoleLines};
  try {
    for(const name of ['index.html','Build/boxer-round2.data','Build/boxer-round2.wasm']){
      const response=await fetch(origin+'/'+name); statuses.push({file:name,status:response.status,bytes:(await response.arrayBuffer()).byteLength});
      if(response.status!==200) throw new Error('HTTP '+name);
    }
    browser=await chromium.launch({executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',headless:true});
    async function measure(prefix,label){
      const page=await browser.newPage({viewport:{width:540,height:960}});
      page.on('pageerror',e=>errors.push({label,error:String(e)}));
      page.on('console',m=>consoleLines.push({label,type:m.type(),text:m.text()}));
      await page.goto(origin+prefix+'?desktop=1',{waitUntil:'domcontentloaded',timeout:60000});
      await page.locator('#enable').click();
      await page.waitForFunction(()=>document.querySelector('#gate').classList.contains('hidden'),{},{timeout:180000});
      const frames=await page.evaluate(()=>new Promise(resolve=>{
        let previous=performance.now(),count=0;const samples=[];
        function tick(now){const dt=now-previous;previous=now;if(++count>120) samples.push(dt);
          if(samples.length===600){const sorted=[...samples].sort((a,b)=>a-b),mean=samples.reduce((a,b)=>a+b)/samples.length;
            resolve({samples:samples.length,mean_ms:mean,fps:1000/mean,p95_ms:sorted[Math.floor(sorted.length*.95)],max_ms:sorted.at(-1)});
          }else requestAnimationFrame(tick);
        }requestAnimationFrame(tick);
      }));
      await page.screenshot({path:path.join(evidence,'web-'+label+'.png')});
      await page.close();return frames;
    }
    result.baseline=await measure('/baseline/','baseline');
    result.candidate=await measure('/','candidate');
    result.mean_frame_delta_ms=result.candidate.mean_ms-result.baseline.mean_ms;
    result.mean_frame_delta_percent=(result.candidate.mean_ms/result.baseline.mean_ms-1)*100;
    result.startup=errors.length===0?'PASS':'FAIL';
  }catch(error){result.startup='FAIL';result.failure=String(error);process.exitCode=1;}
  finally{if(browser) await browser.close();server.close();fs.writeFileSync(path.join(evidence,'web-smoke.json'),JSON.stringify(result,null,2));console.log(JSON.stringify({...result,consoleLines:consoleLines.length}));}
}
main().catch(e=>{console.error(e);server.close();process.exitCode=1;});
