namespace ElUtilitySuite
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    internal class Entry
    {
        #region Public Properties

        public static bool IsHowlingAbyss
        {
            get
            {
                return Game.MapId == GameMapId.HowlingAbyss;
            }
        }

        public static Obj_AI_Hero Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }

        public static string ScriptVersion
        {
            get
            {
                return typeof(Entry).Assembly.GetName().Version.ToString();
            }
        }

        #endregion

        #region Properties

        public static bool IsSummonersRift
        {
            get
            {
                return Game.MapId == GameMapId.SummonersRift;
            }
        }

        public static bool IsTwistedTreeline
        {
            get
            {
                return Game.MapId == GameMapId.TwistedTreeline;
            }
        }

        #endregion

        #region Public Methods and Operators

        public static void OnLoad(EventArgs args)
        {
            try
            {
                Heal.Load();
                Ignite.Load();
                Barrier.Load();
                Notifications.AddNotification(string.Format("El Utility Suite by jQuery v{0}", ScriptVersion), 10000);
                //Game.PrintChat("<font color='#CC0000'>ElUtilitySuite</font> Work in progress.");
                InitializeMenu.Load();

                if (IsSummonersRift)
                {
                    Smite.Load();
                }
                else
                {
                    Console.WriteLine("Map not supported, report to jQuery kappa");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        #endregion
    }
}