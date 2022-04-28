using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using CrazyBikeStore.Infrastructure.AuthFlows;
using CrazyBikeStore.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace CrazyBikeStore
{
    public class CrazyBikeApi
    {
        readonly ILogger logger;
        readonly Fixture fixture;
        public CrazyBikeApi(ILoggerFactory loggerFactory, Fixture fixture)
        {
            logger = loggerFactory.ThrowIfNullOrDefault().CreateLogger<CrazyBikeApi>();
            this.fixture = fixture.ThrowIfNullOrDefault();
        }
    
        [Function(nameof(AddBike))]
        [OpenApiOperation(operationId: "addBike", tags: new[] { "bike" }, 
            Summary = "Add a new bike to the store", Description = "This add a new bike to the store.", Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiSecurity("bikestore_auth", SecuritySchemeType.OAuth2, Flows = typeof(BikeStoreAuth))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Bike), Required = true, 
            Description = "Bike object that needs to be added to the store")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Bike), 
            Summary = "New bike details added", Description = "New bike details added")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.MethodNotAllowed, 
            Summary = "Invalid input", Description = "Invalid input")]
        public async Task<HttpResponseData> AddBike(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "bike")] HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(fixture.Create<Bike>());
            return response;
        }
        
        [Function(nameof(UpdateBike))]
        public async Task<HttpResponseData> UpdateBike(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "bike")] HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(fixture.Create<Bike>());
            return response;
        }
        
        [Function(nameof(DeleteBike))]
        public Task<HttpResponseData> DeleteBike(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "bike/{idBike}")] HttpRequestData req, Guid idBike)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            return Task.FromResult(response);
        }
        
        [Function(nameof(FindByStatus))]
        public async Task<HttpResponseData> FindByStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "bike/findByStatus")] HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            var status = req.Query("status")
                .Select(p =>
                {
                    var converted = Enum.TryParse<BikeStatus>(p, ignoreCase: true, out var result) ? result : BikeStatus.Available;
                    return converted;
                })
                .ToList();
            var bikes = fixture.Create<List<Bike>>().Where(p => status.Contains(p.Status));
            await response.WriteAsJsonAsync(bikes);
            return response;
        }
        
        [Function(nameof(FindByTags))]
        public async Task<HttpResponseData> FindByTags(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "bike/findByTags")] HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            var tags = req.Query("tags").Select(t =>
                {
                    var tag = fixture.Build<BikeTag>().With(x => x.Name, t).Create();
                    return tag;
                })
                .ToList();
            var bikes = fixture.Create<List<Bike>>().Select(b =>
            {
                b.Tags = tags;
                return b;
            });
            await response.WriteAsJsonAsync(bikes);
            return response;
        }
        
        [Function(nameof(GetById))]
        public async Task<HttpResponseData> GetById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "bike/{idBike}")] HttpRequestData req, Guid idBike)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            var bike = fixture.Build<Bike>().With(b => b.Id, idBike).Create();
            await response.WriteAsJsonAsync(bike);
            return response;
        }
        
        [Function(nameof(UploadFile))]
        public async Task<HttpResponseData> UploadFile(
            [HttpTrigger(AuthorizationLevel.Anonymous, "psot", Route = "bike/{idBike}/uploadImage")] HttpRequestData req, Guid idBike)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(fixture.Create<ApiResponse>());
            return response;
        }
    }
}