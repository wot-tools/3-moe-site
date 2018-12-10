using System;
using System.Collections.Generic;
using System.Text;

namespace WGApiDataProvider
{
    public class Player
    {
        public string Name { get; }
        public Clan Clan { get; }
        public int ThreeMoeCount { get; }
        public int BattleCount { get; }
        public decimal WinRatio { get; }
        public double Wn8 { get; }
        public double MoeRating { get; }
        public int WgRating { get; }
        public string ClientLanguage { get; }
        public DateTime LastBattle { get; }
        public DateTime LastLogout { get; }
        public DateTime AccountCreated { get; }
        public DateTime LastWgUpdate { get; }
        public DateTime LastChecked { get; }

        public Player(
            string name,
            Clan clan,
            int threeMoeCount,
            int battleCount,
            decimal winRatio,
            double wn8,
            double moeRating,
            int wgRating,
            string clientLanguage,
            int lastBattle,
            int lastLogout,
            int accountCreated,
            int lastWgUpdate,
            int lastChecked
        ) : this(name, clan, threeMoeCount, battleCount, winRatio, wn8, moeRating, wgRating, clientLanguage, WGApi.EpochDateTime.FromEpoch(lastBattle), WGApi.EpochDateTime.FromEpoch(lastLogout), WGApi.EpochDateTime.FromEpoch(accountCreated), WGApi.EpochDateTime.FromEpoch(lastWgUpdate), WGApi.EpochDateTime.FromEpoch(lastChecked)) { }

        [Newtonsoft.Json.JsonConstructor]
        public Player(
            string name,
            Clan clan,
            int threeMoeCount,
            int battleCount,
            decimal winRatio,
            double wn8,
            double moeRating,
            int wgRating,
            string clientLanguage,
            DateTime lastBattle,
            DateTime lastLogout,
            DateTime accountCreated,
            DateTime lastWgUpdate,
            DateTime lastChecked
        )
        {
            Name = name;
            Clan = clan;
            ThreeMoeCount = threeMoeCount;
            BattleCount = battleCount;
            WinRatio = winRatio;
            Wn8 = wn8;
            MoeRating = moeRating;
            WgRating = wgRating;
            ClientLanguage = clientLanguage;
            LastBattle = lastBattle;
            LastLogout = lastLogout;
            AccountCreated = accountCreated;
            LastWgUpdate = lastWgUpdate;
            LastChecked = lastChecked;
        }
    }
}
