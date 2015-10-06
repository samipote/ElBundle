namespace ElVeigar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using Collision = LeagueSharp.Common.Collision;
    using Color = System.Drawing.Color;

    internal static class Entry
    {
        #region Static Fields

        public static SpellSlot Ignite;

        public static Orbwalking.Orbwalker Orbwalker;

        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>
                                                             {
                                                                 { Spells.Q, new Spell(SpellSlot.Q, 650f) },
                                                                 { Spells.W, new Spell(SpellSlot.W, 900f) },
                                                                 { Spells.E, new Spell(SpellSlot.E, 650f) },
                                                                 { Spells.R, new Spell(SpellSlot.R, 650f) }
                                                             };

        private static float radius;

        private static float radiusSqr;

        private static float widthSqr;

        #endregion

        #region Enums

        internal enum Spells
        {
            Q,

            W,

            E,

            R
        }

        #endregion

        #region Public Properties

        public static Obj_AI_Hero Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }

        public static string ScriptVersion
        {
            get
            {
                return typeof(ElVeigar).Assembly.GetName().Version.ToString();
            }
        }

        #endregion

        #region Public Methods and Operators

        public static float IgniteDamage(Obj_AI_Hero target)
        {
            if (Ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
            {
                return 0f;
            }
            return (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        public static void OnLoad(EventArgs args)
        {
            try
            {
                Ignite = Player.GetSpellSlot("summonerdot");
                Notifications.AddNotification(string.Format("ElVeigar by jQuery v{0}", ScriptVersion), 10000);
                MenuInit.Initialize();
                Game.OnUpdate += OnUpdate;
                Drawing.OnDraw += OnDraw;
                Interrupter2.OnInterruptableTarget += Interrupter_OnPosibleToInterrupt;

                spells[Spells.Q].SetTargetted(0.25f, 1500);
                spells[Spells.W].SetSkillshot(1.25f, 225, float.MaxValue, false, SkillshotType.SkillshotCircle);
                spells[Spells.R].SetTargetted(0.25f, 1400);
                spells[Spells.E].SetSkillshot(0.8f, 350f, float.MaxValue, false, SkillshotType.SkillshotCircle);

                spells[Spells.E].Width = 700;
                widthSqr = spells[Spells.E].Width * spells[Spells.E].Width;
                radius = spells[Spells.E].Width / 2;
                radiusSqr = radius * radius;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        #endregion

        #region Methods

        private static void AutoW()
        {
            if (MenuInit.IsActive("ElVeigar.Combo.W.Stun"))
            {
                var target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            h =>
                            h.IsValidTarget(spells[Spells.W].Range) && h.GetStunDuration() >= spells[Spells.W].Delay);

                if (target.IsValidTarget() && target != null)
                {
                    spells[Spells.W].Cast(target.Position);
                }
            }
        }

        private static float ComboDamage(this Obj_AI_Base target)
        {
            return 0;
        }

        // Later, :cat_lazy: atm.
        private static void ComboModes(this Obj_AI_Base target)
        {
        }

        private static void DoCombo()
        {
            try
            {
                var target = TargetSelector.GetTarget(spells[Spells.E].Range, TargetSelector.DamageType.Magical);
                if (target == null || !target.IsValidTarget())
                {
                    return;
                }

                if (spells[Spells.E].IsReady() && MenuInit.IsActive("ElVeigar.Combo.E")
                    && spells[Spells.E].IsInRange(target))
                {
                    var castPosition = GetCageCastPosition(target);
                    if (castPosition != null)
                    {
                        if (!IsInvulnerable(target))
                        {
                            spells[Spells.E].Cast((Vector3)castPosition);
                        }
                    }
                }

                var prediction = spells[Spells.Q].GetPrediction(target);

                if (spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target)
                    && MenuInit.IsActive("ElVeigar.Combo.Q"))
                {
                    if (prediction.Hitchance >= HitChance.High && prediction.CollisionObjects.Count == 0)
                    {
                        spells[Spells.Q].Cast(target.Position);
                    }
                }

                if (spells[Spells.R].IsReady() && spells[Spells.R].IsInRange(target)
                    && MenuInit.IsActive("ElVeigar.Combo.R"))
                {
                    /* if (spells[Spells.R].GetDamage(target)
                         + (target.TotalMagicalDamage * 0.8) > target.Health)*/
                    if (GetExecuteDamage(target) > target.Health)
                    {
                        if (!IsInvulnerable(target))
                        {
                            spells[Spells.R].CastOnUnit(target);
                        }
                    }
                }

                if (MenuInit.IsActive("ElVeigar.Combo.Use.Ignite") && Player.Distance(target) <= 600
                    && IgniteDamage(target) >= target.Health)
                {
                    Player.Spellbook.CastSpell(Ignite, target);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        private static void DoHarass()
        {
            try
            {
                var target = TargetSelector.GetTarget(spells[Spells.E].Range, TargetSelector.DamageType.Magical);
                if (target == null || !target.IsValidTarget())
                {
                    return;
                }

                if (Player.ManaPercent < MenuInit.Menu.Item("ElVeigar.Harass.Mana").GetValue<Slider>().Value)
                {
                    return;
                }

                switch (MenuInit.IsListActive("Harass.Mode").SelectedIndex)
                {
                    case 0: // E - Q - W
                        if (spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
                        {
                            var castPosition = GetCageCastPosition(target);
                            if (castPosition != null)
                            {
                                if (!IsInvulnerable(target))
                                {
                                    spells[Spells.E].Cast((Vector3)castPosition);
                                }
                            }
                        }

                        var prediction = spells[Spells.Q].GetPrediction(target);

                        if (spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
                        {
                            if (prediction.Hitchance >= HitChance.High && prediction.CollisionObjects.Count == 0)
                            {
                                spells[Spells.Q].Cast(target.Position);
                            }
                        }
                        break;

                    case 1: //E - W
                        if (spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
                        {
                            var castPosition = GetCageCastPosition(target);
                            if (castPosition != null)
                            {
                                if (!IsInvulnerable(target))
                                {
                                    spells[Spells.E].Cast((Vector3)castPosition);
                                }
                            }
                        }
                        break;

                    case 2: // Q
                        var predictionQ = spells[Spells.Q].GetPrediction(target);

                        if (spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
                        {
                            if (predictionQ.Hitchance >= HitChance.High && predictionQ.CollisionObjects.Count == 0)
                            {
                                spells[Spells.Q].Cast(target.Position);
                            }
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        private static void DoJungleClear()
        {
            try
            {
                if (Player.ManaPercent < MenuInit.Menu.Item("ElVeigar.JungleClear.Mana").GetValue<Slider>().Value)
                {
                    return;
                }

                var jungleWMinions = MinionManager.GetMinions(
                    Player.Position,
                    spells[Spells.W].Range,
                    MinionTypes.All,
                    MinionTeam.Neutral,
                    MinionOrderTypes.MaxHealth);
                var minionerinos2 = (from minions in jungleWMinions select minions.Position.To2D()).ToList();

                var minionPosition =
                    MinionManager.GetBestCircularFarmLocation(
                        minionerinos2,
                        spells[Spells.W].Width,
                        spells[Spells.W].Range).Position;

                if (minionPosition.Distance(Player.Position.To2D()) < spells[Spells.W].Range
                    && MinionManager.GetBestCircularFarmLocation(
                        minionerinos2,
                        spells[Spells.W].Width,
                        spells[Spells.W].Range).MinionsHit > 0)
                {
                    spells[Spells.W].Cast(minionPosition);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        private static void DoLaneClear()
        {
            try
            {
                if (Player.ManaPercent < MenuInit.Menu.Item("ElVeigar.LaneClear.Mana").GetValue<Slider>().Value)
                {
                    return;
                }

                var minions =
                    MinionManager.GetBestCircularFarmLocation(
                        MinionManager.GetMinions(spells[Spells.W].Range).Select(x => x.ServerPosition.To2D()).ToList(),
                        spells[Spells.W].Width,
                        spells[Spells.W].Range);

                if (MenuInit.IsActive("ElVeigar.LaneClear.W")
                    && minions.MinionsHit >= MenuInit.Menu.Item("ElVeigar.LaneClear.W.Minions").GetValue<Slider>().Value)
                {
                    spells[Spells.W].Cast(minions.Position);
                }

                if (MenuInit.IsActive("ElVeigar.LaneClear.Q"))
                {
                    QStack();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        private static Vector3? GetCageCastPosition(Obj_AI_Hero target)
        {
            var prediction = Prediction.GetPrediction(target, 0.2f);

            if (prediction.Hitchance < HitChance.High)
            {
                return null;
            }

            var nearTargets =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        h =>
                        h.NetworkId != target.NetworkId && h.IsValidTarget(spells[Spells.E].Range + radius)
                        && h.Distance(target, true) < widthSqr);

            foreach (var target2 in nearTargets)
            {
                var prediction2 = Prediction.GetPrediction(target2, 0.2f);

                if (prediction2.Hitchance < HitChance.High
                    || prediction.UnitPosition.Distance(prediction2.UnitPosition, true) > widthSqr)
                {
                    continue;
                }

                var distanceSqr = prediction.UnitPosition.Distance(prediction2.UnitPosition, true);
                var distance = Math.Sqrt(distanceSqr);
                var middlePoint = (prediction.UnitPosition + prediction2.UnitPosition) / 2;
                var perpendicular =
                    (prediction.UnitPosition - prediction2.UnitPosition).Normalized().To2D().Perpendicular();

                var length = (float)Math.Sqrt(radiusSqr - distanceSqr);
                var castPosition = middlePoint.To2D() + perpendicular * length;

                if (castPosition.Distance(Player.Position.To2D(), true) > radiusSqr)
                {
                    castPosition = middlePoint.To2D() - perpendicular * length;
                }

                if (castPosition.Distance(Player.Position.To2D(), true) > radiusSqr)
                {
                    continue;
                }

                return castPosition.To3D();
            }

            return prediction.UnitPosition.Extend(Player.Position, radius);
        }

        private static float GetExecuteDamage(Obj_AI_Base target)
        {
            if (spells[Spells.R].IsReady())
            {
                return
                    (float)
                    Player.CalcDamage(
                        target,
                        Damage.DamageType.Magical,
                        new float[] { 250, 375, 500 }[spells[Spells.R].Level - 1] + 1.2f * Player.TotalMagicalDamage()
                        + (target.TotalMagicalDamage * 0.8f));
            }

            return 0;
        }

        private static float GetStunDuration(this Obj_AI_Base target)
        {
            return
                target.Buffs.Where(
                    b =>
                    b.IsActive && Game.Time < b.EndTime
                    && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun
                        || b.Type == BuffType.Suppression || b.Type == BuffType.Snare))
                    .Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time;
        }

        private static void Interrupter_OnPosibleToInterrupt(
            Obj_AI_Hero unit,
            Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (!MenuInit.IsActive("Misc.Interrupt"))
            {
                return;
            }

            if (Player.Distance(unit.Position) < spells[Spells.E].Range && spells[Spells.E].IsReady())
            {
                var castPosition = GetCageCastPosition(unit);
                if (castPosition != null)
                {
                    if (!IsInvulnerable(unit))
                    {
                        spells[Spells.E].Cast((Vector3)castPosition);
                    }
                }
            }
        }

        private static bool IsInvulnerable(this Obj_AI_Base target)
        {
            return Helpers.IgnoreList.Any(buff => target.HasBuff(buff.DisplayName) || target.HasBuff(buff.Name));
        }

        private static void KillstealHandler()
        {
            try
            {
                if (MenuInit.IsActive("ElVeigar.Combo.KS.R"))
                {
                    foreach (var target in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (target.IsEnemy && target.IsValidTarget())
                        {
                            var predictionQ = spells[Spells.Q].GetPrediction(target);
                            if (spells[Spells.Q].GetDamage(target) > target.Health
                                && MenuInit.IsActive("ElVeigar.Combo.KS.Q"))
                            {
                                if (predictionQ.Hitchance >= HitChance.High && predictionQ.CollisionObjects.Count == 0
                                    && spells[Spells.Q].IsInRange(target))
                                {
                                    spells[Spells.Q].Cast(target.Position);
                                }
                            }

                            var predictionW = spells[Spells.W].GetPrediction(target);
                            if (spells[Spells.W].GetDamage(target) > target.Health
                                && MenuInit.IsActive("ElVeigar.Combo.KS.W"))
                            {
                                if (predictionW.Hitchance >= HitChance.High)
                                {
                                    spells[Spells.W].Cast(target.ServerPosition); // ehhhhhhhhhhhhh
                                }
                            }

                            var getEnemies = MenuInit.Menu.Item("ElVeigar.KS.R.On" + target.CharData.BaseSkinName);
                            if (getEnemies != null && getEnemies.GetValue<bool>())
                            {
                                if (GetExecuteDamage(target) > target.Health)
                                {
                                    spells[Spells.R].CastOnUnit(target);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (MenuInit.IsActive("Misc.Drawings.Off"))
            {
                return;
            }

            if (MenuInit.Menu.Item("Misc.Drawings.Q").GetValue<Circle>().Active)
            {
                if (spells[Spells.Q].Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.Q].Range, Color.White);
                }
            }

            if (MenuInit.Menu.Item("Misc.Drawings.W").GetValue<Circle>().Active)
            {
                if (spells[Spells.W].Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.W].Range, Color.White);
                }
            }

            if (MenuInit.Menu.Item("Misc.Drawings.E").GetValue<Circle>().Active)
            {
                if (spells[Spells.E].Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.E].Range, Color.White);
                }
            }

            if (MenuInit.Menu.Item("Misc.Drawings.R").GetValue<Circle>().Active)
            {
                if (spells[Spells.R].Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.R].Range, Color.White);
                }
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            try
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        DoCombo();
                        break;

                    case Orbwalking.OrbwalkingMode.LaneClear:
                        DoLaneClear();
                        DoJungleClear();
                        break;

                    case Orbwalking.OrbwalkingMode.Mixed:
                        DoHarass();
                        break;

                    case Orbwalking.OrbwalkingMode.LastHit:
                        QStack();
                        break;
                }

                if (MenuInit.Menu.Item("ElVeigar.Stack.Q").GetValue<KeyBind>().Active)
                {
                    QStack();
                }

                AutoW();
                KillstealHandler();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        private static List<Obj_AI_Base> QGetCollisionMinions(Obj_AI_Hero source, Vector3 targetposition)
        {
            var input = new PredictionInput
                            {
                                Unit = source, Radius = spells[Spells.Q].Width, Delay = spells[Spells.Q].Delay,
                                Speed = spells[Spells.Q].Speed
                            };

            input.CollisionObjects[0] = CollisionableObjects.Minions;

            return
                Collision.GetCollision(new List<Vector3> { targetposition }, input)
                    .OrderBy(obj => obj.Distance(source))
                    .ToList();
        }

        private static void QStack()
        {
            if (Player.IsRecalling() || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo
                || Player.ManaPercent < MenuInit.Menu.Item("ElVeigar.Stack.Mana").GetValue<Slider>().Value)
            {
                return;
            }

            var minions =
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(
                        m =>
                        m.IsMinion && m.IsEnemy && Player.Distance(m.Position) <= spells[Spells.Q].Range
                        && m.Health < ((Player.GetSpellDamage(m, SpellSlot.Q)))
                        && HealthPrediction.GetHealthPrediction(
                            m,
                            (int)(Player.Distance(m) / spells[Spells.Q].Speed),
                            (int)(spells[Spells.Q].Delay * 1000 + Game.Ping / 2)) > 0);

            foreach (var minion in minions.Where(x => x.Health <= spells[Spells.Q].GetDamage(x)))
            {
                var killcount = 0;

                foreach (
                    var colminion in
                        QGetCollisionMinions(
                            Player,
                            Player.ServerPosition.Extend(minion.ServerPosition, spells[Spells.Q].Range)))
                {
                    if (colminion.Health <= spells[Spells.Q].GetDamage(colminion))
                    {
                        killcount++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (killcount >= MenuInit.Menu.Item("ElVeigar.Stack.Q2").GetValue<Slider>().Value)
                {
                    if (!Player.IsWindingUp)
                    {
                        spells[Spells.Q].Cast(minion.ServerPosition);
                        break;
                    }
                }
            }
        }

        #endregion
    }
}