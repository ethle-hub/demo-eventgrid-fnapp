using System;
using System.IO;
using System.Threading.Tasks;
using ChastelFunctionApp.Helper;
using ChastelFunctionApp.Models;
using ChastelFunctionApp.Models.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ChastelFunctionApp
{
    /// <summary>
    /// This function is to intake new client booking request then the passenger places new FUTURE booking from app
    /// </summary>
    public static class PaxRequestsClientBookingFunction
    {
        static string topicEndpoint = Environment.GetEnvironmentVariable("UnassignedBookingEventGridTopicEndpoint");
        static string sasKey = Environment.GetEnvironmentVariable("UnassignedBookingEventGridTopicEndpointKey");

        [FunctionName(nameof(PaxRequestsClientBookingFunction))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            [CosmosDB("chastelDatabase", "clientBookingRequests", Id = "Id", CreateIfNotExists = true,
                PartitionKey = "/DispatchSystemId", ConnectionStringSetting = "ChastelDBConnectionString")]
            IAsyncCollector<PaxRequestsClientBookingModel> documents,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // parsing  client booking data from request's body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var clientBookingData = JsonConvert.DeserializeObject<PaxRequestsClientBookingModel>(requestBody);

            // 1. store request
            documents.AddAsync(clientBookingData).GetAwaiter();

            // 2. post to Azure Event Grid for next handler
            EventGridHelper.RaiseEventToGridAsync(topicEndpoint, 
                sasKey,
                nameof(PaxRequestsClientBookingFunction),
                nameof(ChastelEventTopic.UnassignedClientBooking),
                nameof(ChastelEventType.HttpFunctionEvent),
                clientBookingData).GetAwaiter();
            
            // finally return
            return clientBookingData != null
                ? (ActionResult) new OkObjectResult($"Hello, booking {clientBookingData.DispatchSystemId}-{clientBookingData.DispatchBookingId}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}