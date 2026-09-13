using UnityEngine;

namespace BoxerP0
{
    public struct Round2Frame
    {
        public Vector3 Position, Endpoint;
        public Quaternion Rotation;
        public PunchIntent Intent;
        public ActionPhase Phase;
        public float T, Duration, HeadOffset;
        public Vector3 World(Vector3 p) => Position + Rotation*p;
        public ArmChainSolution Arm(bool left) => Round2Motion.Sample(left,Intent,Phase,T,Endpoint);
        public Vector3 Target(int index)
        {
            if(index<2) return World(Arm(index==0).Wrist);
            Vector3 shift=Round2Motion.Shift(Intent,Phase,T);
            if(index>=4) return World(Round2Motion.Torso(Intent,Phase,T)*new Vector3((index-5)*0.165f,1.37f,0f)+shift);
            return World((index==2 ? new Vector3(HeadOffset,1.62f,0f) : new Vector3(0f,1.16f,0f))+shift);
        }
        public static Round2Frame Between(Round2Frame a, Round2Frame b, float t)
        {
            Round2Frame r=b;
            r.Position=Vector3.Lerp(a.Position,b.Position,t);
            r.Rotation=Quaternion.Slerp(a.Rotation,b.Rotation,t);
            r.HeadOffset=Mathf.Lerp(a.HeadOffset,b.HeadOffset,t);
            if(a.Intent==b.Intent && a.Phase==b.Phase) r.T=Mathf.Lerp(a.T,b.T,t);
            else if(a.Intent==b.Intent && a.Phase==ActionPhase.Extend && b.Phase==ActionPhase.Recover)
            { r.Phase=ActionPhase.Extend; r.T=Mathf.Lerp(a.T,1f,t); }
            else if(b.Phase==ActionPhase.Extend) r.T=b.T*t;
            return r;
        }
    }

    // Only this component writes visible joints, gloves and target transforms.
    // Controllers own intent/timing/root motion. All contact runs after both controllers.
    [DefaultExecutionOrder(250)]
    public sealed class Round2CombatRig : MonoBehaviour
    {
        PlayerBoxer _player;
        OpponentBoxer _opponent;
        Rig _p,_o;
        Round2Frame _previousP,_previousO;
        bool _ready;
        public long Samples { get; private set; }
        public long Contacts { get; private set; }
        public float LastContactFraction { get; private set; }
        public float LastContactGap { get; private set; }
        public float LastUpdateMs { get; private set; }

        sealed class Arm
        {
            public Transform Shoulder,Upper,Elbow,Forearm,Glove;
        }
        sealed class Rig
        {
            public Arm Left,Right;
            public Transform Root, Head, Body, Chest, ChestLeft, ChestRight, Neck;
        }
        public void Initialize(PlayerBoxer player, OpponentBoxer opponent)
        {
            _player=player; _opponent=opponent;
            _p=Build(player.transform,player.LeftGlove,player.RightGlove,player.Head,player.BodyCollider.transform,true);
            _o=Build(opponent.transform,opponent.LeftGlove,opponent.RightGlove,opponent.HeadCollider.transform,opponent.BodyCollider.transform,false);
        }
        Rig Build(Transform root,Transform left,Transform right,Transform head,Transform body,bool player)
        {
            var r=new Rig { Root=root,Head=head,Body=body };
            r.Left=BuildArm(root,left,true,player); r.Right=BuildArm(root,right,false,player);
            body.localScale=Vector3.one;
            var bodyCollider=body.GetComponent<SphereCollider>(); bodyCollider.radius=0.25f; bodyCollider.center=Vector3.zero;
            if(!player)
            {
                // Body target is now a visible sphere, covered only by intersecting torso surfaces.
                var old=body.GetComponent<Renderer>(); if(old!=null) old.enabled=false;
                r.Body=Sphere("R2 Abdomen",root,0.25f,Skin);
                r.Chest=Sphere("R2 Chest",root,0.18f,Skin);
                r.ChestLeft=Sphere("R2 Left Chest",root,0.18f,Skin);
                r.ChestRight=Sphere("R2 Right Chest",root,0.18f,Skin);
                r.Neck=Sphere("R2 Neck",root,0.10f,Skin);
            }
            return r;
        }
        Arm BuildArm(Transform root,Transform glove,bool left,bool player)
        {
            string name=(left?"Left":"Right");
            glove.localScale=Vector3.one*(2f*Round2Motion.GloveRadius);
            glove.GetComponent<SphereCollider>().radius=0.5f;
            Paint(glove.GetComponent<Renderer>(),player?new Color(0.04f,0.04f,0.045f):new Color(0.72f,0.66f,0.52f));
            return new Arm { Glove=glove,Shoulder=Sphere(name+" Shoulder",root,0.095f,Skin),
                Upper=Segment(name+" Upper Arm",root),Elbow=Sphere(name+" Elbow",root,0.066f,Skin),Forearm=Segment(name+" Forearm",root) };
        }
        public Round2Frame PlayerFrame() => new Round2Frame { Position=_player.transform.position,Rotation=_player.transform.rotation,
            Intent=_player.CurrentIntent,Phase=_player.CurrentPhase,T=_player.ActionNormalizedPhase(_player.CurrentActionPhaseDuration),
            Duration=_player.CurrentActionPhaseDuration,Endpoint=_player.Round2Endpoint,HeadOffset=_player.HeadOffset };
        public Round2Frame OpponentFrame() => new Round2Frame { Position=_opponent.transform.position,Rotation=_opponent.transform.rotation,
            Intent=_opponent.CurrentIntent,Phase=_opponent.CurrentPhase,T=_opponent.ActionNormalizedPhase(_opponent.CurrentActionPhaseDuration),
            Duration=_opponent.CurrentActionPhaseDuration,Endpoint=_opponent.AttackTargetLocal };

        void LateUpdate()
        {
            if(_player==null) return;
            double start=Time.realtimeSinceStartupAsDouble;
            Round2Frame p=PlayerFrame(), o=OpponentFrame();
            if(!_ready) { _previousP=p; _previousO=o; _ready=true; }
            if(_player.CombatEnabled && _opponent.CombatEnabled && !_player.Round2Resolved)
                Resolve(_previousP,p,_previousO,o,true);
            if(_player.CombatEnabled && _opponent.CombatEnabled && !_opponent.Round2Resolved)
                Resolve(_previousO,o,_previousP,p,false);
            Draw(_p,p,true); Draw(_o,o,false);
            _previousP=p; _previousO=o;
            LastUpdateMs=(float)((Time.realtimeSinceStartupAsDouble-start)*1000.0);
        }
        void Resolve(Round2Frame old,Round2Frame now,Round2Frame targetOld,Round2Frame target,bool player)
        {
            bool tail=old.Phase==ActionPhase.Extend && now.Phase==ActionPhase.Recover;
            if(now.Phase!=ActionPhase.Extend && !tail) return;
            bool left=!PunchLabels.IsRearHand(now.Intent);
            float begin=old.Phase==ActionPhase.Extend ? old.T : 0f;
            float end=tail ? 1f : now.T;
            int count=Mathf.Max(1,Mathf.CeilToInt((end-begin)*(tail?old.Duration:now.Duration)/Round2Motion.SampleSeconds));
            Round2Frame first=Round2Frame.Between(old,now,0f);
            first.Phase=ActionPhase.Extend; first.T=begin;
            Vector3 a=first.World(first.Arm(left).Wrist);
            Round2Frame ta=Round2Frame.Between(targetOld,target,0f);
            for(int i=1;i<=count;i++)
            {
                float alpha=i/(float)count;
                Round2Frame sample=Round2Frame.Between(old,now,alpha);
                sample.Phase=ActionPhase.Extend; sample.T=Mathf.Lerp(begin,end,alpha);
                Vector3 b=sample.World(sample.Arm(left).Wrist);
                Round2Frame tb=Round2Frame.Between(targetOld,target,alpha);
                int hit=SweepTargets(a,b,ta,tb,!player||!_opponent.CounterWindowOpen,
                    player?0:_opponent.BodyAttack?2:1,out float earliest);
                Samples+=7;
                if(hit>=0)
                {
                    Contacts++; LastContactFraction=earliest;
                    LastContactGap=Vector3.Distance(Vector3.Lerp(a,b,earliest),Vector3.Lerp(ta.Target(hit),tb.Target(hit),earliest))-Round2Motion.GloveRadius-TargetRadius(hit);
                    CombatOutcome outcome=hit<2?CombatOutcome.Block:CombatOutcome.Hit;
                    string reason=(player?"OPPONENT_":"PLAYER_")+(hit<2?"GUARD":hit==2?"HEAD":"BODY")+"_SWEPT_CONTACT";
                    if(player) _player.CompleteRound2Punch(outcome,reason,a,b);
                    else _opponent.CompleteRound2Attack(outcome,reason,a,b);
                    return;
                }
                a=b; ta=tb;
            }
            if(tail)
            {
                // Counter qualification still uses committed aim vs actual target displacement.
                Vector3 origin=now.World(Round2Motion.Commit(now.Intent));
                Vector3 finish=now.World(Round2Motion.Sample(left,now.Intent,ActionPhase.Extend,1f,now.Endpoint).Wrist);
                if(player) _player.CompleteRound2Punch(CombatOutcome.Miss,"NO_SWEPT_TARGET_CONTACT",origin,finish);
                else _opponent.CompleteRound2Attack(CombatOutcome.Miss,"NO_SWEPT_TARGET_CONTACT",origin,finish);
            }
        }
        public static int SweepTargets(Vector3 a,Vector3 b,Round2Frame ta,Round2Frame tb,bool guardAllowed,int targetMode,out float earliest)
        {
            earliest=2f; int hit=-1;
            for(int k=0;k<7;k++)
            {
                if(k<2&&!guardAllowed || targetMode==2&&k<3 || targetMode==1&&k>=3) continue;
                if(Round2Motion.Sweep(a,b,ta.Target(k),tb.Target(k),Round2Motion.GloveRadius+TargetRadius(k),out float fraction)&&fraction<earliest)
                { earliest=fraction; hit=k; }
            }
            return hit;
        }
        public static float TargetRadius(int target) => target<2?Round2Motion.GloveRadius:target==3?0.25f:Round2Motion.HeadRadius;
        void Draw(Rig rig,Round2Frame frame,bool player)
        {
            DrawArm(rig.Left,frame,frame.Arm(true)); DrawArm(rig.Right,frame,frame.Arm(false));
            if(!player)
            {
                rig.Head.position=frame.Target(2);
                rig.Body.position=frame.Target(3);
                _opponent.BodyCollider.transform.position=rig.Body.position;
                Vector3 shift=Round2Motion.Shift(frame.Intent,frame.Phase,frame.T);
                rig.Chest.position=frame.Target(5);
                rig.ChestLeft.position=frame.Target(4); rig.ChestRight.position=frame.Target(6);
                rig.Chest.rotation=frame.Rotation*Round2Motion.Torso(frame.Intent,frame.Phase,frame.T);
                rig.Neck.position=frame.World(new Vector3(0f,1.51f,0f)+shift);
            }
            else { rig.Head.position=frame.Target(2); rig.Body.position=frame.Target(3); }
        }
        static void DrawArm(Arm arm,Round2Frame f,ArmChainSolution s)
        {
            Vector3 shoulder=f.World(s.Shoulder), elbow=f.World(s.Elbow), wrist=f.World(s.Wrist);
            arm.Shoulder.position=shoulder; arm.Elbow.position=elbow; arm.Glove.position=wrist;
            arm.Glove.rotation=f.Rotation;
            Between(arm.Upper,shoulder,elbow,0.072f); Between(arm.Forearm,elbow,wrist,0.056f);
        }
        public static void Between(Transform t,Vector3 a,Vector3 b,float radius)
        {
            Vector3 d=b-a; t.position=(a+b)*0.5f;
            t.rotation=Quaternion.FromToRotation(Vector3.up,d.normalized);
            t.localScale=new Vector3(radius*2f,d.magnitude*0.5f,radius*2f);
        }
        static Transform Segment(string name,Transform parent) => Primitive(name,parent,PrimitiveType.Capsule,Skin);
        public static Transform Sphere(string name,Transform parent,float radius,Color color)
        { var t=Primitive(name,parent,PrimitiveType.Sphere,color); t.localScale=Vector3.one*radius*2f; return t; }
        public static Transform Primitive(string name,Transform parent,PrimitiveType type,Color color)
        {
            var go=GameObject.CreatePrimitive(type); go.name=name; go.transform.SetParent(parent,false);
            go.GetComponent<Collider>().enabled=false; Paint(go.GetComponent<Renderer>(),color); return go.transform;
        }
        public static void Paint(Renderer renderer,Color color)
        { var shader=Resources.Load<Shader>("BoxerRound2Surface"); renderer.sharedMaterial=new Material(shader){color=color}; }
        static readonly Color Skin=new Color(0.56f,0.31f,0.22f);
    }
}
