using UnityEngine;

namespace BoxerP0
{
    // Alternating world-planted feet. Travel-side foot leads; the other gathers without crossing.
    [DefaultExecutionOrder(260)]
    public sealed class Round2Footwork : MonoBehaviour
    {
        OpponentBoxer _opponent;
        Transform _root,_pelvis,_shorts,_waist;
        sealed class Leg { public Transform Thigh,Knee,Shin,Foot; public Vector3 Plant,From,To; public Quaternion Rotation; }
        Leg _left,_right;
        Leg _swing;
        float _stepTime;
        bool _nextLeft=true;
        public float PlantedDrift { get; private set; }
        public int Steps { get; private set; }
        public void Initialize(OpponentBoxer opponent)
        {
            _opponent=opponent; _root=opponent.transform;
            _pelvis=Round2CombatRig.Sphere("R2 Pelvis",_root,0.15f,new Color(0.18f,0.19f,0.21f));
            _left=Build(true); _right=Build(false);
        }
        Leg Build(bool left)
        {
            string n=left?"Left":"Right";
            Color skin=new Color(0.56f,0.31f,0.22f);
            var l=new Leg { Thigh=Round2CombatRig.Primitive(n+" Thigh",_root,PrimitiveType.Capsule,skin),
                Knee=Round2CombatRig.Sphere(n+" Knee",_root,0.06f,skin),
                Shin=Round2CombatRig.Primitive(n+" Shin",_root,PrimitiveType.Capsule,skin),
                Foot=Round2CombatRig.Primitive(n+" Shoe",_root,PrimitiveType.Sphere,new Color(0.07f,0.06f,0.055f)) };
            l.Foot.localScale=new Vector3(0.13f,0.08f,0.25f);
            l.Plant=Desired(left); l.Rotation=_root.rotation;
            return l;
        }
        Vector3 Desired(bool left) => _root.TransformPoint(new Vector3(left?-0.22f:0.22f,0.04f,left?0.17f:-0.17f));
        void LateUpdate()
        {
            if(_root==null) return;
            if(_shorts==null) { _shorts=_root.Find("Opponent Shorts Visual"); _waist=_root.Find("Opponent Gold Waistband"); }
            if(_swing==null)
            {
                float le=Vector3.Distance(_left.Plant,Desired(true)), re=Vector3.Distance(_right.Plant,Desired(false));
                if(Mathf.Max(le,re)>0.055f)
                {
                    Vector3 movement=_root.InverseTransformDirection((Desired(true)-_left.Plant)+(Desired(false)-_right.Plant));
                    bool travelLeft=Mathf.Abs(movement.x)>Mathf.Abs(movement.z) ? movement.x<0f : movement.z>0f;
                    bool choose=le>0.12f || re>0.12f ? le>re : _nextLeft;
                    if(Steps==0) choose=travelLeft;
                    _swing=choose?_left:_right; _swing.From=_swing.Plant; _swing.To=Desired(choose);
                    _stepTime=0f; _nextLeft=!choose; Steps++;
                }
            }
            if(_swing!=null)
            {
                _stepTime+=Time.deltaTime;
                float u=Mathf.Clamp01(_stepTime/0.18f);
                _swing.Plant=Vector3.Lerp(_swing.From,_swing.To,Round2Motion.Smooth(u))+Vector3.up*(0.045f*Mathf.Sin(Mathf.PI*u));
                _swing.Rotation=Quaternion.Slerp(_swing.Rotation,_root.rotation,Round2Motion.Smooth(u));
                if(u>=1f) { _swing.Plant=_swing.To; _swing=null; }
            }
            float t=_opponent.ActionNormalizedPhase(_opponent.CurrentActionPhaseDuration);
            Vector3 shift=Round2Motion.Shift(_opponent.CurrentIntent,_opponent.CurrentPhase,t);
            Vector3 pelvis=_root.TransformPoint(new Vector3(0f,0.85f,0f)+shift);
            _pelvis.position=pelvis;
            Quaternion yaw=_root.rotation*Quaternion.Euler(0f,7f*Round2Motion.Load(_opponent.CurrentIntent,_opponent.CurrentPhase,t)*(PunchLabels.IsRearHand(_opponent.CurrentIntent)?-1f:1f),0f);
            if(_shorts!=null) { _shorts.position=pelvis; _shorts.rotation=yaw; _shorts.localScale=new Vector3(0.55f,0.24f,0.38f); }
            if(_waist!=null) { _waist.position=pelvis+Vector3.up*0.14f; _waist.rotation=yaw; _waist.localScale=new Vector3(0.56f,0.06f,0.39f); }
            Pose(_left,pelvis+yaw*new Vector3(-0.20f,0f,0f));
            Pose(_right,pelvis+yaw*new Vector3(0.20f,0f,0f));
        }
        void Pose(Leg l,Vector3 hip)
        {
            var solved=ArmChainMath.Solve(hip,l.Plant,hip+_root.forward,OpponentLegEmbodiment.ThighLength,OpponentLegEmbodiment.ShinLength);
            l.Knee.position=solved.Elbow;
            Round2CombatRig.Between(l.Thigh,hip,solved.Elbow,0.075f);
            Round2CombatRig.Between(l.Shin,solved.Elbow,solved.Wrist,0.055f);
            l.Foot.position=solved.Wrist; l.Foot.rotation=l.Rotation;
            if(l!=_swing) PlantedDrift=Mathf.Max(PlantedDrift,Vector3.Distance(l.Foot.position,l.Plant));
        }
    }
}
