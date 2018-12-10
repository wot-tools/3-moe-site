using System;
using System.Collections.Generic;
using System.Text;

namespace WGApiDataProvider
{
    public interface IDataProvider
    {
        Player[] Players { get; }
    }
}
