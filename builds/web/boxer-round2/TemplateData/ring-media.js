// Owner-supplied clip/audio only. No score or input mutation outside normal lifecycle callbacks.
window.createBoxerRingMedia = function ({send, version}) {
  const overlay = document.getElementById('ring-intro');
  const video = document.getElementById('ring-video');
  const status = document.getElementById('ring-status');
  const retry = document.getElementById('ring-retry');
  const back = document.getElementById('ring-back');
  const state = {phase:'Idle',token:0,sound:true,events:[],error:null};
  let context, master, loading, buffers, introSound, crowd, epoch=0, introStarted=false;
  const nodes=new Set();
  const url = name => `StreamingAssets/RingMedia/${name}?v=${encodeURIComponent(version)}`;
  video.src=url('ring-girl.mp4');
  const event = name => state.events.push({name,token:state.token,at:performance.now(),videoTime:video.currentTime});
  function prepare() {
    if (!context) {
      context = new (window.AudioContext || window.webkitAudioContext)();
      master=context.createGain(); master.gain.value=state.sound?1:0; master.connect(context.destination);
    }
    if (!loading) loading=Promise.all(['intro','bell','crowd'].map(async name=>{
      const response=await fetch(url(name+'.mp3'));
      if(!response.ok) throw Error(`Ring ${name}: HTTP ${response.status}`);
      const buffer=await context.decodeAudioData(await response.arrayBuffer());
      return [name,buffer];
    })).then(entries=>{buffers=Object.fromEntries(entries);return buffers;})
      .catch(error=>{loading=null;throw error;});
    return loading;
  }
  function unlock() {
    const ready=prepare();
    // Resume synchronously from an actual click, before motion permission/Unity loading awaits.
    const resumed=context.resume();
    return Promise.all([ready,resumed]);
  }
  function source(name, gain=1, loop=false, offset=0) {
    const node=context.createBufferSource(), volume=context.createGain();
    node.buffer=buffers[name]; node.loop=loop; volume.gain.value=gain;
    node.ringVolume=volume;nodes.add(node);
    node.connect(volume); volume.connect(master); node.start(0,offset);
    node.onended=()=>{nodes.delete(node);node.disconnect();volume.disconnect();};
    return node;
  }
  function halt(node) { if(node) {node.onended=null;try{node.stop();}catch(_){}node.disconnect();node.ringVolume.disconnect();nodes.delete(node);} }
  function haltIntro() {halt(introSound);introSound=null;}
  function fail(error) {
    if(state.phase!=='Intro')return;
    video.pause();haltIntro();state.error=String(error);event('media-error');
    status.textContent='Không phát được phần giới thiệu. Trận vẫn chưa bắt đầu.';
    retry.classList.remove('hidden');
  }
  function stop() {
    ++epoch; state.phase='Idle'; video.pause(); haltIntro(); halt(crowd);crowd=null;
    for(const node of nodes)halt(node);
    overlay.classList.add('hidden');retry.classList.add('hidden');
    event('stop');
  }
  async function playIntro(currentEpoch) {
    try {
      await prepare();
      if(currentEpoch!==epoch||state.phase!=='Intro')return;
      if(context.state!=='running') {
        status.textContent='Chạm PHÁT GIỚI THIỆU để bật âm thanh. Trận chưa bắt đầu.';
        retry.classList.remove('hidden');return;
      }
      status.textContent='ROUND 1 · Chuông sẽ mở trận';retry.classList.add('hidden');
      await video.play();
    } catch(error) {if(currentEpoch===epoch)fail(error);}
  }
  function start(token) {
    stop();state.token=token;state.phase='Intro';state.error=null;introStarted=false;
    video.currentTime=0;video.playbackRate=1;overlay.classList.remove('hidden');
    status.textContent='Đang chuẩn bị ROUND 1…';event('intro-request');
    playIntro(epoch);
  }
  video.addEventListener('playing',()=>{
    if(state.phase!=='Intro'||!buffers)return;
    haltIntro();
    if(video.currentTime<buffers.intro.duration)introSound=source('intro',1,false,video.currentTime);
    if(!introStarted){introStarted=true;event('intro-playing');}
  });
  video.addEventListener('waiting',haltIntro);
  video.addEventListener('pause',haltIntro);
  video.addEventListener('error',()=>fail(Error('Ring video decode/load failed')));
  video.addEventListener('ended',()=>{
    if(state.phase!=='Intro'||!introStarted||video.currentTime<5||!buffers)return;
    haltIntro();event('intro-ended');
    // If the device suspended audio, keep combat locked until an explicit retry click.
    if(context.state!=='running') {
      status.textContent='Chạm PHÁT GIỚI THIỆU để tiếp tục với âm thanh.';
      retry.classList.remove('hidden');return;
    }
    state.phase='Fight'; overlay.classList.add('hidden');
    source('bell',.9);event('start-bell');
    crowd=source('crowd',.26,true);event('crowd-start');
    send('BrowserIntroReady',String(state.token));
    document.getElementById('unity-canvas').focus();
  });
  function finish() {
    if(state.phase!=='Fight')return;
    state.phase='Result';halt(crowd);crowd=null;event('crowd-stop');
    if(context.state==='running'){source('bell',.9);event('end-bell');}
    else event('end-bell-audio-suspended');
  }
  retry.addEventListener('click',async()=>{
    const current=epoch;
    try{await unlock();if(current!==epoch)return;if(video.error)video.load();video.currentTime=0;introStarted=false;await playIntro(current);}
    catch(error){if(current===epoch)fail(error);}
  });
  back.addEventListener('click',()=>{
    if(state.phase!=='Intro')return;
    const token=state.token;stop();send('BrowserCancelIntro',String(token));
  });
  function setSound(enabled) {state.sound=enabled;if(master)master.gain.value=enabled?1:0;event(enabled?'sound-on':'sound-off');}
  return {start,stop,finish,unlock,setSound,get state(){return {...state,audioState:context?.state,masterGain:master?.gain.value,crowdActive:!!crowd,buffers:buffers?Object.fromEntries(Object.entries(buffers).map(([n,b])=>[n,b.duration])):null};}};
};
