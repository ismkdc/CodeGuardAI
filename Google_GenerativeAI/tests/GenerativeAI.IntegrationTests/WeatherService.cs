﻿using CSharpToJsonSchema;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace GenerativeAI.IntegrationTests
{
    public enum Unit
    {
        Celsius,
        Fahrenheit,
        Imperial
    }

    public class Weather
    {
        public string Location { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public Unit Unit { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    [GenerateJsonSchema()]
    public interface IWeatherFunctions
    {
        [Description("Get the current weather in a given location")]
        public Weather GetCurrentWeather(
            [Description("The city and state, e.g. San Francisco, CA")]
            string location,
            Unit unit = Unit.Celsius);

        [Description("Get the current weather in a given location")]
        public Task<Weather> GetCurrentWeatherAsync(
            [Description("The city and state, e.g. San Francisco, CA")]
            string location,
            Unit unit = Unit.Celsius,
            CancellationToken cancellationToken = default);
    }

    [Description("Weather Functions")]
    public class WeatherService : IWeatherFunctions
    {
        [Description("Get the current weather in a given location")]
        public Weather GetCurrentWeather(string location, Unit unit = Unit.Celsius)
        {
            return new Weather
            {
                Location = location,
                Temperature = 30.0,
                Unit = unit,
                Description = "Sunny",
            };
        }

        public Task<Weather> GetCurrentWeatherAsync(string location, Unit unit = Unit.Celsius,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new Weather
            {
                Location = location,
                Temperature = 22.0,
                Unit = unit,
                Description = "Sunny",
            });
        }
    }
}