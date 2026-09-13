using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class Round2SelfTests
    {
        public static void RunAll()
        {
            P1DOpponentAttributesSelfTests.RunWithRegressions();
            P1VPresentationSelfTests.Run();
            Round2Experiments.Run();
            Round2ContactMatrix.Run();
            Run();
        }
        static int _passed,_failed;
        static readonly StringBuilder Log=new StringBuilder();
        static void Check(bool ok,string name) { if(ok)_passed++;else _failed++; Log.AppendLine((ok?"PASS ":"FAIL ")+name); }
        public static void Run()
        {
            _passed=0; _failed=0; Log.Clear();
            foreach(PunchIntent intent in new[]{PunchIntent.Jab,PunchIntent.Cross,PunchIntent.LeadHook,PunchIntent.RearHook,PunchIntent.LeadUppercut,PunchIntent.RearUppercut,PunchIntent.LeadOverhand,PunchIntent.RearOverhand})
            {
                bool left=!PunchLabels.IsRearHand(intent);
                Vector3 endpoint=Round2Motion.Endpoint(intent,"NEUTRAL",0.94f);
                float error=0f;
                foreach(ActionPhase phase in new[]{ActionPhase.Guard,ActionPhase.Commit,ActionPhase.Extend,ActionPhase.Recover})
                for(int i=0;i<=100;i++)
                {
                    var arm=Round2Motion.Sample(left,intent,phase,i/100f,endpoint);
                    error=Mathf.Max(error,Mathf.Abs(Vector3.Distance(arm.Shoulder,arm.Elbow)-0.34f),Mathf.Abs(Vector3.Distance(arm.Elbow,arm.Wrist)-0.31f));
                }
                Check(error<0.00001f,intent+" frozen bone lengths across all phases");
                var guard=Round2Motion.Sample(left,intent,ActionPhase.Guard,0f,endpoint);
                var finish=Round2Motion.Sample(left,intent,ActionPhase.Recover,1f,endpoint);
                Check(Vector3.Distance(guard.Wrist,finish.Wrist)<0.00001f,intent+" continuous recovery to guard");
                var c=Round2Motion.Sample(left,intent,ActionPhase.Commit,1f,endpoint);
                var e=Round2Motion.Sample(left,intent,ActionPhase.Extend,0f,endpoint);
                Check(Vector3.Distance(c.Wrist,e.Wrist)<0.00001f,intent+" continuous commit to extension");
                var x=Round2Motion.Sample(left,intent,ActionPhase.Extend,1f,endpoint);
                var r=Round2Motion.Sample(left,intent,ActionPhase.Recover,0f,endpoint);
                Check(Vector3.Distance(x.Wrist,r.Wrist)<0.00001f,intent+" continuous extension to recovery");
                if(P1PunchMechanics.IsStraightPunch(intent))
                {
                    float ahead=Round2Motion.Sample(left,intent,ActionPhase.Extend,1f,Round2Motion.Endpoint(intent,"ADVANCING",0.94f)).Wrist.z;
                    float behind=Round2Motion.Sample(left,intent,ActionPhase.Extend,1f,Round2Motion.Endpoint(intent,"RETREATING",0.94f)).Wrist.z;
                    Check(ahead>x.Wrist.z && x.Wrist.z>behind,intent+" A1 solved advance > neutral > retreat");
                }
                if(P1PunchMechanics.IsHook(intent))
                {
                    float close=Round2Motion.Sample(left,intent,ActionPhase.Extend,1f,Round2Motion.Endpoint(intent,"NEUTRAL",0.69f)).Wrist.z;
                    float far=Round2Motion.Sample(left,intent,ActionPhase.Extend,1f,Round2Motion.Endpoint(intent,"NEUTRAL",1.5f)).Wrist.z;
                    Check(close>far,intent+" A3.1 close extension > far extension");
                }
            }
            Check(!Round2Motion.Sweep(Vector3.zero,Vector3.right,new Vector3(0.5f,0.3f,0f),new Vector3(0.5f,0.3f,0f),0.2f,out _),"outside path MISS");
            Check(Round2Motion.Sweep(Vector3.zero,Vector3.right,new Vector3(0.5f,0f,0f),new Vector3(0.5f,0f,0f),0.2f,out float impact)&&Mathf.Abs(impact-0.3f)<0.00001f,"first surface contact, not endpoint prediction");
            Check(Round2Motion.Sweep(Vector3.zero,Vector3.zero,Vector3.left,Vector3.right,0.2f,out impact)&&Mathf.Abs(impact-0.4f)<0.00001f,"moving target crossing is not missed");
            Check(!Round2Motion.Sweep(Vector3.zero,Vector3.right,Vector3.right*2f,Vector3.right*3f,0.2f,out _),"equal velocity separated volumes MISS");
            Check(Round2Motion.Band(1.5f)=="LONG"&&Round2Motion.Band(0.94f)=="BOXING"&&Round2Motion.Band(0.69f)=="CLOSE","geometry-derived distance ordering");
            Vector3 baseline=Round2Motion.Endpoint(PunchIntent.Cross,"NEUTRAL",0.94f), longer=baseline;
            longer.z*=P1OpponentAttributes.LongReachFactor;
            Check(Round2Motion.Sample(false,PunchIntent.Cross,ActionPhase.Extend,1f,longer).Wrist.z>
                Round2Motion.Sample(false,PunchIntent.Cross,ActionPhase.Extend,1f,baseline).Wrist.z,"P1-D long reach retains effect within frozen bone lengths");
            const int iterations=100000;
            Vector3 end=Round2Motion.Endpoint(PunchIntent.Jab,"NEUTRAL",0.94f);
            for(int i=0;i<100;i++) Round2Motion.Sample(true,PunchIntent.Jab,ActionPhase.Extend,i/100f,end);
            var watch=new Stopwatch(); long bytes=GC.GetAllocatedBytesForCurrentThread(); watch.Start();
            float consume=0f;
            for(int i=0;i<iterations;i++)
            {
                var a=Round2Motion.Sample(true,PunchIntent.Jab,ActionPhase.Extend,(i%100)/100f,end);
                var b=Round2Motion.Sample(true,PunchIntent.Jab,ActionPhase.Extend,(i%100+1)/100f,end);
                Round2Motion.Sweep(a.Wrist,b.Wrist,new Vector3(0f,1.62f,0.94f),new Vector3(0f,1.62f,0.94f),0.295f,out float t);
                consume+=t;
            }
            watch.Stop(); bytes=GC.GetAllocatedBytesForCurrentThread()-bytes;
            Check(bytes==0,"pose plus sweep hot loop allocates zero managed bytes");
            Log.AppendLine($"benchmark_iterations={iterations} elapsed_ms={watch.Elapsed.TotalMilliseconds:F3} allocated_bytes={bytes} checksum={consume}");
            Log.AppendLine($"TOTAL={_passed+_failed} PASS={_passed} FAIL={_failed}");
            string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../evidence/uat-round2/optimization"));Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path,"round2-tests.txt"),Log.ToString()); UnityEngine.Debug.Log(Log);
            if(_failed>0)throw new Exception("Round 2 invariant failure");
        }
    }
}
