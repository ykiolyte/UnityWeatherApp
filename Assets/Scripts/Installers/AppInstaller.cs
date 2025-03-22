using Zenject;

public class AppInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // Регистрация сервисов
        Container.BindInterfacesAndSelfTo<WeatherService>().AsSingle();
        Container.BindInterfacesAndSelfTo<DogBreedsService>().AsSingle();

        // Регистрация RequestQueue
        Container.BindInterfacesAndSelfTo<RequestQueue>().AsSingle();

        // Регистрация UI компонентов
        Container.BindInterfacesAndSelfTo<WeatherView>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesAndSelfTo<BreedsView>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesAndSelfTo<NavigationController>().FromComponentInHierarchy().AsSingle();

        // Регистрация контроллера
        Container.BindInterfacesAndSelfTo<AppController>().AsSingle();
    }
}