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
        public const float YearInputGap = 5f;
        public const float ResetButtonWidth = 120f;
        public const float ResetButtonGap = 10f;

        private static readonly List<TimekeepingMode> AllModes = new List<TimekeepingMode>()
        {
            TimekeepingMode.UseLatest,
            TimekeepingMode.UseCurrent,
            TimekeepingMode.Cumulative,
            TimekeepingMode.Disabled,
        };
        
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
            
            // Start date selector
            rowRect.y += RowHeight;
            Rect selectorRect = rowRect.ContractedBy(rowRect.width * 0.5f * (1f - DateSelectorWidthRatio), 0f);
            selectorRect.x -= 0.5f * (ResetButtonWidth + ResetButtonGap);
            
            Rect buttonRect = new Rect(selectorRect);
            buttonRect.width = ButtonWidth;
            
            // Day selector
            bool changeDay = Widgets.ButtonText(buttonRect, Settings.day.ToString());
            buttonRect.x += ButtonWidth;
            
            // Season selector
            bool changeQuadrum = Widgets.ButtonText(buttonRect, QuadrumString(Settings.quadrum));
            
            // Year selector
            Rect yearRect = new Rect(
                buttonRect.x + ButtonWidth + YearInputGap,
                buttonRect.y,
                selectorRect.width - 2f * ButtonWidth - YearInputGap,
                selectorRect.height);
            int year = Settings.Year + GenDate.DefaultStartingYear;
            string yearBuffer = year.ToString();
            Widgets.IntEntry(yearRect, ref year, ref yearBuffer);
            year -= GenDate.DefaultStartingYear;
            
            // Reset Button
            Rect dateResetRect = new Rect(selectorRect);
            dateResetRect.x += dateResetRect.width + ResetButtonGap;
            dateResetRect.width = ResetButtonWidth;
            bool resetDate = Widgets.ButtonText(dateResetRect, "PersistentDate.Settings.Reset".Translate());
            
            // Mode Selector
            rowRect.y += RowHeight * 2f;
            Rect modeSelectRect = rowRect.ContractedBy(rowRect.width * 0.5f * (1f - DateSelectorWidthRatio), 0f);
            TooltipHandler.TipRegion(modeSelectRect, GetCurrentModeTooltip());
            
            Rect modeSelectHalfRect = new Rect(modeSelectRect);
            modeSelectHalfRect.width *= 0.5f;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(modeSelectHalfRect, "PersistentDate.Settings.TimekeepingMode".Translate());

            modeSelectHalfRect.x += modeSelectHalfRect.width;
            bool changeMode = Widgets.ButtonText(modeSelectHalfRect, GetCurrentModeLabel());
            
            // Process Input
            HandleDayButton(changeDay);
            HandleQuadrumButton(changeQuadrum);
            HandleYearInput(year);
            HandleDateResetButton(resetDate);
            HandleModeButton(changeMode);
            
            // Cleanup
            Text.Anchor = anchor;
        }

        private string GetCurrentModeTooltip()
        {
            string tip = "PersistentDate.Settings.TimekeepingMode.Desc".Translate();

            tip += "\n\n";
            string modeKey = "PersistentDate.Settings.TimekeepingMode." + Settings.mode.ToString() + ".Desc";
            tip += modeKey.Translate();
            
            return tip;
        }

        private string GetCurrentModeLabel() => GetModeLabel(Settings.mode);

        private string GetModeLabel(TimekeepingMode mode)
        {
            string key = "PersistentDate.Settings.TimekeepingMode." + mode.ToString();
            return key.Translate();
        }

        private void HandleDayButton(bool pressed)
        {
            if (!pressed)
                return;

            List<FloatMenuOption> dayOptions = new List<FloatMenuOption>();
            for (int i = 1; i <= GenDate.DaysPerQuadrum; i++)
            {
                int day = i;
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

        private void HandleDateResetButton(bool reset)
        {
            if (reset)
                Settings.ResetDate();
        }

        private void HandleModeButton(bool pressed)
        {
            if (!pressed)
                return;

            List<FloatMenuOption> modeOptions = new List<FloatMenuOption>();
            foreach (TimekeepingMode mode in AllModes)
            {
                modeOptions.Add(new FloatMenuOption(
                    GetModeLabel(mode),
                    delegate { Settings.mode = mode; }
                    ));
            }
            
            Find.WindowStack.Add(new FloatMenu(modeOptions));
        }

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
        public TimekeepingMode mode = TimekeepingMode.UseLatest;
        private string cumulativeTimeStamp = "";

        public int Year
        {
            get => year;
            set => year = Math.Min( Math.Max(value, MinYear),  MaxYear );
        }

        public void ResetDate()
        {
            year = 0;
            quadrum = Quadrum.Aprimay;
            day = 1;
        }

        public void TryIncrementCumulativeDate(string timestamp)
        {
            if (timestamp == cumulativeTimeStamp)
                return;

            cumulativeTimeStamp = timestamp;
            
            day += 1;
            if (day > GenDate.DaysPerQuadrum)
            {
                day = 1;
                quadrum = GetNextQuadrum(quadrum);
                if (quadrum == QuadrumUtility.FirstQuadrum)
                    year += 1;
            }
            
            Write();
        }

        private Quadrum GetNextQuadrum(Quadrum current)
        {
            int index = QuadrumUtility.QuadrumsInChronologicalOrder.IndexOf(current) + 1;
            if (index >= QuadrumUtility.QuadrumsInChronologicalOrder.Count)
                index = 0;
            return QuadrumUtility.QuadrumsInChronologicalOrder[index];
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref year, "year");
            Scribe_Values.Look(ref quadrum, "quadrum");
            Scribe_Values.Look(ref day, "day");
            Scribe_Values.Look(ref mode, "mode");
            Scribe_Values.Look(ref cumulativeTimeStamp, "cumulativeTimeStamp");
        }
    }

    public class PersistentDate_GameComponent : GameComponent
    {
        public int startYearOffset = 0;
        private int dayCounter = 0;

        private PersistentDate_ModSettings Settings => LoadedModManager.GetMod<PersistentDate_Mod>().Settings;
        
        public PersistentDate_GameComponent(Game game)
        {
        }

        private string GetTimestamp()
        {
            string stamp = Find.World.info.name;
            stamp += " " + Find.World.ConstantRandSeed;
            stamp += " " + Find.TickManager.TicksAbs.ToString();
            return stamp;
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

        public override void GameComponentTick()
        {
            dayCounter++;
            if (dayCounter >= GenDate.TicksPerDay)
            {
                dayCounter = 0;
                if (Settings.mode == TimekeepingMode.Cumulative)
                {
                    Settings.TryIncrementCumulativeDate(GetTimestamp());
                    return;
                }
            }

            if (dayCounter % 6000 != 0 || Settings.mode == TimekeepingMode.Disabled)
                return;

            Date currentDate = GetLatestDate();
            Date savedDate = new Date()
            {
                day = Settings.day,
                quadrum = Settings.quadrum,
                year = Settings.Year,
            };

            if (Settings.mode == TimekeepingMode.UseCurrent || currentDate.LaterThan(savedDate))
            {
                Settings.day = currentDate.day;
                Settings.quadrum = currentDate.quadrum;
                Settings.Year = currentDate.year - GenDate.DefaultStartingYear;
                Settings.Write();
            }
        }

        private Date GetLatestDate()
        {
            Date latest = null;
            
            foreach (Map map in Find.Maps)
            {
                if (!map.IsPlayerHome)
                    continue;
                Date newDate = DateOfMap(map);
                if (latest == null || newDate.LaterThan(latest))
                    latest = newDate;
            }

            return latest ?? DateAt(new Vector2(0, 0));
        }

        private Date DateOfMap(Map map)
        {
            if (map.Tile == PlanetTile.Invalid)
                return null;
            
            Vector2 coordinates = Find.WorldGrid.LongLatOf(map.Tile);
            return DateAt(coordinates);
        }

        private Date DateAt(Vector2 coordinates)
        {
            int ticks = Find.TickManager.TicksAbs;
            return new Date()
            {
                day = GenDate.DayOfYear(ticks, coordinates.x) % GenDate.DaysPerQuadrum + 1,
                quadrum = GenDate.Quadrum(ticks, coordinates.x),
                year = GenDate.Year(ticks, coordinates.x),
            };
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref startYearOffset, "startYearOffset");
            Scribe_Values.Look(ref dayCounter, "dayCounter");
        }

        private class Date
        {
            public int day;
            public Quadrum quadrum;
            public int year;

            public bool LaterThan(Date date)
            {
                if (year != date.year)
                    return year > date.year;
                if (quadrum != date.quadrum)
                    return quadrum > date.quadrum;
                return day > date.day;
            }
        }
    }

    public enum TimekeepingMode
    {
        Disabled,
        UseLatest,
        UseCurrent,
        Cumulative,
    }
}