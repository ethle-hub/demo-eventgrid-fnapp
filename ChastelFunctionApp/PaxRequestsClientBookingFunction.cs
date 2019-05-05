using System;
using System.IO;
using System.Threading.Tasks;
using ChastelFunctionApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ChastelFunctionApp
{
    public static class PaxRequestsClientBookingFunction
    {
        [FunctionName(nameof(PaxRequestsClientBookingFunction))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [CosmosDB("chastelDatabase", "clientBookingRequests", Id = "Id", CreateIfNotExists = true, PartitionKey = "/DispatchSystemId", ConnectionStringSetting = "ChastelDBConnectionString")] IAsyncCollector<PaxRequestsClientBookingModel> documents,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // parsing  client booking data from request's body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var clientBookingData = JsonConvert.DeserializeObject<PaxRequestsClientBookingModel>(requestBody);
            
            await documents.AddAsync(clientBookingData);

            return clientBookingData != null
                ? (ActionResult)new OkObjectResult($"Hello, booking {clientBookingData.DispatchBookingId}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}
    