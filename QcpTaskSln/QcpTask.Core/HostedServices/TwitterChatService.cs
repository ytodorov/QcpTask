using LinqToTwitter;
using LinqToTwitter.OAuth;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using QcpTask.Core.Hubs;
using QcpTask.Core.Twitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QcpTask.Core.HostedServices
{
    public class TwitterChatService : IHostedService
    {
        private readonly IHubContext<ChatHub> chatHub;
        private readonly IConfiguration configuration;
        private Timer timer;
        private readonly TwitterContext twitterContext;

        public TwitterChatService(IHubContext<ChatHub> chatHub, IConfiguration configuration)
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
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(DoWork, null, TimeSpan.FromSeconds(10), Timeout.InfiniteTimeSpan);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            chatHub.Clients.All.SendAsync("broadcastMessage",
                $"Started! MachineName: {Environment.MachineName}, CurrentManagedThreadId: {Environment.CurrentManagedThreadId}",
                $"{DateTime.Now.ToString()}");

            TweetsService.DoSampleStreamAsync(twitterContext, chatHub).GetAwaiter().GetResult();


        }
    }
}
