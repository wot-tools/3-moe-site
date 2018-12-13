using System;
using System.Collections.Generic;
using System.Text;
using WGApi;

namespace WGApiDataProvider
{
    public class Tank
    {
        private static DataProvider DataProvider;
        public static void SetDataProvider(DataProvider provider) => DataProvider = provider;

        public readonly int ID;
        public readonly string Tag;
        public readonly string Name;
        public readonly string ShortName;
        public readonly double MoeValue;
        public readonly int Tier;
        public readonly Nations Nation;
        public readonly VehicleTypes Type;

        public int ThreeMoeCount
        {
            get
            {
                if (DataProvider._TanksMarks.TryGetValue(ID, out var result))
                    return result.Count;
                return 0;
            }
        }

        public Tank(int id, WGApi.Tank tank)
        {
            ID = id;
            Tag = tank.Tag;
            Name = tank.Name;
            ShortName = tank.ShortName;
            // MoeValue = ?????
            Tier = tank.Tier;
            Nation = tank.Nation;
            Type = tank.VehicleType;
        }
    }
}
