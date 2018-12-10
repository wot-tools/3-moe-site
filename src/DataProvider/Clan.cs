using System;
using System.Collections.Generic;
using System.Text;

namespace WGApiDataProvider
{
    public class Clan
    {
        public int Number { get; }
        public string Name => $"Clan {Number}";

        public Clan(int number)
        {
            Number = number;
        }

        public override string ToString() => Name;
    }
}
