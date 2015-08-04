using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace ElSmite
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Entry
    {
        static SpellSlot smite;
        static Obj_AI_Hero Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }

        public static String ScriptVersion
        {
            get
            {
                return typeof(Entry).Assembly.GetName().Version.ToString();
            }
        }

        static bool IsSummonersRift
        {
            get
            {
                return Game.MapId == GameMapId.SummonersRift;
            }
        }

        #region Smite

        static readonly int[] RedSmite = { 3715, 3718, 3717, 3716, 3714 };
        static readonly int[] BlueSmite = { 3706, 3710, 3709, 3708, 3707 };
        static readonly string[] BuffsThatActuallyMakeSenseToSmite =
        {
            "SRU_Red", "SRU_Blue", "SRU_Dragon", "SRU_Baron"
        };

        #endregion

        #region InitializeSmite

        static void InitializeSmite()
        {
            if (BlueSmite.Any(id => Items.HasItem(id)))
            {
                smite = Player.GetSpellSlot("s5_summonersmiteplayerganker");
                return;
            }

            if (RedSmite.Any(id => Items.HasItem(id)))
            {
                smite = Player.GetSpellSlot("s5_summonersmiteduel");
                return;
            }

            smite = Player.GetSpellSlot("summonersmite");
        }

        #endregion



        #region OnLoad

        public static void OnLoad(EventArgs args)
        {
            if (!IsSummonersRift)
            {
                Notifications.AddNotification("ElSmite is not supported on this map", 10000);
                return;
            }

            try
            {
                smite = Player.GetSpellSlot("summonersmite");
                Notifications.AddNotification(String.Format("ElSmite by jQuery v{0}", ScriptVersion), 10000);
                InitializeMenu.Load();
                Game.OnUpdate += OnUpdate;
                Drawing.OnDraw += OnDraw;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        #endregion

        #region OnUpdate    

        static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead) return;

            try
            {
                InitializeSmite();
                JungleSmite();
                SmiteKill();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        #endregion

        #region Smite

        static void JungleSmite()
        {
            if (!InitializeMenu.Menu.Item("ElSmite.Activated").GetValue<KeyBind>().Active) return;

            Obj_AI_Minion minions = null;
            foreach (var buff in ObjectManager.Get<Obj_AI_Minion>())
            {
                if (buff.Distance(Player) <= 500 && BuffsThatActuallyMakeSenseToSmite.Contains(buff.CharData.BaseSkinName))
                {
                    minions = buff;
                    break;
                }
            }

            if (minions == null) return;
            if (InitializeMenu.Menu.Item(minions.CharData.BaseSkinName).GetValue<bool>())
            {
                if (smite != SpellSlot.Unknown && SmiteDamage() > minions.Health)
                {
                    Player.Spellbook.CastSpell(smite, minions);
                }
            }
            //Player.Spellbook.CanUseSpell(smite) == SpellState.Ready && 
        }

        #endregion


        #region SmiteKill

        static void SmiteKill()
        {
            if (!InitializeMenu.Menu.Item("ElSmite.KS.Activated").GetValue<bool>()) return;

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.Distance(Player) <= 500 && hero.IsEnemy && !hero.IsDead && hero.IsValidTarget() && SmiteChampDamage() > hero.Health))
            {
                Player.Spellbook.CastSpell(smite, enemy);
            }
        }
        #endregion

        #region SmiteDamages

        static double SmiteDamage()
        {
            var damage = new int[] { 20 * Player.Level + 370, 30 * Player.Level + 330, 40 *+ Player.Level + 240, 50 * Player.Level + 100 };

            return Player.Spellbook.CanUseSpell(smite) == SpellState.Ready ? damage.Max() : 0;
        }

        static double SmiteChampDamage()
        {
            if (smite == Player.GetSpellSlot("s5_summonersmiteduel"))
            {
                var damage = new int[] { 54 + 6 * Player.Level };
                return Player.Spellbook.CanUseSpell(smite) == SpellState.Ready ? damage.Max() : 0;
            }

            if (smite == Player.GetSpellSlot("s5_summonersmiteplayerganker"))
            {
                var damage = new int[] { 20 + 8 * Player.Level };
                return Player.Spellbook.CanUseSpell(smite) == SpellState.Ready ? damage.Max() : 0;
            }

            return 0;
        }

        #endregion

        #region OnDraw

        static void OnDraw(EventArgs args)
        {
            var smiteActive = InitializeMenu.Menu.Item("ElSmite.Activated").GetValue<KeyBind>().Active;
            var drawSmite = InitializeMenu.Menu.Item("ElSmite.Draw.Range").GetValue<Circle>();
            var drawText = InitializeMenu.Menu.Item("ElSmite.Draw.Text").GetValue<bool>();

            if (!smiteActive) return;
           
            if (drawSmite.Active && Player.Spellbook.CanUseSpell(smite) == SpellState.Ready)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 500, Color.White);

            if (drawSmite.Active && Player.Spellbook.CanUseSpell(smite) != SpellState.Ready)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 500, Color.Red);

            if (drawText && Player.Spellbook.CanUseSpell(smite) == SpellState.Ready)
                Drawing.DrawText(Player.HPBarPosition.X + 40, Player.HPBarPosition.Y - 10, Color.GhostWhite, "Smite active");

            if (drawText && Player.Spellbook.CanUseSpell(smite) != SpellState.Ready)
                Drawing.DrawText(Player.HPBarPosition.X + 40, Player.HPBarPosition.Y - 10, Color.Red, "Smite cooldown");
        }
        #endregion
    }
}
