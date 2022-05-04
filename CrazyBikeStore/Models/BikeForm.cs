using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace CrazyBikeStore.Models;

public class BikeForm
{
    public string Metadata { get; set; }
    public byte[] File { get; set; }
}