using System;
using System.Collections.Generic;
using System.Text;

namespace WGApiDataProvider
{
    public class Mark
    {
        public Player Player { get; }
        public Clan Clan { get; }
        public Tank Tank { get; }
        public DateTime FirstDetected { get; }
    }
}
