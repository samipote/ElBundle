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
            Notifications.AddNotification($"ElTristana by jQuery v{ScriptVersion}", 8000);

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

            if (spells[Spells.Q].IsReady() && IsActive("ElTristana.Combo.Q"))
            {
                
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

        public static bool IsECharged(this Obj_AI_Hero target)
        {
            return target.HasBuff("tristanaecharge");
        }

        #endregion

    }
}