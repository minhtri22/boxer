using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BoxerP0
{
    [DefaultExecutionOrder(320)]
    public sealed class Wave1WebAudit : MonoBehaviour
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern int Wave1AuditEnabled();
        [DllImport("__Internal")] private static extern void PublishWave1Snapshot(string payload);
        private bool _enabled;
        private float _next;
        private BoxerBootstrap _bootstrap;
        private Phase0Telemetry _telemetry;
        [Serializable] private sealed class Snapshot
        {
            public string scope="SYNTHETIC_DESKTOP_READONLY_NOT_HUMAN_UAT",screen,result,reason,trainingStage,trainingGuide;
            public bool trainingReady,tutorialSeen;
            public double seconds,playerHP,opponentHP,playerStamina,opponentStamina,playerCapacity,opponentCapacity;
            public bool playerEnabled,opponentEnabled,gameplayInput;
            public int players,opponents;
            public int playerHits,opponentHits,playerBlocks,opponentBlocks,playerMisses,opponentMisses;
            public string playerPhase,opponentPhase,opponentIntent,playerIntent,playerReason,combatLog;
            public double playerQuality,opponentQuality,distance;
            public uint opponentAttacks;
            public bool opponentBody;
            public float playerX,playerZ,opponentX,opponentZ,moveX,moveY;
            public int povArms,trainingBlurWidth,trainingBlurHeight;
            public bool trainingGlass;
            public Vector3 leftElbowViewport,rightElbowViewport;
            public string punchFeel,lastImpactRegion;
            public string coachModule;
            public int coachCompletionMask;
            public bool trainingFromCoach,coachArtLoaded;
            public float coachInfoContentHeight;
            public uint acceptedPunches,rejectedGestures,rejectedBusyPunches;
            public float contactAgeMs,recoveryAgeMs,releaseToAcceptMs,gestureDurationMs;
            public int impactHits,impactBlocks,impactMisses,swings;
            public Vector3 contactPoint;
        }
        private readonly Snapshot _snapshot=new();
        private void Start()
        { _enabled=Wave1AuditEnabled()==1;_bootstrap=GetComponent<BoxerBootstrap>();_telemetry=FindAnyObjectByType<Phase0Telemetry>(); }
        private void LateUpdate()
        {
            if(!_enabled||Time.unscaledTime<_next)return;_next=Time.unscaledTime+.05f;
            var b=_telemetry.Bout;_snapshot.screen=_bootstrap.Flow.Screen.ToString();_snapshot.result=b.Result;_snapshot.reason=b.EndReason;
            _snapshot.seconds=b.Seconds;_snapshot.playerHP=b.Player.HP;_snapshot.opponentHP=b.Opponent.HP;
            _snapshot.playerStamina=b.Player.Stamina;_snapshot.opponentStamina=b.Opponent.Stamina;
            _snapshot.playerCapacity=b.Player.Capacity;_snapshot.opponentCapacity=b.Opponent.Capacity;
            _snapshot.playerEnabled=_telemetry.Player.CombatEnabled;_snapshot.opponentEnabled=_telemetry.Opponent.CombatEnabled;
            _snapshot.gameplayInput=_telemetry.InputSource.GameplayInput;
            _snapshot.trainingStage=_bootstrap.TrainingToken;_snapshot.trainingReady=_bootstrap.TrainingReady;
            _snapshot.trainingGuide=TrainingGestureGuide.Cue(_bootstrap.TrainingToken,Time.unscaledTime);
            _snapshot.tutorialSeen=_bootstrap.Flow.TutorialSeen;
            _snapshot.coachModule=_bootstrap.SelectedCoachModule.ToString();_snapshot.coachCompletionMask=_bootstrap.CoachCompletionMask;
            _snapshot.trainingFromCoach=_bootstrap.Flow.TrainingFromCoach;_snapshot.coachArtLoaded=_bootstrap.GetComponent<ProductScreens>().CoachArtLoaded;
            _snapshot.coachInfoContentHeight=_bootstrap.GetComponent<ProductScreens>().CoachInfoContentHeight;
            _snapshot.players=FindObjectsByType<PlayerBoxer>(FindObjectsSortMode.None).Length;
            _snapshot.opponents=FindObjectsByType<OpponentBoxer>(FindObjectsSortMode.None).Length;
            _snapshot.playerHits=_telemetry.PlayerHits;_snapshot.opponentHits=_telemetry.OpponentHits;
            _snapshot.playerBlocks=_telemetry.PlayerBlocks;_snapshot.opponentBlocks=_telemetry.OpponentBlocks;
            _snapshot.playerMisses=_telemetry.PlayerMisses;_snapshot.opponentMisses=_telemetry.OpponentMisses;
            _snapshot.playerPhase=_telemetry.Player.CurrentPhase.ToString();_snapshot.opponentPhase=_telemetry.Opponent.CurrentPhase.ToString();
            _snapshot.playerIntent=_telemetry.Player.CurrentIntent.ToString();_snapshot.opponentIntent=_telemetry.Opponent.CurrentIntent.ToString();
            _snapshot.playerReason=_telemetry.Player.LastResolutionReason;_snapshot.combatLog=_telemetry.RecentCombatLog;
            _snapshot.playerQuality=b.Player.Quality;_snapshot.opponentQuality=b.Opponent.Quality;
            _snapshot.distance=Vector3.Distance(_telemetry.Player.transform.position,_telemetry.Opponent.transform.position);
            _snapshot.opponentAttacks=_telemetry.Opponent.AttackEventCount;
            _snapshot.opponentBody=_telemetry.Opponent.BodyAttack;
            _snapshot.playerX=_telemetry.Player.transform.position.x;_snapshot.playerZ=_telemetry.Player.transform.position.z;
            _snapshot.opponentX=_telemetry.Opponent.transform.position.x;_snapshot.opponentZ=_telemetry.Opponent.transform.position.z;
            _snapshot.moveX=_telemetry.InputSource.MovementIntent.x;_snapshot.moveY=_telemetry.InputSource.MovementIntent.y;
            _snapshot.punchFeel=PunchMotionProfile.Mode.ToString();_snapshot.acceptedPunches=_telemetry.Player.AcceptedPunches;
            _snapshot.contactAgeMs=_telemetry.Player.LastContactAgeMs;_snapshot.recoveryAgeMs=_telemetry.Player.LastRecoveryAgeMs;
            _snapshot.releaseToAcceptMs=_telemetry.Player.LastAcceptedReleaseLatencyMs;_snapshot.rejectedBusyPunches=_telemetry.Player.RejectedBusyPunches;
            _snapshot.gestureDurationMs=_telemetry.InputSource.LastGestureDurationMs;_snapshot.rejectedGestures=_telemetry.InputSource.RejectedGestures;
            _snapshot.impactHits=BoxerFeedback.HitEvents;_snapshot.impactBlocks=BoxerFeedback.BlockEvents;
            _snapshot.impactMisses=BoxerFeedback.MissEvents;_snapshot.swings=BoxerFeedback.SwingEvents;
            _snapshot.lastImpactRegion=BoxerFeedback.LastRegion;_snapshot.contactPoint=BoxerFeedback.LastContactPoint;
            var camera=FindFirstObjectByType<Camera>();
            var glass=camera.GetComponent<TrainingGlassBackground>();
            _snapshot.trainingGlass=glass!=null&&glass.enabled;
            _snapshot.trainingBlurWidth=glass==null?0:glass.BufferWidth;
            _snapshot.trainingBlurHeight=glass==null?0:glass.BufferHeight;
            var arms=FindObjectsByType<PlayerPOVArmVisual>(FindObjectsSortMode.None);
            _snapshot.povArms=arms.Length;
            foreach(var arm in arms)
                if(arm.name.Contains("Left"))_snapshot.leftElbowViewport=camera.WorldToViewportPoint(arm.Elbow);
                else _snapshot.rightElbowViewport=camera.WorldToViewportPoint(arm.Elbow);
            PublishWave1Snapshot(JsonUtility.ToJson(_snapshot));
        }
#endif
    }
}
