using System;
using System.Collections.Generic;
using System.Text;

namespace WGApiDataProvider
{
    public abstract class DataProvider : IDataProvider
    {
        public abstract Dictionary<int, Player> _Players { get; }
        public abstract Player[] Players { get; }

        public abstract Dictionary<int, Dictionary<int, Mark>> _PlayersMarks { get; }
        public abstract Dictionary<int, Dictionary<int, Mark>> _TanksMarks { get; }
        public abstract Mark[] Marks { get; }

        public abstract Dictionary<int, Clan> _Clans { get; }
        public abstract Clan[] Clans { get; }

        public abstract Dictionary<int, Tank> _Tanks { get; }
        public abstract Tank[] Tanks { get; }

    }

    public interface IDataProvider
    {
        Player[] Players { get; }
        Mark[] Marks { get; }

    }
}
