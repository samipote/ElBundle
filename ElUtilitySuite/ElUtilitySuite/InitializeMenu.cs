namespace ElUtilitySuite
{
    using LeagueSharp.Common;

    public class InitializeMenu
    {
        #region Static Fields

        public static Menu Menu;

        #endregion

        #region Public Methods and Operators

        public static void Load()
        {
            Menu = new Menu("ElUtilitySuite", "ElUtilitySuite", true);

            var smiteMenu = Menu.AddSubMenu(new Menu("Smite", "Smite"));
            {
                //Important smites
                smiteMenu.AddItem(
                    new MenuItem("ElSmite.Activated", "Activated").SetValue(
                        new KeyBind("M".ToCharArray()[0], KeyBindType.Toggle, true)));
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

            var healMenu = Menu.AddSubMenu(new Menu("Heal", "Heal"));
            {
                healMenu.AddItem(new MenuItem("Heal.Activated", "Heal").SetValue(true));
                healMenu.AddItem(new MenuItem("Heal.Predicted", "Predict damage").SetValue(true));
                healMenu.AddItem(new MenuItem("Heal.HP", "Health percentage").SetValue(new Slider(20, 1)));

                healMenu.AddItem(new MenuItem("Heal.Ally.HP", "Ally health percentage")).SetValue(new Slider(20, 1));
                healMenu.AddItem(new MenuItem("seperator", ""));
                /*healMenu.AddItem(new MenuItem("seperator1", "Don't heal: ")).SetFontStyle(FontStyle.Bold, SharpDX.Color.Red);

                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly && !hero.IsMe))
                {
                    healMenu.AddItem(new MenuItem("Heal." + hero.CharData.BaseSkinName.ToLowerInvariant() + ".noult." + hero.ChampionName, hero.ChampionName).SetValue(false));
                }*/
            }

            var igniteMenu = Menu.AddSubMenu(new Menu("Ignite", "Ignite"));
            {
                igniteMenu.AddItem(new MenuItem("Ignite.Activated", "Ignite").SetValue(true));
            }

            /*var barrierMenu = Menu.AddSubMenu(new Menu("Barrier", "Barrier"));
            {
                barrierMenu.AddItem(new MenuItem("Barrier.Activated", "Barrier").SetValue(true));
                barrierMenu.AddItem(new MenuItem("Barrier.HP", "Health percentage").SetValue(new Slider(20, 1)));
            }*/

            Menu.AddItem(new MenuItem("seperator", ""));
            Menu.AddItem(new MenuItem("Versionnumber", string.Format("Version: {0}", Entry.ScriptVersion)));
            Menu.AddItem(new MenuItem("by.jQuery", "Made By jQuery"));

            Menu.AddToMainMenu();
        }

        #endregion
    }
}