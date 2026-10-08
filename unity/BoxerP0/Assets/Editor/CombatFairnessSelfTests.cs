using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class CombatFairnessSelfTests
    {
        private static readonly StringBuilder Log=new();
        private static int _pass,_fail;
        private static void Check(bool ok,string label)
        { if(ok)_pass++;else _fail++;Log.AppendLine((ok?"PASS ":"FAIL ")+label); }
        public static void Run()
        {
            _pass=_fail=0;Log.Clear();
            Log.AppendLine("SCOPE=SOLVED_GEOMETRY_AND_DETERMINISTIC_RULES_NOT_INPUT_OR_HUMAN_UAT");
            uint rng=0xC0FFEEu,replay=rng;int[] counts=new int[4];bool brokeOldLoop=false;
            for(int i=0;i<128;i++)
            {
                int selection=OpponentAttackRandom.NextIndex(ref rng,4);
                int again=OpponentAttackRandom.NextIndex(ref replay,4);
                Check(selection==again&&selection>=0&&selection<4,"AI reproducible bounded choice "+i);
                counts[selection]++;brokeOldLoop|=selection==0||selection==2;
                // Same single gap draw between real attacks; preserve the original seed.
                rng=unchecked(1664525u*rng+1013904223u);replay=unchecked(1664525u*replay+1013904223u);
            }
            for(int i=0;i<4;i++)Check(counts[i]>0,"AI choice "+i+" occurs with interleaved gap draw, count="+counts[i]);
            Check(brokeOldLoop,"AI no longer trapped in alternating head/body Cross");
            try { OpponentAttackRandom.NextIndex(ref rng,0);Check(false,"invalid random count rejected"); }
            catch(ArgumentOutOfRangeException){Check(true,"invalid random count rejected");}
            var defender=new Round2Frame{Rotation=Quaternion.identity,Phase=ActionPhase.Guard,Intent=PunchIntent.None};
            Vector3 guard=defender.Target(0),a=guard+Vector3.forward*2,b=guard-Vector3.forward*2;
            foreach(int mode in new[]{0,1,2})
            {
                int hit=Round2CombatRig.SweepTargets(a,b,defender,defender,true,mode,out float fraction);
                Check(hit==0&&fraction>0&&fraction<1,"real intersecting glove blocks target mode "+mode);
            }
            var mirrored=defender;mirrored.Rotation=Quaternion.Euler(0,180,0);mirrored.Position=new Vector3(1,0,1);
            int mirroredHit=Round2CombatRig.SweepTargets(mirrored.World(a),mirrored.World(b),mirrored,mirrored,true,0,out _);
            Check(mirroredHit==0,"guard contact invariant under actor rotation and translation");
            a=new Vector3(0,1.16f,2);b=new Vector3(0,1.16f,-2);
            int body=Round2CombatRig.SweepTargets(a,b,defender,defender,true,2,out _);
            Check(body>=3,"body path below a non-intersecting high guard still hits body");
            var moving=defender;moving.Position=Vector3.right*3;
            Check(Round2CombatRig.SweepTargets(a,b,moving,moving,true,0,out _)<0,"non-contact is MISS, not proximity damage");
            foreach(var intent in new[]{PunchIntent.Jab,PunchIntent.Cross})
            {
                foreach(float distance in new[]{Round2Motion.EngagementDistance-.03f,Round2Motion.EngagementDistance,Round2Motion.EngagementDistance+.03f})
                    Check(SampleHead(intent,distance),intent+" can physically reach exposed head in engagement band "+distance);
                Check(!SampleHead(intent,.955f),intent+" reproduces old out-of-reach exposed head at .955m");
                Check(!SampleHead(intent,1.5f),intent+" long-range punch remains MISS");
            }
            foreach(var intent in new[]{PunchIntent.LeadHook,PunchIntent.RearHook,PunchIntent.LeadUppercut,PunchIntent.RearUppercut,PunchIntent.LeadOverhand,PunchIntent.RearOverhand})
                Check(SampleHead(intent,.69f),intent+" physically reaches exposed head at close range");
            Log.AppendLine($"TOTAL={_pass+_fail} PASS={_pass} FAIL={_fail}");
            string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../evidence/wave1/combat-v3"));Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir,"combat-tests.txt"),Log.ToString());Debug.Log(Log);
            if(_fail!=0)throw new Exception("Combat fairness geometry invariant failure");
            Wave1SelfTests.Run();
        }
        private static bool SampleHead(PunchIntent intent,float distance)
        {
            bool left=!PunchLabels.IsRearHand(intent);Vector3 endpoint=Round2Motion.Endpoint(intent,"NEUTRAL",distance);
            var defender=new Round2Frame{Position=Vector3.forward*distance,Rotation=Quaternion.Euler(0,180,0),Phase=ActionPhase.Guard};
            Vector3 a=Round2Motion.Sample(left,intent,ActionPhase.Extend,0,endpoint).Wrist;
            for(int i=1;i<=128;i++)
            {
                Vector3 b=Round2Motion.Sample(left,intent,ActionPhase.Extend,i/128f,endpoint).Wrist;
                int hit=Round2CombatRig.SweepTargets(a,b,defender,defender,false,1,out _);
                if(hit==2)return true;a=b;
            }
            return false;
        }
    }
}
