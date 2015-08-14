using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;


namespace ElTristana
{

    public class MenuInit
    {

        public static Menu Menu;

        public static void Initialize()
        {
            Menu = new Menu("ElTristana WIP", "menu", true);

            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Tristana.Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            Menu.AddSubMenu(orbwalkerMenu);

            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);

            Menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("Combo", "Combo");
            cMenu.AddItem(new MenuItem("ElTristana.Combo.Q", "Use Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElTristana.Combo.E", "Use E").SetValue(true));
            cMenu.AddItem(new MenuItem("ElTristana.Combo.R", "Use R").SetValue(true));

            Menu.AddSubMenu(cMenu);

            var credits = new Menu("Credits", "jQuery");
            credits.AddItem(new MenuItem("ElTristana.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElTristana.Email", "info@zavox.nl"));
            Menu.AddSubMenu(credits);

            Menu.AddItem(new MenuItem("422442fsaafs4242f", ""));

            Menu.AddItem(new MenuItem("422442fsaafsf", ($"ElTristana by jQuery v{Tristana.ScriptVersion}")));
            Menu.AddItem(new MenuItem("fsasfafsfsafsa", "Made By jQuery"));

            Menu.AddToMainMenu();

            Console.WriteLine("Menu Loaded");
        }
    }
}