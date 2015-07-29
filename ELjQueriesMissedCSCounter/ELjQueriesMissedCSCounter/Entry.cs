using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

//http://base64myass.com/AMK/images/9ihe5.png

namespace ELjQueriesMissedCSCounter
{
    internal class Entry
    {
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        private static Notification notification;
        private static int totalMinionsThatDied = 0;
        private static int missedCreeps = 0;
        private static int minionsInMyRange = 0;
        public static List<GameObject> MinionList { get; private set; }
        public static List<GameObject> MinionsCloseToMeList { get; private set; }

        public static void Game_OnGameLoad(EventArgs args)
        {
            MinionList = new List<GameObject>();
            MinionsCloseToMeList = new List<GameObject>();
            Notifications.AddNotification("jQueriesMissedCSCounter", 10000);
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Minion.OnCreate += OnCreate;
        }

        private static void OnUpdate(EventArgs args)
        {
            foreach (var minion in MinionList)
            {
                if (!minion.IsValid<Obj_AI_Minion>())
                {
                    MinionList.RemoveAll(m => m.NetworkId == minion.NetworkId);
                    break;
                }
                else
                {
                    if (minion.Position.Distance(Player.Position) < 500 && !MinionsCloseToMeList.Any(m => m.NetworkId == minion.NetworkId))
                    {
                        MinionsCloseToMeList.Add(minion);
                    }
                }
            }

            foreach (var minion in MinionsCloseToMeList)
            {
                if (!minion.IsValid<Obj_AI_Minion>())
                {
                    totalMinionsThatDied++;
                    MinionsCloseToMeList.RemoveAll(m => m.NetworkId == minion.NetworkId);
                    break;
                }
            }

            //patented
            missedCreeps += (totalMinionsThatDied - Player.MinionsKilled) > missedCreeps
                                ? (missedCreeps - (totalMinionsThatDied - Player.MinionsKilled)) * -1
                                : 0;
            Console.WriteLine("DEBUG:");
            Console.WriteLine("ALLMINIONSCOUNT: " + MinionList.Count);
            Console.WriteLine("CLOSESTMINIONSCOUNT: " + MinionsCloseToMeList.Count);
            Console.WriteLine("DED MINIONS: " + totalMinionsThatDied);
            Console.WriteLine("CS: " + Player.MinionsKilled);
            Console.WriteLine("MISSED CS: " + (totalMinionsThatDied - Player.MinionsKilled));

        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender is Obj_AI_Minion && sender.IsEnemy)
            {
                MinionList.Add(sender);
            }
        }

        //Myo is better than jQuery at last hitting.
        //fuck whiteboi
        private static void OnDraw(EventArgs args)
        {
            var minionList = MinionManager.GetMinions(Player.Position, Player.AttackRange + 500, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
            foreach (var minion in minionList.Where(minion => minion.IsValidTarget(Player.AttackRange + 500)).Where(minion => minion.Health <= Player.GetAutoAttackDamage(minion, true)))
            {
                Render.Circle.DrawCircle(minion.Position, minion.BoundingRadius, Color.LawnGreen);
            }
        }
    }
}