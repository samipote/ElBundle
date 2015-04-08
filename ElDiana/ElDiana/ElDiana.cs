﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using LeagueSharp.Common.Data;
using Color = System.Drawing.Color;

namespace ElDiana
{
    /// <summary>
    /// ElDiana by jQuery
    /// Version 1.0.0.0
    /// 
    /// Combo
    /// Q, W, E, R
    /// When in combo and Q is not ready to cast but target can be killed with R, it will cast R on target.
    /// Auto ignite when target is killable
    /// 
    /// Harass
    /// Q, W, E
    /// 
    /// Clear settings
    /// 
    /// - Lane clear
    /// Q, W, E, R (off my default) 
    /// R only kills minion/mob that can be executed and is targetted by Q buff so your R won't go on CD.
    /// 
    /// - Jungleclear
    /// Q, W, E, R (off my default) 
    /// R only kills minion/mob that can be executed and is targetted by Q buff so your R won't go on CD.
    /// 
    /// Drawings (Misc)
    /// Draws combo damage
    /// Q, W, E, R ranges
    /// 
    /// Extra
    /// Custom hitchanes in combo menu, default is set to high.
    /// 
    /// </summary>


    internal enum Spells
    {
        Q,
        W,
        E,
        R
    }

    internal static class Diana
    {
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static Orbwalking.Orbwalker Orbwalker;
        private static SpellSlot Ignite;
        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, 900) },
            { Spells.W, new Spell(SpellSlot.W, 240) },
            { Spells.E, new Spell(SpellSlot.E, 450) },
            { Spells.R, new Spell(SpellSlot.R, 825) }
        };

        #region OnLoad

        #region hitchance

        private static HitChance CustomHitChance
        {
            get { return GetHitchance(); }
        }

        private static HitChance GetHitchance()
        {
            switch (ElDianaMenu._menu.Item("ElDiana.hitChance").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        #endregion


        public static void OnLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "Diana")
                return;

            Notifications.AddNotification("ElDiana by jQuery v1.0.0.0", 1000);
            spells[Spells.Q].SetSkillshot(0.35f, 180f, 1800f, false, SkillshotType.SkillshotCircle);
            Ignite = Player.GetSpellSlot("summonerdot");


            ElDianaMenu.Initialize();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += Drawings.Drawing_OnDraw;
        }

        #endregion

        #region OnGameUpdate

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                     LaneClear();
                    JungleClear();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
            }


      

            //Render.Circle.DrawCircle(playerPos, spells[Spells.Q].Range, Color.red);
        }
        #endregion

        #region Combo

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValid)
                return;

            var useQ = ElDianaMenu._menu.Item("ElDiana.Combo.Q").GetValue<bool>();
            var useW = ElDianaMenu._menu.Item("ElDiana.Combo.W").GetValue<bool>();
            var useE = ElDianaMenu._menu.Item("ElDiana.Combo.E").GetValue<bool>();
            var useR = ElDianaMenu._menu.Item("ElDiana.Combo.R").GetValue<bool>();
            var useIgnite = ElDianaMenu._menu.Item("ElDiana.Combo.Ignite").GetValue<bool>();
            var secondR = ElDianaMenu._menu.Item("ElDiana.Combo.Secure").GetValue<bool>();
            // var useMisayaCombo = ElDianaMenu._menu.Item("ElDiana.Combo.Misaya").GetValue<bool>();


            /* var leapminions = MinionManager.GetMinions(
                            ObjectManager.Player.ServerPosition, spells[Spells.Q].Range, MinionTypes.All, MinionTeam.NotAlly);


             var qMinions = leapminions.FindAll(minionQ => minionQ.IsValidTarget(spells[Spells.Q].Range));
             var qMinion = qMinions.Find(minionQ => minionQ.IsValidTarget() && minionQ.Distance(target) < 900);

             Render.Circle.DrawCircle(qMinion.Position, qMinion.BoundingRadius, Color.Red);

             if (spells[Spells.Q].IsReady())
             {
                 spells[Spells.Q].Cast(qMinion);


             }

             if (useR && spells[Spells.R].IsReady() &&
                 qMinion.HasBuff("dianamoonlight", true))
             {
                 spells[Spells.R].Cast(qMinion);
             }*/

            /* if (useMisayaCombo && Player.Distance(target) <= spells[Spells.Q].Range && spells[Spells.R].IsInRange(target) && spells[Spells.R].IsReady())
       {
           if (spells[Spells.Q].GetPrediction(target).Hitchance >= CustomHitChance)
           {
               spells[Spells.R].Cast(target, true);
               spells[Spells.Q].CastIfHitchanceEquals(target, CustomHitChance, true);
           }
       }
       else
       {

       }*/

            if (useQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
            {
                var pred = spells[Spells.Q].GetPrediction(target);
                if (pred.Hitchance >= CustomHitChance)
                    spells[Spells.Q].Cast(target);
            }

            if (useR && spells[Spells.R].IsReady() && spells[Spells.R].IsInRange(target) &&
                target.HasBuff("dianamoonlight", true))
            {
                spells[Spells.R].Cast(target);
            }

            if (useW && spells[Spells.W].IsReady() && spells[Spells.W].IsInRange(target))
            {
                spells[Spells.W].Cast();
            }

            if (useE && spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
            {
                var pred = spells[Spells.E].GetPrediction(target);
                if (pred.Hitchance >= CustomHitChance)
                    spells[Spells.E].Cast();
            }

            if (secondR && useR && !spells[Spells.Q].IsReady() && spells[Spells.R].IsReady())
            {
                if (target.Health < spells[Spells.R].GetDamage(target))
                {
                    spells[Spells.R].Cast(target);
                }
            }

            if (secondR && spells[Spells.R].IsReady())
            {
                if (target.Health < spells[Spells.R].GetDamage(target))
                {
                    spells[Spells.R].Cast(target);
                }
            }

            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health && useIgnite)
            {
                Player.Spellbook.CastSpell(Ignite, target);
            }     
        }

        #endregion

        #region Harass

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValid)
                return;

            var useQ = ElDianaMenu._menu.Item("ElDiana.Harass.Q").GetValue<bool>();
            var useW = ElDianaMenu._menu.Item("ElDiana.Harass.W").GetValue<bool>();
            var useE = ElDianaMenu._menu.Item("ElDiana.Harass.E").GetValue<bool>();
            var checkMana = ElDianaMenu._menu.Item("ElDiana.Harass.Mana").GetValue<Slider>().Value;

            if (Player.ManaPercentage() < checkMana)
                return;

            if (useQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
            {
                var pred = spells[Spells.Q].GetPrediction(target);
                if (pred.Hitchance >= CustomHitChance)
                    spells[Spells.Q].Cast(target);
            }

            if (useW && spells[Spells.W].IsReady() && spells[Spells.W].IsInRange(target))
            {
                spells[Spells.W].Cast();
            }

            if (useE && spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
            {
                var pred = spells[Spells.E].GetPrediction(target);
                if (pred.Hitchance >= CustomHitChance)
                    spells[Spells.E].Cast();
            }
        }

        #endregion

        #region LaneClear

        private static void LaneClear()
        {
            var minion = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spells[Spells.Q].Range).FirstOrDefault();
            if (minion == null || minion.Name.ToLower().Contains("ward"))
                return;

            var useQ = ElDianaMenu._menu.Item("ElDiana.LaneClear.Q").GetValue<bool>();
            var useW = ElDianaMenu._menu.Item("ElDiana.LaneClear.W").GetValue<bool>();
            var useE = ElDianaMenu._menu.Item("ElDiana.LaneClear.E").GetValue<bool>();
            var useR = ElDianaMenu._menu.Item("ElDiana.LaneClear.R").GetValue<bool>();

            var countQ = ElDianaMenu._menu.Item("ElDiana.LaneClear.Count.Minions.Q").GetValue<Slider>().Value;
            var countW = ElDianaMenu._menu.Item("ElDiana.LaneClear.Count.Minions.W").GetValue<Slider>().Value;
            var countE = ElDianaMenu._menu.Item("ElDiana.LaneClear.Count.Minions.E").GetValue<Slider>().Value;

            var minions = MinionManager.GetMinions(
               ObjectManager.Player.ServerPosition, spells[Spells.Q].Range, MinionTypes.All, MinionTeam.NotAlly);

            var qMinions = minions.FindAll(minionQ => minion.IsValidTarget(spells[Spells.Q].Range));
            var qMinion = qMinions.Find(
                        minionQ => minionQ.IsValidTarget());

            if (useQ && spells[Spells.Q].IsReady() && spells[Spells.Q].GetCircularFarmLocation(minions).MinionsHit >= countQ)
            {
                spells[Spells.Q].Cast(qMinion);
            }

            if (useW && spells[Spells.W].IsReady() && spells[Spells.W].GetCircularFarmLocation(minions).MinionsHit >= countW)
            {
                spells[Spells.W].Cast();
            }

            if (useE && spells[Spells.E].IsReady() && Player.Distance(qMinion) < 200 && spells[Spells.E].GetCircularFarmLocation(minions).MinionsHit >= countE)
            {
                spells[Spells.E].Cast();
            }

            var minionsR = MinionManager.GetMinions(
               ObjectManager.Player.ServerPosition, spells[Spells.Q].Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);

            if (useR && spells[Spells.R].IsReady())
            {
                //find Mob with moonlight buff
                var moonlightMob = minionsR.FindAll(x => x.HasBuff("dianamoonlight", true)).OrderBy(x => minion.HealthPercentage());
                if (moonlightMob.Any())
                {
                    //only cast when killable
                    var canBeKilled = moonlightMob.Find(
                        x => minion.Health < spells[Spells.R].GetDamage(minion));

                    //cast R on mob that can be killed
                    if (canBeKilled.IsValidTarget())
                    {
                        spells[Spells.R].Cast(canBeKilled);
                    }
                }
            }
        }

        #endregion

        #region JungleClear

        private static void JungleClear()
        {
            var minions = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, spells[Spells.Q].Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            var useQ = ElDianaMenu._menu.Item("ElDiana.JungleClear.Q").GetValue<bool>();
            var useW = ElDianaMenu._menu.Item("ElDiana.JungleClear.W").GetValue<bool>();
            var useE = ElDianaMenu._menu.Item("ElDiana.JungleClear.E").GetValue<bool>();
            var useR = ElDianaMenu._menu.Item("ElDiana.JungleClear.R").GetValue<bool>();

            var qMinions = minions.FindAll(minion => minion.IsValidTarget(spells[Spells.Q].Range));
            var qMinion = qMinions.Find(
                        minion => minion.IsValidTarget());

            if (useQ && spells[Spells.Q].IsReady() )
            {
                if(qMinion.IsValidTarget())
                    spells[Spells.Q].Cast(qMinion);
            }

            if (useW && spells[Spells.W].IsReady())
            {
                spells[Spells.W].Cast();
            }

            //hmmpff
            if (useE && spells[Spells.E].IsReady() && Player.Distance(qMinion) < 200)
            {
                spells[Spells.E].Cast();
            }

            if (useR && spells[Spells.R].IsReady())
            {
                //find Mob with moonlight buff
                var moonlightMob = minions.FindAll(minion => minion.HasBuff("dianamoonlight", true)).OrderBy(minion => minion.HealthPercentage());
                if (moonlightMob.Any())
                {
                    //only cast when killable
                    var canBeKilled = moonlightMob.Find(
                        minion => minion.Health < spells[Spells.R].GetDamage(minion));

                    //cast R on mob that can be killed
                    if (canBeKilled.IsValidTarget())
                    {
                        spells[Spells.R].Cast(canBeKilled);
                    }
                }        
            }
        }

        #endregion

        #region IgniteDamage

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (Ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
            {
                return 0f;
            }
            return (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        #endregion

        #region ComboDamage

        public static float GetComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (spells[Spells.Q].IsReady())
            {
                damage += spells[Spells.Q].GetDamage(enemy);
            }

            if (spells[Spells.W].IsReady())
            {
                damage += spells[Spells.W].GetDamage(enemy);
            }

            if (spells[Spells.E].IsReady())
            {
                damage += spells[Spells.E].GetDamage(enemy);
            }

            if (spells[Spells.R].IsReady())
            {
                damage += spells[Spells.R].GetDamage(enemy);
            }

            if (Ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
            {
                damage += (float)Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            return damage;
        }

        #endregion
    }
}