// Verify the deployed immutable candidate; fresh isolated browser, not device UAT.
const fs=require('fs'),path=require('path'),crypto=require('crypto');
const {chromium}=require('playwright');
const source='4940f8061eaf1ed58bcf5356a1bce4817de6eab0';
const base='https://minhtri22.github.io/boxer/';
const local=path.resolve(__dirname,'../../builds/web/boxer-round2');
const meta=Object.fromEntries(fs.readFileSync(path.join(local,'provenance.txt'),'utf8').trim().split(/\r?\n/).map(line=>{const index=line.indexOf('=');return [line.slice(0,index),line.slice(index+1)];}));
const result={scope:'DEPLOYED_HTTPS_HEADLESS_EDGE_DESKTOP_SYNTHETIC_NOT_HUMAN_UAT',source_sha:source,files:[],errors:[],console:[],requestsFailed:[]};
const hash=bytes=>crypto.createHash('sha256').update(bytes).digest('hex');
let browser;
async function main(){
 try{
  for(const file of ['provenance.txt',...Object.keys(meta).filter(k=>k==='index.html'||k.startsWith('Build/')||k.startsWith('TemplateData/'))]){
   const response=await fetch(base+file+'?uat='+source,{headers:{'Cache-Control':'no-cache'},signal:AbortSignal.timeout(60000)});
   const bytes=Buffer.from(await response.arrayBuffer()),actual=hash(bytes),expected=file==='provenance.txt'?hash(fs.readFileSync(path.join(local,file))):meta[file];
   result.files.push({file,status:response.status,bytes:bytes.length,sha256:actual,expected,match:actual===expected});
   if(response.status!==200||actual!==expected)throw Error('Deployed mismatch: '+file);
   if(file==='provenance.txt'&&!bytes.toString('utf8').includes('source_sha='+source))throw Error('Wrong deployed source');
  }
  result.byte_verification='PASS';console.log('DEPLOYED_BYTE_VERIFICATION_PASS files='+result.files.length);
  browser=await chromium.launch({executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',headless:true,args:['--disable-renderer-backgrounding','--disable-backgrounding-occluded-windows']});
  const page=await browser.newPage({viewport:{width:540,height:960}});
  page.on('pageerror',error=>result.errors.push(String(error)));page.on('console',m=>result.console.push({type:m.type(),text:m.text()}));
  page.on('requestfailed',r=>result.requestsFailed.push({url:r.url(),error:r.failure()}));
  await page.goto(base+'?desktop=1&uat='+source,{waitUntil:'domcontentloaded'});await page.locator('#enable').click();
  await page.waitForFunction(()=>document.querySelector('#gate').classList.contains('hidden'),{},{timeout:180000});
  await page.bringToFront();await page.locator('canvas').click({position:{x:270,y:700}});
  for(const key of ['q','e','w']){await page.keyboard.down(key);await new Promise(r=>setTimeout(r,1000));await page.keyboard.up(key);}
  await page.screenshot({path:path.join(__dirname,'pages-candidate.png')});
  result.engineErrors=result.console.filter(m=>m.type==='error'&&!m.text.startsWith('Failed to load resource: the server responded with a status of 404'));
  result.startup=result.errors.length||result.engineErrors.length||result.requestsFailed.length?'FAIL':'PASS';
  if(result.startup==='FAIL')process.exitCode=1;
 }catch(error){result.startup='FAIL';result.failure=String(error);process.exitCode=1;}
 finally{fs.writeFileSync(path.join(__dirname,'pages-smoke.json'),JSON.stringify(result,null,2));console.log(JSON.stringify({...result,console:result.console.length}));if(browser)await Promise.race([browser.close(),new Promise(r=>setTimeout(r,5000))]);}
}
main().then(()=>process.exit(process.exitCode||0)).catch(error=>{console.error(error);process.exit(1);});
