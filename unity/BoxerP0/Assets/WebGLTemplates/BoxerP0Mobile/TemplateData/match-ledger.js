/* Device ledger, not an account/anti-cheat service. History and coins share one transaction. */
const BoxerLedgerModel = (() => {
  const limit=1000, id=v=>typeof v==='string'&&/^[a-f0-9]{32}$/.test(v);
  const reward=r=>r==='PLAYER_WIN'?100:r==='DRAW'?50:r==='OPPONENT_WIN'?25:-1;
  function text(v,max){
    if(typeof v!=='string'||v.length===0||v.length>max*4||v!==v.normalize('NFC')||v!==v.trim()||v.includes('  '))return false;
    if(!/^[\p{L}\p{Nd}][\p{L}\p{Nd}\p{Mn}\p{Mc} .’'\-]*$/u.test(v))return false;
    if([...v].some(c=>c.codePointAt(0)>65535))return false;
    return (v.match(/[^\p{M}]\p{M}*/gu)||[]).length<=max;
  }
  const range=(v,a,b)=>typeof v==='number'&&Number.isFinite(v)&&v>=a&&v<=b;
  function review(r){
    if(!r||r.schema!==1||!id(r.id)||typeof r.sourceVersion!=='string'||!r.sourceVersion.length||r.sourceVersion.length>100||r.opponent!=='RAMIREZ'||reward(r.result)<0||!['KO','POINTS'].includes(r.reason))return false;
    if(!range(r.seconds,0,45.001)||!['playerHP','opponentHP','playerStamina','playerCapacity'].every(k=>range(r[k],0,100)))return false;
    if(!['accepted','hits','blocked','misses','received','defended','opponentMisses'].every(k=>Number.isInteger(r[k])&&range(r[k],0,10000))||r.hits+r.blocked+r.misses>r.accepted)return false;
    return r.reason==='KO' ? r.result==='PLAYER_WIN'&&r.opponentHP===0||r.result==='OPPONENT_WIN'&&r.playerHP===0 : r.result===(Math.abs(r.playerHP-r.opponentHP)<=.0001?'DRAW':r.playerHP>r.opponentHP?'PLAYER_WIN':'OPPONENT_WIN');
  }
  function entry(e){
    if(!e||!review(e.review)||e.reward!==reward(e.review.result)||typeof e.completedUtc!=='string'||!/^\d{4}-\d\d-\d\dT\d\d:\d\d:\d\d\.\d{3}Z$/.test(e.completedUtc))return false;
    const time=new Date(e.completedUtc);if(!Number.isFinite(+time)||time.getUTCFullYear()<1||time.toISOString()!==e.completedUtc)return false;
    return e.profileId==='' ? e.fighterName==='BOXER'&&e.nationality==='' : id(e.profileId)&&text(e.fighterName,24)&&text(e.nationality,40);
  }
  function valid(l){
    if(!l||l.schema!==1||!Array.isArray(l.entries)||l.entries.length>limit)return false;
    const ids=new Set();let sum=0;
    for(const e of l.entries){if(!entry(e)||ids.has(e.review.id))return false;ids.add(e.review.id);sum+=e.reward;}
    return Number.isInteger(l.balance)&&l.balance===sum;
  }
  function canonical(e){
    const r={};for(const k of ['schema','id','sourceVersion','opponent','result','reason','seconds','playerHP','opponentHP','playerStamina','playerCapacity','accepted','hits','blocked','misses','received','defended','opponentMisses'])r[k]=e.review[k];
    return JSON.stringify({review:r,profileId:e.profileId,fighterName:e.fighterName,nationality:e.nationality,completedUtc:e.completedUtc,reward:e.reward});
  }
  function append(l,e){
    if(!valid(l)||!entry(e))throw Error('Dữ liệu ví/trận không hợp lệ; không tự xóa hoặc cộng coin.');
    const old=l.entries.find(x=>x.review.id===e.review.id);
    if(old){if(canonical(old)!==canonical(e))throw Error('ID trận trùng nhưng khác nội dung. Không cộng thưởng.');return l;}
    if(l.entries.length>=limit)throw Error('Đã đạt giới hạn thử nghiệm 1000 trận. Dữ liệu cũ được giữ nguyên.');
    return {schema:1,balance:l.balance+e.reward,entries:[...l.entries,JSON.parse(canonical(e))]};
  }
  return {id,reward,text,review,entry,valid,canonical,append,empty:()=>({schema:1,balance:0,entries:[]})};
})();

function createBoxerLedgerStorage({databaseName='BOXER_MATCH_HISTORY_V1',factory=window.indexedDB}={}) {
  let opening;
  function open(){
    if(!opening)opening=new Promise((resolve,reject)=>{
      if(!factory){reject(Error('Trình duyệt không cho phép IndexedDB.'));return;}
      const request=factory.open(databaseName,1);
      request.onupgradeneeded=()=>request.result.createObjectStore('ledger');
      let failed=false;
      request.onerror=()=>{failed=true;reject(request.error||Error('Không mở được bộ nhớ cục bộ.'));};
      request.onblocked=()=>{failed=true;reject(Error('Bộ nhớ đang bị tab khác chặn; hãy thử lại.'));};
      request.onsuccess=()=>{const db=request.result;if(failed){db.close();return;}db.onversionchange=()=>db.close();resolve(db);};
    }).catch(error=>{opening=null;throw error;});return opening;
  }
  async function transaction(entry){
    const db=await open();return new Promise((resolve,reject)=>{
      let result,error;
      const tx=db.transaction('ledger',entry?'readwrite':'readonly'),store=tx.objectStore('ledger'),request=store.get('state');
      request.onsuccess=()=>{
        try{
          const stored=request.result;
          const prior=stored===undefined?BoxerLedgerModel.empty():JSON.parse(stored);
          if(!BoxerLedgerModel.valid(prior))throw Error('Dữ liệu ví bị lỗi; giữ nguyên, không đặt lại số dư.');
          result=entry?BoxerLedgerModel.append(prior,entry):prior;
          if(entry)store.put(JSON.stringify(result),'state');
        }catch(e){error=e;tx.abort();}
      };
      tx.oncomplete=()=>resolve(result);
      tx.onabort=()=>reject(error||tx.error||Error('Giao dịch lưu bị hủy. Thưởng chưa được xác nhận.'));
      tx.onerror=()=>{}; // onabort is the single error receipt; never acknowledge request success alone.
    });
  }
  return {read:()=>transaction(null),append:entry=>transaction(entry),close:async()=>{if(opening)(await opening).close();}};
}

function createBoxerLedger({send}) {
  const storage=createBoxerLedgerStorage(),root=document.createElement('section');
  root.id='match-history';root.lang='vi';root.className='hidden';root.setAttribute('aria-label','Lịch sử trận và ví coin');
  root.innerHTML=`<div id="history-panel"><p class="history-kicker">BOXER · DEVICE RECORD</p><h1>LỊCH SỬ TRẬN</h1>
    <p class="history-note">Chỉ các trận được lưu từ đợt này trên trình duyệt / thiết bị này. Không phải bảng xếp hạng online.</p>
    <div id="history-wallet"></div><div id="history-tally"></div><p id="history-error" role="alert"></p>
    <p id="history-empty"></p><div id="history-list"></div>
    <p class="history-note">Thắng 100 · hòa 50 · thua 25 coin ảo. Mỗi trận hoàn tất nhận một lần. Tập luyện / hủy trận không có thưởng; KO không thưởng thêm.</p>
    <p class="history-note">Chưa có mua sắm. Giới hạn thử nghiệm 1000 trận, không tự cắt lịch sử. Xóa dữ liệu trình duyệt có thể mất lịch sử và ví.</p>
    <button id="history-retry" class="hidden">THỬ LƯU THƯỞNG LẠI</button><button id="history-home">HOME</button></div>`;
  document.body.appendChild(root);
  const field=id=>root.querySelector('#history-'+id);
  field('home').addEventListener('click',()=>send('BrowserHistoryCommand','home'));
  field('retry').addEventListener('click',()=>send('BrowserHistoryCommand','retry'));
  let session=null,available=false,queued=[],view=null;
  function notify(payload){if(available)send('BrowserLedgerState',JSON.stringify(payload));else queued.push(payload);}
  async function read(){const epoch=session;try{const ledger=await storage.read();if(epoch===session)notify({session:epoch,ledger});}catch(e){if(epoch===session)notify({session:epoch,error:String(e.message||e)});}}
  function resize(){const v=window.visualViewport;root.style.height=`${v?v.height:innerHeight}px`;root.style.top=`${v?v.offsetTop:0}px`;}
  window.addEventListener('resize',resize);if(window.visualViewport){visualViewport.addEventListener('resize',resize);visualViewport.addEventListener('scroll',resize);}
  window.addEventListener('focus',()=>{if(session)read();});
  function render(v){
    const wasOpen=!!view?.open;
    view=v;root.classList.toggle('hidden',!v.open);root.inert=!v.open;
    if(!v.open){if(wasOpen)document.querySelector('canvas')?.focus();return;}resize();field('wallet').textContent=v.status||'Đang đọc ví…';field('error').textContent=v.error||'';
    field('retry').classList.toggle('hidden',!(v.pending>0));field('list').replaceChildren();
    const ledger=v.ledger&&BoxerLedgerModel.valid(v.ledger)?v.ledger:null;
    field('empty').textContent=!ledger?'Chưa xác nhận được dữ liệu ví.':ledger.entries.length===0?'CHƯA CÓ TRẬN ĐƯỢC LƯU · 0 COIN':'';
    field('tally').textContent=ledger?`THẮNG ${ledger.entries.filter(e=>e.review.result==='PLAYER_WIN').length} · HÒA ${ledger.entries.filter(e=>e.review.result==='DRAW').length} · THUA ${ledger.entries.filter(e=>e.review.result==='OPPONENT_WIN').length}`:'';
    if(!ledger)return;
    for(const e of [...ledger.entries].reverse()){
      const card=document.createElement('details');card.className='history-entry';card.dataset.matchId=e.review.id;
      const title=document.createElement('summary'),r=e.review,verdict=r.result==='PLAYER_WIN'?'THẮNG':r.result==='DRAW'?'HÒA':'THUA';
      title.textContent=`${e.fighterName} vs RAMIREZ · ${verdict} · +${e.reward} COIN`;card.append(title);
      const text=document.createElement('p');text.className='history-endpoints';
      text.textContent=`${e.completedUtc} · ${r.reason} · ${r.seconds.toFixed(1)}s\nID ${r.id}\nTên / quốc tịch lúc đấu: ${e.fighterName} / ${e.nationality||'chưa khai'}\nHP cuối: bạn ${r.playerHP.toFixed(1)} · Ramirez ${r.opponentHP.toFixed(1)}\nStamina ${r.playerStamina.toFixed(1)} · Capacity ${r.playerCapacity.toFixed(1)}\nĐã ra ${r.accepted} · trúng ${r.hits} · bị đỡ ${r.blocked} · hụt ${r.misses}\nBị trúng ${r.received} · đã đỡ ${r.defended} · đối thủ hụt ${r.opponentMisses}\nBản game: ${r.sourceVersion}`;
      card.append(text);field('list').append(card);
    }
  }
  return {
    ready(){available=true;for(const message of queued)send('BrowserLedgerState',JSON.stringify(message));queued=[];},
    contains:target=>!!view?.open&&root.contains(target),
    command(code,payload){
      if(code===0){session=payload;read();return;}
      if(code===2){render(JSON.parse(payload));return;}
      if(code===3){if(payload===session)read();return;}
      if(code!==1)return;
      const request=JSON.parse(payload),epoch=session;
      if(request.session!==epoch||!BoxerLedgerModel.entry(request.entry))return;
      storage.append(request.entry).then(ledger=>{if(epoch===session)notify({session:epoch,id:request.entry.review.id,ledger});})
        .catch(e=>{if(epoch===session)notify({session:epoch,id:request.entry.review.id,error:String(e.message||e)});});
    }
  };
}
