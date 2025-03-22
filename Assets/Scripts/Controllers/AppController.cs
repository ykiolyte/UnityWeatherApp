using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

public class AppController : MonoBehaviour
{
    [Inject] private WeatherService _weatherService;
    [Inject] private DogBreedsService _dogBreedsService;
    [Inject] private WeatherView _weatherView;
    [Inject] private BreedsView _breedsView;
    [Inject] private NavigationController _navigationController;

    private CancellationTokenSource _cts;

    private void Start()
    {
        _cts = new CancellationTokenSource();
        StartAsync(_cts.Token).Forget(); // Запускаем асинхронный метод
    }

    private async UniTaskVoid StartAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (_navigationController.IsWeatherTabActive())
            {
                var weatherData = await _weatherService.GetWeatherAsync(ct);
                _weatherView.UpdateWeather(weatherData);
                await UniTask.Delay(5000, cancellationToken: ct);
            }
            else if (_navigationController.IsBreedsTabActive())
            {
                var breedsData = await _dogBreedsService.GetBreedsAsync(ct);
                if (breedsData != null && breedsData.Count > 0)
                {
                    Debug.Log("Breeds data received: " + breedsData.Count + " items.");
                    _breedsView.UpdateBreeds(breedsData); // Передаем список BreedData
                }
                else
                {
                    Debug.LogError("Failed to load breeds data.");
                }

                await UniTask.WaitUntil(() => !_navigationController.IsBreedsTabActive(), cancellationToken: ct);
            }
        }
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}