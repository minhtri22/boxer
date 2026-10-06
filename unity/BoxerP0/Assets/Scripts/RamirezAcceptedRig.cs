using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoxerP0
{
    // Presentation only: native lengths and bind offsets, never writes combat anchors.
    public sealed class RamirezAcceptedRig : IDisposable
    {
        sealed class Bone
        {
            public Transform Transform;
            public Vector3 Position;
            public Quaternion Rotation;
            public Bone(Transform t) { Transform=t; Position=t.localPosition; Rotation=t.localRotation; }
            public void Reset() { Transform.localPosition=Position; Transform.localRotation=Rotation; }
        }
        sealed class Limb
        {
            public Transform Upper, Lower, End, Target, Pole, Clavicle;
            public Vector3 UpperDirection, LowerDirection, EndDirection, EndOffset;
            public Quaternion UpperRotation, LowerRotation, EndRotation;
            public float UpperLength, LowerLength;
        }
        readonly Transform _model, _actor, _pelvis, _chest, _root, _spine, _head;
        readonly Bone[] _bones;
        readonly Limb _leftArm, _rightArm, _leftLeg, _rightLeg;
        readonly Quaternion _rootRest, _spineRest, _headRest;
        readonly Vector3 _rootOffset;
        readonly List<Material> _materials=new();
        public float MaxGloveError { get; private set; }
        public float MaxFootError { get; private set; }
        public float LastGloveError { get; private set; }
        public float LastFootError { get; private set; }
        public float LastUpdateMs { get; private set; }
        public long LastAllocatedBytes { get; private set; }
        public float MaxNativeLengthError { get; private set; }
        public Vector3 LeftGloveCenter => Center(_leftArm);
        public Vector3 RightGloveCenter => Center(_rightArm);
        public float UpperArmLength => _leftArm.UpperLength;
        public float ForearmLength => _leftArm.LowerLength;
        public Transform RigRoot => _root;

        public RamirezAcceptedRig(Transform model, Transform actor)
        {
            _model=model; _actor=actor;
            var rig=Find(model,"RAMIREZ_RIG");
            _root=Find(rig,"root"); _spine=Find(rig,"spine01"); _head=Find(rig,"head");
            _pelvis=Find(actor,"R2 Pelvis"); _chest=Find(actor,"R2 Chest");
            _rootOffset=Vector3.zero; // Native pelvis reads the authoritative pelvis, with no anatomical scale.
            _rootRest=Quaternion.Inverse(actor.rotation)*_root.rotation;
            _spineRest=Quaternion.Inverse(actor.rotation)*_spine.rotation;
            _headRest=Quaternion.Inverse(actor.rotation)*_head.rotation;
            var nodes=rig.GetComponentsInChildren<Transform>();
            _bones=new Bone[nodes.Length];
            for(int i=0;i<nodes.Length;i++) _bones[i]=new Bone(nodes[i]);
            _leftArm=Arm(rig,true); _rightArm=Arm(rig,false);
            _leftLeg=Leg(rig,true); _rightLeg=Leg(rig,false);
            foreach(var r in model.GetComponentsInChildren<Renderer>())
            {
                var old=r.sharedMaterials; var replacement=new Material[old.Length];
                for(int i=0;i<old.Length;i++)
                {
                    string n=old[i]==null?"":old[i].name;
                    Color color=n.Contains("Skin")?EVVisualShell.Skin:n.Contains("Gold")?new Color(.78f,.52f,.16f):n.Contains("White")?new Color(.82f,.79f,.71f):n.Contains("Eye")?new Color(.11f,.07f,.035f):new Color(.48f,.025f,.035f);
                    replacement[i]=new Material(Resources.Load<Shader>("EVSurface")) { color=color };
                    replacement[i].SetFloat("_Kind",n.Contains("Skin")?0:n.Contains("White")?2:1);
                    _materials.Add(replacement[i]);
                }
                r.sharedMaterials=replacement;
                if(r is SkinnedMeshRenderer skin) skin.updateWhenOffscreen=true;
            }
        }
        Limb Make(Transform rig,string upper,string lower,string end,Transform target,Transform pole)
        {
            var a=Find(rig,upper);var b=Find(rig,lower);var c=Find(rig,end);
            return new Limb { Upper=a,Lower=b,End=c,Target=target,Pole=pole,
                UpperLength=Vector3.Distance(a.position,b.position),LowerLength=Vector3.Distance(b.position,c.position),
                UpperDirection=Quaternion.Inverse(_actor.rotation)*(b.position-a.position).normalized,
                LowerDirection=Quaternion.Inverse(_actor.rotation)*(c.position-b.position).normalized,
                EndDirection=Quaternion.Inverse(_actor.rotation)*c.up,
                UpperRotation=Quaternion.Inverse(_actor.rotation)*a.rotation,
                LowerRotation=Quaternion.Inverse(_actor.rotation)*b.rotation,
                EndRotation=Quaternion.Inverse(_actor.rotation)*c.rotation };
        }
        Limb Arm(Transform rig,bool left)
        {
            string s=left?"L":"R", n=left?"Left":"Right";
            var limb=Make(rig,"upperarm."+s,"forearm."+s,"hand."+s,Find(_actor,"Opponent "+n+" Glove"),Find(_actor,n+" Elbow"));
            limb.Clavicle=Find(rig,"clavicle."+s);
            var shell=Find(_model,"Glove padded shell."+s).GetComponent<SkinnedMeshRenderer>();
            // Rest bounds center is the visible glove center; bone origin is the wrist.
            limb.EndOffset=limb.End.InverseTransformPoint(shell.transform.TransformPoint(shell.sharedMesh.bounds.center));
            return limb;
        }
        Limb Leg(Transform rig,bool left)
        {
            string s=left?"L":"R", n=left?"Left":"Right";
            var limb=Make(rig,"thigh."+s,"shin."+s,"foot."+s,Find(_actor,n+" Shoe"),Find(_actor,n+" Knee"));
            // Authoritative shoe origin is 4 cm above floor; native ankle is 10 cm.
            limb.EndOffset=limb.End.InverseTransformVector(Vector3.down*.06f);
            return limb;
        }
        static Vector3 Center(Limb l) => l.End.TransformPoint(l.EndOffset);
        public void Apply()
        {
            long before=GC.GetAllocatedBytesForCurrentThread(); double start=Time.realtimeSinceStartupAsDouble;
            _model.SetPositionAndRotation(_actor.position,_actor.rotation);
            foreach(var bone in _bones) bone.Reset();
            _root.SetPositionAndRotation(_pelvis.position+_actor.rotation*_rootOffset,_actor.rotation*_rootRest);
            // Native legs are shorter than the legacy presentation. Read both planted
            // feet and use only the necessary visual crouch to keep soles attached.
            float crouch=Mathf.Max(RequiredCrouch(_leftLeg),RequiredCrouch(_rightLeg));
            _root.position-=Vector3.up*crouch;
            _spine.rotation=_chest.rotation*_spineRest;
            _head.rotation=_actor.rotation*_headRest;
            Drive(_leftArm,true); Drive(_rightArm,true); Drive(_leftLeg,false); Drive(_rightLeg,false);
            LastGloveError=Mathf.Max(Vector3.Distance(Center(_leftArm),_leftArm.Target.position),Vector3.Distance(Center(_rightArm),_rightArm.Target.position));
            LastFootError=Mathf.Max(Vector3.Distance(Center(_leftLeg),_leftLeg.Target.position),Vector3.Distance(Center(_rightLeg),_rightLeg.Target.position));
            MaxGloveError=Mathf.Max(MaxGloveError,LastGloveError);
            MaxFootError=Mathf.Max(MaxFootError,LastFootError);
            MaxNativeLengthError=Mathf.Max(MaxNativeLengthError,Mathf.Max(Mathf.Max(LengthError(_leftArm),LengthError(_rightArm)),Mathf.Max(LengthError(_leftLeg),LengthError(_rightLeg))));
            LastUpdateMs=(float)((Time.realtimeSinceStartupAsDouble-start)*1000);
            LastAllocatedBytes=GC.GetAllocatedBytesForCurrentThread()-before;
        }
        static float LengthError(Limb limb) => Mathf.Max(Mathf.Abs(Vector3.Distance(limb.Upper.position,limb.Lower.position)-limb.UpperLength),Mathf.Abs(Vector3.Distance(limb.Lower.position,limb.End.position)-limb.LowerLength));
        float RequiredCrouch(Limb limb)
        {
            Quaternion orientation=limb.Target.rotation*limb.EndRotation;
            Vector3 goal=limb.Target.position-orientation*Vector3.Scale(limb.EndOffset,limb.End.lossyScale);
            Vector3 difference=limb.Upper.position-goal;
            float reach=limb.UpperLength+limb.LowerLength-.003f;
            float horizontal=difference.x*difference.x+difference.z*difference.z;
            return Mathf.Max(0,difference.y-Mathf.Sqrt(Mathf.Max(0,reach*reach-horizontal)));
        }
        public void Dispose()
        {
            foreach(var material in _materials) if(material!=null) UnityEngine.Object.Destroy(material);
            _materials.Clear();
        }
        void Drive(Limb l,bool arm)
        {
            Vector3 direction=arm?(l.Target.position-l.Pole.position).normalized:_actor.up;
            Quaternion orientation=arm?Quaternion.FromToRotation(_actor.rotation*l.EndDirection,direction)*_actor.rotation*l.EndRotation:l.Target.rotation*l.EndRotation;
            Vector3 offset=orientation*Vector3.Scale(l.EndOffset,l.End.lossyScale);
            if(arm)
            {
                // Minimal clavicle protraction when the native two-bone chain needs
                // it. Rotate the existing bone, retaining its length and bind offset.
                Vector3 pivot=l.Clavicle.position, rest=l.Upper.position-pivot;
                Vector3 target=l.Target.position-offset-pivot;
                float radius=rest.magnitude, distance=target.magnitude;
                float reach=l.UpperLength+l.LowerLength-.003f;
                if(distance>.0001f&&radius>.0001f)
                {
                    float allowed=Mathf.Acos(Mathf.Clamp((distance*distance+radius*radius-reach*reach)/(2*distance*radius),-1,1));
                    float current=Vector3.Angle(rest,target)*Mathf.Deg2Rad;
                    if(current>allowed)
                    {
                        Vector3 adjusted=Vector3.RotateTowards(rest,target,current-allowed,0);
                        l.Clavicle.rotation=Quaternion.FromToRotation(rest,adjusted)*l.Clavicle.rotation;
                    }
                }
            }
            var solved=ArmChainMath.Solve(l.Upper.position,l.Target.position-offset,l.Pole.position,l.UpperLength,l.LowerLength);
            l.Upper.rotation=Quaternion.FromToRotation(_actor.rotation*l.UpperDirection,(solved.Elbow-solved.Shoulder).normalized)*_actor.rotation*l.UpperRotation;
            l.Lower.rotation=Quaternion.FromToRotation(_actor.rotation*l.LowerDirection,(solved.Wrist-solved.Elbow).normalized)*_actor.rotation*l.LowerRotation;
            l.End.rotation=orientation;
        }
        public static Transform Find(Transform root,string name)
        {
            foreach(var t in root.GetComponentsInChildren<Transform>(true)) if(t.name==name) return t;
            throw new InvalidOperationException("Accepted Ramirez mapping missing "+name);
        }
    }
}
