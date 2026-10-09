// Deterministic generated diagnostic artifact, not a hand-authored source asset.
const fs=require('fs'),path=require('path');
const dir=path.resolve(__dirname,'../evidence/wave1/punch-feel');
const rows=fs.readFileSync(path.join(dir,'motion-trace.csv'),'utf8').trim().split(/\r?\n/).slice(1).map(l=>{
 const [mode,intent,phase,seconds,x,y,z,speed]=l.split(',');return{mode,intent,phase,ms:+seconds*1000,speed:+speed};
});
const summary=fs.readFileSync(path.join(dir,'motion-summary.csv'),'utf8').trim().split(/\r?\n/).slice(1).map(l=>{
 const [mode,intent,commit,extend,recover,peak]=l.split(',');return{mode,intent,commit:+commit,peak:+peak};
});
const colors={Legacy:'#aaa',Curve:'#f1b94a',Fast:'#4ce0b0'};
let svg='<svg xmlns="http://www.w3.org/2000/svg" width="920" height="590" viewBox="0 0 920 590"><rect width="920" height="590" fill="#14191e"/><g font-family="Arial" fill="#eee"><text x="32" y="34" font-size="20">Shared-pose strike velocity: timing and curve A/B</text><text x="32" y="57" font-size="13">Static roots, neutral footwork, after IK. Synthetic measurements, not phone latency or physiological validation.</text>';
for(const [index,intent] of ['Jab','Cross'].entries()){
 const ox=65+index*445,oy=103,w=365,h=370;
 svg+=`<text x="${ox}" y="${oy-10}" font-size="18">${intent}</text><path d="M ${ox} ${oy} V ${oy+h} H ${ox+w}" stroke="#82939c" fill="none"/>`;
 for(let v=0;v<=10;v+=2){const y=oy+h-v/10*h;svg+=`<path d="M ${ox} ${y} H ${ox+w}" stroke="#2d3943"/><text x="${ox-24}" y="${y+5}" font-size="12">${v}</text>`;}
 for(let t=0;t<=250;t+=50){const x=ox+t/250*w;svg+=`<text x="${x-12}" y="${oy+h+22}" font-size="12">${t}</text>`;}
 for(const mode of ['Legacy','Curve','Fast']){
  const entry=summary.find(r=>r.intent===intent&&r.mode===mode);
  const pts=rows.filter(r=>r.intent===intent&&r.mode===mode&&r.phase==='Extend').map(r=>`${ox+(entry.commit+r.ms)/250*w},${oy+h-r.speed/10*h}`).join(' ');
  svg+=`<polyline points="${pts}" stroke="${colors[mode]}" stroke-width="2.5" fill="none"/>`;
 }
 svg+=`<text x="${ox+30}" y="${oy+h+48}" font-size="13">Milliseconds since accepted action</text>`;
}
svg+='<text x="18" y="310" font-size="12" transform="rotate(-90 18 310)">Glove velocity (m/s)</text>';
for(const [i,mode] of ['Legacy','Curve','Fast'].entries())svg+=`<rect x="${90+i*265}" y="549" width="25" height="4" fill="${colors[mode]}"/><text x="${125+i*265}" y="556" font-size="14">${mode}: ${mode==='Fast'?'new durations':'old durations'}</text>`;
svg+='</g></svg>';
fs.writeFileSync(path.join(dir,'motion-comparison.svg'),svg);console.log('GENERATED motion-comparison.svg');
