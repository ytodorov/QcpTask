using LinqToTwitter.Common;
using LinqToTwitter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using QcpTask.Core.Hubs;
using static System.Net.Mime.MediaTypeNames;

namespace QcpTask.Core.Twitter
{
    public class TweetsService
    {
        public static async Task DoSampleStreamAsync(TwitterContext twitterCtx, IHubContext<ChatHub> chathub)
        {
            string stringToCheckInTweetText = "sellMyBitcoins";

            Console.WriteLine("\nStreamed Content: \n");

            int retries = 3;
            int count = 0;
            var cancelTokenSrc = new CancellationTokenSource();

            // existing rules

            Streaming? streaming =
                await
                (from strm in twitterCtx.Streaming
                 where strm.Type == StreamingType.Rules
                 select strm)
                .SingleOrDefaultAsync();

            var existingRules = streaming.Rules;
            var existingRulesIds = existingRules.Select(s => s.ID).ToList();

            Streaming? resultDelete = await twitterCtx.DeleteStreamingFilterRulesAsync(existingRulesIds);

            var rules = new List<StreamingAddRule>
            {
                new StreamingAddRule { Tag = "has BTCtoUSD string", Value = stringToCheckInTweetText },
            };

            Streaming? result = await twitterCtx.AddStreamingFilterRulesAsync(rules);
            if (result?.Meta?.Summary != null)
            {
                StreamingMeta meta = result.Meta;
                Console.WriteLine($"\nSent: {meta.Sent}");

                StreamingMetaSummary summary = meta.Summary;

                Console.WriteLine($"Created:  {summary.Created}");
                Console.WriteLine($"!Created: {summary.NotCreated}");
            }
            do
            {
                try
                {
                    await twitterCtx.Streaming

                        .WithCancellation(cancelTokenSrc.Token)

                        .Where(strm => strm.Type == StreamingType.Filter
                         //
                         && strm.TweetFields == TweetField.AllFields)
                        .StartAsync(async strm =>
                        {
                            if (strm.EntityType == StreamEntityType.Tweet)
                            {
                                if (strm?.Entity?.Tweet?.Text?.Contains(stringToCheckInTweetText, StringComparison.InvariantCultureIgnoreCase) == true)
                                {
                                    await HandleStreamResponse(strm, chathub);
                                }
                            }
                        });

                    retries = 0;
                }
                catch (IOException ex)
                {
                    chathub.Clients.All.SendAsync("broadcastMessage", "twitterError", ex.Message);
                    // Twitter might have closed the stream,
                    // which they do sometimes. You should
                    // restart the stream, but be sure to
                    // read Twitter documentation on stream
                    // back-off strategies to prevent your
                    // app from being blocked.
                    Console.WriteLine(ex.ToString());
                    retries--;
                }
                catch (OperationCanceledException ex)
                {
                    chathub.Clients.All.SendAsync("broadcastMessage", "twitterError", ex.Message);
                    Console.WriteLine("Stream cancelled.");
                    retries = 0;
                }
                catch (TwitterQueryException tqe) when (tqe.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    chathub.Clients.All.SendAsync("broadcastMessage", "twitterError", tqe.Message);
                
                    int millisecondsToDelay = 1000 * (4 - retries);
                    retries--;

                    string message = retries >= 0 ?
                        $"Tried to reconnect too quickly. Delaying for {millisecondsToDelay} milliseconds..."
                        :
                        "Too many retries. Stopping query.";

                    Console.WriteLine(message);

                    await Task.Delay(millisecondsToDelay);
                }
            } while (retries > 0);

            

            //Thread.Sleep(int.MaxValue);
        }


        public static async Task<int> HandleStreamResponse(LinqToTwitter.StreamContent strm, IHubContext<ChatHub> chatHub)
        {
            if (strm.HasError)
            {
                Console.WriteLine($"Error during streaming: {strm.ErrorMessage}");
            }
            else
            {
                Tweet? tweet = strm?.Entity?.Tweet;
                if (tweet != null)
                {
                    Console.WriteLine($"\n{tweet.CreatedAt}, Tweet ID: {tweet.ID}, Tweet Text: {tweet.Text}");
                }

                var fullJson = JsonConvert.SerializeObject(strm.Entity, Formatting.Indented);

                Console.WriteLine(fullJson);

                chatHub.Clients.All.SendAsync("broadcastMessage",
                "twitterTweet",
                $", Tweet Text: {tweet.Text} CreatedAt: {tweet.CreatedAt}, Tweet ID: {tweet.ID}");
            }

            return await Task.FromResult(0);
        }
    }
}
