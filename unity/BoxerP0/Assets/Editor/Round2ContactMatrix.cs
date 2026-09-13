using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class Round2ContactMatrix
    {
        public static void Run()
        {
            var log=new StringBuilder("intent,range,defense,fps,result,dense_result,min_dense_gap_m\n");
            int cases=0,disagreements=0;
            foreach(PunchIntent intent in new[]{PunchIntent.Jab,PunchIntent.Cross,PunchIntent.LeadHook,PunchIntent.RearHook,PunchIntent.LeadUppercut,PunchIntent.RearUppercut,PunchIntent.LeadOverhand,PunchIntent.RearOverhand})
            foreach(float distance in new[]{1.5f,0.94f,0.69f}) foreach(int defense in new[]{0,1,2}) foreach(int fps in new[]{15,30,60,120})
            {
                bool left=!PunchLabels.IsRearHand(intent);
                Vector3 endpoint=Round2Motion.Endpoint(intent,"NEUTRAL",distance);
                Func<float,Round2Frame> target=t=>new Round2Frame { Position=new Vector3(0f,0f,distance+(defense==2?t*0.12f:0f)),
                    Rotation=Quaternion.Euler(0f,180f,0f),Phase=ActionPhase.Guard,Intent=PunchIntent.None,HeadOffset=defense==2?t*0.12f:0f };
                bool guard=defense!=1;
                int dense=-1,swept=-1; float gap=float.PositiveInfinity;
                for(int j=0;j<=2048;j++)
                {
                    float t=j/2048f; var body=target(t);
                    var wrist=Round2Motion.Sample(left,intent,ActionPhase.Extend,t,endpoint).Wrist;
                    for(int k=guard?0:2;k<7;k++)
                    {
                        float g=Vector3.Distance(wrist,body.Target(k))-Round2Motion.GloveRadius-Round2CombatRig.TargetRadius(k);
                        gap=Mathf.Min(gap,g);
                        if(g<=0f&&dense<0) dense=k;
                    }
                }
                int frames=Mathf.CeilToInt(0.14f*fps);
                for(int f=0;f<frames && swept<0;f++)
                {
                    float begin=Mathf.Min(1f,f/(0.14f*fps)), end=Mathf.Min(1f,(f+1)/(0.14f*fps));
                    int subs=Mathf.Max(1,Mathf.CeilToInt((end-begin)*0.14f/Round2Motion.SampleSeconds));
                    for(int s=0;s<subs && swept<0;s++)
                    {
                        float a=Mathf.Lerp(begin,end,s/(float)subs), b=Mathf.Lerp(begin,end,(s+1)/(float)subs);
                        var wa=Round2Motion.Sample(left,intent,ActionPhase.Extend,a,endpoint).Wrist;
                        var wb=Round2Motion.Sample(left,intent,ActionPhase.Extend,b,endpoint).Wrist;
                        swept=Round2CombatRig.SweepTargets(wa,wb,target(a),target(b),guard,0,out _);
                    }
                }
                string result=Token(swept), reference=Token(dense);
                cases++; if(result!=reference) disagreements++;
                log.AppendLine(string.Join(",",intent,distance.ToString("F2",System.Globalization.CultureInfo.InvariantCulture),defense,fps,result,reference,gap.ToString("F6",System.Globalization.CultureInfo.InvariantCulture)));
            }
            string directory=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../evidence/uat-round2/optimization"));Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory,"contact-matrix.csv"),log.ToString());
            string summary=$"CASES={cases} CLASSIFICATION_DISAGREEMENTS={disagreements} dense=2049_samples moving_targets_included=true";
            File.WriteAllText(Path.Combine(directory,"contact-matrix-summary.txt"),summary);
            Debug.Log(summary);
            if(disagreements!=0) throw new Exception("Contact matrix disagrees with dense visible-volume oracle");
        }
        static string Token(int target) => target<0?"MISS":target<2?"BLOCK":"HIT";
    }
}
