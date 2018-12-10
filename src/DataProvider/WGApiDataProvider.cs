using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WGApiDataProvider
{
    public class WGApiDataProvider : IDataProvider
    {
        private static object InstanceLock = new object();
        private static WGApiDataProvider _Instance;
        public static WGApiDataProvider Instance
        {
            get
            {
                if (_Instance is null)
                    lock (InstanceLock)
                        if (_Instance is null)
                            _Instance = new WGApiDataProvider();
                return _Instance;
            }
        }

        public Player[] Players { get; }

        private WGApiDataProvider()
        {
            Players = JsonConvert.DeserializeObject<Player[]>(File.ReadAllText("test.json"));
        }
    }
}
