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
