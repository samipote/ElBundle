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
        //


        static readonly int[] PurpleSmite = { 3713, 3726, 3725, 3724, 3723, 3933 };
        static readonly int[] GreySmite = { 3711, 3722, 3721, 3720, 3719, 3932 };
        static readonly int[] RedSmite = { 3715, 3718, 3717, 3716, 3714, 3931 };
        static readonly int[] BlueSmite = { 3706, 3710, 3709, 3708, 3707, 3930 };

        //static readonly int[] DevourerSmite = { 3930, 3710, 3722, 3726, 3930, 3931, 3932, 3933 };

        static readonly string[] BuffsThatActuallyMakeSenseToSmite =
        {
            "SRU_Red", "SRU_Blue", "SRU_Dragon", "SRU_Baron",
            "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak",
            "SRU_Krug",  "Sru_Crab"
        };

        #endregion

        #region InitializeSmite
        /**
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
        */
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
                var smiteNames = new[]
                {
                    "s5_summonersmiteplayerganker", "itemsmiteaoe", "s5_summonersmitequick", "s5_summonersmiteduel",
                    "summonersmite"
                };

                if (smiteNames.Contains(slot1.Name))
                {
                    smite = new Spell(SpellSlot.Summoner1, 550f);
                    smiteSlot = SpellSlot.Summoner1;
                }else if (smiteNames.Contains(slot2.Name))
                {
                    smite = new Spell(SpellSlot.Summoner2, 550f);
                    smiteSlot = SpellSlot.Summoner2;
                }
                else
                {
                    Console.WriteLine("You don't have smite faggot");
                    return;
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
                //InitializeSmite();
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

            Obj_AI_Minion minion = (Obj_AI_Minion) MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 500, MinionTypes.All, MinionTeam.Neutral).ToList().FirstOrDefault(buff => buff.IsValidTarget() && BuffsThatActuallyMakeSenseToSmite.Contains(buff.CharData.BaseSkinName));

            if (minion == null)
            {
                return;
            }

            if (InitializeMenu.Menu.Item(minion.CharData.BaseSkinName).GetValue<bool>())
            {
                if (SmiteDamage() > minion.Health + 10)
                {
                    Player.Spellbook.CastSpell(smite.Slot, minion);
                }
            }
        }

        #endregion


        #region SmiteKill

        static void SmiteKill()
        {
            if (!InitializeMenu.Menu.Item("ElSmite.KS.Activated").GetValue<bool>()) return;

            var KSableEnemy =
                HeroManager.Enemies.FirstOrDefault(
                    hero => hero.IsValidTarget(550) &&
                        SmiteChampDamage() >= hero.Health);
            if (KSableEnemy != null)
            {
                Player.Spellbook.CastSpell(smite.Slot, KSableEnemy);
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
            var playerPos = Drawing.WorldToScreen(ObjectManager.Player.Position);

            //if (!smiteActive) return;

            if (smiteActive)
            {
                if (drawText && Player.Spellbook.CanUseSpell(smite.Slot) == SpellState.Ready)
                {
                    Drawing.DrawText(playerPos.X - 70, playerPos.Y + 40, Color.GhostWhite, "Smite active");
                }

                if (drawText && Player.Spellbook.CanUseSpell(smite.Slot) != SpellState.Ready)
                    Drawing.DrawText(playerPos.X - 70, playerPos.Y + 40, Color.Red, "Smite cooldown");
            }
            else
            {
                if(drawText)
                    Drawing.DrawText(playerPos.X - 70, playerPos.Y + 40, Color.Red, "Smite not active");
            }
            

            if (smiteActive && drawSmite.Active && Player.Spellbook.CanUseSpell(smite.Slot) == SpellState.Ready)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 500, Color.Green);

            if (drawSmite.Active && Player.Spellbook.CanUseSpell(smite.Slot) != SpellState.Ready)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 500, Color.Red);
        }
        #endregion
    }
}
