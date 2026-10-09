using UnityEngine;

namespace BoxerP0
{
    public readonly struct PunchImpact
    {
        public readonly bool PlayerAttacker;
        public readonly long ReceiptId;
        public readonly PunchIntent Intent;
        public readonly CombatOutcome Outcome;
        public readonly string Region;
        public readonly Vector3 Point, Direction;
        public readonly float Quality;
        public PunchImpact(bool player, long receipt, PunchIntent intent, CombatOutcome outcome,
            string reason, Vector3 point, Vector3 direction, float quality)
        {
            PlayerAttacker=player;ReceiptId=receipt;Intent=intent;Outcome=outcome;
            Region=reason.Contains("BODY")?"BODY":outcome==CombatOutcome.Block?"GUARD":"HEAD";
            Point=point;Direction=direction.sqrMagnitude>1e-10f?direction.normalized:Vector3.forward;
            Quality=Mathf.Clamp01(quality);
        }
    }
    public sealed class BoxerFeedback : MonoBehaviour
    {
        private static BoxerFeedback _instance;
        private AudioSource _audio;
        private AudioClip _headHit,_bodyHit,_block,_swing;
        private PunchImpact _lastImpact,_opponentImpact;
        private float _lastImpactAt=-10f,_opponentImpactAt=-10f;
        public static int HitEvents { get; private set; }
        public static int BlockEvents { get; private set; }
        public static int MissEvents { get; private set; }
        public static int SwingEvents { get; private set; }
        public static string LastRegion => _instance==null?"NONE":_instance._lastImpact.Region;
        public static Vector3 LastContactPoint => _instance==null?Vector3.zero:_instance._lastImpact.Point;

        public bool AudioEnabled { get; private set; } = true;
        public bool HapticsEnabled { get; private set; } = true;

        private void Awake()
        {
            _instance = this;
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _headHit=CreateClip("Punch leather snap",0);_bodyHit=CreateClip("Punch body thud",1);
            _block=CreateClip("Punch glove block",2);_swing=CreateClip("Punch air whoosh",3);
            ResetImpacts();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            { AudioEnabled = !AudioEnabled; RingPresentation.Command(AudioEnabled ? 3 : 4); }
            if (Input.GetKeyDown(KeyCode.H)) HapticsEnabled = !HapticsEnabled;
        }

        public static void ResetImpacts()
        {
            HitEvents=BlockEvents=MissEvents=SwingEvents=0;
            if(_instance!=null) { _instance._lastImpact=default;_instance._opponentImpact=default;
                _instance._lastImpactAt=_instance._opponentImpactAt=-10f; }
        }
        public static void Swing(PunchIntent intent,float quality)
        {
            SwingEvents++;
            if(_instance!=null && _instance.AudioEnabled) _instance._audio.PlayOneShot(_instance._swing,.12f+.08f*Mathf.Clamp01(quality));
        }
        public static void Emit(PunchImpact impact)
        {
            // Only resolved scored receipts produce HIT/BLOCK feedback. Training is air practice.
            if(_instance==null || impact.ReceiptId==0 || impact.Outcome==CombatOutcome.None) return;
            if(impact.Outcome==CombatOutcome.Miss) { MissEvents++;return; }
            if(impact.Outcome==CombatOutcome.Hit) HitEvents++;else BlockEvents++;
            _instance._lastImpact=impact;_instance._lastImpactAt=Time.unscaledTime;
            if(impact.PlayerAttacker) { _instance._opponentImpact=impact;_instance._opponentImpactAt=Time.unscaledTime; }
            float weight=Mathf.Lerp(.75f,1f,(float)CombatBout.BaseDamage(impact.Intent)/12f);
            if(_instance.AudioEnabled) _instance._audio.PlayOneShot(impact.Outcome==CombatOutcome.Block?_instance._block:
                impact.Region=="BODY"?_instance._bodyHit:_instance._headHit,weight*Mathf.Lerp(.55f,.85f,impact.Quality));
#if UNITY_IOS || UNITY_ANDROID
            if (_instance.HapticsEnabled && impact.Outcome == CombatOutcome.Hit)
            {
                Handheld.Vibrate();
            }
#endif
        }

        public static float Pulse(float age,float duration) => age<0||age>=duration?0f:Mathf.Pow(1f-age/duration,2f);
        public static float PlayerGlovePulse(bool left)
        {
            if(_instance==null)return 0;
            var hit=_instance._lastImpact;
            if(!hit.PlayerAttacker || left==PunchLabels.IsRearHand(hit.Intent))return 0;
            return Pulse(Time.unscaledTime-_instance._lastImpactAt,.035f)*(hit.Outcome==CombatOutcome.Block?.4f:1f);
        }
        public static Quaternion OpponentReaction(bool head)
        {
            if(_instance==null)return Quaternion.identity;
            var hit=_instance._opponentImpact;
            if(hit.Outcome!=CombatOutcome.Hit || (head && hit.Region!="HEAD"))return Quaternion.identity;
            float pulse=Pulse(Time.unscaledTime-_instance._opponentImpactAt,head?.16f:.12f);
            Vector3 axis=Vector3.Cross(Vector3.up,hit.Direction);
            if(axis.sqrMagnitude<1e-8f)axis=Vector3.right;
            return Quaternion.AngleAxis((head?1.5f:.65f)*pulse*hit.Quality,axis.normalized);
        }
        public static Quaternion OpponentGuardReaction(Vector3 gloveCenter)
        {
            if(_instance==null)return Quaternion.identity;
            var hit=_instance._opponentImpact;
            if(hit.Outcome!=CombatOutcome.Block || Vector3.Distance(gloveCenter,hit.Point)>.27f)return Quaternion.identity;
            float pulse=Pulse(Time.unscaledTime-_instance._opponentImpactAt,.09f);
            Vector3 axis=Vector3.Cross(Vector3.up,hit.Direction);
            return axis.sqrMagnitude<1e-8f?Quaternion.identity:Quaternion.AngleAxis(2f*pulse,axis.normalized);
        }
        public static float[] Samples(int kind,int rate=44100)
        {
            float seconds=kind==1?.16f:kind==0?.12f:.09f;
            var data=new float[Mathf.CeilToInt(rate*seconds)];uint random=0x71A6Fu+(uint)kind;
            float filtered=0;
            for(int i=0;i<data.Length;i++)
            {
                float t=i/(float)rate;
                random=unchecked(1664525u*random+1013904223u);
                float noise=((random>>8)/16777215f)*2f-1f;
                filtered=.72f*filtered+.28f*noise;
                float attack=Mathf.Min(1f,t/.0015f),sample;
                if(kind==3) sample=filtered*Mathf.Sin(Mathf.PI*i/data.Length)*.35f;
                else
                {
                    float frequency=kind==1?72:kind==2?165:105;
                    float body=Mathf.Sin(2f*Mathf.PI*frequency*t)*Mathf.Exp(-t*(kind==1?23:38));
                    float snap=(noise-filtered)*Mathf.Exp(-t*(kind==2?130:210));
                    sample=attack*(body*(kind==2?.25f:.50f)+filtered*Mathf.Exp(-t*55)*.25f+snap*.28f);
                }
                data[i]=Mathf.Clamp(sample,-.95f,.95f);
            }
            return data;
        }
        static AudioClip CreateClip(string name,int kind)
        { var data=Samples(kind);var clip=AudioClip.Create(name,data.Length,1,44100,false);clip.SetData(data,0);return clip; }
        private void OnDestroy()
        {
            if(_instance==this)_instance=null;
            foreach(var clip in new[]{_headHit,_bodyHit,_block,_swing})if(clip!=null)Destroy(clip);
        }
    }
}

