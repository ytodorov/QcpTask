using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using QcpTask.Core.Hubs;
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
        private Timer timer;
        public TwitterChatService(IHubContext<ChatHub> chatHub)
        {
            this.chatHub = chatHub;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(DoWork, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            chatHub.Clients.All.SendAsync("broadcastMessage",
                $"MachineName: {Environment.MachineName}, CurrentManagedThreadId: {Environment.CurrentManagedThreadId}",
                $"{DateTime.Now.ToString()}");
        }
    }
}
