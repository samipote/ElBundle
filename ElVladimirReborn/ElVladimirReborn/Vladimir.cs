﻿namespace ElVladimirReborn
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    internal enum Spells
    {
        Q,

        W,

        E,

        R
    }

    internal static class Vladimir
    {
        #region Static Fields

        public static Orbwalking.Orbwalker Orbwalker;

        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>
                                                             {
                                                                 { Spells.Q, new Spell(SpellSlot.Q, 600) },
                                                                 { Spells.W, new Spell(SpellSlot.W) },
                                                                 { Spells.E, new Spell(SpellSlot.E, 610) },
                                                                 { Spells.R, new Spell(SpellSlot.R, 625) }
                                                             };

        private static SpellSlot _ignite;

        private static int lastNotification;

        #endregion

        #region Properties

        private static Obj_AI_Hero Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }

        #endregion

        #region Public Methods and Operators

        public static void OnLoad(EventArgs args)
        {
            if (ObjectManager.Player.CharData.BaseSkinName != "Vladimir")
            {
                return;
            }

            spells[Spells.R].SetSkillshot(0.25f, 175, 700, false, SkillshotType.SkillshotCircle);

            Notifications.AddNotification("ElVladimirReborn", 1000);
            _ignite = Player.GetSpellSlot("summonerdot");

            ElVladimirMenu.Initialize();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += Drawings.OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            //Orbwalking.BeforeAttack += OrbwalkingBeforeAttack;
        }

        #endregion

        #region Methods

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var gapCloserActive = ElVladimirMenu._menu.Item("ElVladimir.Settings.AntiGapCloser.Active").GetValue<bool>();

            if (gapCloserActive && spells[Spells.W].IsReady()
                && gapcloser.Sender.Distance(Player) < spells[Spells.W].Range
                && Player.CountEnemiesInRange(spells[Spells.Q].Range) >= 1)
            {
                spells[Spells.W].Cast(Player);
            }
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (spells[Spells.Q].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
            }

            if (spells[Spells.W].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);
            }

            if (spells[Spells.E].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);
            }

            if (spells[Spells.R].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);
            }
            else if (enemy.HasBuff("vladimirhemoplaguedebuff", true))
            {
                damage += damage * 1.12;
            }

            return (float)(damage + Player.GetAutoAttackDamage(enemy));
        }

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (_ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(_ignite) != SpellState.Ready)
            {
                return 0f;
            }
            return (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        private static void OnAutoHarass()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid)
            {
                return;
            }

            var useQ = ElVladimirMenu._menu.Item("ElVladimir.AutoHarass.Q").GetValue<bool>();
            var useE = ElVladimirMenu._menu.Item("ElVladimir.AutoHarass.E").GetValue<bool>();
            var playerHp = ElVladimirMenu._menu.Item("ElVladimir.AutoHarass.Health.E").GetValue<Slider>().Value;

            if (spells[Spells.Q].IsReady() && target.IsValidTarget() && useQ)
            {
                spells[Spells.Q].CastOnUnit(target, true);
            }

            if (spells[Spells.E].IsReady() && target.IsValidTarget(spells[Spells.E].Range)
                && (Player.Health / Player.MaxHealth) * 100 >= playerHp && useE)
            {
                spells[Spells.E].Cast(target);
            }
        }

        private static void OnAutoStack()
        {
            if (Player.IsRecalling() || Player.InFountain() || MenuGUI.IsChatOpen || MenuGUI.IsShopOpen)
            {
                return;
            }

            var stackHp = ElVladimirMenu._menu.Item("ElVladimir.Settings.Stack.HP").GetValue<Slider>().Value;

            if (Environment.TickCount - spells[Spells.E].LastCastAttemptT >= 9900 && spells[Spells.E].IsReady()
                && (Player.Health / Player.MaxHealth) * 100 >= stackHp)
            {
                spells[Spells.E].Cast();
            }
        }

        private static void OnCombo()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid)
            {
                return;
            }

            var useQ = ElVladimirMenu._menu.Item("ElVladimir.Combo.Q").GetValue<bool>();
            var useW = ElVladimirMenu._menu.Item("ElVladimir.Combo.W").GetValue<bool>();
            var useE = ElVladimirMenu._menu.Item("ElVladimir.Combo.E").GetValue<bool>();
            var useR = ElVladimirMenu._menu.Item("ElVladimir.Combo.R").GetValue<bool>();
            var useSmartR = ElVladimirMenu._menu.Item("ElVladimir.Combo.SmartUlt").GetValue<bool>();
            var onKill = ElVladimirMenu._menu.Item("ElVladimir.Combo.R.Killable").GetValue<bool>();
            var useIgnite = ElVladimirMenu._menu.Item("ElVladimir.Combo.Ignite").GetValue<bool>();
            var countEnemy = ElVladimirMenu._menu.Item("ElVladimir.Combo.Count.R").GetValue<Slider>().Value;

            var comboDamage = GetComboDamage(target);

            if (spells[Spells.Q].IsReady() && target.IsValidTarget() && useQ)
            {
                spells[Spells.Q].CastOnUnit(target, true);
            }

            if (spells[Spells.E].IsReady() && target.IsValidTarget(spells[Spells.E].Range) && useE)
            {
                spells[Spells.E].Cast(target);
            }

            if (spells[Spells.W].IsReady() && target.IsValidTarget(spells[Spells.W].Range) && useW)
            {
                spells[Spells.W].Cast(Player);
            }

            if (onKill)
            {
                if (useSmartR)
                {
                    var eQDamage = (spells[Spells.Q].GetDamage(target) + spells[Spells.E].GetDamage(target));

                    if (spells[Spells.Q].IsReady() && target.IsValidTarget(spells[Spells.Q].Range)
                        && spells[Spells.Q].GetDamage(target) >= target.Health)
                    {
                        spells[Spells.Q].Cast();
                    }
                    else if (spells[Spells.E].IsReady() && spells[Spells.E].GetDamage(target) >= target.Health)
                    {
                        spells[Spells.E].Cast(target);
                    }
                    else if (spells[Spells.Q].IsReady() && spells[Spells.E].IsReady()
                             && target.IsValidTarget(spells[Spells.Q].Range) && eQDamage >= target.Health)
                    {
                        spells[Spells.Q].Cast();
                        spells[Spells.E].Cast(target);
                    }
                    else if (spells[Spells.R].IsReady() && GetComboDamage(target) >= target.Health && !target.IsDead)
                    {
                        spells[Spells.R].Cast(target);
                    }
                }
                else
                {
                    if (comboDamage >= target.Health && useR && !target.IsDead)
                    {
                        spells[Spells.R].Cast(target);
                    }
                }
            }
            else
            {
                if (spells[Spells.R].IsReady()
                    && ObjectManager.Get<Obj_AI_Hero>()
                           .Count(hero => !hero.IsDead && hero.IsValidTarget(spells[Spells.R].Range)) >= countEnemy
                    && useR)
                {
                    spells[Spells.R].Cast(target);
                }
            }

            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health && useIgnite)
            {
                Player.Spellbook.CastSpell(_ignite, target);
            }
        }

        private static void OnHarass()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid)
            {
                return;
            }

            var useQ = ElVladimirMenu._menu.Item("ElVladimir.Harass.Q").GetValue<bool>();
            var useE = ElVladimirMenu._menu.Item("ElVladimir.Harass.E").GetValue<bool>();

            if (spells[Spells.Q].IsReady() && target.IsValidTarget() && useQ)
            {
                spells[Spells.Q].CastOnUnit(target, true);
            }

            if (spells[Spells.E].IsReady() && target.IsValidTarget(spells[Spells.E].Range) && useE)
            {
                spells[Spells.E].Cast(target);
            }
        }

        private static void OnJungleClear()
        {
            var useQ = ElVladimirMenu._menu.Item("ElVladimir.JungleClear.Q").GetValue<bool>();
            var useE = ElVladimirMenu._menu.Item("ElVladimir.JungleClear.E").GetValue<bool>();
            var playerHp = ElVladimirMenu._menu.Item("ElVladimir.WaveClear.Health.E").GetValue<Slider>().Value;

            if (spells[Spells.Q].IsReady() && useQ)
            {
                var allMinions = MinionManager.GetMinions(
                    ObjectManager.Player.ServerPosition,
                    spells[Spells.W].Range,
                    MinionTypes.All,
                    MinionTeam.Neutral,
                    MinionOrderTypes.MaxHealth);
                {
                    foreach (var minion in
                        allMinions.Where(minion => minion.IsValidTarget()))
                    {
                        spells[Spells.Q].CastOnUnit(minion);
                        return;
                    }
                }
            }

            if (spells[Spells.E].IsReady() && (Player.Health / Player.MaxHealth) * 100 >= playerHp && useE)
            {
                var minions = MinionManager.GetMinions(
                    ObjectManager.Player.ServerPosition,
                    spells[Spells.W].Range,
                    MinionTypes.All,
                    MinionTeam.Neutral,
                    MinionOrderTypes.MaxHealth);
                if (minions.Count <= 0)
                {
                    return;
                }

                if (minions.Count > 1)
                {
                    var farmLocation = spells[Spells.E].GetCircularFarmLocation(minions);
                    spells[Spells.E].Cast(farmLocation.Position);
                }
            }
        }

        private static void OnLaneClear()
        {
            var useQ = ElVladimirMenu._menu.Item("ElVladimir.WaveClear.Q").GetValue<bool>();
            var useE = ElVladimirMenu._menu.Item("ElVladimir.WaveClear.E").GetValue<bool>();
            var playerHp = ElVladimirMenu._menu.Item("ElVladimir.WaveClear.Health.E").GetValue<Slider>().Value;

            if (spells[Spells.Q].IsReady() && useQ)
            {
                var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spells[Spells.Q].Range);
                {
                    foreach (var minion in
                        allMinions.Where(
                            minion => minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q)))
                    {
                        if (minion.IsValidTarget())
                        {
                            spells[Spells.Q].CastOnUnit(minion);
                            return;
                        }
                    }
                }
            }

            if (spells[Spells.E].IsReady() && (Player.Health / Player.MaxHealth) * 100 >= playerHp && useE)
            {
                var minions = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.E].Range);
                if (minions.Count <= 0)
                {
                    return;
                }

                if (minions.Count > 1)
                {
                    spells[Spells.E].Cast();
                }
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    OnCombo();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    OnHarass();
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    OnLaneClear();
                    OnJungleClear();
                    break;
            }

            var showNotifications = ElVladimirMenu._menu.Item("ElVladimir.misc.Notifications").GetValue<bool>();

            if (showNotifications && Environment.TickCount - lastNotification > 5000)
            {
                foreach (var enemy in
                    ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(1000) && GetComboDamage(h) > h.Health))
                {
                    ShowNotification(enemy.ChampionName + ": is killable", Color.White, 4000);
                    lastNotification = Environment.TickCount;
                }
            }

        
            if (ElVladimirMenu._menu.Item("ElVladimir.AutoHarass.Activated").GetValue<KeyBind>().Active)
            {
                OnAutoHarass();
            }

            if (ElVladimirMenu._menu.Item("ElVladimir.Settings.Stack.E").GetValue<KeyBind>().Active)
            {
                OnAutoStack();
            }
        }

        private static void ShowNotification(string message, Color color, int duration = -1, bool dispose = true)
        {
            Notifications.AddNotification(new Notification(message, duration, dispose).SetTextColor(color));
        }

        #endregion
    }
}