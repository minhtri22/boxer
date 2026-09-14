using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoxerP0
{
    // Presentation only. Reads existing anatomical anchors; never writes a combat transform.
    [DefaultExecutionOrder(280)]
    public sealed class EVVisualShell : MonoBehaviour
    {
        public static bool SegmentedExperiment;
        public bool Ready { get; private set; }
        public int ReplacementCount { get; private set; }
        public const bool BottomControlsVisible=false;
        readonly List<Mesh> _meshes=new();
        readonly List<Material> _materials=new();
        readonly List<(Transform cuff,Transform elbow,Transform glove)> _cuffs=new();
        Transform _body,_chest;
        Material _skin,_red,_gold,_black,_white,_head;
        public Transform Torso => _body;
        public static readonly Color Skin=new(.60f,.36f,.235f);

        void Start()
        {
            _skin=Material(Skin,0); _red=Material(new Color(.48f,.025f,.035f),1);
            _gold=Material(new Color(.78f,.52f,.16f),1); _black=Material(new Color(.028f,.032f,.038f),1);
            _white=Material(new Color(.82f,.79f,.71f),2);
            var leather=Resources.Load<Texture2D>("EV/GloveLeather");
            _black.SetTexture("_MainTex",leather); _red.SetTexture("_MainTex",leather);
            _head=Material(Color.white,0); _head.SetTexture("_MainTex",Resources.Load<Texture2D>("EV/RamirezHead"));
            var opponent=FindFirstObjectByType<OpponentBoxer>();
            var player=FindFirstObjectByType<PlayerBoxer>();
            if(opponent==null||player==null) return;
            BuildOpponent(opponent.transform); BuildArms(player.transform,true);
            Arena();
            var referenceVisuals=gameObject.AddComponent<EVReferenceVisuals>();
            referenceVisuals.Initialize(opponent.transform,player.transform,_body);
            Ready=referenceVisuals.Ready;
        }
        Material Material(Color c,float kind)
        {
            var m=new Material(Resources.Load<Shader>("EVSurface")); m.color=c; m.SetFloat("_Kind",kind); _materials.Add(m); return m;
        }
        Transform MeshObject(string name,Transform parent,Mesh mesh,Material material)
        {
            var go=new GameObject(name); go.transform.SetParent(parent,false);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial=material; _meshes.Add(mesh); return go.transform;
        }
        void Replace(Transform anchor,Mesh mesh,Material material)
        {
            if(anchor==null) throw new InvalidOperationException("Missing visual anchor");
            anchor.GetComponent<MeshFilter>().sharedMesh=mesh;
            anchor.GetComponent<Renderer>().sharedMaterial=material;
            _meshes.Add(mesh); ReplacementCount++;
        }
        void BuildOpponent(Transform root)
        {
            // The old primitive renderers are replaced, not covered by a second opaque fighter.
            Transform head=root.Find("Opponent Head");
            Replace(head,SphereMesh(64,48),_head);
            foreach(Transform child in head) child.gameObject.SetActive(false); // old eye/hair dots are redundant with the UV face
            BuildArms(root,false);
            _chest=root.Find("R2 Chest");
            if(!SegmentedExperiment)
            {
                foreach(string n in new[]{"R2 Abdomen","R2 Chest","R2 Left Chest","R2 Right Chest","R2 Neck"}) root.Find(n).GetComponent<Renderer>().enabled=false;
                _body=MeshObject("EV Continuous Contact Torso",root,TorsoMesh(),Material(Skin,4));
            }
            else foreach(string n in new[]{"R2 Abdomen","R2 Chest","R2 Left Chest","R2 Right Chest","R2 Neck"}) root.Find(n).GetComponent<Renderer>().sharedMaterial=_skin;
            root.Find("R2 Pelvis").GetComponent<Renderer>().enabled=false; // inside the replacement trunks
            Transform shorts=root.Find("Opponent Shorts Visual"), waist=root.Find("Opponent Gold Waistband");
            Replace(shorts,Loft(new[]{new Vector3(.43f,-.5f,.43f),new Vector3(.50f,0,.50f),new Vector3(.46f,.5f,.46f)},40),_red);
            Replace(waist,Loft(new[]{new Vector3(.47f,-.5f,.47f),new Vector3(.50f,0,.50f),new Vector3(.47f,.5f,.47f)},48),_gold);
            Word(waist,"RAMIREZ",new Vector3(0,0,.505f),.12f,_black);
            foreach(string side in new[]{"Left","Right"})
            {
                Transform thigh=root.Find(side+" Thigh"),shin=root.Find(side+" Shin"),foot=root.Find(side+" Shoe");
                Replace(thigh,Loft(new[]{new Vector3(.39f,-1,.39f),new Vector3(.65f,-.25f,.58f),new Vector3(.66f,.45f,.60f),new Vector3(.45f,1,.45f)},24),_skin);
                root.Find(side+" Knee").GetComponent<Renderer>().sharedMaterial=_skin;
                Replace(shin,Loft(new[]{new Vector3(.32f,-1,.34f),new Vector3(.43f,-.35f,.42f),new Vector3(.67f,.35f,.53f),new Vector3(.48f,1,.45f)},24),_skin);
                // Each trunk leg follows the actual thigh, preserving hip/knee motion.
                var leg=MeshObject("EV "+side+" Trunk Leg",thigh,Loft(new[]{new Vector3(.74f,-1.06f,.68f),new Vector3(.91f,-.80f,.83f),new Vector3(.91f,-.15f,.85f),new Vector3(.85f,.05f,.83f)},32),_red);
                MeshObject("EV "+side+" Gold Hem",leg,Loft(new[]{new Vector3(.86f,-.025f,.84f),new Vector3(.85f,.065f,.835f)},32),_gold);
                Replace(foot,Loft(new[]{new Vector3(.41f,-.5f,.49f),new Vector3(.5f,-.20f,.5f),new Vector3(.42f,.50f,.40f),new Vector3(.34f,1.6f,.27f)},32),_white);
                var sole=MeshObject("EV "+side+" Boot Sole",foot,Loft(new[]{new Vector3(.43f,-.53f,.5f),new Vector3(.5f,-.32f,.51f)},32),_red);
                var boot=MeshObject("EV "+side+" Boot Upper",shin,Loft(new[]{new Vector3(.46f,.26f,.40f),new Vector3(.48f,.65f,.46f),new Vector3(.42f,1,.43f)},24),_white);
                // Crossed laces are local details on the same shin; not independent foot writers.
                for(int j=0;j<5;j++)
                {
                    float y=.30f+j*.13f;
                    Line("Lace",boot,new Vector3(-.30f,y,.40f),new Vector3(.30f,y+.1f,.40f),.024f,_red);
                    Line("Lace",boot,new Vector3(.30f,y,.40f),new Vector3(-.30f,y+.1f,.40f),.024f,_red);
                }
            }
        }
        void BuildArms(Transform root,bool player)
        {
            foreach(string side in new[]{"Left","Right"})
            {
                root.Find(side+" Shoulder").GetComponent<Renderer>().sharedMaterial=_skin;
                root.Find(side+" Elbow").GetComponent<Renderer>().sharedMaterial=_skin;
                Replace(root.Find(side+" Upper Arm"),Loft(new[]{new Vector3(.38f,-1,.38f),new Vector3(.54f,-.6f,.5f),new Vector3(.66f,.2f,.56f),new Vector3(.58f,.7f,.5f),new Vector3(.42f,1,.42f)},28),_skin);
                Replace(root.Find(side+" Forearm"),Loft(new[]{new Vector3(.39f,-1,.38f),new Vector3(.55f,-.65f,.45f),new Vector3(.62f,.25f,.50f),new Vector3(.49f,1,.45f)},28),_skin);
                Transform glove=root.Find((player?"Player ":"Opponent ")+side+" Glove");
                foreach(Transform child in glove) child.gameObject.SetActive(false);
                // The padded striking surface remains the exact authoritative radius .115 sphere.
                Replace(glove,SphereMesh(48,32),player?_black:_red);
                var cuff=MeshObject("EV "+side+" Glove Cuff",glove,Loft(new[]{new Vector3(.32f,-.78f,.29f),new Vector3(.37f,-.49f,.31f),new Vector3(.34f,-.27f,.30f)},32),player?_black:_white);
                _cuffs.Add((cuff,root.Find(side+" Elbow"),glove));
                MeshObject("EV "+side+" Cuff Piping",cuff,Loft(new[]{new Vector3(.325f,-.79f,.295f),new Vector3(.34f,-.73f,.30f)},32),player?_gold:_red);
                // Branding is attached surface detail, never an impact proxy.
                Crown(glove,new Vector3(0,0,-.497f),.19f,_gold);
                Crown(glove,new Vector3(0,0,.497f),.19f,_gold);
                if(player) Word(cuff,"BXR",new Vector3(0,-.52f,-.315f),.12f,_gold);
                else for(int j=0;j<5;j++) Line("Wrap seam",cuff,new Vector3(-.24f,-.70f+j*.07f,.26f),new Vector3(.24f,-.70f+j*.07f,.26f),.012f,_red);
            }
        }
        void LateUpdate()
        {
            if(_body!=null) _body.SetPositionAndRotation(_chest.position,_chest.rotation);
            foreach(var c in _cuffs)
                c.cuff.rotation=Quaternion.FromToRotation(Vector3.up,c.glove.position-c.elbow.position);
        }
        void Word(Transform parent,string text,Vector3 position,float size,Material color)
        {
            var go=new GameObject("EV Lettering "+text); go.transform.SetParent(parent,false); go.transform.localPosition=position;
            // TextMesh font is a local attached decal, not screen-space fighter art.
            var tm=go.AddComponent<TextMesh>(); tm.text=text; tm.font=Resources.Load<Font>("Fonts/Oswald"); tm.fontSize=64; tm.characterSize=size*.1f;
            tm.anchor=TextAnchor.MiddleCenter; tm.alignment=TextAlignment.Center; tm.color=color.color;
            if(tm.font!=null) { tm.font.RequestCharactersInTexture(text,64);var mat=new Material(Resources.Load<Shader>("EVText"));mat.mainTexture=tm.font.material.mainTexture;_materials.Add(mat);go.GetComponent<MeshRenderer>().sharedMaterial=mat; }
            if(position.z>0) go.transform.localRotation=Quaternion.Euler(0,180,0);
        }
        void Crown(Transform parent,Vector3 position,float scale,Material mat)
        {
            var m=new Mesh(); m.vertices=new[]{new Vector3(-1,.4f,0),new Vector3(-.6f,-.5f,0),new Vector3(.6f,-.5f,0),new Vector3(1,.4f,0),new Vector3(.45f,0,0),new Vector3(0,.8f,0),new Vector3(-.45f,0,0)};
            m.triangles=new[]{0,1,6,6,1,2,6,2,4,6,4,5,4,2,3}; m.RecalculateNormals();
            var t=MeshObject("EV Crown",parent,m,mat); t.localPosition=position; t.localScale=Vector3.one*scale;
            if(position.z<0) t.localRotation=Quaternion.Euler(0,180,0);
        }
        void Line(string name,Transform parent,Vector3 a,Vector3 b,float radius,Material mat)
        {
            var t=MeshObject("EV "+name,parent,Loft(new[]{new Vector3(1,-1,1),new Vector3(1,1,1)},8),mat);
            t.localPosition=(a+b)*.5f; t.localRotation=Quaternion.FromToRotation(Vector3.up,b-a); t.localScale=new Vector3(radius,(b-a).magnitude*.5f,radius);
        }
        public static Mesh Loft(Vector3[] rings,int sides)
        {
            var v=new Vector3[rings.Length*(sides+1)]; var uv=new Vector2[v.Length]; var tri=new int[(rings.Length-1)*sides*6];
            for(int j=0;j<rings.Length;j++) for(int i=0;i<=sides;i++) { float a=2*Mathf.PI*i/sides; int k=j*(sides+1)+i; v[k]=new Vector3(Mathf.Sin(a)*rings[j].x,rings[j].y,Mathf.Cos(a)*rings[j].z); uv[k]=new Vector2(i/(float)sides,j/(float)(rings.Length-1)); }
            int n=0; for(int j=0;j<rings.Length-1;j++) for(int i=0;i<sides;i++) { int a=j*(sides+1)+i,b=a+sides+1; tri[n++]=a;tri[n++]=a+1;tri[n++]=b;tri[n++]=a+1;tri[n++]=b+1;tri[n++]=b; }
            var mesh=new Mesh { name="EV Shaped Surface",vertices=v,uv=uv,triangles=tri }; mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }
        public static Mesh SphereMesh(int sides,int rows)
        {
            var rings=new Vector3[rows+1]; for(int j=0;j<=rows;j++) { float a=Mathf.PI*j/rows; rings[j]=new Vector3(.5f*Mathf.Sin(a),-.5f*Mathf.Cos(a),.5f*Mathf.Sin(a)); }
            Mesh m=Loft(rings,sides); var uv=m.uv;
            // Front (+Z) is U=.5, with ears a quarter-turn to either side.
            for(int j=0;j<=rows;j++) for(int i=0;i<=sides;i++) uv[j*(sides+1)+i]=new Vector2(i/(float)sides+.5f,j/(float)rows);
            m.uv=uv; return m;
        }
        public static Mesh TorsoMesh()
        {
            // Clip each sphere's tessellation against the other sphere intersection planes.
            // Unlike a radial envelope this preserves non-star-shaped shoulder/neck recesses.
            Vector3[] centers={new(0,1.16f,0),new(-.165f,1.37f,0),new(0,1.37f,0),new(.165f,1.37f,0),new(0,1.51f,0)};
            float[] radii={.25f,.18f,.18f,.18f,.10f};
            var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
            var lookup=new Dictionary<Vector3Int,int>();var poly=new List<Vector3>(12);var next=new List<Vector3>(12);
            var sphere=SphereMesh(96,64);var sv=sphere.vertices;var st=sphere.triangles;
            int Add(Vector3 p) {
                var key=new Vector3Int(Mathf.RoundToInt(p.x*1000000),Mathf.RoundToInt(p.y*1000000),Mathf.RoundToInt(p.z*1000000));
                if(lookup.TryGetValue(key,out int index))return index;
                index=vertices.Count;lookup.Add(key,index);vertices.Add(p-Vector3.up*1.37f);
                uv.Add(new Vector2(Mathf.Atan2(p.x,p.z)/(2*Mathf.PI),(p.y-.91f)/.70f));return index;
            }
            for(int k=0;k<centers.Length;k++)for(int i=0;i<st.Length;i+=3) {
                poly.Clear();for(int j=0;j<3;j++)poly.Add(centers[k]+sv[st[i+j]]*(2*radii[k]));
                for(int other=0;other<centers.Length&&poly.Count>0;other++) {
                    if(other==k)continue;
                    Vector3 delta=centers[k]-centers[other];
                    float d=delta.sqrMagnitude+radii[k]*radii[k]-radii[other]*radii[other];
                    float Plane(Vector3 p)=>2*Vector3.Dot(p-centers[k],delta)+d;
                    next.Clear();Vector3 previous=poly[poly.Count-1];float pd=Plane(previous);
                    foreach(Vector3 current in poly){float cd=Plane(current);if((pd>=0)!=(cd>=0))next.Add(Vector3.Lerp(previous,current,pd/(pd-cd)));if(cd>=0)next.Add(current);previous=current;pd=cd;}
                    var swap=poly;poly=next;next=swap;
                }
                for(int j=1;j<poly.Count-1;j++){int aa=Add(poly[0]),bb=Add(poly[j]),cc=Add(poly[j+1]);if(aa!=bb&&bb!=cc&&aa!=cc){triangles.Add(aa);triangles.Add(bb);triangles.Add(cc);}}
            }
            if(Application.isPlaying)Destroy(sphere);else DestroyImmediate(sphere);
            var normals=new List<Vector3>(vertices.Count);
            foreach(Vector3 local in vertices) {
                Vector3 p=local+Vector3.up*1.37f,normal=Vector3.zero;
                for(int k=0;k<centers.Length;k++){Vector3 delta=p-centers[k];float gap=Mathf.Abs(delta.magnitude-radii[k]);normal+=delta.normalized*Mathf.Exp(-gap*gap/.0009f);}
                normals.Add(normal.normalized);
            }
            var mesh=new Mesh {name="EV Clipped Original Contact Union",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.SetNormals(normals);mesh.RecalculateBounds();return mesh;
        }
        void Arena()
        {
            Transform root=new GameObject("EV Arena Details").transform;
            var dark=Material(new Color(.017f,.020f,.028f),2);
            var canvas=Material(new Color(.65f,.62f,.56f),3);
            var arena=Resources.Load<Texture2D>("P1V/championship-arena");
            canvas.mainTexture=arena;canvas.mainTextureScale=new Vector2(.7f,.18f);canvas.mainTextureOffset=new Vector2(.15f,.01f);
            var floor=GameObject.Find("Underground Ring Canvas");if(floor!=null)floor.GetComponent<Renderer>().sharedMaterial=canvas;
            // Distant scenery only: a depth-tested world plane behind the physical ring/fighters.
            var backdropMat=new Material(Resources.Load<Shader>("EVBackdrop"));backdropMat.mainTexture=arena;_materials.Add(backdropMat);
            var quad=new Mesh {vertices=new[]{new Vector3(-.5f,-.5f,0),new Vector3(-.5f,.5f,0),new Vector3(.5f,.5f,0),new Vector3(.5f,-.5f,0)},uv=new[]{new Vector2(0,.42f),new Vector2(0,1),new Vector2(1,1),new Vector2(1,.42f)},triangles=new[]{0,1,2,0,2,3}};quad.RecalculateNormals();
            var backdrop=MeshObject("EV Distant Arena Scenery",root,quad,backdropMat);backdrop.position=new Vector3(0,3.0f,4.24f);backdrop.localScale=new Vector3(8.1f,5.5f,1);
            foreach(var renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None)) {
                string n=renderer.name;
                if(n=="Crowd Silhouette"||n=="Crowd Head"||n=="Back Crowd"||n=="Back Crowd Head"||n=="String Bulb"||n.StartsWith("Crate ")) renderer.enabled=false;
            }
            for(int side=-1;side<=1;side+=2)
            {
                var banner=MeshObject("EV Discipline Banner",root,Loft(new[]{new Vector3(.5f,-1,.04f),new Vector3(.5f,1,.04f)},4),dark);
                banner.position=new Vector3(side*2.5f,2.3f,3.5f);banner.localScale=new Vector3(1,1,1);
                Word(banner,"DISCIPLINE\nBUILDS\nCHAMPIONS",new Vector3(0,0,-.06f),.25f,_gold);
                for(int j=0;j<4;j++) Line("Ring Rope",root,new Vector3(-2.55f,.50f+j*.28f,side>0?2.75f:-2.35f),new Vector3(2.55f,.50f+j*.28f,side>0?2.75f:-2.35f),.025f,j==3?_red:_white);
            }
            foreach(float x in new[]{-2.55f,2.55f})foreach(float z in new[]{-2.35f,2.75f})
            {
                var post=MeshObject("EV Corner Pad",root,Loft(new[]{new Vector3(.09f,0,.09f),new Vector3(.09f,1.45f,.09f)},12),_black);post.position=new Vector3(x,0,z);
                Word(post,"BOXER",new Vector3(0,1,-.10f),.07f,_gold);
            }
            var logo=new GameObject("EV Arena Brand").transform;logo.SetParent(root);logo.position=new Vector3(0,2.8f,4.18f);
            Word(logo,"BOXER",Vector3.zero,.90f,_gold);
        }
        void OnDestroy(){foreach(var m in _meshes)if(m!=null)Destroy(m);foreach(var m in _materials)if(m!=null)Destroy(m);}
    }
}
