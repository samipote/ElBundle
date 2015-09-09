namespace ElUtilitySuite
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    // ReSharper disable once ClassNeverInstantiated.Global
    public static class Ignite
    {
        #region Static Fields

        private static Spell igniteSpell;

        private static SpellDataInst slot1;

        private static SpellDataInst slot2;

        private static SpellSlot summonerDot;

        #endregion

        #region Public Methods and Operators

        public static void Load()
        {
            try
            {
                slot1 = Entry.Player.Spellbook.GetSpell(SpellSlot.Summoner1);
                slot2 = Entry.Player.Spellbook.GetSpell(SpellSlot.Summoner2);

                //Soon riot will introduce multiple ignote, mark my words.
                var igniteNames = new[] { "summonerdot" };

                if (igniteNames.Contains(slot1.Name))
                {
                    igniteSpell = new Spell(SpellSlot.Summoner1, 550f);
                    summonerDot = SpellSlot.Summoner1;
                }
                else if (igniteNames.Contains(slot2.Name))
                {
                    igniteSpell = new Spell(SpellSlot.Summoner2, 550f);
                    summonerDot = SpellSlot.Summoner2;
                }
                else
                {
                    Console.WriteLine("You don't have ignite faggot");
                    return;
                }

                Game.OnUpdate += OnUpdate;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        #endregion

        #region Methods

        private static bool IgniteCheck(this Obj_AI_Base hero)
        {
            return hero.HasBuff("summonerdot") || hero.HasBuff("summonerbarrier") || hero.HasBuff("BlackShield")
                   || hero.HasBuff("SivirShield") || hero.HasBuff("BansheesVeil") || hero.HasBuff("ShroudofDarkness");
        }

        private static void IgniteKs()
        {
            var kSableEnemy =
                HeroManager.Enemies.FirstOrDefault(
                    hero =>
                    hero.IsValidTarget(550) && !IgniteCheck(hero) && !hero.IsZombie
                    && Entry.Player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite) >= hero.Health);

            if (kSableEnemy != null)
            {
                Entry.Player.Spellbook.CastSpell(summonerDot, kSableEnemy);
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
                IgniteKs();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        #endregion
    }
}