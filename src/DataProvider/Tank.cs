using System;
using System.Collections.Generic;
using System.Text;
using WGApi;

namespace WGApiDataProvider
{
    public class Tank : WGApi.Tank
    {
        private static DataProvider DataProvider;
        public static void SetDataProvider(DataProvider provider) => DataProvider = provider;

        public readonly int ID;
        public readonly double MoeValue;

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
            // MoeValue = ?????
            Tag = tank.Tag;

            Name = tank.Name;
            ShortName = tank.ShortName;
            Tier = tank.Tier;
            Nation = tank.Nation;
            VehicleType = tank.VehicleType;
            Icons = tank.Icons;
            IsPremium = tank.IsPremium;
            Tag = tank.Tag;
        }
    }
}
