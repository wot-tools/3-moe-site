using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WGApiDataProvider
{
    public class Player
    {
        public readonly int ID;
        public readonly string Name;

        private readonly int ClanID;
        private static Clan EmptyClan = new Clan(0, String.Empty, String.Empty, 0, 0, 0, 0, 0, 0, 0, new WGApi.ClanEmblems());
        [JsonIgnore]
        public Clan Clan
        {
            get
            {
                if (ClanID == 0)
                    return EmptyClan;
                if (WGApiDataProvider.Instance._Clans.TryGetValue(ClanID, out Clan result))
                    return result;
                return EmptyClan;
            }
        }

        private static DataProvider DataProvider;
        public static void SetDataProvider(DataProvider provider) => DataProvider = provider;

        [JsonIgnore]
        public int ThreeMoeCount
        {
            get
            {
                if (DataProvider._PlayersMarks.TryGetValue(ID, out var result))
                    return result.Count;
                return 0;
            }
        }

        public readonly int BattleCount;
        public readonly decimal WinRatio;
        public readonly double Wn8;
        public double MoeRating
        {
            get
            {
                if (DataProvider._PlayersMarks.TryGetValue(ID, out var result))
                    return result.Values.Sum(x => x.Tank.MoeValue);
                return 0;
            }
        }
        public readonly int WgRating;
        public readonly string ClientLanguage;
        public readonly DateTime LastBattle;
        public readonly DateTime LastLogout;
        public readonly DateTime AccountCreated;
        public readonly DateTime LastWgUpdate;
        public readonly DateTime LastChecked;

        public Player(
            int id,
            string name,
            int clanID,
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
        ) : this(id, name, clanID, threeMoeCount, battleCount, winRatio, wn8, moeRating, wgRating, clientLanguage,
                 WGApi.EpochDateTime.FromEpoch(lastBattle), WGApi.EpochDateTime.FromEpoch(lastLogout),
                 WGApi.EpochDateTime.FromEpoch(accountCreated), WGApi.EpochDateTime.FromEpoch(lastWgUpdate),
                 WGApi.EpochDateTime.FromEpoch(lastChecked)) { }

        [JsonConstructor]
        public Player(
            int id,
            string name,
            int clanID,
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
            ID = id;
            Name = name;
            ClanID = clanID;
            //ThreeMoeCount = threeMoeCount;
            BattleCount = battleCount;
            WinRatio = winRatio;
            Wn8 = wn8;
            WgRating = wgRating;
            ClientLanguage = clientLanguage;
            LastBattle = lastBattle;
            LastLogout = lastLogout;
            AccountCreated = accountCreated;
            LastWgUpdate = lastWgUpdate;
            LastChecked = lastChecked;
        }

        public Player(WGApi.PlayerInfo player)
        {
            ID = player.AccountID;
            Name = player.Nick;
            ClanID = player.ClanID ?? 0;
            //ThreeMoeCount = -1;
            BattleCount = player.Statistics.Random.Battles;
            WinRatio = player.Statistics.Random.Victories / Math.Max(1m, player.Statistics.Random.Battles);
            Wn8 = -1;
            WgRating = player.WGRating;
            ClientLanguage = player.ClientLanguage;
            LastBattle = player.LastBattle;
            LastLogout = player.LastLogout;
            AccountCreated = player.AccountCreated;
            LastWgUpdate = player.UpdatedAt;
            LastChecked = DateTime.Now;
        }
    }
}
