using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WGApiDataProvider
{
    public class MockupDataProvider
    {
        private static object InstanceLock = new object();
        private static MockupDataProvider _Instance;
        public static MockupDataProvider Instance
        {
            get
            {
                if (_Instance is null)
                    lock (InstanceLock)
                        if (_Instance is null)
                            _Instance = new MockupDataProvider();
                return _Instance;
            }
        }

        public Player[] Players;
        public Mark[] Marks;
        public Clan[] Clans;
        public Tank[] Tanks;

        private MockupDataProvider()
        {
            Players = JsonConvert.DeserializeObject<Player[]>(File.ReadAllText("test.json"));
            Marks = new Mark[0];
            Clans = new Clan[0];
            Tanks = new Tank[0];
        }

    }
}
