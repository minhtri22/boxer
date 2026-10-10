using UnityEngine;
using UnityEngine.Rendering;

namespace BoxerP0
{
    // Scenery only. No collider, gameplay transform, camera, light or shared random writes.
    [DefaultExecutionOrder(310)]
    public sealed class ArenaSurround : MonoBehaviour
    {
        public const int Sides=4, FlashesPerSide=6, FlashNodes=Sides*FlashesPerSide;
        public const float Distance=4.04f, Width=8.1f, Height=5.5f, Interval=1.5f, Duration=.16f, Delay=.5f;
        public static readonly Vector3 Center=new(0,0,.2f);
        public readonly struct FlashSample
        {
            public readonly int Index;
            public readonly float Strength;
            public FlashSample(int index,float strength){Index=index;Strength=strength;}
        }
        public static FlashSample Sample(float elapsed,bool active)
        {
            if(!active || !float.IsFinite(elapsed) || elapsed<Delay)return new(-1,0);
            float t=elapsed-Delay; int cycle=Mathf.FloorToInt(t/Interval);
            float age=t-cycle*Interval;
            if(age>=Duration)return new(-1,0);
            return new(cycle%FlashNodes,.75f*Mathf.Sin(Mathf.PI*age/Duration));
        }
        public static Vector3 SidePosition(int side)=>Center+Quaternion.Euler(0,side*90,0)*new Vector3(0,3,Distance);
        public static Vector3 FlashPosition(int index)
        {
            int side=index%Sides,slot=index/Sides;
            float x=-2.85f+slot*1.14f;
            float y=1.7f+((slot*3+side*2)%7)*.22f;
            return Center+Quaternion.Euler(0,side*90,0)*new Vector3(x,y,Distance-.035f);
        }
        readonly MeshRenderer[] _stands=new MeshRenderer[Sides],_flashes=new MeshRenderer[FlashNodes];
        MaterialPropertyBlock _properties;
        static readonly int Intensity=Shader.PropertyToID("_Intensity");
        Mesh _standMesh,_flashMesh;
        Material _standMaterial,_flashMaterial;
        BoxerBootstrap _bootstrap;
        int _active=-1;
        float _elapsed;
        bool _initialized;
        public int StandCount=>_initialized?Sides:0;
        public int FlashCount=>_initialized?FlashNodes:0;
        public int ActiveFlash=>_active;
        public float Strength{get;private set;}
        public int ObservedSideMask{get;private set;}
        public int EnabledFlashes
        {get{int count=0;for(int i=0;i<FlashNodes;i++)if(_flashes[i]!=null&&_flashes[i].enabled)count++;return count;}}
        public void Initialize(Texture2D audience)
        {
            if(_initialized)return;
            _properties=new MaterialPropertyBlock();
            _standMesh=Quad(.42f);_flashMesh=Quad(0);
            _standMaterial=new Material(Resources.Load<Shader>("EVBackdrop")){mainTexture=audience};
            _flashMaterial=new Material(Resources.Load<Shader>("AudienceFlash"));
            for(int side=0;side<Sides;side++)
            {
                _stands[side]=Node("Audience Side "+side,_standMesh,_standMaterial);
                var t=_stands[side].transform;t.position=SidePosition(side);t.rotation=Quaternion.Euler(0,side*90,0);t.localScale=new Vector3(Width,Height,1);
            }
            for(int i=0;i<FlashNodes;i++)
            {
                _flashes[i]=Node("Audience Camera Flash "+i,_flashMesh,_flashMaterial);
                var t=_flashes[i].transform;t.position=FlashPosition(i);t.rotation=Quaternion.Euler(0,(i%Sides)*90,0);t.localScale=Vector3.one*.13f;
                _flashes[i].enabled=false;
            }
            _bootstrap=FindFirstObjectByType<BoxerBootstrap>();_initialized=true;
        }
        MeshRenderer Node(string label,Mesh mesh,Material material)
        {
            var node=new GameObject(label);node.transform.SetParent(transform,false);
            node.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=node.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;return renderer;
        }
        static Mesh Quad(float lowUV)
        {
            var mesh=new Mesh{name="Shared Audience Quad",vertices=new[]{new Vector3(-.5f,-.5f,0),new Vector3(-.5f,.5f,0),new Vector3(.5f,.5f,0),new Vector3(.5f,-.5f,0)},
                uv=new[]{new Vector2(0,lowUV),new Vector2(0,1),new Vector2(1,1),new Vector2(1,lowUV)},triangles=new[]{0,1,2,0,2,3}};
            mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        void LateUpdate()
        {
            if(!_initialized)return;
            bool fight=_bootstrap!=null&&_bootstrap.Flow.Screen==ProductScreen.Fight;
            if(fight)_elapsed+=Time.unscaledDeltaTime;
            else{_elapsed=0;ObservedSideMask=0;}
            Apply(Sample(_elapsed,fight));
        }
        // Pure presentation application. Native isolated rendering tests use this, never gameplay receipts/state.
        public void Apply(FlashSample sample)
        {
            if(!_initialized)return;
            int index=sample.Index>=0&&sample.Index<FlashNodes&&sample.Strength>0?sample.Index:-1;
            if(_active!=index){if(_active>=0)_flashes[_active].enabled=false;_active=index;}
            Strength=index>=0?Mathf.Clamp(sample.Strength,0,.75f):0;
            if(index<0)return;
            _properties.SetFloat(Intensity,Strength);_flashes[index].SetPropertyBlock(_properties);_flashes[index].enabled=true;
            if(Strength>.1f)ObservedSideMask|=1<<(index%Sides);
        }
        void OnDisable(){if(_initialized)Apply(new(-1,0));_elapsed=0;ObservedSideMask=0;}
        void OnDestroy()
        {
            Release(_standMesh);Release(_flashMesh);Release(_standMaterial);Release(_flashMaterial);
        }
        static void Release(Object item){if(item==null)return;if(Application.isPlaying)Destroy(item);else DestroyImmediate(item);}
    }
}
