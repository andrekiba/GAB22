using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using CrazyBikeStore.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace CrazyBikeStore.Infrastructure;

public static class BikeFakerExtensions
{
    static readonly string[] bikeNames = 
    { 
        "spark rc", "genius", "scalpel", "twostroke", "lux",
        "jam", "anthem", "ninety", "supercaliber", "fourstroke" 
    };
    
    static readonly string[] bikeVendors = 
    { 
        "scott", "cannondale", "bmc", "wilier", "trek", "giant", "santa cruz",
        "merida", "ghost", "canyon", "conway", "cube", "focus" 
    };
    
    static readonly string[] bikeCategories = 
    { 
        "mtb-xc", "mtb-trail", "mtb-enduro", "mtb-downhill", "bdc-aero",
        "bdc-endurance", "gravel", "ciclocross", "trekking", "urban" 
    };
    
    static readonly string[] bikeTags = 
    { 
        "full", "front", "fat", "sram", "shimano",
        "fox", "rock shock", "ceramic", "fulcrum"
    };
    
    public static void AddBikeFaker(this IServiceCollection services, IConfiguration configuration)
    {
        var bikeFaker = new Faker<Bike>()
            .RuleFor(x => x.Id, Guid.NewGuid)
            .RuleFor(x => x.Name, f => f.PickRandom(bikeNames))
            .RuleFor(x => x.Category, f => f.PickRandom(bikeCategories))
            .RuleFor(x => x.Vendor, f => f.PickRandom(bikeVendors))
            .RuleFor(x => x.Tags, f => f.Random.ListItems(bikeTags))
            .RuleFor(x => x.Status, f => BikeStatus.Available);
        services.AddSingleton(bikeFaker);
    }
}