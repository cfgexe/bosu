﻿using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Bosu.Objects;
using osu.Game.Rulesets.Bosu.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Bosu.Mods
{
    public class BosuModCinema : ModCinema<BosuHitObject>
    {
        public override Score CreateReplayScore(IBeatmap beatmap, IReadOnlyList<Mod> mods) => new Score
        {
            ScoreInfo = new ScoreInfo { User = new APIUser { Username = "auto" } },
            Replay = new BosuAutoGenerator(beatmap).Generate(),
        };
    }
}
