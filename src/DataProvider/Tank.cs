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
        public double MoeValue
        {
            get
            {
                double tierParam = Math.Pow(1.5, -1.3 * (10 - Tier));
                double tankTypeParam = VehicleType == VehicleTypes.SPG ? 1.25 : 1;
                double playerParam = Math.Pow(Math.Log(0.0001 + 2) / Math.Log(0.0001 * Math.Max(ThreeMoeCount, 1) + 2), 10);

                return tierParam * tankTypeParam * playerParam * 1000;
            }
        }

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
