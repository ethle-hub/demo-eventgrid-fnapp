using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ChastelFunctionApp.Models;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Newtonsoft.Json;

namespace ChastelFunctionApp.Helper
{
    internal class EventGridHelper
    {
        //static HttpClient client = new HttpClient();

        //public static async Task SendMessageToEventGridTopicFilterred(EventGridEvent[] allEvents, string sasKey,
        //    string topicEndpoint)
        //{
        //    client.DefaultRequestHeaders.Add("aeg-sas-key", sasKey);
        //    client.DefaultRequestHeaders.UserAgent.ParseAdd("democlient");

        //    var json = JsonConvert.SerializeObject(allEvents);
        //    var request = new HttpRequestMessage(HttpMethod.Post, topicEndpoint)
        //    {
        //        Content = new StringContent(json, Encoding.UTF8, "application/json")
        //    };

        //    var response = await client.SendAsync(request);
        //    if (!response.IsSuccessStatusCode)
        //        throw new Exception("Unable to send message to event grid");
        //}

        public static async Task RaiseEventToGridAsync(string topicEndpoint, string topicKey,
            string subject, string topic, string eventType,  object data)
        {
            try
            {
                // https://docs.microsoft.com/en-us/dotnet/api/overview/azure/eventgrid?view=azure-dotnet
                var eventGridEvent = new EventGridEvent
                {
                    Subject = subject,
                    Topic = topic,
                    EventType = eventType,
                    EventTime = DateTime.UtcNow,
                    Id = Guid.NewGuid().ToString(),
                    Data = data,
                    DataVersion = "1.0.0",
                };

                var events = new List<EventGridEvent> {eventGridEvent};
                var topicHostname = new Uri(topicEndpoint).Host;
                var credentials = new TopicCredentials(topicKey);
                var client = new EventGridClient(credentials);

                await client.PublishEventsWithHttpMessagesAsync(topicHostname, events);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
