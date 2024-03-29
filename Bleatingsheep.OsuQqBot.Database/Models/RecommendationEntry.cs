﻿using System;
using System.Collections.Generic;
using Bleatingsheep.Osu;
using Bleatingsheep.Osu.ApiClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static Bleatingsheep.Osu.Mods;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
    public class RecommendationEntry
    {
        public int Id { get; set; }
        public Mode Mode { get; set; }
        public RecommendationBeatmapId Left { get; set; }
        public RecommendationBeatmapId Recommendation { get; set; }
        public double RecommendationDegree { get; set; }
        public double Performance { get; set; }
    }

    [Owned]
    public sealed class RecommendationBeatmapId : IEquatable<RecommendationBeatmapId>, IComparable<RecommendationBeatmapId>
    {
        private static readonly IList<Mods> s_modFilters = new Mods[4]
        {
            DoubleTime | HalfTime | Easy | HardRock | Hidden | Flashlight | TouchDevice,
            DoubleTime,
            DoubleTime,
            DoubleTime,
        };

        public static readonly ValueConverter<RecommendationBeatmapId, long> ValueConverter = new(
            v => ((long)v.BeatmapId << 32) | (long)v.ValidMods,
            v => new RecommendationBeatmapId
            {
                BeatmapId = (int)(v >> 32),
                ValidMods = (Mods)(v & 0xffffff),
            });

        public static RecommendationBeatmapId Create(UserBest best, Mode mode)
        {
            return new RecommendationBeatmapId
            {
                BeatmapId = best.BeatmapId,
                ValidMods = best.EnabledMods & s_modFilters[(int)mode],
            };
        }

        public int BeatmapId { get; set; }
        public Mods ValidMods { get; set; }

        public override bool Equals(object obj) => Equals(obj as RecommendationBeatmapId);
        public bool Equals(RecommendationBeatmapId other) => other != null && BeatmapId == other.BeatmapId && ValidMods == other.ValidMods;
        public override int GetHashCode() => HashCode.Combine(BeatmapId, ValidMods);

        public int CompareTo(RecommendationBeatmapId other)
        {
            if (other is null)
                return 1;
            if (ReferenceEquals(this, other))
                return 0;
            return (BeatmapId.CompareTo(other.BeatmapId), ValidMods.CompareTo(other.ValidMods)) switch
            {
                ( > 0, _) => 1,
                ( < 0, _) => -1,
                (_, > 0) => 1,
                (_, < 0) => -1,
                _ => 0,
            };
        }

        public static bool operator ==(RecommendationBeatmapId left, RecommendationBeatmapId right) => EqualityComparer<RecommendationBeatmapId>.Default.Equals(left, right);
        public static bool operator !=(RecommendationBeatmapId left, RecommendationBeatmapId right) => !(left == right);
    }
}
