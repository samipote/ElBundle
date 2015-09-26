namespace ElUtilitySuite
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    internal static class Defensive
    {
        #region Public Methods and Operators

        public static void Load()
        {
            try
            {
                Game.OnUpdate += OnUpdate;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        #endregion

        #region Methods

        private static int CountHerosInRange(this Obj_AI_Hero target, bool checkteam, float range = 1200f)
        {
            var objListTeam = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(range, false));

            return objListTeam.Count(hero => checkteam ? hero.Team != target.Team : hero.Team == target.Team);
        }

        private static void DefensiveItemManager()
        {
            if (InitializeMenu.Menu.Item("usecombo").GetValue<KeyBind>().Active)
            {
                UseItemCount("Randuins", 3143, 450f);
            }

            if (Items.HasItem(3069) && Items.CanUseItem(3069)
                && InitializeMenu.Menu.Item("useTalisman").GetValue<bool>())
            {
                if (!InitializeMenu.Menu.Item("usecombo").GetValue<KeyBind>().Active
                    && InitializeMenu.Menu.Item("talismanMode").GetValue<StringList>().SelectedIndex == 1)
                {
                    return;
                }

                var target = Entry.Allies();
                if (target.Distance(Entry.Player.ServerPosition, true) > 600 * 600)
                {
                    return;
                }

                var lowTarget =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .OrderBy(ex => ex.Health / ex.MaxHealth * 100)
                        .First(x => x.IsValidTarget(1000));

                var aHealthPercent = target.Health / target.MaxHealth * 100;
                var eHealthPercent = lowTarget.Health / lowTarget.MaxHealth * 100;

                if (lowTarget.Distance(target.ServerPosition, true) <= 900 * 900
                    && (target.CountHerosInRange(false) > target.CountHerosInRange(true)
                        && eHealthPercent <= InitializeMenu.Menu.Item("useEnemyPct").GetValue<Slider>().Value))
                {
                    Items.UseItem(3069);
                }

                if (target.CountHerosInRange(false) > target.CountHerosInRange(true)
                    && aHealthPercent <= InitializeMenu.Menu.Item("useAllyPct").GetValue<Slider>().Value)
                {
                    Items.UseItem(3069);
                }
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Entry.Player.IsDead)
            {
                return;
            }

            try
            {
                DefensiveItemManager();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        private static void UseItemCount(string name, int itemId, float itemRange)
        {
            if (!Items.HasItem(itemId) || !Items.CanUseItem(itemId))
            {
                return;
            }

            if (InitializeMenu.Menu.Item("use" + name).GetValue<bool>())
            {
                if (Entry.Player.CountHerosInRange(true, itemRange)
                    >= InitializeMenu.Menu.Item("use" + name + "Count").GetValue<Slider>().Value)
                {
                    Items.UseItem(itemId);
                }
            }
        }

        #endregion
    }
}