// Presentation-only unit harness. No real video/audio/UAT claim.
const assert=require('assert/strict'),fs=require('fs'),vm=require('vm');
const script=fs.readFileSync('unity/BoxerP0/Assets/WebGLTemplates/BoxerP0Mobile/TemplateData/ring-media.js','utf8');
let checks=0;const log=[];
function check(ok,name){assert.ok(ok,name);checks++;log.push('PASS '+name);}
function harness(){
  const sent=[],listeners={},nodes=[];let fetchError=false,now=0;
  function element(id){return {id,hidden:true,classList:{add(){},remove(){}},textContent:'',currentTime:0,playbackRate:1,
    addEventListener(name,fn){(listeners[id+':'+name]??=[]).push(fn);},play(){return Promise.resolve();},pause(){},load(){},focus(){}};}
  const els=Object.fromEntries(['ring-intro','ring-video','ring-status','ring-retry','ring-back','unity-canvas'].map(n=>[n,element(n)]));
  class Context{constructor(){this.state='suspended';}resume(){this.state='running';return Promise.resolve();}createGain(){return {gain:{value:1},connect(){},disconnect(){}};}
    decodeAudioData(){return Promise.resolve({duration:6});}createBufferSource(){const n={connect(){},disconnect(){},start(){this.started=true;},stop(){this.stopped=true;}};nodes.push(n);return n;}}
  const window={AudioContext:Context};
  vm.runInNewContext(script,{window,document:{getElementById:id=>els[id]},performance:{now:()=>++now},fetch:async()=>{if(fetchError)throw Error('offline');return {ok:true,arrayBuffer:async()=>new ArrayBuffer(0)};}});
  const media=window.createBoxerRingMedia({send:(...args)=>sent.push(args),version:'test'});
  const emit=async(id,name)=>{for(const f of listeners[id+':'+name]||[])await f();};
  const flush=async()=>{for(let i=0;i<12;i++)await Promise.resolve();};
  return {media,sent,els,nodes,emit,flush,setError(v){fetchError=v;}};
}
(async()=>{
  const template=fs.readFileSync('unity/BoxerP0/Assets/WebGLTemplates/BoxerP0Mobile/index.html','utf8');
  check(template.includes("unityInstance.SendMessage('Boxer P0 Bootstrap', method")&&template.includes('createBoxerRingMedia({send:sendRing'),'media callback routes to bootstrap, not input systems');
  const h=harness();await h.media.unlock();h.media.start(10);await h.flush();
  check(h.media.state.phase==='Intro'&&h.sent.length===0,'intro does not call native start before ended');
  await h.emit('ring-video','playing');h.els['ring-video'].currentTime=4.9;await h.emit('ring-video','ended');
  check(h.sent.length===0,'premature under-five-second ended is rejected');
  h.els['ring-video'].currentTime=5.166667;await h.emit('ring-video','ended');
  check(h.sent.length===1&&h.sent[0][0]==='BrowserIntroReady'&&h.sent[0][1]==='10','completed media sends exact current token');
  check(h.media.state.crowdActive&&h.media.state.events.filter(e=>e.name==='start-bell').length===1,'completed intro schedules one bell/crowd');
  await h.emit('ring-video','ended');check(h.sent.length===1,'duplicate ended does not send callback again');
  h.media.finish();h.media.finish();check(!h.media.state.crowdActive&&h.media.state.events.filter(e=>e.name==='end-bell').length===1,'result stops crowd and emits end bell once');
  h.media.start(11);await h.flush();await h.emit('ring-video','playing');await h.emit('ring-back','click');h.els['ring-video'].currentTime=6;await h.emit('ring-video','ended');
  check(h.media.state.phase==='Idle'&&h.sent.length===2&&h.sent[1][0]==='BrowserCancelIntro','cancelled media cannot start combat');
  check(h.nodes.every(n=>n.stopped),'cancel stops all live audio nodes');
  h.media.setSound(false);check(h.media.state.masterGain===0,'mute follows master gain');h.media.setSound(true);check(h.media.state.masterGain===1,'unmute restores master gain');
  const blocked=harness();blocked.media.start(20);await blocked.flush();check(blocked.media.state.phase==='Intro'&&blocked.sent.length===0,'locked AudioContext never bypasses intro');
  await blocked.emit('ring-retry','click');await blocked.flush();await blocked.emit('ring-video','playing');blocked.els['ring-video'].currentTime=5.17;await blocked.emit('ring-video','ended');
  check(blocked.sent.length===1,'real retry handler resumes context and replays intro');
  const failed=harness();failed.setError(true);failed.media.start(30);await failed.flush();check(failed.media.state.error&&failed.sent.length===0,'media failure keeps combat locked');
  failed.setError(false);await failed.emit('ring-retry','click');await failed.flush();await failed.emit('ring-video','playing');failed.els['ring-video'].currentTime=5.17;await failed.emit('ring-video','ended');
  check(failed.sent.length===1,'failed asset load can retry successfully');
  log.push(`TOTAL=${checks} PASS=${checks} FAIL=0`);fs.mkdirSync('evidence/wave1/ring-intro',{recursive:true});fs.writeFileSync('evidence/wave1/ring-intro/media-unit-tests.txt',log.join('\n')+'\n');console.log(log.join('\n'));
})().catch(error=>{console.error(error);process.exitCode=1;});
