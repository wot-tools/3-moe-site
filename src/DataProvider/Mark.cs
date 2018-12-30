using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WGApiDataProvider
{
    public class Mark
    {
        private static DataProvider DataProvider;
        public static void SetDataProvider(DataProvider provider) => DataProvider = provider;

        private readonly int _PlayerID;
        public int PlayerID => _PlayerID;
        [JsonIgnore]
        public Player Player => DataProvider._Players[_PlayerID];
        [JsonIgnore]
        public Clan Clan => Player.Clan;

        private readonly int _TankID;
        public int TankID => _TankID;
        [JsonIgnore]
        public Tank Tank => DataProvider._Tanks[_TankID];

        public readonly DateTime FirstDetected;

        public Mark(int playerID, int tankID, DateTime firstDetected)
        {
            _PlayerID = playerID;
            _TankID = tankID;
            FirstDetected = firstDetected;
        }
    }
}
