using UnityEngine;

namespace BoxerP0
{
    // Presentation only: these examples never inject input or update lesson progress.
    public static class TrainingGestureGuide
    {
        private const float SecondsPerCue = 2.4f;
        private static Texture2D _shoe, _head, _glove, _disc;
        private static readonly Color Gold = new(.98f, .76f, .32f);
        private static readonly Vector2[] BootOutline = { new(.16f,.89f),new(.48f,.89f),new(.5f,.49f),
            new(.75f,.34f),new(.92f,.26f),new(.94f,.11f),new(.12f,.11f),new(.1f,.3f) };
        public static string Cue(string stage, float seconds)
        {
            int step = Mathf.FloorToInt(Mathf.Max(0, seconds) / SecondsPerCue);
            return stage switch
            {
                "HEADCONTROL" => step % 2 == 0 ? "HEAD_LEFT" : "HEAD_RIGHT",
                "FOOTWORK" => (step % 4) switch { 0 => "MOVE_UP", 1 => "MOVE_DOWN", 2 => "MOVE_LEFT", _ => "MOVE_RIGHT" },
                "PUNCHES" => (step % 5) switch { 0 => "PUNCH_DOWN", 1 => "PUNCH_UP", 2 => "PUNCH_RIGHT", 3 => "PUNCH_LEFT", _ => "TAP_REPEAT" },
                _ => string.Empty
            };
        }
        public static Vector2 Direction(string cue) => cue switch
        {
            "MOVE_UP" or "PUNCH_UP" => Vector2.down, // GUI y increases down the screen.
            "MOVE_DOWN" or "PUNCH_DOWN" => Vector2.up,
            "HEAD_LEFT" or "MOVE_LEFT" or "PUNCH_LEFT" => Vector2.left,
            "HEAD_RIGHT" or "MOVE_RIGHT" or "PUNCH_RIGHT" => Vector2.right,
            _ => Vector2.zero
        };
        public static string Caption(string cue) => cue switch
        {
            "HEAD_LEFT" => "NÉ SANG TRÁI", "HEAD_RIGHT" => "NÉ SANG PHẢI",
            "MOVE_UP" => "VUỐT LÊN · TIẾN", "MOVE_DOWN" => "VUỐT XUỐNG · LÙI",
            "MOVE_LEFT" => "VUỐT TRÁI", "MOVE_RIGHT" => "VUỐT PHẢI",
            "PUNCH_DOWN" => "VUỐT XUỐNG · ĐÒN TỪ TRÊN", "PUNCH_UP" => "VUỐT LÊN · MÓC TỪ DƯỚI",
            "PUNCH_RIGHT" => "VUỐT PHẢI · MÓC NGANG", "PUNCH_LEFT" => "VUỐT TRÁI · MÓC NGANG",
            "TAP_REPEAT" => "CHẠM NHIỀU LẦN · JAB / CROSS", _ => string.Empty
        };
        public static void Draw(string stage, float seconds, GUIStyle label)
        {
            if (stage != "HEADCONTROL" && stage != "FOOTWORK" && stage != "PUNCHES") return;
            EnsureTextures();
            Color prior = GUI.color;
            bool head = stage == "HEADCONTROL", feet = stage == "FOOTWORK";
            float x = head ? 270 : feet ? 137 : 402;
            Vector2 center = new(x, head ? 91 : 773);
            string cue = Cue(stage, seconds);
            GUI.color = Gold;
            GUI.DrawTexture(new Rect(x-22, head ? 22 : 678, 44, 44), head ? _head : feet ? _shoe : _glove);
            Vector2 direction = Direction(cue);
            float phase = Mathf.Repeat(seconds, SecondsPerCue) / SecondsPerCue;
            if (direction == Vector2.zero)
            {
                // Fixed target, repeated flashes + expanding halo: never implies a swipe.
                float pulse = Mathf.Repeat(seconds * 2.8f, 1);
                Disc(center, 20 + pulse*30, new Color(Gold.r, Gold.g, Gold.b, .24f*(1-pulse)));
                Disc(center, 13, new Color(1, .94f, .65f, .35f + .65f*(1-pulse)));
            }
            else
            {
                float reach=head?65:48;
                Vector2 start = center-direction*reach, end = center+direction*reach;
                Line(start, end, 2, new Color(Gold.r,Gold.g,Gold.b,.22f));
                Vector2 normal = new(-direction.y,direction.x);
                Line(end,end-direction*14+normal*9,3,Gold);
                Line(end,end-direction*14-normal*9,3,Gold);
                Vector2 tip = Vector2.Lerp(start,end,Mathf.Clamp01(phase/ .82f));
                for (int i=12;i>=0;i--)
                {
                    float age=i/12f;
                    Vector2 p=tip-direction*(age*50);
                    if(Vector2.Dot(p-start,direction)<0)continue;
                    Disc(p, 7+age*3, new Color(Gold.r,Gold.g,Gold.b,.035f+(1-age)*.09f));
                    Disc(p, 2.5f, new Color(1,.94f,.68f,(1-age)*.8f));
                }
                Disc(tip,7,new Color(Gold.r,Gold.g,Gold.b,.32f));
                Disc(tip,3.5f,Color.white);
            }
            GUI.color=Color.white;
            GUI.Label(new Rect(head ? 35 : x-111,head ? 111 : 837,head ? 470 : 222,head ? 30 : 50),Caption(cue),label);
            GUI.color=prior;
        }
        private static void Disc(Vector2 center,float radius,Color color)
        { GUI.color=color;GUI.DrawTexture(new Rect(center.x-radius,center.y-radius,radius*2,radius*2),_disc); }
        private static void Line(Vector2 start,Vector2 end,float width,Color color)
        {
            Matrix4x4 prior=GUI.matrix;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(end.y-start.y,end.x-start.x)*Mathf.Rad2Deg,start);
            GUI.color=color;GUI.DrawTexture(new Rect(start.x,start.y-width/2,(end-start).magnitude,width),Texture2D.whiteTexture);
            GUI.matrix=prior;
        }
        private static void EnsureTextures()
        {
            if(_disc!=null)return;
            _disc=Raster((x,y)=>(new Vector2(x-.5f,y-.5f)).sqrMagnitude<.24f);
            _head=Raster((x,y)=>Ellipse(x,y,.5f,.7f,.21f,.27f) ||
                x>.4f&&x<.6f&&y>.26f&&y<.55f || Ellipse(x,y,.5f,.16f,.39f,.2f));
            _shoe=Raster((x,y)=>Inside(x,y,BootOutline) &&
                !(x>.23f&&x<.42f&&(Mathf.Abs(y-.68f)<.022f||Mathf.Abs(y-.58f)<.022f||Mathf.Abs(y-.48f)<.022f)) && !(y>.18f&&y<.205f));
            _glove=Raster((x,y)=>(Ellipse(x,y,.49f,.68f,.31f,.29f)||Ellipse(x,y,.8f,.45f,.15f,.23f)||
                x>.24f&&x<.73f&&y>.14f&&y<.5f) && !(x<.73f&&y>.27f&&y<.30f));
        }
        private static bool Ellipse(float x,float y,float cx,float cy,float rx,float ry)
        { float dx=(x-cx)/rx,dy=(y-cy)/ry;return dx*dx+dy*dy<1; }
        private static bool Inside(float x,float y,Vector2[] polygon)
        {
            bool inside=false;
            for(int i=0,j=polygon.Length-1;i<polygon.Length;j=i++)
                if((polygon[i].y>y)!=(polygon[j].y>y)&&x<(polygon[j].x-polygon[i].x)*(y-polygon[i].y)/(polygon[j].y-polygon[i].y)+polygon[i].x)inside=!inside;
            return inside;
        }
        private static Texture2D Raster(System.Func<float,float,bool> mask)
        {
            const int size=96;var pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                int hits=0;for(int sy=0;sy<2;sy++)for(int sx=0;sx<2;sx++)
                    if(mask((x+(sx+.5f)/2)/size,(y+(sy+.5f)/2)/size))hits++;
                pixels[y*size+x]=new Color(1,1,1,hits/4f);
            }
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){name="Training guide",hideFlags=HideFlags.HideAndDontSave,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
            texture.SetPixels(pixels);texture.Apply(false,true);return texture;
        }
    }
}
