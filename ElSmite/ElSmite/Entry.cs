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
        static SpellSlot smiteSlot;
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


        static SpellDataInst slot1;
        static SpellDataInst slot2;

        static Spell smite;

        static readonly int[] PurpleSmite = { 3713, 3726, 3725, 3724, 3723, 3933 };
        static readonly int[] GreySmite = { 3711, 3722, 3721, 3720, 3719, 3932 };
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
            if (BlueSmite.Any(x => Items.HasItem(x)))
            {
                smiteSlot = ObjectManager.Player.GetSpellSlot("s5_summonersmiteplayerganker");
            }
            else if (RedSmite.Any(x => Items.HasItem(x)))
            {
                smiteSlot = ObjectManager.Player.GetSpellSlot("s5_summonersmiteduel");
            }
            else if (GreySmite.Any(x => Items.HasItem(x)))
            {
                smiteSlot = ObjectManager.Player.GetSpellSlot("s5_summonersmitequick");
            }
            else if (PurpleSmite.Any(x => Items.HasItem(x)))
            {
                smiteSlot = ObjectManager.Player.GetSpellSlot("itemsmiteaoe");
            }
            else
            {
                smiteSlot = ObjectManager.Player.GetSpellSlot("summonersmite");
            }
   
            smite.Slot = smiteSlot;
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
                slot1 = Player.Spellbook.GetSpell(SpellSlot.Summoner1);
                slot2 = Player.Spellbook.GetSpell(SpellSlot.Summoner2);

                if (new[] { "s5_summonersmiteplayerganker", "itemsmiteaoe", "s5_summonersmitequick", "s5_summonersmiteduel", "summonersmite" }.Contains(slot1.Name))
                {
                    smite = new Spell(SpellSlot.Summoner1, 500f);
                }

                if (new[] { "s5summonersmiteplayerganker", "itemsmiteaoe", "s5_summonersmitequick", "s5_summonersmiteduel", "summonersmite" }.Contains(slot2.Name))
                {
                    smite = new Spell(SpellSlot.Summoner2, 500f);
                }

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
                if (SmiteDamage() > minions.Health)
                {
                    Player.Spellbook.CastSpell(smite.Slot, minions);
                }
            }
        }

        #endregion


        #region SmiteKill

        static void SmiteKill()
        {
            if (!InitializeMenu.Menu.Item("ElSmite.KS.Activated").GetValue<bool>()) return;

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.Distance(Player) <= 500 && hero.IsEnemy && !hero.IsDead && hero.IsValidTarget() && SmiteChampDamage() > hero.Health))
            {
                Player.Spellbook.CastSpell(smite.Slot, enemy);
            }
        }
        #endregion

        #region SmiteDamages

        static double SmiteDamage()
        {
            var damage = new int[] { 20 * Player.Level + 370, 30 * Player.Level + 330, 40 *+ Player.Level + 240, 50 * Player.Level + 100 };

            return Player.Spellbook.CanUseSpell(smite.Slot) == SpellState.Ready ? damage.Max() : 0;
        }

        static double SmiteChampDamage()
        {
            if (smite.Slot == Player.GetSpellSlot("s5_summonersmiteduel"))
            {
                var damage = new int[] { 54 + 6 * Player.Level };
                return Player.Spellbook.CanUseSpell(smite.Slot) == SpellState.Ready ? damage.Max() : 0;
            }

            if (smite.Slot == Player.GetSpellSlot("s5_summonersmiteplayerganker"))
            {
                var damage = new int[] { 20 + 8 * Player.Level };
                return Player.Spellbook.CanUseSpell(smite.Slot) == SpellState.Ready ? damage.Max() : 0;
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
           
            if (drawSmite.Active && Player.Spellbook.CanUseSpell(smite.Slot) == SpellState.Ready)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 500, Color.White);

            if (drawSmite.Active && Player.Spellbook.CanUseSpell(smite.Slot) != SpellState.Ready)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 500, Color.Red);

            if (drawText && Player.Spellbook.CanUseSpell(smite.Slot) == SpellState.Ready)
                Drawing.DrawText(Player.HPBarPosition.X + 40, Player.HPBarPosition.Y - 10, Color.GhostWhite, "Smite active");

            if (drawText && Player.Spellbook.CanUseSpell(smite.Slot) != SpellState.Ready)
                Drawing.DrawText(Player.HPBarPosition.X + 40, Player.HPBarPosition.Y - 10, Color.Red, "Smite cooldown");
        }
        #endregion
    }
}
