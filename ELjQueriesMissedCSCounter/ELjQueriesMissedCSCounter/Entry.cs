using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace ELjQueriesMissedCSCounter
{
    internal class Entry
    {
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        private static Notification notification;
        private static int siwyyOpensATopicAndHisContentContainsTheWordTitle = 0;
        private static int creepAdminOnGoSMeme = 0;
        private static int lastTimeScreederWarnedYouForSpamInDevChat = 0;
        public static List<GameObject> iStole14CreepsFromMyoAndHeBlockedMyOneSkype { get; private set; }
        public static List<GameObject> NerdyILikeItAsunaIsAFuckingWeebo { get; private set; }

        public static void Game_OnGameLoad(EventArgs args)
        {
            iStole14CreepsFromMyoAndHeBlockedMyOneSkype = new List<GameObject>();
            NerdyILikeItAsunaIsAFuckingWeebo = new List<GameObject>();
            Notifications.AddNotification("jQueriesMissedCSCounter", 5000);
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
                if (!minion.IsValid<Obj_AI_Minion>())
                {
                    iStole14CreepsFromMyoAndHeBlockedMyOneSkype.RemoveAll(m => m.NetworkId == minion.NetworkId);
                    break;
                }

                if (minion.Position.Distance(Player.Position) < 0x1f4 && NerdyILikeItAsunaIsAFuckingWeebo.All(m => m.NetworkId != minion.NetworkId))
                {
                    NerdyILikeItAsunaIsAFuckingWeebo.Add(minion);
                }
            }

            foreach (var minion in NerdyILikeItAsunaIsAFuckingWeebo.Where(minion => minion.IsDead))
            {
                siwyyOpensATopicAndHisContentContainsTheWordTitle++;
                NerdyILikeItAsunaIsAFuckingWeebo.RemoveAll(m => m.NetworkId == minion.NetworkId);
            }

            //patented
            creepAdminOnGoSMeme += (siwyyOpensATopicAndHisContentContainsTheWordTitle - Player.MinionsKilled) > creepAdminOnGoSMeme
                                ? Math.Abs(creepAdminOnGoSMeme - (siwyyOpensATopicAndHisContentContainsTheWordTitle - Player.MinionsKilled))
                                : 0x0;

            /*Console.WriteLine("DEBUG:");
            Console.WriteLine("ALLMINIONSCOUNT: " + MinionList.Count);
            Console.WriteLine("CLOSESTMINIONSCOUNT: " + MinionsCloseToMeList.Count);
            Console.WriteLine("DED MINIONS: " + totalMinionsThatDied);
            Console.WriteLine("CS: " + Player.MinionsKilled);
            Console.WriteLine("MISSED CS: " + Math.Abs(totalMinionsThatDied - Player.MinionsKilled));*/

            var text = "";

            if (notification == null)
            {
                notification = new Notification(text)
                {
                    TextColor = new ColorBGRA(red: 0xff, green: 0xff, blue: 255, alpha: 0xff)
                };

                Notifications.AddNotification(notification);
            }

            notification.Text = creepAdminOnGoSMeme + " missed creeps" + text;
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
            var minionList = MinionManager.GetMinions(Player.Position, Player.AttackRange + 0x1f4, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
            foreach (var minion in minionList.Where(minion => minion.IsValidTarget(Player.AttackRange + 0x1f4)).Where(minion => minion.Health <= Player.GetAutoAttackDamage(minion, true)))//.Where().Where().Where().Where()
            {
                Render.Circle.DrawCircle(minion.Position, minion.BoundingRadius, Color.LawnGreen);
            }
        }
    }
}