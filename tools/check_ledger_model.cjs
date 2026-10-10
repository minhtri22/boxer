const fs=require('fs'),path=require('path'),vm=require('vm');
const source=fs.readFileSync(path.resolve(__dirname,'../unity/BoxerP0/Assets/WebGLTemplates/BoxerP0Mobile/TemplateData/match-ledger.js'),'utf8');
const model=vm.runInNewContext(source+';BoxerLedgerModel');
const dir=path.resolve(__dirname,'../evidence/wave1/history-wallet'),fixtures=JSON.parse(fs.readFileSync(path.join(dir,'shared-ledger-fixtures.json'),'utf8'));
const report={scope:'SHARED_CSHARP_JS_MODEL_FIXTURES_NOT_GAMEPLAY_UAT',checks:[]};
function check(ok,name){report.checks.push({name,pass:!!ok});if(!ok)throw Error(name);}
try{
 for(const f of fixtures.cases)check(model.valid(f.ledger)===f.valid,'same validation '+f.label);
 const entries=fixtures.cases.find(f=>f.label==='valid loss ledger').ledger.entries;
 let ledger=model.empty();for(const e of entries)ledger=model.append(ledger,e);
 check(ledger.balance===175&&ledger.entries.length===3,'trial reward schedule 100 50 25');
 check(JSON.stringify(model.append(ledger,entries[0]))===JSON.stringify(ledger),'duplicate retry does not award');
 const conflict=JSON.parse(JSON.stringify(entries[0]));conflict.completedUtc='2026-10-11T01:00:00.000Z';let rejected=false;try{model.append(ledger,conflict);}catch{rejected=true;}check(rejected,'conflicting duplicate rejected');
 check(!model.entry({...entries[0],reward:200}),'caller reward cannot override schedule');
 check(!model.valid({...ledger,balance:176}),'balance equals exact ledger sum');
 check(!model.text('😎',24)&&!model.text('X\u202eY',24),'same unsupported emoji and bidi policy');report.status='PASS';
}catch(e){report.status='FAIL';report.failure=String(e);process.exitCode=1;}
fs.writeFileSync(path.join(dir,'js-model-report.json'),JSON.stringify(report,null,2));console.log(report.status,report.checks.length,report.failure||'');
