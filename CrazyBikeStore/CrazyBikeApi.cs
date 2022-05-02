using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using Bogus;
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
        //readonly ILogger logger;
        readonly Fixture fixture;
        readonly Faker<Bike> faker;
        public CrazyBikeApi(ILoggerFactory loggerFactory, Fixture fixture, Faker<Bike> faker)
        {
            //logger = loggerFactory.ThrowIfNullOrDefault().CreateLogger<CrazyBikeApi>();
            this.fixture = fixture.ThrowIfNullOrDefault();
            this.faker = faker.ThrowIfNullOrDefault();
        }
    
        [Function(nameof(AddBike))]
        [OpenApiOperation(operationId: "addBike", tags: new[] { "bike" }, 
            Summary = "Add a new bike to the store", Description = "This add a new bike to the store.", 
            Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiSecurity("bikestore_auth", SecuritySchemeType.OAuth2, Flows = typeof(BikeStoreAuth))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Bike), Required = true, 
            Description = "Bike that needs to be added to the store")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Bike), 
            Summary = "New bike added", Description = "New bike added")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.MethodNotAllowed, 
            Summary = "Invalid input", Description = "Invalid input")]
        public async Task<HttpResponseData> AddBike(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "bike")] HttpRequestData req)
        {
            var bike = await req.ReadFromJsonAsync<Bike>();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(bike);
            return response;
        }
        
        [Function(nameof(UpdateBike))]
        [OpenApiOperation(operationId: "updateBike", tags: new[] { "bike" }, 
            Summary = "Update an existing bike", Description = "This updates an existing bike.", Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiSecurity("bikestore_auth", SecuritySchemeType.OAuth2, Flows = typeof(BikeStoreAuth))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Bike), Required = true, 
            Description = "Bike that needs to be updated")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Bike), 
            Summary = "bike updated", Description = "bike updated")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, 
            Summary = "Invalid idBike supplied", Description = "Invalid idBike supplied")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, 
            Summary = "Bike not found", Description = "Bike not found")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.MethodNotAllowed, 
            Summary = "Validation exception", Description = "Validation exception")]
        public async Task<HttpResponseData> UpdateBike(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "bike")] HttpRequestData req)
        {
            var bike = await req.ReadFromJsonAsync<Bike>();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(bike);
            return response;
        }
        
        [Function(nameof(DeleteBike))]
        [OpenApiOperation(operationId: "deleteBike", tags: new[] { "bike" }, 
            Summary = "Deletes a bike", Description = "This deletes a bike", Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiSecurity("bikestore_auth", SecuritySchemeType.OAuth2, Flows = typeof(BikeStoreAuth))]
        [OpenApiParameter(name: "api_key", In = ParameterLocation.Header, Type = typeof(string), Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "idBike", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), 
            Summary = "Bike id to delete", Description = "Bike id to delete", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Summary = "Successful operation", Description = "Successful operation")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "Invalid idBike supplied", Description = "Invalid idBike supplied")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Summary = "Bike not found", Description = "Bike not found")]
        public Task<HttpResponseData> DeleteBike(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "bike/{idBike}")] HttpRequestData req, Guid idBike)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            return Task.FromResult(response);
        }
        
        [Function(nameof(FindByStatus))]
        [OpenApiOperation(operationId: "findBikeByStatus", tags: new[] { "bike" }, Summary = "Find bikes by status", 
            Description = "Multiple status values can be provided with comma separated strings.", Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiSecurity("bikestore_auth", SecuritySchemeType.OAuth2, Flows = typeof(BikeStoreAuth))]
        [OpenApiParameter(name: "status", In = ParameterLocation.Query, Required = true, Type = typeof(List<BikeStatus>), Explode = true, 
            Summary = "Bike status value", Description = "Status values that need to be considered for filter", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Bike>), 
            Summary = "Successful operation", Description = "Successful operation")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, 
            Summary = "Invalid status value", Description = "Invalid status value")]
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
            var bikes = faker
                .RuleFor(x => x.Status, f => f.PickRandom(status))
                .GenerateBetween(1, 10);
            await response.WriteAsJsonAsync(bikes);
            return response;
        }
        
        [Function(nameof(FindByTags))]
        [OpenApiOperation(operationId: "findBikeByTags", tags: new[] { "bike" }, Summary = "Find bikes by tags", 
            Description = "Muliple tags can be provided with comma separated strings.", Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiSecurity("bikestore_auth", SecuritySchemeType.OAuth2, Flows = typeof(BikeStoreAuth))]
        [OpenApiParameter(name: "tags", In = ParameterLocation.Query, Required = true, Type = typeof(List<string>), Explode = true,
            Summary = "Tags to filter by", Description = "Tags to filter by", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Bike>), 
            Summary = "Successful operation", Description = "Successful operation")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, 
            Summary = "Invalid tag value", Description = "Invalid tag value")]
        public async Task<HttpResponseData> FindByTags(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "bike/findByTags")] HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            var tags = req.Query("tags").ToList();
            var bikes = faker
                .RuleFor(x => x.Tags, tags)
                .GenerateBetween(1, 10);
            await response.WriteAsJsonAsync(bikes);
            return response;
        }
        
        [Function(nameof(GetById))]
        [OpenApiOperation(operationId: "getBikeById", tags: new[] { "bike" }, 
            Summary = "Find bike by idBike", Description = "Returns a single bike.", Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiSecurity("bikestore_auth", SecuritySchemeType.OAuth2, Flows = typeof(BikeStoreAuth))]
        [OpenApiParameter(name: "idBike", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), 
            Summary = "ID of of the bike", Description = "ID of the bike", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Bike), 
            Summary = "Successful operation", Description = "Successful operation")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, 
            Summary = "Invalid idBike", Description = "Invalid idBike")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, 
            Summary = "Bike not found", Description = "Bike not found")]
        public async Task<HttpResponseData> GetById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "bike/{idBike}")] HttpRequestData req, Guid idBike)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            var bike = faker
                .RuleFor(x => x.Id, idBike)
                .Generate();
            await response.WriteAsJsonAsync(bike);
            return response;
        }
        
        [Function(nameof(UploadFile))]
        [OpenApiOperation(operationId: "uploadFile", tags: new[] { "bike" }, 
            Summary = "Uploads a bike image", Description = "Uploads a bike image", Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiSecurity("bikestore_auth", SecuritySchemeType.OAuth2, Flows = typeof(BikeStoreAuth))]
        [OpenApiParameter(name: "idBike", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), 
            Summary = "idBike of bike to update", Description = "idBike of bike to update", Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiRequestBody(contentType: "multipart/form-data", bodyType: typeof(BikeFormData))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ApiResponse), 
            Summary = "Successful operation", Description = "Successful operation")]
        public async Task<HttpResponseData> UploadFile(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "bike/{idBike}/uploadImage")] HttpRequestData req, Guid idBike)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(fixture.Create<ApiResponse>());
            return response;
        }
    }
}