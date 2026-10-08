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
            public string scope="SYNTHETIC_DESKTOP_READONLY_NOT_HUMAN_UAT",screen,result,reason,trainingStage;
            public bool trainingReady,tutorialSeen;
            public double seconds,playerHP,opponentHP,playerStamina,opponentStamina,playerCapacity,opponentCapacity;
            public bool playerEnabled,opponentEnabled,gameplayInput;
            public int players,opponents;
        }
        private readonly Snapshot _snapshot=new();
        private void Start()
        { _enabled=Wave1AuditEnabled()==1;_bootstrap=GetComponent<BoxerBootstrap>();_telemetry=FindAnyObjectByType<Phase0Telemetry>(); }
        private void LateUpdate()
        {
            if(!_enabled||Time.unscaledTime<_next)return;_next=Time.unscaledTime+.25f;
            var b=_telemetry.Bout;_snapshot.screen=_bootstrap.Flow.Screen.ToString();_snapshot.result=b.Result;_snapshot.reason=b.EndReason;
            _snapshot.seconds=b.Seconds;_snapshot.playerHP=b.Player.HP;_snapshot.opponentHP=b.Opponent.HP;
            _snapshot.playerStamina=b.Player.Stamina;_snapshot.opponentStamina=b.Opponent.Stamina;
            _snapshot.playerCapacity=b.Player.Capacity;_snapshot.opponentCapacity=b.Opponent.Capacity;
            _snapshot.playerEnabled=_telemetry.Player.CombatEnabled;_snapshot.opponentEnabled=_telemetry.Opponent.CombatEnabled;
            _snapshot.gameplayInput=_telemetry.InputSource.GameplayInput;
            _snapshot.trainingStage=_bootstrap.TrainingToken;_snapshot.trainingReady=_bootstrap.TrainingReady;
            _snapshot.tutorialSeen=_bootstrap.Flow.TutorialSeen;
            _snapshot.players=FindObjectsByType<PlayerBoxer>(FindObjectsSortMode.None).Length;
            _snapshot.opponents=FindObjectsByType<OpponentBoxer>(FindObjectsSortMode.None).Length;
            PublishWave1Snapshot(JsonUtility.ToJson(_snapshot));
        }
#endif
    }
}
