using UnityEngine;

public class NavigationController : MonoBehaviour
{
    [SerializeField] private GameObject weatherPanel;
    [SerializeField] private GameObject breedsPanel;

    private void Start()
    {
        // По умолчанию показываем вкладку с погодой
        ShowWeatherPanel();
    }

    public void ShowWeatherPanel()
    {
        weatherPanel.SetActive(true);
        breedsPanel.SetActive(false);
    }

    public void ShowBreedsPanel()
    {
        weatherPanel.SetActive(false);
        breedsPanel.SetActive(true);
    }

    public bool IsWeatherTabActive()
    {
        return weatherPanel.activeSelf;
    }

    public bool IsBreedsTabActive()
    {
        return breedsPanel.activeSelf;
    }
}