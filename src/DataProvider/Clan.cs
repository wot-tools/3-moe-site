using System;
using System.Collections.Generic;
using System.Text;

namespace WGApiDataProvider
{
    public class Clan
    {
        public readonly int ID;
        public readonly string Name;
        public readonly string Tag;
        public readonly int Members;
        public readonly int ThreeMoe;
        public readonly double MoeRating;
        public readonly DateTime UpdatedWG;
        public readonly DateTime CreatedAt;
        public readonly DateTime TrackingStarted;
        public readonly DateTime LastChecked;
        public readonly WGApi.ClanEmblems Emblems;
        
        public Clan(int id, string name, string tag, int members, int threeMoe, double moeRating, int updatedWG, int createdAt, int trackingStarted, int lastChecked, WGApi.ClanEmblems emblems)
            : this(id, name, tag, members, threeMoe, moeRating, WGApi.EpochDateTime.FromEpoch(updatedWG), WGApi.EpochDateTime.FromEpoch(createdAt), WGApi.EpochDateTime.FromEpoch(trackingStarted), WGApi.EpochDateTime.FromEpoch(lastChecked), emblems)
        {
        }

        [Newtonsoft.Json.JsonConstructor]
        public Clan(int id, string name, string tag, int members, int threeMoe, double moeRating, DateTime updatedWG, DateTime createdAt, DateTime trackingStarted, DateTime lastChecked, WGApi.ClanEmblems emblems)
        {
            ID = id;
            Name = name;
            Tag = tag;
            Members = members;
            ThreeMoe = threeMoe;
            MoeRating = moeRating;
            UpdatedWG = updatedWG;
            CreatedAt = createdAt;
            TrackingStarted = trackingStarted;
            LastChecked = lastChecked;
            Emblems = emblems;
        }


        public override string ToString() => Name;
    }
}
