using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
//http://base64myass.com/AMK/images/9ihe5.png
/*
-- ###################################################################################################### --
-- #                                                                                                    # --
-- #                                        Sida's Missed CS Counter                                    # --
-- #                                                by Sida                                             # --
-- #                                                                                                    # --
-- ###################################################################################################### --
*/
/*༼ つ ◕_◕ ༽つ I RITO ༼ つ ◕_◕ ༽つ -T R I G G E R E D */
namespace ELjQueriesMissedCSCounter
{
    internal class Entry
    {
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        private static Notification notification;
        private static int lastTimeScreederWarnedYouForSpamInDevChat = 0;
        private static int creepAdminOnGoSMeme = 0;
        public static List<GameObject> iStole14CreepsFromMyoAndHeBlockedMyOneSkype { get; private set; }
        public static List<GameObject> NerdyILikeItAsunaIsAFuckingWeebo { get; private set; }

        public static void Game_OnGameLoad(EventArgs args)
        {
            iStole14CreepsFromMyoAndHeBlockedMyOneSkype = new List<GameObject>();
            NerdyILikeItAsunaIsAFuckingWeebo = new List<GameObject>();
            Notifications.AddNotification("jQueriesMissedCSCounter", 10000);
            Game.OnUpdate += KurisuAndL33TAreBadAtLeagueAndCoreyIsGayWithZezzy;
            Drawing.OnDraw += ImehDrawsACatForAsuna;
            GameObject.OnCreate += DetuksYasuoSource;
            Game.OnNotify += TreesStillFightingWithPlebsInPredictionTopic;
        }

        private static void TreesStillFightingWithPlebsInPredictionTopic(GameNotifyEventArgs args)
        {
            if (args.EventId != GameEventId.OnMinionKill)
            {
                return;
            }

            NerdyILikeItAsunaIsAFuckingWeebo.RemoveAll(x => x.NetworkId == args.NetworkId);
        }

        private static void KurisuAndL33TAreBadAtLeagueAndCoreyIsGayWithZezzy(EventArgs args)
        {
            foreach (var minion in iStole14CreepsFromMyoAndHeBlockedMyOneSkype)
            {
                if (minion == null || !minion.IsValid)
                {
                    iStole14CreepsFromMyoAndHeBlockedMyOneSkype.RemoveAll(m => m.NetworkId == minion.NetworkId);
                    break;
                }

                if (minion.IsVisible && minion.Position.Distance(Player.Position) < 0x1f4 && NerdyILikeItAsunaIsAFuckingWeebo.All(m => m.NetworkId != minion.NetworkId))
                {
                    NerdyILikeItAsunaIsAFuckingWeebo.Add(minion);
                }
            }

            foreach (var minion in NerdyILikeItAsunaIsAFuckingWeebo.Where(minion => !iStole14CreepsFromMyoAndHeBlockedMyOneSkype.Contains(minion)))
            {
                NerdyILikeItAsunaIsAFuckingWeebo.RemoveAll(m => m.NetworkId == minion.NetworkId);
                lastTimeScreederWarnedYouForSpamInDevChat += 1 + 1 - 1; //meme? No.
                break;
            }

            //patented
            creepAdminOnGoSMeme += (lastTimeScreederWarnedYouForSpamInDevChat - Player.MinionsKilled) > creepAdminOnGoSMeme
                                ? Math.Abs(creepAdminOnGoSMeme - (lastTimeScreederWarnedYouForSpamInDevChat - Player.MinionsKilled))
                                : 0;

            var text = "";

            if (notification == null)
            {
                notification = new Notification(text)
                {
                    TextColor = new ColorBGRA(red: 0xff, green: 0xff, blue: 255, alpha: 0xff)
                };

                Notifications.AddNotification(notification);
            }

            notification.Text = lastTimeScreederWarnedYouForSpamInDevChat + " missed creeps" + text;
        }

        private static void DetuksYasuoSource(GameObject sender, EventArgs args)
        {
            if (sender is Obj_AI_Minion && sender.IsEnemy)
            {
                iStole14CreepsFromMyoAndHeBlockedMyOneSkype.Add(sender);
            }
        }

        //Myo is better than jQuery at last hitting.
        //fuck whiteboi
        private static void ImehDrawsACatForAsuna(EventArgs args)
        {
            var minionList = NerdyILikeItAsunaIsAFuckingWeebo.Select(m => (Obj_AI_Minion)m);
            foreach (var minion in minionList.Where(minion => minion.IsValidTarget(Player.AttackRange)).Where(minion => minion.Health <= Player.GetAutoAttackDamage(minion, true)))
            {
                Render.Circle.DrawCircle(minion.Position, minion.BoundingRadius, Color.LawnGreen);
            }
        }
    }
}