using BinanceExchange.API.Models.Response;
using BinanceExchange.API.Models.WebSocket;
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
            SymbolPriceChangeTickerResponseList = new List<SymbolPriceChangeTickerResponse>();
        }
        public static List<SymbolPriceChangeTickerResponse> SymbolPriceChangeTickerResponseList { get; set; }
    }
}
