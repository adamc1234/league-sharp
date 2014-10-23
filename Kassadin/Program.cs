#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

//Credits to Snorflake for share templates, tutorials.

namespace Kassadin
{
    internal class Program
    {
        public const string ChampionName = "Kassadin";

        public static Orbwalking.Orbwalker Orbwalker;

        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        public static Menu DM;

        private static Obj_AI_Hero Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;

            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, 200);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 700);

            E.SetSkillshot(0.5f, float.MaxValue, float.MaxValue, false, SkillshotType.SkillshotCone);
            R.SetSkillshot(0.5f, 200f, 1200f, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            Game.PrintChat("Kassadin loaded by Dangerous Mind");

            DM = new Menu(ChampionName, ChampionName, true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            DM.AddSubMenu(targetSelectorMenu);

            DM.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

            Orbwalker = new Orbwalking.Orbwalker(DM.SubMenu("Orbwalking"));

            DM.AddSubMenu(new Menu("Combo", "Combo"));
            DM.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
            DM.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);
            DM.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true);
            DM.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R")).SetValue(true);
            DM.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            DM.SubMenu("Misc").AddItem(new MenuItem("packet", "Use Packets").SetValue(true));

            DM.AddSubMenu(new Menu("Harass", "Harass"));
            DM.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q")).SetValue(true);
            DM.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W")).SetValue(true);
            DM.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E")).SetValue(true);
            DM.SubMenu("Harass").AddItem(new MenuItem("UseRHarass", "Use R")).SetValue(true);
            DM.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind(menu.Item("Farm").GetValue<KeyBind>().Key, KeyBindType.Press)));

            
            DM.AddSubMenu(new Menu("Drawings", "Drawings"));
            DM.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, Color.Aquamarine)));
            DM.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(false, Color.Aquamarine)));
            DM.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(false, Color.Aquamarine)));
            DM.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R range").SetValue(new Circle(false, Color.Aquamarine)));

            

            DM.AddToMainMenu();

            //Add the events we are going to use:
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            //Draw the ranges of the spells.
            foreach (var spell in SpellList)
            {
                var menuItem = DM.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                {
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
                }
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            Orbwalker.SetAttacks(true);
            if (DM.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }

        }

        private static void Combo()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            {
                Orbwalker.SetMovement(true);
            }
            if (target != null)
            {
                if (R.IsReady() && Player.Distance(target) <= R.Range && DM.Item("UseRCombo").GetValue<bool>())
                {
                    R.Cast(target);
                }
                else if (W.IsReady() && Player.Distance(target) <= W.Range && DM.Item("UseWCombo").GetValue<bool>())
                {
                    W.Cast(target);
                }
                else if (Q.IsReady() && Player.Distance(target) <= Q.Range && DM.Item("UseQCombo").GetValue<bool>())
                {
                    Q.Cast(target);
                }
                else if (E.IsReady() && Player.Distance(target) <= E.Range && DM.Item("UseECombo").GetValue<bool>())
                {
                    E.Cast(target);
                }

            }

        }
        
        private static void UseSpells(bool useQ, bool useW, bool useE, bool useR)
        {
            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var wTarget = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
            var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            var rTarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);

            if (useQ && qTarget != null && Q.IsReady() && Player.Distance(qTarget) <= Q.Range)
            {
                Q.Cast(qTarget, packets());
                return;
            }

            if (useE && eTarget != null && E.IsReady() && Player.Distance(eTarget) <= E.Range)
            {
                E.Cast();
            }

            if (useR && rTarget != null && R.IsReady() && R.GetPrediction(rTarget).Hitchance >= HitChance.High && GetComboDamage(rTarget) > qTarget.Health + 150 && Player.Distance(rTarget) <= R.Range)
            {
                R.Cast(rTarget, packets());
                return;
            }

        }
        
        public static bool packets()
        {
            return menu.Item("packet").GetValue<bool>();
        }

        private static void Harass()
        {
            Orbwalker.SetAttack(!(menu.Item("MoveToMouse").GetValue<KeyBind>().Active));
            UseSpells(menu.Item("UseQHarass").GetValue<bool>(), menu.Item("UseWHarass").GetValue<bool>(),
                menu.Item("UseEHarass").GetValue<bool>(), false);
        }
    }
}
