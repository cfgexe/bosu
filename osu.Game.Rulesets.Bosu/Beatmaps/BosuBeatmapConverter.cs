﻿using osu.Game.Beatmaps;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Bosu.Objects;
using osuTK;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Bosu.Beatmaps
{
    public class BosuBeatmapConverter : BeatmapConverter<BosuHitObject>
    {
        private const int bullets_per_hitcircle = 4;
        private const int bullets_per_slider_head = 7;
        private const int bullets_per_slider_tail = 5;
        private const int bullets_per_spinner_span = 20;

        private const float spinner_span_delay = 250f;
        private const float spinner_angle_per_span = 8f;

        public BosuBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
        }

        public override bool CanConvert() => Beatmap.HitObjects.All(h => h is IHasPosition);

        private int index = -1;

        protected override IEnumerable<BosuHitObject> ConvertHitObject(HitObject obj, IBeatmap beatmap)
        {
            var positionData = obj as IHasPosition;
            var comboData = obj as IHasCombo;
            var difficulty = beatmap.BeatmapInfo.BaseDifficulty;

            if (comboData?.NewCombo ?? false)
                index++;

            List<BosuHitObject> hitObjects = new List<BosuHitObject>();

            switch (obj)
            {
                // Slider
                case IHasCurve curve:

                    // head
                    for (int i = 0; i < bullets_per_slider_head; i++)
                    {
                        hitObjects.Add(new MovingCherry
                        {
                            Angle = getBulletDistribution(bullets_per_slider_head, 360f, i),
                            StartTime = obj.StartTime,
                            Position = new Vector2(positionData?.X ?? 0, positionData?.Y * 0.5f ?? 0),
                            NewCombo = comboData?.NewCombo ?? false,
                            ComboOffset = comboData?.ComboOffset ?? 0,
                            IndexInBeatmap = index
                        });
                    }

                    hitObjects.Add(new SoundHitObject
                    {
                        StartTime = obj.StartTime,
                        Samples = obj.Samples
                    });

                    // ticks

                    var controlPointInfo = beatmap.ControlPointInfo;
                    TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(obj.StartTime);
                    DifficultyControlPoint difficultyPoint = controlPointInfo.DifficultyPointAt(obj.StartTime);

                    double scoringDistance = 100 * difficulty.SliderMultiplier * difficultyPoint.SpeedMultiplier;

                    var velocity = scoringDistance / timingPoint.BeatLength;
                    var tickDistance = scoringDistance / difficulty.SliderTickRate;

                    foreach (var e in SliderEventGenerator.Generate(obj.StartTime, curve.Duration / (curve.RepeatCount + 1), velocity, tickDistance, curve.Path.Distance, curve.RepeatCount + 1, (obj as IHasLegacyLastTickOffset)?.LegacyLastTickOffset ?? 0))
                    {
                        switch (e.Type)
                        {
                            case SliderEventType.Tick:
                                var tickPosition = curve.CurvePositionAt(e.PathProgress);

                                hitObjects.Add(new TickCherry
                                {
                                    StartTime = e.Time,
                                    Position = new Vector2(tickPosition.X + positionData.X, (tickPosition.Y + positionData.Y) * 0.5f),
                                    NewCombo = comboData?.NewCombo ?? false,
                                    ComboOffset = comboData?.ComboOffset ?? 0,
                                    IndexInBeatmap = index
                                });

                                hitObjects.Add(new SoundHitObject
                                {
                                    StartTime = e.Time,
                                    Samples = getTickSamples(obj.Samples)
                                });
                                break;
                        }
                    }

                    //body

                    var bodyCherriesCount = curve.Distance / 10;

                    for(int i = 0; i < bodyCherriesCount; i++)
                    {
                        var progress = (float)i / bodyCherriesCount;
                        var position = curve.CurvePositionAt(curve.ProgressAt(progress));

                        hitObjects.Add(new SliderPartCherry
                        {
                            StartTime = obj.StartTime + curve.Duration * progress,
                            Position = new Vector2(position.X + positionData.X, (position.Y + positionData.Y) * 0.5f),
                            NewCombo = comboData?.NewCombo ?? false,
                            ComboOffset = comboData?.ComboOffset ?? 0,
                            IndexInBeatmap = index
                        });
                    }

                    // tail
                    var tailPosition = curve.CurvePositionAt(curve.ProgressAt(1));

                    for (int i = 0; i < bullets_per_slider_tail; i++)
                    {
                        hitObjects.Add(new MovingCherry
                        {
                            Angle = getBulletDistribution(bullets_per_slider_tail, 360f, i),
                            StartTime = curve.EndTime,
                            Position = new Vector2(tailPosition.X + positionData.X, (tailPosition.Y + positionData.Y) * 0.5f),
                            NewCombo = comboData?.NewCombo ?? false,
                            ComboOffset = comboData?.ComboOffset ?? 0,
                            IndexInBeatmap = index
                        });
                    }

                    hitObjects.Add(new SoundHitObject
                    {
                        StartTime = curve.EndTime,
                        Samples = obj.Samples
                    });

                    return hitObjects;

                // Spinner
                case IHasEndTime endTime:
                    var spansPerSpinner = endTime.Duration / spinner_span_delay;

                    for (int i = 0; i < spansPerSpinner; i++)
                    {
                        for (int j = 0; j < bullets_per_spinner_span; j++)
                        {
                            hitObjects.Add(new MovingCherry
                            {
                                Angle = getBulletDistribution(bullets_per_spinner_span, 360f, j) + (i * spinner_angle_per_span),
                                StartTime = obj.StartTime + i * spinner_span_delay,
                                Position = new Vector2(positionData?.X ?? 0, positionData?.Y * 0.5f ?? 0),
                                NewCombo = comboData?.NewCombo ?? false,
                                ComboOffset = comboData?.ComboOffset ?? 0,
                                IndexInBeatmap = index
                            });
                        }
                    }

                    return hitObjects;

                // Hitcircle
                default:
                    for (int i = 0; i < bullets_per_hitcircle; i++)
                    {
                        hitObjects.Add(new MovingCherry
                        {
                            Angle = getBulletDistribution(bullets_per_hitcircle, 120, i),
                            StartTime = obj.StartTime,
                            Position = new Vector2(positionData?.X ?? 0, positionData?.Y * 0.5f ?? 0),
                            NewCombo = comboData?.NewCombo ?? false,
                            ComboOffset = comboData?.ComboOffset ?? 0,
                            IndexInBeatmap = index
                        });
                    }

                    hitObjects.Add(new SoundHitObject
                    {
                        StartTime = obj.StartTime,
                        Samples = obj.Samples
                    });

                    return hitObjects;
            }
        }

        protected override Beatmap<BosuHitObject> CreateBeatmap() => new BosuBeatmap();

        private float getBulletDistribution(int bulletsPerObject, float angleRange, int index)
        {
            return getAngleBuffer(bulletsPerObject, angleRange) + index * getPerBulletAngle(bulletsPerObject, angleRange);

            static float getAngleBuffer(int bulletsPerObject, float angleRange) => (360 - angleRange + getPerBulletAngle(bulletsPerObject, angleRange)) / 2f;

            static float getPerBulletAngle(int bulletsPerObject, float angleRange) => angleRange / bulletsPerObject;
        }

        private List<HitSampleInfo> getTickSamples(IList<HitSampleInfo> objSamples) => objSamples.Select(s => new HitSampleInfo
        {
            Bank = s.Bank,
            Name = @"slidertick",
            Volume = s.Volume
        }).ToList();
    }
}
