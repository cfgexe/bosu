using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Bosu.Mods.CustomMods
{
    public abstract class BosuTest : ModRateAdjust
    {
        public override string Name => "Test";
        public override string Acronym => "XD";
        public override IconUsage? Icon => OsuIcon.ModDoubleTime;
        public override ModType Type => ModType.DifficultyIncrease;
        public override string Description => "aaaa";

        public override Type[] IncompatibleMods => [];

        [SettingSource("Speeda", "Testing speed")]
        public override BindableNumber<double> SpeedChange { get; } = new BindableDouble
        {
            MinValue = 1.01,
            MaxValue = 2,
            Default = 1.5,
            Value = 1.5,
            Precision = 0.01,
        };
    }
}
