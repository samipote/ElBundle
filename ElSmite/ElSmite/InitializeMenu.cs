﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ElSmite
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class InitializeMenu
    {
        public static Menu Menu;
        public static void Load()
        {
            Menu = new Menu("ElSmite", "ElSmite", true);

            var mainMenu = Menu.AddSubMenu(new Menu("Default", "Default"));
            {
                mainMenu.AddItem(new MenuItem("ElSmite.Activated", "Activated").SetValue(new KeyBind("M".ToCharArray()[0], KeyBindType.Toggle, true)));
                mainMenu.AddItem(new MenuItem("SRU_Dragon", "Dragon").SetValue(true));
                mainMenu.AddItem(new MenuItem("SRU_Baron", "Baron").SetValue(true));
                mainMenu.AddItem(new MenuItem("SRU_Red", "Red buff").SetValue(true));
                mainMenu.AddItem(new MenuItem("SRU_Blue", "Blue buff").SetValue(true));

                //Bullshit smites
                mainMenu.AddItem(new MenuItem("SRU_Gromp", "Gromp").SetValue(false));
                mainMenu.AddItem(new MenuItem("SRU_Murkwolf", "Wolves").SetValue(false));
                mainMenu.AddItem(new MenuItem("SRU_Krug", "Krug").SetValue(false));
                mainMenu.AddItem(new MenuItem("SRU_Razorbeak", "Chicken camp").SetValue(false));
                mainMenu.AddItem(new MenuItem("Sru_Crab", "Crab").SetValue(false));
            }

            var combatMenu = Menu.AddSubMenu(new Menu("Killsteal", "Killsteal"));
            {
                combatMenu.AddItem(new MenuItem("ElSmite.KS.Activated", "Use smite to killsteal").SetValue(true));
            }

            var drawMenu = Menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            {
                drawMenu.AddItem(new MenuItem("ElSmite.Draw.Range", "Draw smite Range").SetValue(new Circle()));
                drawMenu.AddItem(new MenuItem("ElSmite.Draw.Text", "Draw smite text").SetValue(true));
            }

            var credits = Menu.AddSubMenu(new Menu("Credits", "jQuery"));
            {
                credits.AddItem(new MenuItem("ElRengar.Paypal", "if you would like to donate via paypal:"));
                credits.AddItem(new MenuItem("ElRengar.Email", "info@zavox.nl"));
            }

            Menu.AddItem(new MenuItem("seperator", ""));
            Menu.AddItem(new MenuItem("422442fsaafsf", String.Format("Version: {0}", Entry.ScriptVersion)));
            Menu.AddItem(new MenuItem("by.jQuery", "Made By jQuery"));

            Menu.AddToMainMenu();
        }
    }
}
