namespace Shears.Services
{
    public class ServiceLocatorSceneBootstrapper : Bootstrapper
    {
        protected override void Bootstrap()
        {
            Locator.ConfigureForScene();
        }
    }
}
