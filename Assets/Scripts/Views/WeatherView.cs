using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;

public class WeatherView : MonoBehaviour
{
    [SerializeField] private Text weatherText; // Текстовый элемент для отображения данных
    [SerializeField] private Image weatherIcon; // Иконка погоды
    [SerializeField] private Button prevButton; // Кнопка "Назад"
    [SerializeField] private Button nextButton; // Кнопка "Вперед"

    private List<(string temperature, string description, string iconUrl, string startTime, string endTime, string windSpeed, string windDirection, string detailedForecast)> weatherData;
    private int currentIndex = 0;

    private void Start()
    {
        // Назначаем обработчики для кнопок
        prevButton.onClick.AddListener(ShowPrevious);
        nextButton.onClick.AddListener(ShowNext);
    }

    public void UpdateWeather(List<(string temperature, string description, string iconUrl, string startTime, string endTime, string windSpeed, string windDirection, string detailedForecast)> data)
    {
        // Сохраняем текущий индекс
        int previousIndex = currentIndex;

        // Обновляем данные
        weatherData = data;

        // Восстанавливаем индекс, если он в пределах нового списка
        currentIndex = Mathf.Clamp(previousIndex, 0, weatherData.Count - 1);

        // Показываем текущий блок данных
        if (weatherData.Count > 0)
        {
            UpdateWeatherText();
            UpdateNavigationButtons();
        }
    }

    private void UpdateWeatherText()
    {
        var period = weatherData[currentIndex];
        weatherText.text = 
            $"Time: {period.startTime} - {period.endTime}\n" +
            $"Temperature: {period.temperature}\n" +
            $"Description: {period.description}\n" +
            $"Wind: {period.windSpeed} {period.windDirection}\n" +
            $"Details: {period.detailedForecast}";

        // Загружаем иконку погоды
        StartCoroutine(LoadWeatherIcon(period.iconUrl));
    }

    private IEnumerator LoadWeatherIcon(string iconUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(iconUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            weatherIcon.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            Debug.LogError("Failed to load weather icon: " + request.error);
        }
    }

    private void ShowPrevious()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdateWeatherText();
            UpdateNavigationButtons();
        }
    }

    private void ShowNext()
    {
        if (currentIndex < weatherData.Count - 1)
        {
            currentIndex++;
            UpdateWeatherText();
            UpdateNavigationButtons();
        }
    }

    private void UpdateNavigationButtons()
    {
        // Делаем кнопки активными/неактивными в зависимости от текущего индекса
        prevButton.interactable = currentIndex > 0;
        nextButton.interactable = currentIndex < weatherData.Count - 1;
    }
}