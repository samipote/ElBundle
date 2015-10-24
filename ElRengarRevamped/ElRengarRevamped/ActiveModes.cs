namespace ElRengarRevamped
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using ItemData = LeagueSharp.Common.Data.ItemData;

    // ReSharper disable once ClassNeverInstantiated.Global
    public class ActiveModes : Standards
    {
        #region Static Fields

        public static int Kappa = 1;

        #endregion

        #region Public Methods and Operators

        public static void Combo()
        {
            try
            {
               /* var target = TargetSelector.GetSelectedTarget();
                if (target == null || !target.IsValidTarget() || TargetSelector.GetSelectedTarget() == null)
                {
                    target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Physical);
                }

                if (TargetSelector.GetSelectedTarget() != null)
                {
                    if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < spells[Spells.Q].Range)
                    {
                        target = TargetSelector.GetSelectedTarget();
                        TargetSelector.SetTarget(target);
                    }
                }*/

                var target = TargetSelector.GetTarget(spells[Spells.R].Range, TargetSelector.DamageType.Physical);
                if (!target.IsValidTarget() || target == null)
                {
                    return;
                }

                if (Rengar._selectedEnemy.IsValidTarget())
                {
                    Rengar._selectedEnemy = target;
                }

                if (Player.Spellbook.IsAutoAttacking || Player.IsWindingUp)
                {
                    return;
                }

                if (Youmuu.IsReady() && Player.Distance(target) <= spells[Spells.Q].Range && target.IsValidTarget())
                {
                    Youmuu.Cast(Player);
                }

                UseItems(target);

                #region RengarR

                if (Ferocity == 5)
                {
                    switch (IsListActive("Combo.Prio").SelectedIndex)
                    {
                        case 0:
                            if (!RengarR && Rengar.LastE + 200 < Environment.TickCount && target.IsValidTarget())
                            {
                                var prediction = spells[Spells.E].GetPrediction(target);
                                if (prediction.Hitchance >= HitChance.High && prediction.CollisionObjects.Count == 0)
                                {
                                    if (spells[Spells.E].Cast(target) == Spell.CastStates.SuccessfullyCasted)
                                    {
                                        if (IsActive("Combo.Switch.E") && Utils.GameTimeTickCount - LastSwitch >= 350)
                                        {
                                            Console.WriteLine("SWITCH COMBO");
                                            MenuInit.Menu.Item("Combo.Prio")
                                                .SetValue(new StringList(new[] { "E", "W", "Q" }, 2));
                                            LastSwitch = Utils.GameTimeTickCount;
                                        }
                                    }
                                    return;
                                }
                            }
                            else if (!RengarR && IsActive("Combo.Use.E") && spells[Spells.E].IsReady()
                                     && Vector3.Distance(Player.ServerPosition, target.ServerPosition)
                                     <= spells[Spells.E].Range)
                            {
                                var prediction = spells[Spells.E].GetPrediction(target);
                                if (prediction.Hitchance >= HitChance.High && prediction.CollisionObjects.Count == 0 && target.IsValidTarget())
                                {
                                    if (spells[Spells.E].Cast(target) == Spell.CastStates.SuccessfullyCasted)
                                    {
                                        if (IsActive("Combo.Switch.E") && Utils.GameTimeTickCount - LastSwitch >= 350)
                                        {
                                            Console.WriteLine("SWITCH COMBO");
                                            MenuInit.Menu.Item("Combo.Prio")
                                                .SetValue(new StringList(new[] { "E", "W", "Q" }, 2));
                                            LastSwitch = Utils.GameTimeTickCount;
                                        }
                                    }
                                    return;
                                }
                            }
                            break;
                        case 1:
                            if (spells[Spells.W].IsReady() && !RengarR
                                && Vector3.Distance(Player.ServerPosition, target.ServerPosition)
                                < spells[Spells.W].Range && !Player.IsDashing() && !HasPassive)
                            {
                                if (Rengar.LastW + 200 < Environment.TickCount && target.IsValidTarget())
                                {
                                    CastW(target);
                                    return;
                                }
                            }
                            break;
                        case 2:
                            if (IsActive("Combo.Use.Q") && spells[Spells.Q].IsInRange(target) && target.IsValidTarget())
                            {
                                if (Rengar.LastQ + 200 < Environment.TickCount)
                                {
                                    spells[Spells.Q].Cast();
                                    return;
                                }
                            }
                            break;
                    }
                }

                if (Ferocity <= 4)
                {
                    if (IsActive("Combo.Use.Q") && Rengar.LastQ + 200 < Environment.TickCount
                        && target.Distance(Player) < spells[Spells.Q].Range && target.IsValidTarget(spells[Spells.Q].Range))
                    {

                        if (target.IsValidTarget(spells[Spells.Q].Range) && !Orbwalking.IsAutoAttack(Player.LastCastedSpellName()))
                        {
                            return;
                        }

                        spells[Spells.Q].Cast();
                    }

                    if (RengarR)
                    {
                        return;
                    }

                   if (IsActive("Combo.Use.W"))
                    {
                        CastW(target);
                    }
                    
                    if (IsActive("Combo.Use.E"))
                    {
                        if (!target.IsValidTarget(spells[Spells.E].Range) && !HasPassive)
                        {
                            return;
                        }

                        CastE(target);
                        return;
                    }

                    if (IsActive("Combo.Use.E.OutOfRange") && Player.Distance(target) > Player.AttackRange + 100
                        && !RengarR && Ferocity == 5)
                    {
                        var prediction = spells[Spells.E].GetPrediction(target);
                        if (prediction.Hitchance >= HitChance.VeryHigh && prediction.CollisionObjects.Count == 0)
                        {
                            spells[Spells.E].Cast(target);
                            return;
                        }
                    }
                }

                #region Summoner spells

                if (IsActive("Combo.Use.Ignite") && Player.Distance(target) <= 600
                    && IgniteDamage(target) >= target.Health)
                {
                    Player.Spellbook.CastSpell(Ignite, target);
                    return;
                }

                if (IsActive("Combo.Use.Smite") && Smite != SpellSlot.Unknown
                    && Player.Spellbook.CanUseSpell(Smite) == SpellState.Ready)
                {
                    Player.Spellbook.CastSpell(Smite, target);
                }

                #endregion
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }

        #endregion

        #endregion

        #region Methods

        private static void CastW(Obj_AI_Base target)
        {
            if (!spells[Spells.W].IsReady() || !target.IsValidTarget(range: spells[Spells.W].Range) && !HasPassive)
            {
                return;
            }

            spells[Spells.W].Cast();
        }


        private static void CastE(Obj_AI_Base target)
        {
            if (!spells[Spells.E].IsReady() || !target.IsValidTarget(spells[Spells.E].Range) || Player.IsDashing())
            {
                return;
            }

            var prediction = spells[Spells.E].GetPrediction(target);

            if (prediction.Hitchance != HitChance.Impossible && prediction.Hitchance != HitChance.OutOfRange && prediction.Hitchance != HitChance.Collision)
            {
                spells[Spells.E].Cast(prediction.CastPosition);
            }
        }

        private static void UseItems(Obj_AI_Base target)
        {
            if (ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady()
                && ItemData.Ravenous_Hydra_Melee_Only.Range > Player.Distance(target))
            {
                ItemData.Ravenous_Hydra_Melee_Only.GetItem().Cast();
            }
            if (ItemData.Tiamat_Melee_Only.GetItem().IsReady()
                && ItemData.Tiamat_Melee_Only.Range > Player.Distance(target))
            {
                ItemData.Tiamat_Melee_Only.GetItem().Cast();
            }

            if (ItemData.Blade_of_the_Ruined_King.GetItem().IsReady()
                && ItemData.Blade_of_the_Ruined_King.Range > Player.Distance(target))
            {
                ItemData.Blade_of_the_Ruined_King.GetItem().Cast(target);
            }

            if (ItemData.Bilgewater_Cutlass.GetItem().IsReady()
                && ItemData.Bilgewater_Cutlass.Range > Player.Distance(target))
            {
                ItemData.Bilgewater_Cutlass.GetItem().Cast(target);
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (ItemData.Youmuus_Ghostblade.GetItem().IsReady()
                    && Orbwalking.GetRealAutoAttackRange(Player) > Player.Distance(target))
                {
                    ItemData.Youmuus_Ghostblade.GetItem().Cast();
                }
            }
        }

        #endregion

        public static void Harass()
        {
            var target = TargetSelector.GetSelectedTarget();
            if (target == null || !target.IsValidTarget() || TargetSelector.GetSelectedTarget() == null)
            {
                target = TargetSelector.GetTarget(spells[Spells.R].Range, TargetSelector.DamageType.Physical);
            }

            if (TargetSelector.GetSelectedTarget() != null)
            {
                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < spells[Spells.R].Range)
                {
                    target = TargetSelector.GetSelectedTarget();
                    TargetSelector.SetTarget(target);
                    Hud.SelectedUnit = target;
                }
            }

            target = TargetSelector.GetTarget(spells[Spells.R].Range, TargetSelector.DamageType.Physical);
            if (!target.IsValidTarget())
            {
                return;
            }

            #region RengarR

            if (Ferocity == 5)
            {
                switch (IsListActive("Harass.Prio").SelectedIndex)
                {
                    case 0:
                        if (!HasPassive && IsActive("Harass.Use.E") && spells[Spells.E].IsReady()
                            && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= spells[Spells.E].Range)
                        {
                            var prediction = spells[Spells.E].GetPrediction(target);
                            if (prediction.Hitchance >= HitChance.High && prediction.CollisionObjects.Count == 0)
                            {
                                spells[Spells.E].Cast(target.ServerPosition);
                            }
                        }
                        break;

                    case 1:
                        if (IsActive("Harass.Use.Q") && !ObjectManager.Player.Spellbook.IsAutoAttacking
                            && !Player.IsWindingUp
                            && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= spells[Spells.Q].Range)
                        {
                            spells[Spells.Q].Cast();
                        }
                        break;
                }
            }

            if (Ferocity <= 4)
            {
                if (IsActive("Harass.Use.Q") && !ObjectManager.Player.Spellbook.IsAutoAttacking && !Player.IsWindingUp
                    && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= spells[Spells.Q].Range)
                {
                    spells[Spells.Q].Cast();
                }

                if (RengarR)
                {
                    return;
                }

                if (!HasPassive && IsActive("Harass.Use.E") && spells[Spells.E].IsReady()
                    && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= spells[Spells.E].Range)
                {
                    var prediction = spells[Spells.E].GetPrediction(target);
                    if (prediction.Hitchance >= HitChance.High && prediction.CollisionObjects.Count == 0)
                    {
                        spells[Spells.E].Cast(target.ServerPosition);
                    }
                }

                if (IsActive("Harass.Use.W")
                    && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < spells[Spells.W].Range * 1 / 3)
                {
                    UseHydra();
                    spells[Spells.W].Cast();
                }

                if (!IsActive("Harass.Use.W")
                    || !spells[Spells.W].IsReady()
                    && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < spells[Spells.W].Range)
                {
                    UseHydra();
                }
            }
        }

        public static void Jungleclear()
        {
            var minion =
                MinionManager.GetMinions(
                    Player.ServerPosition,
                    spells[Spells.W].Range + 100,
                    MinionTypes.All,
                    MinionTeam.Neutral,
                    MinionOrderTypes.MaxHealth).FirstOrDefault();

            if (minion == null)
            {
                return;
            }

            UseItems(minion);

            if (Ferocity == 5 && IsActive("Jungle.Save.Ferocity"))
            {
                if (Vector3.Distance(Player.ServerPosition, minion.ServerPosition) < spells[Spells.W].Range)
                {
                    UseHydra();
                }
                return;
            }

            if (IsActive("Jungle.Use.Q") && spells[Spells.Q].IsReady() && Rengar.LastQ + 200 < Environment.TickCount)
            {
                spells[Spells.Q].Cast();
                return;
            }

            if (IsActive("Jungle.Use.W") && spells[Spells.W].IsReady()
                && Vector3.Distance(Player.ServerPosition, minion.ServerPosition) <= 450)
            {
                UseHydra();
                spells[Spells.W].Cast();
            }

            if (IsActive("Jungle.Use.E") && spells[Spells.E].IsReady()
                && Vector3.Distance(Player.ServerPosition, minion.ServerPosition) < spells[Spells.E].Range)
            {
                spells[Spells.E].Cast(minion.Position);
            }
        }

        public static void Laneclear()
        {
            var minion = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.W].Range).FirstOrDefault();
            if (minion == null)
            {
                return;
            }

            if (Player.Spellbook.IsAutoAttacking || Player.IsWindingUp)
            {
                return;
            }

            if (Ferocity == 5 && IsActive("Clear.Save.Ferocity"))
            {
                if (Vector3.Distance(Player.ServerPosition, minion.ServerPosition) < spells[Spells.W].Range * 1 / 3)
                {
                    UseHydra();
                }
                return;
            }

            if (!Player.Spellbook.IsAutoAttacking && IsActive("Clear.Use.Q") && spells[Spells.Q].IsReady()
                && Utils.GameTimeTickCount - Rengar.LastAutoAttack < 5700 && Player.CanMove)
            {
                spells[Spells.Q].Cast();
                return;
            }

            if (IsActive("Clear.Use.W") && spells[Spells.W].IsReady()
                && Vector3.Distance(Player.ServerPosition, minion.ServerPosition) < spells[Spells.W].Range * 1 / 3)
            {
                UseHydra();
                spells[Spells.W].Cast();
            }

            if (IsActive("Clear.Use.E") && spells[Spells.E].GetDamage(minion) > minion.Health
                && spells[Spells.E].IsReady()
                && Vector3.Distance(Player.ServerPosition, minion.ServerPosition) < spells[Spells.E].Range)
            {
                spells[Spells.E].Cast(minion.Position);
            }
        }

        #endregion
    }
}
