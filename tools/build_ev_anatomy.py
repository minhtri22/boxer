"""Bake CC0 MakeHuman mesh/weights into the existing Boxer joint vocabulary.

Offline asset preparation only; no third-party program code is bundled.
Input directory contains base.obj, default.mhskel, default_weights.mhw and
the two documented CC0 morphology targets. Output is a Unity TextAsset.
"""
import argparse, json, math, struct
from pathlib import Path
import numpy as np

p=argparse.ArgumentParser();p.add_argument('source',type=Path);p.add_argument('output',type=Path);a=p.parse_args()
verts=[];faces=[];group=''
for line in (a.source/'base.obj').read_text().splitlines():
    t=line.split()
    if not t:continue
    if t[0]=='v':verts.append(list(map(float,t[1:4])))
    elif t[0]=='g':group=t[1]
    elif t[0]=='f' and group=='body':faces.append([int(x.split('/')[0])-1 for x in t[1:]])
v=np.array(verts)
for filename,amount in [('caucasian-male-young.target',1),('universal-male-young-maxmuscle-averageweight.target',.85)]:
    for l in (a.source/filename).read_text().splitlines():
        t=l.split()
        if len(t)==4 and not l.startswith('#'):v[int(t[0])]+=np.array(list(map(float,t[1:])))*amount
rig=json.loads((a.source/'default.mhskel').read_text());weights=json.loads((a.source/'default_weights.mhw').read_text())['weights']
def joint(name):return v[rig['joints'][name+'____head']].mean(0)
def end(name):return v[rig['joints'][name+'____tail']].mean(0)
print('morphed joints', {n:joint(n).round(4).tolist() for n in ['head','upperarm01.L','lowerarm01.L','wrist.L','upperleg01.L','lowerleg01.L','foot.L','eye.L']})
print('body top',v[np.unique(np.concatenate(faces))].max(0))
# Each bone is an independent read-only follower of an existing anatomical anchor.
# 0 torso, 1 head, 2/3 lead arm, 4/5 rear arm, 6/7 lead leg, 8/9 rear leg.
src=[];dst=[]
src.append((np.array([0.,joint('upperarm01.L')[1],0]),np.array([0.,joint('upperarm01.L')[1]+1,0])))
dst.append((np.array([0,1.43,0]),np.array([0,1.55,0])))
src.append((joint('head'),end('head')));dst.append((np.array([0,1.62,0]),np.array([0,1.77,0])))
for side,sign in [('L',-1),('R',1)]:
    shoulder=np.array([sign*.38,1.43,.02]);elbow=shoulder+np.array([sign*.12,-.318,0]);wrist=elbow+np.array([sign*.11,-.275,.09]);wrist=elbow+(wrist-elbow)/np.linalg.norm(wrist-elbow)*.31
    src.extend([(joint('upperarm01.'+side),joint('lowerarm01.'+side)),(joint('lowerarm01.'+side),joint('wrist.'+side))]);dst.extend([(shoulder,elbow),(elbow,wrist)])
for side,sign in [('L',-1),('R',1)]:
    hip=np.array([sign*.20,.85,0]);ankle=np.array([sign*.22,.04,.17 if side=='L' else -.17])
    delta=ankle-hip;distance=np.linalg.norm(delta);direction=delta/distance;along=(.46**2-.44**2+distance**2)/(2*distance);height=math.sqrt(.46**2-along**2)
    pole=np.array([0.,0.,1.]);pole-=direction*np.dot(pole,direction);pole/=np.linalg.norm(pole);knee=hip+direction*along+pole*height
    src.extend([(joint('upperleg01.'+side),joint('lowerleg01.'+side)),(joint('lowerleg01.'+side),joint('foot.'+side))]);dst.extend([(hip,knee),(knee,ankle)])
def frame(start,end):
    y=(end-start)/np.linalg.norm(end-start);z=np.array([0.,0.,1.]);x=np.cross(y,z);x/=np.linalg.norm(x);z=np.cross(x,y);return np.stack([x,y,z],axis=1)
sw=np.zeros((len(v),10));hands=np.zeros(len(v))
for name,pairs in weights.items():
    b=0;side=0 if name.endswith('.L') else 1
    if 'upperarm' in name:b=2+side*2
    elif 'lowerarm' in name or 'wrist' in name or 'finger' in name or 'metacarpal' in name:b=3+side*2
    elif 'upperleg' in name:b=6+side*2
    elif 'lowerleg' in name or 'foot' in name or 'toe' in name:b=7+side*2
    elif name in ['head','neck03'] or name.startswith(('special','eye','jaw','tongue','levator','oris','temporalis','risorius','oculi')):b=1
    for i,w in pairs:
        sw[i,b]+=w
        if 'finger' in name or 'metacarpal' in name or name.startswith('wrist'):hands[i]+=w
sw[sw.sum(1)==0,0]=1;sw/=sw.sum(1)[:,None]
# Neck and face keep one head transform; torso is a rigid contact surface. Shoulder
# transition vertices retain the supplied smooth weights, eliminating separate balls.
head_start=joint('head')[1]-.55
for i in range(len(v)):
    if v[i,1]>head_start:sw[i]=0;sw[i,1]=1
out=np.zeros_like(v)
for b,((s,e),(t,u)) in enumerate(zip(src,dst)):
    if b==0:
        q=v.copy();q[:,0]*=-.17;q[:,2]*=.105;q[:,1]=np.interp(v[:,1],[joint('upperleg01.L')[1],s[1],joint('head')[1]],[.85,1.43,1.62])
    elif b==1:
        # Uniform head scale preserves facial proportions, unlike mapping onto a ball.
        q=(v-s)*np.array([-.115,.115,.115])+t
    else:
        r=frame(s,e);rr=frame(t,u);local=(v-s)@r
        radial=.095 if b>=6 else .115
        local[:,0]*=-radial;local[:,2]*=radial;local[:,1]*=np.linalg.norm(u-t)/np.linalg.norm(e-s)
        q=local@rr.T+t
    out+=q*sw[:,b,None]
# Trim invisible hands/feet and anatomy entirely below trunks. No explicit anatomy
# is needed or exported for this clothed sporting character.
tris=[]
for f in faces:
    c=v[f].mean(0);w=sw[f].mean(0);q=out[f].mean(0)
    if hands[f].mean()>.08:continue
    if q[1]<.1:continue
    if .65<q[1]<.90 and abs(q[0])<.24:continue
    if w[3]>.9 and c[0]>joint('wrist.L')[0]-.08:continue
    if w[5]>.9 and c[0]<joint('wrist.R')[0]+.08:continue
    for j in range(1,len(f)-1):tris.append([f[0],f[j+1],f[j]])
used=np.unique(tris);remap={int(old):i for i,old in enumerate(used)}
tri=np.array([[remap[i] for i in t] for t in tris],dtype=np.int32);pos=out[used];ws=sw[used]
uv=[]
ey=joint('eye.L');eye_x=abs(ey[0]);eye_y=ey[1]
for old in used:
    x,y,z=v[old]
    if sw[old,1]>.5:
        # Front projection uses landmark spacing. Side/back wraps continuously.
        angle=math.atan2(-x,z-joint('head')[2])
        u=.5+angle*.195
        vv=.59+(y-eye_y)*.36
        uv.append([u,float(np.clip(vv,.02,.98))])
    else:uv.append([math.atan2(out[old,0],out[old,2])/(2*math.pi),out[old,1]])
# Per-face head material classification in a second submesh.
bodytri=[];headtri=[]
for t in tri:
    (headtri if ws[t,1].mean()>.6 else bodytri).extend(t.tolist())
payload={'positions':pos.round(7).flatten().tolist(),'uv':np.array(uv).round(7).flatten().tolist(),'weights':ws.round(7).flatten().tolist(),'body':bodytri,'head':headtri,'boneStarts':np.array([x[0] for x in dst]).flatten().tolist(),'boneEnds':np.array([x[1] for x in dst]).flatten().tolist()}
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(payload,separators=(',',':')))
print('output',len(pos),'vertices',len(tri),'triangles','head',len(headtri)//3,'body',len(bodytri)//3)
