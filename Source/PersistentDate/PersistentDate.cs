using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
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
        public const float ResetButtonWidth = 120f;
        public const float YearInputGap = 5f;
        
        public PersistentDate_ModSettings Settings => GetSettings<PersistentDate_ModSettings>();
        
        public PersistentDate_Mod(ModContentPack content) : base(content)
        {
        }

        public override string SettingsCategory() => "PersistentDate.SettingsCategory".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect rowRect = new Rect(0, HeaderSpace, inRect.width, RowHeight);
            
            // Header for date selector
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rowRect, "PersistentDate.Settings.CurrentDateLabel".Translate());
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
                    return "PersistentDate.Settings.QuadrumShort.Aprimay".Translate();
                case Quadrum.Jugust:
                    return "PersistentDate.Settings.QuadrumShort.Jugust".Translate();
                case Quadrum.Septober:
                    return "PersistentDate.Settings.QuadrumShort.Septober".Translate();
                case Quadrum.Decembary:
                    return "PersistentDate.Settings.QuadrumShort.Decembary".Translate();
            }
            return "PersistentDate.Settings.QuadrumShort.Unknown".Translate();
        }
    }

    public class PersistentDate_ModSettings : ModSettings
    {
        private const int MinYear = -3500;
        private const int MaxYear = 500_000_000;
        
        private int year = 0;
        public Quadrum quadrum = Quadrum.Aprimay;
        public int day = 1;

        public int Year
        {
            get => year;
            set => year = Math.Min( Math.Max(value, MinYear),  MaxYear );
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref year, "year");
            Scribe_Values.Look(ref quadrum, "quadrum");
            Scribe_Values.Look(ref day, "day");
        }
    }

    public class PersistentDate_GameComponent : GameComponent
    {
        public int startYearOffset = GenDate.DefaultStartingYear;

        private PersistentDate_ModSettings Settings => LoadedModManager.GetMod<PersistentDate_Mod>().Settings;
        
        public PersistentDate_GameComponent(Game game)
        {
        }

        public override void StartedNewGame()
        {
            startYearOffset = Settings.Year;

            PlanetTile startTile = Find.GameInitData.startingTile;
            Vector2 startCoordinates = Find.WorldGrid.LongLatOf(startTile);
            Quadrum startQuadrum = GenDate.Quadrum(Find.TickManager.TicksAbs, startCoordinates.x);
            int startDay = GenDate.DayOfTwelfth(Find.TickManager.TicksAbs, startCoordinates.x);

            if (startQuadrum > Settings.quadrum || (startQuadrum == Settings.quadrum && startDay <= Settings.day))
                return;

            startYearOffset += 1;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref startYearOffset, "startYearOffset");
        }
    }
}