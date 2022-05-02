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
        [Required]
        public string Name { get; set; }
        public string Category { get; set; }
        public string Vendor { get; set; }
        public List<string> PhotoUrls { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        [OpenApiProperty(Description = "Bike status in the store")]
        public BikeStatus Status { get; set; }
    }
}