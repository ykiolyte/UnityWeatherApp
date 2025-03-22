using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading;

public class WeatherService
{
    private const string WeatherApiUrl = "https://api.weather.gov/gridpoints/TOP/32,81/forecast";
    private readonly RequestQueue _requestQueue;

    public WeatherService(RequestQueue requestQueue)
    {
        _requestQueue = requestQueue;
    }

    public async UniTask<List<(string temperature, string description, string iconUrl, string startTime, string endTime, string windSpeed, string windDirection, string detailedForecast)>> GetWeatherAsync(CancellationToken ct)
    {
        var tcs = new UniTaskCompletionSource<List<(string, string, string, string, string, string, string, string)>>();

        _requestQueue.AddRequest(async (cancellationToken) =>
        {
            UnityWebRequest request = UnityWebRequest.Get(WeatherApiUrl);
            try
            {
                await request.SendWebRequest().ToUniTask().AttachExternalCancellation(cancellationToken);
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var json = request.downloadHandler.text;
                    var weatherData = ParseWeatherData(json);
                    tcs.TrySetResult(weatherData);
                }
                else
                {
                    tcs.TrySetException(new Exception(request.error));
                }
            }
            finally
            {
                request.Dispose();
            }
            return request;
        });

        return await tcs.Task;
    }

    private List<(string temperature, string description, string iconUrl, string startTime, string endTime, string windSpeed, string windDirection, string detailedForecast)> ParseWeatherData(string json)
    {
        var weatherData = JsonUtility.FromJson<WeatherResponse>(json);
        var periods = new List<(string, string, string, string, string, string, string, string)>();
        if (weatherData != null && weatherData.properties.periods.Length > 0)
        {
            foreach (var period in weatherData.properties.periods)
            {
                periods.Add((
                    period.temperature + "F",
                    period.shortForecast,
                    period.icon,
                    period.startTime,
                    period.endTime,
                    period.windSpeed,
                    period.windDirection,
                    period.detailedForecast
                ));
            }
        }
        else
        {
            Debug.LogError("No weather data available.");
        }
        return periods;
    }
    

    [Serializable]
    private class WeatherResponse
    {
        public Properties properties;
    }

    [Serializable]
    private class Properties
    {
        public Period[] periods;
    }

    [Serializable]
    private class Period
    {
        public int temperature;
        public string shortForecast;
        public string icon;
        public string startTime;
        public string endTime;
        public string windSpeed;
        public string windDirection;
        public string detailedForecast;
    }
}