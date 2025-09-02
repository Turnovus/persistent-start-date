using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentDate
{
    public class PersistentDate_Mod : Mod
    {
        public const float HeaderSpace = 26f;
        public const float RowHeight = 35f;
        public const float DateSelectorWidthRatio = 0.46f;
        public const float ButtonWidth = 45f;
        public const float YearInputGap = 5f;
        
        public PersistentDate_ModSettings Settings => GetSettings<PersistentDate_ModSettings>();
        
        public PersistentDate_Mod(ModContentPack content) : base(content)
        {
        }

        public override string SettingsCategory() => "Persistent Date";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect rowRect = new Rect(0, HeaderSpace, inRect.width, RowHeight);
            
            // Header for date selector
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rowRect, "!!TODO!! Current Date:");
            Text.Anchor = anchor;
            
            rowRect.y += RowHeight;
            Rect selectorRect = rowRect.ContractedBy(rowRect.width * 0.5f * (1f - DateSelectorWidthRatio), 0f);
            Widgets.DrawHighlight(selectorRect);

            Rect buttonRect = new Rect(selectorRect);
            buttonRect.width = ButtonWidth;

            bool changeDay = Widgets.ButtonText(buttonRect, Settings.day.ToString());
            buttonRect.x += ButtonWidth;

            bool changeQuadrum = Widgets.ButtonText(buttonRect, QuadrumString(Settings.quadrum));

            Rect yearRect = new Rect(
                buttonRect.x + ButtonWidth + YearInputGap,
                buttonRect.y,
                selectorRect.width - 2f * ButtonWidth - YearInputGap,
                selectorRect.height);
            
            int year = Settings.Year + GenDate.DefaultStartingYear;
            string yearBuffer = year.ToString();
            Widgets.IntEntry(yearRect, ref year, ref yearBuffer);
            year -= GenDate.DefaultStartingYear;
            
            HandleDayButton(changeDay);
            HandleQuadrumButton(changeQuadrum);
            HandleYearInput(year);
        }

        private void HandleDayButton(bool pressed)
        {
            if (!pressed)
                return;

            List<FloatMenuOption> dayOptions = new List<FloatMenuOption>();
            for (int i = 1; i <= GenDate.DaysPerQuadrum; i++)
            {
                var day = i;
                dayOptions.Add(new FloatMenuOption(
                    i.ToString(),
                    delegate { Settings.day = day; }
                    ));
            }
            
            Find.WindowStack.Add(new FloatMenu(dayOptions));
        }

        private void HandleQuadrumButton(bool pressed)
        {
            if (!pressed)
                return;

            List<FloatMenuOption> quadrumOptions = new List<FloatMenuOption>();
            foreach (Quadrum quadrum in QuadrumUtility.QuadrumsInChronologicalOrder)
            {
                quadrumOptions.Add(new FloatMenuOption(
                    QuadrumString(quadrum),
                    delegate { Settings.quadrum = quadrum; }
                    ));
            }
            
            Find.WindowStack.Add(new FloatMenu(quadrumOptions));
        }

        private void HandleYearInput(int year) => Settings.Year = year;

        private static string QuadrumString(Quadrum quadrum)
        {
            switch (quadrum)
            {
                case Quadrum.Aprimay:
                    return "Apr";
                case Quadrum.Jugust:
                    return "Jug";
                case Quadrum.Septober:
                    return "Sep";
                case Quadrum.Decembary:
                    return "Dec";
            }
            return "???";
        }
    }

    public class PersistentDate_ModSettings : ModSettings
    {
        private int year = 0;
        public Quadrum quadrum = Quadrum.Aprimay;
        public int day = 1;

        public int Year
        {
            get => year;
            set
            {
                year = Math.Max(value, 0);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref year, "year");
            Scribe_Values.Look(ref quadrum, "quadrum");
            Scribe_Values.Look(ref day, "day");
        }
    }
}