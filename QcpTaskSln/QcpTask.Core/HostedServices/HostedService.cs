using BinanceExchange.API.Client;
using BinanceExchange.API.Client.Interfaces;
using BinanceExchange.API.Enums;
using BinanceExchange.API.Models.Request;
using BinanceExchange.API.Models.WebSocket;
using BinanceExchange.API.Websockets;
using LinqToTwitter;
using LinqToTwitter.OAuth;
using log4net;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using QcpTask.Core.Binance;
using QcpTask.Core.Caching;
using QcpTask.Core.Hubs;
using QcpTask.Core.Twitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QcpTask.Core.HostedServices
{
    public class HostedService : IHostedService
    {
        private readonly IHubContext<ChatHub> chatHub;
        private readonly IConfiguration configuration;
        private Timer timer;
        private readonly TwitterContext twitterContext;
        private readonly BinanceClient binanceClient;

        public HostedService(IHubContext<ChatHub> chatHub, IConfiguration configuration)
        {
            this.chatHub = chatHub;
            this.configuration = configuration;


            var auth = new ApplicationOnlyAuthorizer()
            {
                CredentialStore = new InMemoryCredentialStore
                {
                    ConsumerKey = configuration.GetValue<string>("twitterConsumerKey"),  //Environment.GetEnvironmentVariable(OAuthKeys.TwitterConsumerKey),
                    ConsumerSecret = configuration.GetValue<string>("twitterConsumerSecret") //Environment.GetEnvironmentVariable(OAuthKeys.TwitterConsumerSecret)
                },
            };
            auth.AuthorizeAsync().GetAwaiter().GetResult();

            twitterContext = new TwitterContext(auth);

            binanceClient = new BinanceClient(new ClientConfiguration()
            {
                ApiKey = "YOUR_API_KEY",
                SecretKey = "YOUR_API_KEY",
                Logger = LogManager.GetLogger(this.GetType())

            }); ;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(DoWork, null, TimeSpan.FromSeconds(2), Timeout.InfiniteTimeSpan);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            try
            {
                chatHub.Clients.All.SendAsync("broadcastMessage",
                    $"Started! MachineName: {Environment.MachineName}, CurrentManagedThreadId: {Environment.CurrentManagedThreadId}",
                    $"{DateTime.Now}");

                TweetsService.DoSampleStreamAsync(twitterContext, chatHub).GetAwaiter().GetResult();


                Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < int.MaxValue; i++)
                    {
                        var dailyTicker = binanceClient.GetDailyTicker("BTCUSDT").GetAwaiter().GetResult();
                        Thread.Sleep(1000);

                        SimpleCache.SymbolPriceChangeTickerResponseList.Insert(0, dailyTicker);
                    }

                });



                // For some strange reason this does not work in Azure Linux Container

                //var manualBinanceWebSocket = new InstanceBinanceWebSocketClient(binanceClient);
                //manualBinanceWebSocket.ConnectToKlineWebSocket("BTCUSDT", BinanceExchange.API.Enums.KlineInterval.OneMinute, b =>
                //{
                //    SimpleCache.BinanceKlineDataList.Insert(0, b);

                //    chatHub.Clients.All.SendAsync("broadcastMessage",
                //    $"{JsonConvert.SerializeObject(b, Formatting.Indented)}",
                //    $"{DateTime.Now}");
                //});
            }
            catch (Exception ex)
            {
                Console.WriteLine("ExceptionY" + ex.StackTrace + ex.Message);
            }

        }
    }
}
