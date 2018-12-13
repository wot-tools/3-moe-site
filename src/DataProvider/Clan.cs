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
        
        [Newtonsoft.Json.JsonConstructor]
        public Clan(int id, string name, string tag, int members, int threeMoe, int moeRating, int updatedWG, int createdAt, int trackingStarted, int lastChecked)
            : this(id, name, tag, members, threeMoe, moeRating, WGApi.EpochDateTime.FromEpoch(updatedWG), WGApi.EpochDateTime.FromEpoch(createdAt), WGApi.EpochDateTime.FromEpoch(trackingStarted), WGApi.EpochDateTime.FromEpoch(lastChecked))
        {
        }

        public Clan(int id, string name, string tag, int members, int threeMoe, int moeRating, DateTime updatedWG, DateTime createdAt, DateTime trackingStarted, DateTime lastChecked)
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
        }


        public override string ToString() => Name;
    }
}
