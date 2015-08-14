using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using LeagueSharp.Common.Data;
using ItemData = LeagueSharp.Common.Data.ItemData;

namespace ElTristana
{
    using System.Dynamic;

    #region 

    internal enum Spells
    {
        Q,

        W,

        E,

        R
    }

    #endregion

    internal static class Tristana
    {
        #region 

        public static String ScriptVersion
        {
            get
            {
                return typeof(Tristana).Assembly.GetName().Version.ToString();

            }
        }

        private static Obj_AI_Hero Player
        {
            get
            {
                return ObjectManager.Player;

            }
        }

        #endregion

        public static Orbwalking.Orbwalker Orbwalker;
        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, 0)},
            { Spells.W, new Spell(SpellSlot.W, 900)},
            { Spells.E, new Spell(SpellSlot.E, 630)},
            { Spells.R, new Spell(SpellSlot.R, 630)},
        };


        #region 

        public static void OnLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Tristana") return;

            Console.WriteLine("Injected");
            Notifications.AddNotification(String.Format("ElTristana by jQuery v{0}", ScriptVersion), 8000);

            try
            {
                MenuInit.Initialize();
                Game.OnUpdate += OnUpdate;
                //Drawing.OnDraw += Drawings.Drawing_OnDraw;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        #endregion

        #region OnUpdate    

        private static void OnUpdate(EventArgs args)
        {
            
            if (Player.IsDead) return;

            try
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        OnCombo();
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        break;
                }

                spells[Spells.Q].Range = 541 + 9 * (Player.Level - 1);
                spells[Spells.E].Range = 541 + 9 * (Player.Level - 1);
                spells[Spells.R].Range = 541 + 9 * (Player.Level - 1);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        #endregion

        #region 

        private static void OnCombo()
        {
            var target = TargetSelector.GetTarget(spells[Spells.E].Range, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValidTarget())
                return;

            //Console.WriteLine("Buffs: {0}", string.Join(" | ", target.Buffs.Where(b => b.Caster.NetworkId == Player.NetworkId).Select(b => b.DisplayName)));
            // * TristanaECharge
            // * TristanaEChargesound

            if (spells[Spells.E].IsReady() && IsActive("ElTristana.Combo.E"))
            {
                foreach (var enemy in from enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy ) let getEnemies = MenuInit.Menu.Item("ElTristana.E.On" + enemy.CharData.BaseSkinName) where getEnemies != null && getEnemies.GetValue<bool>() select enemy)
                {
                    spells[Spells.E].Cast(enemy);
                }
            }

            UseItems(target);

            if (spells[Spells.Q].IsReady() && IsActive("ElTristana.Combo.Q") && target.IsValidTarget(spells[Spells.E].Range))
            {
                spells[Spells.Q].Cast();
            }

            if (spells[Spells.R].IsReady() && IsActive("ElTristana.Combo.R"))
            {
                if (IsECharged(target))
                {
                    if (!IsBusterShotable(target)) return;

                    spells[Spells.R].Cast(target);
                }
                else if (spells[Spells.R].GetDamage(target) > target.Health)
                {
                    // check if E + stacks is gonna kill target, if so don't cast R.
                    if (GetExecuteDamage(target) > target.Health) return; 

                    spells[Spells.R].Cast(target);
                }
            }
        }

        #endregion

        #region 

        private static void UseItems(Obj_AI_Base target)
        {
            var botrk = ItemData.Blade_of_the_Ruined_King.GetItem();
            var ghost = ItemData.Youmuus_Ghostblade.GetItem();
            var cutlass = ItemData.Bilgewater_Cutlass.GetItem();

            var useBladeEhp = MenuInit.Menu.Item("ElTristana.Items.Blade.EnemyEHP").GetValue<Slider>().Value;
            var useBladeMhp = MenuInit.Menu.Item("ElTristana.Items.Blade.EnemyMHP").GetValue<Slider>().Value;

            if (botrk.IsReady() && botrk.IsOwned(Player) && botrk.IsInRange(target)
                && target.HealthPercent <= useBladeEhp && IsActive("ElTristana.Items.Blade"))
            {
                botrk.Cast(target);
            }

            if (botrk.IsReady() && botrk.IsOwned(Player) && botrk.IsInRange(target)
                && Player.HealthPercent <= useBladeMhp && IsActive("ElTristana.Items.Blade"))
            {
                botrk.Cast(target);
            }

            if (cutlass.IsReady() && cutlass.IsOwned(Player) && cutlass.IsInRange(target)
                && target.HealthPercent <= useBladeEhp && IsActive("ElTristana.Items.Blade"))
            {
                cutlass.Cast(target);
            }

            if (ghost.IsReady() && ghost.IsOwned(Player) && target.IsValidTarget(spells[Spells.Q].Range)
                && IsActive("ElTristana.Items.Youmuu"))
            {
                ghost.Cast();
            }
        }

        #endregion

        #region 

        private static bool IsActive(string menuItem)
        {
            return MenuInit.Menu.Item(menuItem).GetValue<bool>();
        }

        #endregion

        #region

        private static bool IsBusterShotable(this Obj_AI_Base target)
        {
            return GetExecuteDamage(target) + spells[Spells.R].GetDamage(target) > target.Health;
        }

        private static BuffInstance GetECharge(this Obj_AI_Base target)
        {
            return target.Buffs.Find(x => x.DisplayName == "TristanaECharge");
        }

        private static bool IsECharged(this Obj_AI_Base target)
        {
            return target.GetECharge() != null;
        }

        private static double GetExecuteDamage(Obj_AI_Base target)
        {
            if (target.GetBuffCount("TristanaECharge") != 0)
            {
                return (spells[Spells.E].GetDamage(target) * ((0.30 * target.GetBuffCount("TristanaECharge") + 1)) * (Player.TotalAttackDamage()));
            }

            return 0;
        }

        private static float TotalAttackDamage(this Obj_AI_Base target)
        {
            return target.BaseAttackDamage + target.FlatPhysicalDamageMod;
        }

        #endregion
    }
}