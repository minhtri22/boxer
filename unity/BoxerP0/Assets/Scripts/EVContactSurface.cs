using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoxerP0
{
    // Refines the existing relative swept-glove resolver against the same anatomical
    // surface that is rendered. It does not own input, pose, timing, damage or physics.
    public static class EVContactSurface
    {
        public const float SurfaceTolerance=.00001f;
        public static bool Ready {get;private set;}
        public static float LastGap {get;private set;}
        public static long TriangleQueries {get;private set;}
        public static int SweepBudgetExhaustions {get;private set;}
        public static float JabReachBoundary {get;private set;}
        public static float CrossReachBoundary {get;private set;}
        public static float PreferredDistance {get;private set;}
        static Tree _head,_body;
        public static int HeadTriangles=>_head?.Triangles.Length??0;
        public static int BodyTriangles=>_body?.Triangles.Length??0;
        public static void Initialize(EVAnatomy.Asset data)
        {
            var head=new List<Triangle>();var body=new List<Triangle>();
            void Add(int[] source,List<Triangle> list,int bone,Vector3 origin) {
                for(int i=0;i<source.Length;i+=3) {
                    int a=source[i],b=source[i+1],c=source[i+2];
                    // Arms/neck transition are not scoring torso targets. Only rigid
                    // head/chest/abdomen faces are in these two target families.
                    if(data.weights[a*10+bone]<.999f||data.weights[b*10+bone]<.999f||data.weights[c*10+bone]<.999f)continue;
                    list.Add(new Triangle(data.Point(a)-origin,data.Point(b)-origin,data.Point(c)-origin));
                }
            }
            Add(data.head,head,1,new Vector3(0,1.62f,0));Add(data.body,body,0,new Vector3(0,1.37f,0));
            _head=new Tree(head.ToArray());_body=new Tree(body.ToArray());Ready=true;
            JabReachBoundary=Reach(PunchIntent.Jab);CrossReachBoundary=Reach(PunchIntent.Cross);
            // Maintain the existing BOXING band, with a quarter-pad depth of usable
            // jab contact. Existing movement speeds and 45 mm hysteresis are unchanged.
            PreferredDistance=Mathf.Max(Round2Motion.CloseBoundary+.02f,JabReachBoundary-Round2Motion.GloveRadius*.25f);
        }
        static float Reach(PunchIntent intent)
        {
            float low=.6f,high=1.18f;bool left=!PunchLabels.IsRearHand(intent);
            for(int search=0;search<12;search++) {
                float distance=(low+high)*.5f;var target=new Round2Frame{Position=new Vector3(0,0,distance),Rotation=Quaternion.Euler(0,180,0),Phase=ActionPhase.Guard};
                Vector3 endpoint=Round2Motion.Endpoint(intent,"NEUTRAL",distance),a=Round2Motion.Sample(left,intent,ActionPhase.Extend,0,endpoint).Wrist;bool contact=false;
                for(int i=1;i<=96;i++){Vector3 b=Round2Motion.Sample(left,intent,ActionPhase.Extend,i/96f,endpoint).Wrist;if(Sweep(a,b,target,target,true,out _)){contact=true;break;}a=b;}
                if(contact)low=distance;else high=distance;
            }
            return low;
        }
        public static Vector3 Local(Vector3 p,Round2Frame target,bool head)
        {
            Quaternion rotation=target.Rotation*(head?Quaternion.identity:Round2Motion.Torso(target.Intent,target.Phase,target.T));
            return Quaternion.Inverse(rotation)*(p-target.Target(head?2:5));
        }
        public static bool Sweep(Vector3 a,Vector3 b,Round2Frame ta,Round2Frame tb,bool head,out float fraction)
        {
            return SweepLocal(Local(a,ta,head),Local(b,tb,head),head,out fraction);
        }
        public static float Distance(Vector3 p,bool head)=>(head?_head:_body).Distance(p);
        public static bool SweepLocal(Vector3 a,Vector3 b,bool head,out float fraction)
        {
            Tree tree=head?_head:_body;Vector3 delta=b-a;float length=delta.magnitude;fraction=0;
            // Conservative advancement uses the 1-Lipschitz distance to triangles.
            // No collision can be crossed by a step shorter than the remaining gap.
            for(int i=0;i<256;i++) {
                float gap=tree.Distance(a+delta*fraction,delta)-Round2Motion.GloveRadius;
                if(gap<=SurfaceTolerance){LastGap=gap;return true;}
                if(length<SurfaceTolerance)return false;
                fraction+=gap/length;
                if(fraction>1)return false;
            }
            // Extremely grazing convergence: no invented HIT. Focused tests include
            // tangent and near-tangent sweeps; the finite bound is logged by tests.
            SweepBudgetExhaustions++;return false;
        }
        struct Triangle
        {
            public Vector3 A,B,C,Center,Normal;public Bounds Bounds;
            public Triangle(Vector3 a,Vector3 b,Vector3 c){A=a;B=b;C=c;Center=(a+b+c)/3;Normal=Vector3.Cross(b-a,c-a).normalized;Bounds=new Bounds(a,Vector3.zero);Bounds.Encapsulate(b);Bounds.Encapsulate(c);}
            public float DistanceSquared(Vector3 p)
            {
                Vector3 ab=B-A,ac=C-A,ap=p-A;float d1=Vector3.Dot(ab,ap),d2=Vector3.Dot(ac,ap);
                if(d1<=0&&d2<=0)return ap.sqrMagnitude;
                Vector3 bp=p-B;float d3=Vector3.Dot(ab,bp),d4=Vector3.Dot(ac,bp);
                if(d3>=0&&d4<=d3)return bp.sqrMagnitude;
                float vc=d1*d4-d3*d2;
                if(vc<=0&&d1>=0&&d3<=0)return (p-(A+ab*(d1/(d1-d3)))).sqrMagnitude;
                Vector3 cp=p-C;float d5=Vector3.Dot(ab,cp),d6=Vector3.Dot(ac,cp);
                if(d6>=0&&d5<=d6)return cp.sqrMagnitude;
                float vb=d5*d2-d1*d6;
                if(vb<=0&&d2>=0&&d6<=0)return (p-(A+ac*(d2/(d2-d6)))).sqrMagnitude;
                float va=d3*d6-d5*d4;
                if(va<=0&&(d4-d3)>=0&&(d5-d6)>=0)return (p-(B+(C-B)*((d4-d3)/(d4-d3+d5-d6)))).sqrMagnitude;
                float denom=va+vb+vc;
                if(Mathf.Abs(denom)<1e-18f)return Mathf.Min(ap.sqrMagnitude,Mathf.Min(bp.sqrMagnitude,cp.sqrMagnitude));
                return (p-(A+ab*(vb/denom)+ac*(vc/denom))).sqrMagnitude;
            }
        }
        sealed class Tree
        {
            public readonly Triangle[] Triangles;
            struct Node{public Bounds Bounds;public int Start,Count,Left,Right;}
            readonly Node[] _nodes;int _used;
            sealed class Order:IComparer<Triangle>{readonly int _axis;public Order(int axis){_axis=axis;}public int Compare(Triangle a,Triangle b)=>a.Center[_axis].CompareTo(b.Center[_axis]);}
            static readonly Order[] Orders={new(0),new(1),new(2)};
            public Tree(Triangle[] triangles){Triangles=triangles;_nodes=new Node[triangles.Length*2];Build(0,triangles.Length);}
            int Build(int start,int count)
            {
                int id=_used++;Bounds bounds=Triangles[start].Bounds;for(int i=start+1;i<start+count;i++)bounds.Encapsulate(Triangles[i].Bounds);
                var n=new Node{Bounds=bounds,Start=start,Count=count,Left=-1,Right=-1};
                if(count>8){Vector3 s=bounds.size;int axis=s.x>s.y?(s.x>s.z?0:2):(s.y>s.z?1:2);Array.Sort(Triangles,start,count,Orders[axis]);int half=count/2;n.Left=Build(start,half);n.Right=Build(start+half,count-half);}
                _nodes[id]=n;return id;
            }
            public float Distance(Vector3 p,Vector3 direction=default){float best=float.MaxValue;Nearest(0,p,direction,ref best);return Mathf.Sqrt(best);}
            void Nearest(int id,Vector3 p,Vector3 direction,ref float best)
            {
                Node n=_nodes[id];if(n.Bounds.SqrDistance(p)>best)return;
                if(n.Left<0){for(int i=n.Start;i<n.Start+n.Count;i++){if(Vector3.Dot(Triangles[i].Normal,direction)>1e-8f)continue;TriangleQueries++;best=Mathf.Min(best,Triangles[i].DistanceSquared(p));}return;}
                float l=_nodes[n.Left].Bounds.SqrDistance(p),r=_nodes[n.Right].Bounds.SqrDistance(p);
                if(l<r){Nearest(n.Left,p,direction,ref best);Nearest(n.Right,p,direction,ref best);}else{Nearest(n.Right,p,direction,ref best);Nearest(n.Left,p,direction,ref best);}
            }
        }
    }
}
