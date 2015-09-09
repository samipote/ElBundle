namespace ElUtilitySuite
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    public class InitializeMenu
    {
        #region Static Fields

        public static Menu Menu;

        #endregion

        #region Public Methods and Operators

        /*
        * TO-DO:
        * Ignite drawing
        * Heal helper
        */

        public static void Load()
        {
            Menu = new Menu("ElUtilitySuite", "ElUtilitySuite", true);

            if (Entry.Player.GetSpellSlot("summonersmite") != SpellSlot.Unknown)
            {
                var smiteMenu = Menu.AddSubMenu(new Menu("Smite", "Smite"));
                {
                    //Important smites
                    smiteMenu.AddItem(
                        new MenuItem("ElSmite.Activated", "Activated").SetValue(
                            new KeyBind("M".ToCharArray()[0], KeyBindType.Toggle, true)));
                    if (Entry.IsSummonersRift)
                    {
                        smiteMenu.AddItem(new MenuItem("SRU_Dragon", "Dragon").SetValue(true));
                        smiteMenu.AddItem(new MenuItem("SRU_Baron", "Baron").SetValue(true));
                        smiteMenu.AddItem(new MenuItem("SRU_Red", "Red buff").SetValue(true));
                        smiteMenu.AddItem(new MenuItem("SRU_Blue", "Blue buff").SetValue(true));

                        //Bullshit smites
                        smiteMenu.AddItem(new MenuItem("SRU_Gromp", "Gromp").SetValue(false));
                        smiteMenu.AddItem(new MenuItem("SRU_Murkwolf", "Wolves").SetValue(false));
                        smiteMenu.AddItem(new MenuItem("SRU_Krug", "Krug").SetValue(false));
                        smiteMenu.AddItem(new MenuItem("SRU_Razorbeak", "Chicken camp").SetValue(false));
                        smiteMenu.AddItem(new MenuItem("Sru_Crab", "Crab").SetValue(false));
                    }

                    if (Entry.IsTwistedTreeline)
                    {
                        smiteMenu.AddItem(new MenuItem("TT_Spiderboss", "Vilemaw Enabled").SetValue(true));
                        smiteMenu.AddItem(new MenuItem("TT_NGolem", "Golem Enabled").SetValue(true));
                        smiteMenu.AddItem(new MenuItem("TT_NWolf", "Wolf Enabled").SetValue(true));
                        smiteMenu.AddItem(new MenuItem("TT_NWraith", "Wraith Enabled").SetValue(true));
                    }

                    //Killsteal submenu
                    smiteMenu.SubMenu("Killsteal")
                        .AddItem(new MenuItem("ElSmite.KS.Activated", "Use smite to killsteal").SetValue(true));

                    //Drawings
                    smiteMenu.SubMenu("Drawings")
                        .AddItem(new MenuItem("ElSmite.Draw.Range", "Draw smite Range").SetValue(new Circle()));
                    smiteMenu.SubMenu("Drawings")
                        .AddItem(new MenuItem("ElSmite.Draw.Text", "Draw smite text").SetValue(true));
                    smiteMenu.SubMenu("Drawings")
                        .AddItem(new MenuItem("ElSmite.Draw.Damage", "Draw smite Damage").SetValue(true));
                }
            }


            if (Entry.Player.GetSpellSlot("summonerheal") != SpellSlot.Unknown)
            {
                var healMenu = Menu.AddSubMenu(new Menu("Heal", "Heal"));
                {
                    healMenu.AddItem(new MenuItem("Heal.Activated", "Heal").SetValue(true));
                    healMenu.AddItem(new MenuItem("Heal.Predicted", "Predict damage").SetValue(true));
                    healMenu.AddItem(new MenuItem("Heal.HP", "Health percentage").SetValue(new Slider(20, 1)));

                    //healMenu.AddItem(new MenuItem("Heal.Ally.HP", "Ally health percentage")).SetValue(new Slider(20, 1));
                    //healMenu.AddItem(new MenuItem("seperator", ""));
                    /*healMenu.AddItem(new MenuItem("seperator1", "Don't heal: ")).SetFontStyle(FontStyle.Bold, SharpDX.Color.Red);

                    foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly && !hero.IsMe))
                    {
                        healMenu.AddItem(new MenuItem("Heal." + hero.CharData.BaseSkinName.ToLowerInvariant() + ".noult." + hero.ChampionName, hero.ChampionName).SetValue(false));
                    }*/
                }
            }

            if (Entry.Player.GetSpellSlot("summonerdot") != SpellSlot.Unknown)
            {
                var igniteMenu = Menu.AddSubMenu(new Menu("Ignite", "Ignite"));
                {
                    igniteMenu.AddItem(new MenuItem("Ignite.Activated", "Ignite").SetValue(true));
                    igniteMenu.AddItem(new MenuItem("Ignite.shieldCheck", "Check for shields").SetValue(true));
                }
            }

            /*if (Entry.Player.GetSpellSlot("summonerbarrier") != SpellSlot.Unknown)
            {
                var barrierMenu = Menu.AddSubMenu(new Menu("Barrier", "Barrier"));
                {
                    barrierMenu.AddItem(new MenuItem("Barrier.Activated", "Barrier").SetValue(true));
                    barrierMenu.AddItem(new MenuItem("Barrier.HP", "Health percentage").SetValue(new Slider(20, 1)));
                }
            }*/


            var credits = Menu.AddSubMenu(new Menu("Credits", "jQuery"));
            {
                credits.AddItem(new MenuItem("Paypal", "if you would like to donate via paypal:"));
                credits.AddItem(new MenuItem("Email", "info@zavox.nl"));
            }

            Menu.AddItem(new MenuItem("seperator", ""));
            Menu.AddItem(new MenuItem("Versionnumber", string.Format("Version: {0}", Entry.ScriptVersion)));
            Menu.AddItem(new MenuItem("by.jQuery", "Made By jQuery"));

            Menu.AddToMainMenu();
        }

        #endregion
    }
}