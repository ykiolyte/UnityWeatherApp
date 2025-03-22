using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using Zenject;
using System;

public class BreedsView : MonoBehaviour
{
    [SerializeField] private Transform breedsContent; // Контейнер для списка пород
    [SerializeField] private GameObject breedItemPrefab; // Префаб элемента списка
    [SerializeField] private GameObject popup; // Попап с деталями породы
    [SerializeField] private Text popupText; // Текст внутри попапа
    [SerializeField] private GameObject loadingIndicator; // Индикатор загрузки
    [SerializeField] private Button closePopupButton; // Кнопка закрытия попапа

    private DogBreedsService _dogBreedsService;
    private CancellationTokenSource _cts;

    [Inject]
    public void Construct(DogBreedsService dogBreedsService)
    {
        _dogBreedsService = dogBreedsService;
    }

    private void Start()
    {
        // Назначаем обработчик для кнопки закрытия попапа
        closePopupButton.onClick.AddListener(HidePopup);
    }

    // Обновление списка пород
    public void UpdateBreeds(List<BreedData> breeds)
    {
        if (breeds == null || breeds.Count == 0)
        {
            Debug.LogError("Breeds list is null or empty.");
            return;
        }

        // Очищаем старый список
        foreach (Transform child in breedsContent)
        {
            Destroy(child.gameObject);
        }

        // Создаем новые элементы списка
        for (int i = 0; i < breeds.Count; i++)
        {
            var breedItem = Instantiate(breedItemPrefab, breedsContent);
            breedItem.GetComponentInChildren<Text>().text = $"{i + 1} - {breeds[i].attributes.name}";
            var breedId = breeds[i].id; // Получаем ID породы
            breedItem.GetComponent<Button>().onClick.AddListener(() => OnBreedClicked(breedId));
        }
    }

    // Обработка клика на породу
    private async void OnBreedClicked(string breedId)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        try
        {
            ShowLoadingIndicator(true); // Показываем индикатор загрузки
            var breedDetails = await _dogBreedsService.GetBreedDetailsAsync(breedId, _cts.Token);
            ShowPopup(breedDetails); // Показываем попап с деталями
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Breed details request was canceled.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to fetch breed details: {ex.Message}");
        }
        finally
        {
            ShowLoadingIndicator(false); // Скрываем индикатор загрузки
        }
    }

    // Показ попапа с деталями породы
    public void ShowPopup(string text)
    {
        popupText.text = text;
        popup.SetActive(true);

        // Принудительно обновляем макет, чтобы размер попапа адаптировался под текст
        LayoutRebuilder.ForceRebuildLayoutImmediate(popup.GetComponent<RectTransform>());
    }

    // Скрытие попапа
    public void HidePopup()
    {
        popup.SetActive(false);
    }

    // Показ/скрытие индикатора загрузки
    private void ShowLoadingIndicator(bool show)
    {
        loadingIndicator.SetActive(show);
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}