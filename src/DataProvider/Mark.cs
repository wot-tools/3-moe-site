using System;
using System.Collections.Generic;
using System.Text;

namespace WGApiDataProvider
{
    public class Mark
    {
        private static DataProvider DataProvider;
        public static void SetDataProvider(DataProvider provider) => DataProvider = provider;

        private readonly int PlayerID;
        public Player Player => DataProvider._Players[PlayerID];

        private readonly int ClanID;
        public Clan Clan => Player.Clan;

        private readonly int TankID;
        public Tank Tank => DataProvider._Tanks[TankID];

        public readonly DateTime FirstDetected;

        public Mark(int playerID, int clanID, int tankID, DateTime firstDetected)
        {
            PlayerID = playerID;
            ClanID = clanID;
            TankID = tankID;
            FirstDetected = firstDetected;
        }
    }
}
