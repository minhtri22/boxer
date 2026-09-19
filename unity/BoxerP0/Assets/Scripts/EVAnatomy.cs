using System;
using UnityEngine;

namespace BoxerP0
{
    // One skinned surface, ten read-only bone followers. No Animation/Animator/IK.
    [DefaultExecutionOrder(290)]
    public sealed class EVAnatomy : MonoBehaviour
    {
        [Serializable] public sealed class Asset
        {
            public float[] positions,uv,weights,boneStarts,boneEnds;
            public int[] body,head;
            public Vector3 Point(int i)=>new(positions[i*3],positions[i*3+1],positions[i*3+2]);
        }
        public static Asset ReadAsset()=>JsonUtility.FromJson<Asset>(Resources.Load<TextAsset>("EV/RamirezAnatomy").text);
        public SkinnedMeshRenderer Skin {get;private set;}
        public float LastUpdateMs {get;private set;}
        public long LastAllocatedBytes {get;private set;}
        readonly Transform[] _bones=new Transform[10];
        readonly Transform[] _segments=new Transform[8];
        Transform _head,_chest;Mesh _mesh,_hair;Material _hairMaterial;
        readonly System.Collections.Generic.List<UnityEngine.Object> _eyeAssets=new();
        public void Initialize(Transform root,Material body,Material face)
        {
            transform.SetParent(root,false);var data=ReadAsset();int count=data.positions.Length/3;
            var vertices=new Vector3[count];var uv=new Vector2[count];var weights=new BoneWeight[count];
            for(int i=0;i<count;i++) {
                vertices[i]=data.Point(i);uv[i]=new Vector2(data.uv[i*2],data.uv[i*2+1]);
                int[] indices={0,0,0,0};float[] values={0,0,0,0};
                for(int b=0;b<10;b++){float w=data.weights[i*10+b];for(int k=0;k<4;k++)if(w>values[k]){for(int j=3;j>k;j--){values[j]=values[j-1];indices[j]=indices[j-1];}values[k]=w;indices[k]=b;break;}}
                float sum=values[0]+values[1]+values[2]+values[3];
                weights[i]=new BoneWeight{boneIndex0=indices[0],boneIndex1=indices[1],boneIndex2=indices[2],boneIndex3=indices[3],weight0=values[0]/sum,weight1=values[1]/sum,weight2=values[2]/sum,weight3=values[3]/sum};
            }
            var bind=new Matrix4x4[10];
            for(int i=0;i<10;i++) {
                var t=new GameObject("EV Skin Bone "+i).transform;t.SetParent(transform,false);_bones[i]=t;
                Vector3 start=new(data.boneStarts[i*3],data.boneStarts[i*3+1],data.boneStarts[i*3+2]),end=new(data.boneEnds[i*3],data.boneEnds[i*3+1],data.boneEnds[i*3+2]);
                t.localPosition=start;t.localRotation=Quaternion.FromToRotation(Vector3.up,end-start);
                bind[i]=t.worldToLocalMatrix*transform.localToWorldMatrix;
            }
            var bodyTriangles=new System.Collections.Generic.List<int>();var bootTriangles=new System.Collections.Generic.List<int>();
            for(int i=0;i<data.body.Length;i+=3){int a=data.body[i],b=data.body[i+1],c=data.body[i+2];bool boot=(vertices[a].y+vertices[b].y+vertices[c].y)/3<.32f;var list=boot?bootTriangles:bodyTriangles;list.Add(a);list.Add(b);list.Add(c);}
            var bootMaterial=new Material(body);bootMaterial.color=new Color(.78f,.76f,.69f);bootMaterial.SetFloat("_Kind",6);_eyeAssets.Add(bootMaterial);
            _mesh=new Mesh{name="EV Anatomical Ramirez"};_mesh.vertices=vertices;_mesh.uv=uv;_mesh.boneWeights=weights;_mesh.bindposes=bind;_mesh.subMeshCount=3;_mesh.SetTriangles(bodyTriangles,0);_mesh.SetTriangles(data.head,1);_mesh.SetTriangles(bootTriangles,2);_mesh.RecalculateNormals();_mesh.RecalculateBounds();
            Skin=gameObject.AddComponent<SkinnedMeshRenderer>();Skin.sharedMesh=_mesh;Skin.bones=_bones;Skin.rootBone=transform;Skin.sharedMaterials=new[]{body,face,bootMaterial};Skin.localBounds=new Bounds(Vector3.up,Vector3.one*4);Skin.updateWhenOffscreen=false;Skin.quality=SkinQuality.Bone4;
            // A small scalp shell supplies actual hair silhouette; it is bound to the same
            // head follower, with no second head or face renderer underneath.
            var hairVertices=new System.Collections.Generic.List<Vector3>();var hairTriangles=new System.Collections.Generic.List<int>();var hairNormals=new System.Collections.Generic.List<Vector3>();var hairUV=new System.Collections.Generic.List<Vector2>();
            bool Hair(Vector3 p)=>p.y>1.731f || (p.y>1.672f && (p.z<-.035f || (Mathf.Abs(p.x)>.075f&&p.z<.035f)));
            for(int i=0;i<data.head.Length;i+=3){Vector3 a=vertices[data.head[i]],b=vertices[data.head[i+1]],c=vertices[data.head[i+2]];if(!Hair((a+b+c)/3))continue;foreach(Vector3 p in new[]{a,b,c}){Vector3 d=p-new Vector3(0,1.68f,-.01f);float curl=.004f+.0015f*Mathf.Sin(p.x*440)*Mathf.Sin(p.y*420);hairVertices.Add(p-new Vector3(0,1.62f,0)+d.normalized*curl);hairTriangles.Add(hairTriangles.Count);hairNormals.Add(d.normalized);hairUV.Add(new Vector2(p.x*5,p.y*5));}}
            _hair=new Mesh{name="EV Cropped Curly Hair",vertices=hairVertices.ToArray(),triangles=hairTriangles.ToArray(),normals=hairNormals.ToArray(),uv=hairUV.ToArray()};
            var hairObject=new GameObject("EV Head Hair");hairObject.transform.SetParent(_bones[1],false);hairObject.AddComponent<MeshFilter>().sharedMesh=_hair;_hairMaterial=new Material(face);_hairMaterial.SetFloat("_Kind",5);_hairMaterial.color=new Color(.6f,.6f,.6f);hairObject.AddComponent<MeshRenderer>().sharedMaterial=_hairMaterial;
            // Geometry in the eyelid sockets, attached to the head bone (not painted holes).
            foreach(float side in new[]{-1f,1f}) {
                Eye("White",new Vector3(side*.03371f,.04470f,.09594f),new Vector3(.026f,.026f,.026f),new Color(.63f,.60f,.54f),body);
                Eye("Iris",new Vector3(side*.03371f,.04470f,.1081f),new Vector3(.011f,.011f,.0025f),new Color(.10f,.055f,.025f),body);
                Eye("Pupil",new Vector3(side*.03371f,.04470f,.1094f),new Vector3(.0055f,.0055f,.001f),Color.black,body);
            }
            _head=root.Find("Opponent Head");_chest=root.Find("R2 Chest");
            string[] names={"Left Upper Arm","Left Forearm","Right Upper Arm","Right Forearm","Left Thigh","Left Shin","Right Thigh","Right Shin"};
            for(int i=0;i<8;i++)_segments[i]=root.Find(names[i]);
            foreach(string n in new[]{"Opponent Head","Left Shoulder","Right Shoulder","Left Elbow","Right Elbow","Left Knee","Right Knee","R2 Abdomen","R2 Chest","R2 Left Chest","R2 Right Chest","R2 Neck"})root.Find(n).GetComponent<Renderer>().enabled=false;
            foreach(var s in _segments)s.GetComponent<Renderer>().enabled=false;
            EVContactSurface.Initialize(data);LateUpdate();
        }
        void Eye(string name,Vector3 position,Vector3 scale,Color color,Material source)
        {
            var go=new GameObject("EV Eye "+name);go.transform.SetParent(_bones[1],false);go.transform.localPosition=position;go.transform.localScale=scale;
            Mesh mesh=EVVisualShell.SphereMesh(16,12);Material material=new Material(source);material.color=color;go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;_eyeAssets.Add(mesh);_eyeAssets.Add(material);
        }
        void LateUpdate()
        {
            if(Skin==null)return;
            double t=Time.realtimeSinceStartupAsDouble;long alloc=GC.GetAllocatedBytesForCurrentThread();
            _bones[0].SetPositionAndRotation(_chest.position+_chest.rotation*Vector3.up*.06f,_chest.rotation);
            _bones[1].SetPositionAndRotation(_head.position,_head.rotation);
            for(int i=0;i<8;i++){Transform s=_segments[i];_bones[i+2].SetPositionAndRotation(s.position-s.up*s.localScale.y,s.rotation);}
            LastAllocatedBytes=GC.GetAllocatedBytesForCurrentThread()-alloc;LastUpdateMs=(float)((Time.realtimeSinceStartupAsDouble-t)*1000);
        }
        void OnDestroy(){if(_mesh!=null)Destroy(_mesh);if(_hair!=null)Destroy(_hair);if(_hairMaterial!=null)Destroy(_hairMaterial);foreach(var o in _eyeAssets)if(o!=null)Destroy(o);}
    }
}
