// Actual decoded PCM signal gate; not a speaker/listening UAT claim.
const fs=require('fs'),path=require('path'),crypto=require('crypto'),{execFileSync}=require('child_process');
const root=path.resolve(__dirname,'..'),dir=path.join(root,'evidence/wave1/bell-repair');fs.mkdirSync(dir,{recursive:true});
const source=path.join(root,'art/media/ring-girl/source.mp4'),old=path.join(dir,'bell-silent-before.mp3'),bell=path.join(root,'unity/BoxerP0/Assets/StreamingAssets/RingMedia/bell.mp3');
const hash=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
function signal(file){const b=execFileSync('ffmpeg',['-v','error','-i',file,'-map','0:a:0','-ac','1','-ar','44100','-f','f32le','pipe:1'],{maxBuffer:4000000});let peak=0,sum=0,active=0;for(let i=0;i<b.length;i+=4){const v=b.readFloatLE(i);if(!Number.isFinite(v))throw Error('Nonfinite PCM');peak=Math.max(peak,Math.abs(v));sum+=v*v;if(Math.abs(v)>.001)active++;}return {seconds:b.length/4/44100,peak,rms:Math.sqrt(sum/(b.length/4)),activeFraction:active/(b.length/4),sha256:hash(file)};}
const report={scope:'ACTUAL_DECODED_PCM_NOT_PHONE_LISTENING_UAT',sourceSHA:hash(source),before:signal(old),after:signal(bell),checks:[]};
function check(pass,name){report.checks.push({name,pass:!!pass});}
check(report.sourceSHA==='1dec4970847aa75bc8ef8d2cf518253554486ef8d2ee4460b3f270c9ca3569ac','owner source hash unchanged');
check(report.before.sha256==='0ecb168ed8e1b7081c6112a3457ddc397b5a6355b03aede94c23a94ba80e8a13','exact released silent asset preserved');
check(report.before.peak<.00001&&report.before.rms<.00001,'new amplitude gate rejects former duration-only silent bell');
check(report.after.seconds>.8&&report.after.seconds<1,'corrected cue has bounded intended duration');
check(report.after.peak>.05&&report.after.peak<.98&&report.after.rms>.003&&report.after.activeFraction>.5,'corrected decoded signal is non-silent without clipping or sparse click');
check(report.after.sha256!==report.before.sha256,'new bell differs from released silent asset');
report.status=report.checks.every(c=>c.pass)?'PASS':'FAIL';fs.writeFileSync(path.join(dir,'bell-signal.json'),JSON.stringify(report,null,2)+'\n');console.log(JSON.stringify(report,null,2));if(report.status!=='PASS')process.exitCode=1;
