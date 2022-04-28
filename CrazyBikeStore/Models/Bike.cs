using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;

namespace CrazyBikeStore.Models
{
    public class Bike
    {
        public Guid Id { get; set; }
        public BikeCategory Category { get; set; }
        [Required]
        public string Name { get; set; }
        [JsonRequired]
        public List<string> PhotoUrls { get; set; } = new();
        public List<BikeTag> Tags { get; set; }
        [OpenApiProperty(Description = "Bike status in the store")]
        public BikeStatus Status { get; set; }
    }
}