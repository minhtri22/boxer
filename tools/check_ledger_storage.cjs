// Real IndexedDB transactions in isolated test-only databases. Not gameplay score injection/UAT.
const {chromium}=require('playwright'),fs=require('fs'),path=require('path'),http=require('http'),crypto=require('crypto');
const dir=path.resolve(__dirname,'../evidence/wave1/history-wallet'),source=path.resolve(__dirname,'../unity/BoxerP0/Assets/WebGLTemplates/BoxerP0Mobile/TemplateData/match-ledger.js');
const report={scope:'ISOLATED_REAL_INDEXEDDB_CONCURRENCY_ABORT_RELOAD_NOT_GAMEPLAY_OR_PHONE_UAT',checks:[],errors:[]};
function check(ok,name){report.checks.push({name,pass:!!ok});if(!ok)throw Error(name);}
(async()=>{
 const server=http.createServer((req,res)=>{res.writeHead(200,{'Content-Type':'text/html'});res.end('<!doctype html><title>Isolated ledger test</title>');});await new Promise(r=>server.listen(0,'127.0.0.1',r));
 const origin='http://127.0.0.1:'+server.address().port,db='BOXER_TEST_LEDGER_'+crypto.randomUUID(),browser=await chromium.launch({executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',headless:true});
 try{
  const context=await browser.newContext(),a=await context.newPage(),b=await context.newPage();
  async function prepare(p){p.on('pageerror',e=>report.errors.push(String(e)));await p.goto(origin);await p.addScriptTag({path:source});await p.evaluate(name=>{window.testStore=createBoxerLedgerStorage({databaseName:name});},db);}
  await prepare(a);await prepare(b);
  const fixtures=JSON.parse(fs.readFileSync(path.join(dir,'shared-ledger-fixtures.json'),'utf8')),entries=fixtures.cases.find(f=>f.label==='valid loss ledger').ledger.entries;
  const read=p=>p.evaluate(()=>testStore.read()),append=(p,e)=>p.evaluate(entry=>testStore.append(entry),e);
  let l=await read(a);check(l.balance===0&&l.entries.length===0,'fresh real DB does not backfill old review');
  l=await append(a,entries[0]);check(l.balance===100&&l.entries.length===1,'transaction commit stores one win and 100 together');
  l=await append(a,entries[0]);check(l.balance===100&&l.entries.length===1,'same real transaction retry exactly once');
  await Promise.all([append(a,entries[1]),append(b,entries[2])]);l=await read(a);check(l.balance===175&&l.entries.length===3,'two tabs distinct transactions retain both completions');
  const fourth=JSON.parse(JSON.stringify(entries[0]));fourth.review.id='00000000000000000000000000000004';
  await Promise.all([append(a,fourth),append(b,fourth)]);l=await read(b);check(l.balance===275&&l.entries.length===4,'two tabs same ID pays only once');const confirmed=JSON.stringify(l);
  const conflict=JSON.parse(JSON.stringify(entries[0]));conflict.completedUtc='2026-10-11T01:00:00.000Z';let rejected=false;try{await append(a,conflict);}catch{rejected=true;}check(rejected,'conflicting duplicate aborts');
  check(JSON.stringify(await read(b))===confirmed,'abort retains original ledger and balance together');
  const bad=JSON.parse(JSON.stringify(fourth));bad.review.id='00000000000000000000000000000005';bad.reward=200;rejected=false;try{await append(b,bad);}catch{rejected=true;}check(rejected&&JSON.stringify(await read(a))===confirmed,'caller reward mutation rejected with no partial write');
  await a.reload();await a.addScriptTag({path:source});await a.evaluate(name=>{window.testStore=createBoxerLedgerStorage({databaseName:name});},db);check(JSON.stringify(await read(a))===confirmed,'real page reload persists exact committed record');
  await b.evaluate(()=>testStore.close());rejected=false;try{await append(b,fourth);}catch{rejected=true;}check(rejected&&JSON.stringify(await read(a))===confirmed,'closed database failure never changes confirmed coins');
  async function raw(value){return a.evaluate(async args=>{const d=await new Promise((r,j)=>{const q=indexedDB.open(args.db,1);q.onsuccess=()=>r(q.result);q.onerror=()=>j(q.error);});try{return await new Promise((r,j)=>{const t=d.transaction('ledger',args.value===null?'readonly':'readwrite'),s=t.objectStore('ledger');let result;const q=args.value===null?s.get('state'):s.put(args.value,'state');q.onsuccess=()=>result=q.result;t.oncomplete=()=>r(result);t.onabort=()=>j(t.error);});}finally{d.close();}},{db,value});}
  const corrupt=JSON.stringify({...l,balance:999});await raw(corrupt);rejected=false;try{await read(a);}catch{rejected=true;}check(rejected,'corrupt persisted balance fails closed');
  rejected=false;try{await append(a,fourth);}catch{rejected=true;}check(rejected,'cannot reward on corrupt base');check(await raw(null)===corrupt,'corrupt bytes preserved no silent reset');
  const future=JSON.stringify({...l,schema:2});await raw(future);rejected=false;try{await read(a);}catch{rejected=true;}check(rejected&&await raw(null)===future,'unsupported schema retained and rejected');
  const unavailable=await a.evaluate(async()=>{try{await createBoxerLedgerStorage({factory:null,databaseName:'BOXER_TEST_UNUSED'}).read();return false;}catch{return true;}});check(unavailable,'injected unavailable storage returns error not fabricated empty wallet');
  check(report.errors.length===0,'no unexpected browser errors');report.status='PASS';
  await a.evaluate(()=>testStore.close());await a.evaluate(name=>new Promise((r,j)=>{const q=indexedDB.deleteDatabase(name);q.onsuccess=r;q.onerror=()=>j(q.error);q.onblocked=()=>j(Error('test database cleanup blocked'));}),db);report.testDatabaseRemoved=true;
 }catch(e){report.status='FAIL';report.failure=String(e);process.exitCode=1;}
 finally{fs.writeFileSync(path.join(dir,'storage-report.json'),JSON.stringify(report,null,2));console.log(report.status,report.checks.length,report.failure||'');await browser.close();await new Promise(r=>server.close(r));}
})();
