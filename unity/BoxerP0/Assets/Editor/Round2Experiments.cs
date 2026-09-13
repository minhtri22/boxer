using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class Round2Experiments
    {
        public static void Run()
        {
            var log = new StringBuilder("ROUND2 CANDIDATE EXPERIMENT — identical anatomical pose; independent dense contact oracle\n");
            PunchIntent[] intents={PunchIntent.Jab,PunchIntent.Cross,PunchIntent.LeadHook,PunchIntent.LeadUppercut,PunchIntent.LeadOverhand};
            int[] rates={15,30,60,120};
            int checks=0, falsePoint=0, falseSweep=0, earlyPoint=0, earlySweep=0;
            float maxChordGap=0f;
            var watch=Stopwatch.StartNew();
            foreach(var intent in intents) foreach(int hz in rates)
            for(int range=0;range<3;range++) for(int offset=-15;offset<=15;offset++)
            {
                float distance=range==0?1.5f:range==1?0.94f:0.69f;
                Vector3 endpoint=Round2Motion.Endpoint(intent,"NEUTRAL",distance);
                Vector3 center=new Vector3(offset*0.025f,1.54f,distance);
                float radius=Round2Motion.GloveRadius+Round2Motion.HeadRadius;
                float denseGap=float.PositiveInfinity;
                for(int j=0;j<=2048;j++)
                {
                    Vector3 w=Round2Motion.Sample(true,intent,ActionPhase.Extend,j/2048f,endpoint).Wrist;
                    // Rear cross uses right hand.
                    if(intent==PunchIntent.Cross) w=Round2Motion.Sample(false,intent,ActionPhase.Extend,j/2048f,endpoint).Wrist;
                    denseGap=Mathf.Min(denseGap,Vector3.Distance(w,center)-radius);
                }
                bool oracle=denseGap<=0f, point=false, sweep=false;
                int frames=Mathf.CeilToInt(0.14f*hz);
                bool left=intent!=PunchIntent.Cross;
                Vector3 previous=Round2Motion.Sample(left,intent,ActionPhase.Extend,0f,endpoint).Wrist;
                for(int f=1;f<=frames;f++)
                {
                    float end=Mathf.Min(1f,f/(0.14f*hz));
                    Vector3 current=Round2Motion.Sample(left,intent,ActionPhase.Extend,end,endpoint).Wrist;
                    point |= Vector3.Distance(current,center)<=radius;
                    float begin=Mathf.Min(1f,(f-1)/(0.14f*hz));
                    int sub=Mathf.CeilToInt((end-begin)*0.14f/Round2Motion.SampleSeconds);
                    Vector3 a=previous;
                    for(int s=1;s<=sub;s++)
                    {
                        float u=Mathf.Lerp(begin,end,s/(float)sub);
                        Vector3 b=Round2Motion.Sample(left,intent,ActionPhase.Extend,u,endpoint).Wrist;
                        sweep |= Round2Motion.Sweep(a,b,center,center,radius,out _);
                        Vector3 mid=Round2Motion.Sample(left,intent,ActionPhase.Extend,Mathf.Lerp(begin,end,(s-0.5f)/sub),endpoint).Wrist;
                        maxChordGap=Mathf.Max(maxChordGap,Vector3.Distance(mid,(a+b)*0.5f));
                        a=b;
                    }
                    previous=current;
                }
                if(oracle&&!point) falsePoint++; if(oracle&&!sweep) falseSweep++;
                if(!oracle&&point) earlyPoint++; if(!oracle&&sweep) earlySweep++;
                checks++;
            }
            watch.Stop();
            log.AppendLine($"fixtures={checks} families=5 bands=3 fps=15,30,60,120 lateral_offsets=31 dense_oracle=2049_samples");
            log.AppendLine($"B_POINT false_miss={falsePoint} false_contact={earlyPoint}");
            log.AppendLine($"C_SUBSAMPLED_SWEEP false_miss={falseSweep} false_contact={earlySweep}");
            log.AppendLine($"max_midpoint_chord_deviation_m={maxChordGap:R} combined_experiment_ms={watch.Elapsed.TotalMilliseconds:F2}");
            log.AppendLine("A_LEGACY_ENDPOINT rejected_before_AB: baseline arm excess 0.622574m jab; frozen bones cannot reach without shoulder detachment or disproportionate torso translation.");
            log.AppendLine("Selection must also pass moving-target, runtime ownership, regression and visual gates; this experiment alone is not implementation PASS.");
            string repo=Path.GetFullPath(Path.Combine(Application.dataPath,"../../.."));
            Directory.CreateDirectory(Path.Combine(repo,"evidence/uat-round2/optimization"));
            File.WriteAllText(Path.Combine(repo,"evidence/uat-round2/optimization/candidates.txt"),log.ToString());
            UnityEngine.Debug.Log(log);
        }
    }
}
