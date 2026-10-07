using System;

namespace BoxerP0
{
    public readonly struct AttackReceipt
    {
        public readonly long Id;
        public readonly PunchIntent Intent;
        public readonly double Quality;
        public double RecoveryFactor => 1 + 0.75 * (1 - Quality);
        public AttackReceipt(long id, PunchIntent intent, double quality)
        { Id = id; Intent = intent; Quality = quality; }
        public static AttackReceipt Unscored(PunchIntent intent) => new(0, intent, 1);
    }

    public sealed class FighterVitals
    {
        public double HP { get; private set; } = 100;
        public double Stamina { get; private set; } = 100;
        public double Capacity { get; private set; } = 100;
        public double Quality => 0.4 + 0.6 * (0.6 * Capacity / 100 + 0.4 * Stamina / 100);
        internal void Reset() { HP = Stamina = Capacity = 100; }
        internal void Spend(double capacity, double stamina)
        { Capacity = Clamp(Capacity - capacity); Stamina = Clamp(Stamina - stamina); }
        internal void Damage(double hp, bool body)
        { HP = Clamp(HP - hp); if (body) Stamina = Clamp(Stamina - hp * 0.35); }
        internal void Tick(double dt, ActionPhase phase, double movement)
        {
            double capacityRate = phase == ActionPhase.Guard ? 18 : phase == ActionPhase.Recover ? 6 : 0;
            double staminaRate = phase == ActionPhase.Guard ? 4 : phase == ActionPhase.Recover ? 1 : 0;
            Capacity = Clamp(Capacity + capacityRate * dt);
            Stamina = Clamp(Stamina + (staminaRate - 2 * Math.Max(0, Math.Min(1, movement))) * dt);
        }
        private static double Clamp(double x) => Math.Max(0, Math.Min(100, x));
    }

    // Pure game model. No Unity time, input, UI, RNG or physics dependencies.
    public sealed class CombatBout
    {
        public FighterVitals Player { get; } = new();
        public FighterVitals Opponent { get; } = new();
        public bool Active { get; private set; }
        public bool Ended { get; private set; }
        public string Result { get; private set; } = "PENDING";
        public string EndReason { get; private set; } = "NONE";
        public double Seconds { get; private set; }
        private long _serial, _playerPending, _opponentPending;
        private AttackReceipt _playerAttack, _opponentAttack;

        public void Reset()
        {
            Player.Reset(); Opponent.Reset(); Active = Ended = false;
            Result = "PENDING"; EndReason = "NONE"; Seconds = 0;
            _playerPending = _opponentPending = 0;
            // Serial intentionally survives reset: stale previous-bout receipts stay invalid.
        }
        public void Start() { Reset(); Active = true; Result = "IN_PROGRESS"; }
        public static double Cost(PunchIntent intent) => intent switch
        {
            PunchIntent.Jab => 12, PunchIntent.Cross => 16,
            PunchIntent.LeadHook or PunchIntent.RearHook => 18,
            PunchIntent.LeadUppercut or PunchIntent.RearUppercut => 20,
            PunchIntent.LeadOverhand or PunchIntent.RearOverhand => 22,
            _ => 0
        };
        public static double BaseDamage(PunchIntent intent) => intent switch
        {
            PunchIntent.Jab => 6, PunchIntent.Cross => 9,
            PunchIntent.LeadHook or PunchIntent.RearHook => 8,
            PunchIntent.LeadUppercut or PunchIntent.RearUppercut => 10,
            PunchIntent.LeadOverhand or PunchIntent.RearOverhand => 12,
            _ => 0
        };
        public AttackReceipt Accept(bool player, PunchIntent intent)
        {
            if (!Active || Cost(intent) <= 0) return AttackReceipt.Unscored(intent);
            FighterVitals attacker = player ? Player : Opponent;
            var receipt = new AttackReceipt(++_serial, intent, attacker.Quality);
            attacker.Spend(Cost(intent), Cost(intent) * 0.2);
            if (player) { _playerPending = receipt.Id; _playerAttack = receipt; }
            else { _opponentPending = receipt.Id; _opponentAttack = receipt; }
            return receipt;
        }
        public bool Resolve(bool player, AttackReceipt receipt, CombatOutcome outcome, bool body)
        {
            long pending = player ? _playerPending : _opponentPending;
            if (!Active || receipt.Id == 0 || pending != receipt.Id ||
                (outcome != CombatOutcome.Hit && outcome != CombatOutcome.Block && outcome != CombatOutcome.Miss)) return false;
            // Read the stored receipt, not caller-controlled quality/intent.
            AttackReceipt accepted = player ? _playerAttack : _opponentAttack;
            if (player) _playerPending = 0; else _opponentPending = 0;
            FighterVitals defender = player ? Opponent : Player;
            if (outcome == CombatOutcome.Hit) defender.Damage(BaseDamage(accepted.Intent) * accepted.Quality, body);
            else if (outcome == CombatOutcome.Block) defender.Spend(Cost(accepted.Intent) * 0.25, 0);
            if (defender.HP <= 0) End(player ? "PLAYER_WIN" : "OPPONENT_WIN", "KO");
            return true;
        }
        public void Tick(double dt, ActionPhase playerPhase, ActionPhase opponentPhase, double playerMove, double opponentMove)
        {
            if (double.IsNaN(dt) || double.IsInfinity(dt) || dt < 0) throw new ArgumentOutOfRangeException(nameof(dt));
            if (double.IsNaN(playerMove) || double.IsInfinity(playerMove) || double.IsNaN(opponentMove) || double.IsInfinity(opponentMove))
                throw new ArgumentOutOfRangeException(nameof(playerMove));
            if (!Active) return;
            double step = Math.Min(dt, Math.Max(0, 45 - Seconds));
            Player.Tick(step, playerPhase, playerMove); Opponent.Tick(step, opponentPhase, opponentMove);
            Seconds += step;
            if (Seconds >= 45 - 1e-9) FinishTimeout();
        }
        public void FinishTimeout()
        {
            if (!Active) return;
            double delta = Player.HP - Opponent.HP;
            End(Math.Abs(delta) <= 0.0001 ? "DRAW" : delta > 0 ? "PLAYER_WIN" : "OPPONENT_WIN", "POINTS");
        }
        private void End(string result, string reason)
        { Active = false; Ended = true; Result = result; EndReason = reason; }
    }

    public enum ProductScreen { Home, Preview, Onboarding, Fight, Result }
    public sealed class ProductFlow
    {
        public ProductScreen Screen { get; private set; } = ProductScreen.Home;
        public bool TutorialSeen { get; private set; }
        public bool Preview() { if (Screen != ProductScreen.Home && Screen != ProductScreen.Result) return false; Screen = ProductScreen.Preview; return true; }
        public bool Begin(bool tutorial = false)
        {
            if (Screen != ProductScreen.Preview && Screen != ProductScreen.Result && !(tutorial && Screen == ProductScreen.Home)) return false;
            Screen = tutorial || !TutorialSeen ? ProductScreen.Onboarding : ProductScreen.Fight; return true;
        }
        public void TutorialFinished() { TutorialSeen = true; Screen = ProductScreen.Fight; }
        public void Finish() { if (Screen == ProductScreen.Fight) Screen = ProductScreen.Result; }
        public void Home() { Screen = ProductScreen.Home; }
        public bool Gameplay => Screen == ProductScreen.Onboarding || Screen == ProductScreen.Fight;
    }
}
