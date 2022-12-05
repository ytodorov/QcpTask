using BinanceExchange.API.Models.Response;
using BinanceExchange.API.Models.WebSocket;
using CCXT.NET.Shared.Coin.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QcpTask.Core.Caching
{
    /// <summary>
    /// Very simple caching mechanism. In real application Redis Cache or some other form of durrable service must be use.
    /// Warning: This cache will be wiped out if the application restarts or the application pool restarts.
    /// </summary>
    public static class SimpleCache
    {
        static SimpleCache()
        {
            TickersList = new List<Ticker>();
            BinanceKlineDatas = new List<BinanceKlineData>();

        }
        public static List<Ticker> TickersList { get; set; }

        public static List<BinanceKlineData> BinanceKlineDatas { get; set; }

    }
}
