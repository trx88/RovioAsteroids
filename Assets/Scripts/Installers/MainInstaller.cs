using RovioAsteroids.Actions;
using RovioAsteroids.Controllers;
using RovioAsteroids.MonoBehaviors;
using RovioAsteroids.MonoBehaviors.Abstraction;
using RovioAsteroids.MonoBehaviors.GameObjectFactories;
using RovioAsteroids.MonoBehaviors.GameObjectFactories.Abstraction;
using RovioAsteroids.MVVM.ViewModels;
using RovioAsteroids.Repository.Items.DataModels;
using RovioAsteroids.Repository.Repositories.RepositoryFactories;
using RovioAsteroids.Services;
using RovioAsteroids.Signals;
using RovioAsteroids.Utils;
using UnityEngine;
using Zenject;

namespace RovioAsteroids.Installers
{
    /// <summary>
    /// Used to install all services/controllers/signals/factories/repositories
    /// that will be injected throughout the project.
    /// </summary>
    public class MainInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            InstallRepositoryFactories();
            InstallSignals();
            InstallUtils();
            InstallFactories();
            InstallServices();
            InstallControllers();
            InstallViewModels();

            //Done here, so GameController does not need to inject EnemyCollisionController for it to be created on game start.
            Container.Resolve<EnemyCollisionController>();
        }

        private void InstallUtils()
        {
            Container.Bind<MapHelper>().AsSingle();
        }

        //Used for UI's MVVM.
        private void InstallViewModels()
        {
            Container.BindInterfacesAndSelfTo<HudScreenViewModel>().AsSingle();
        }

        private void InstallControllers()
        {
            Container.BindInterfacesAndSelfTo<EnemyCollisionController>().AsSingle();
        }

        private void InstallServices()
        {
            Container.BindInterfacesAndSelfTo<EnemySpawnerService>().AsSingle();
            Container.BindInterfacesAndSelfTo<EnemyHandlerService>().AsSingle();
            Container.BindInterfacesAndSelfTo<ScoringService>().AsSingle();
        }

        private void InstallSignals()
        {
            SignalBusInstaller.Install(Container);
            Container.DeclareSignal<EnemyCollisionSignal>();
        }

        private void InstallFactories()
        {
            Container.BindInterfacesAndSelfTo<AddressableLoader>().AsSingle();
            Container.BindFactory<string, Vector3, Quaternion, IAddresableGameObject, GameObjectFactory>().FromFactory<CustomGameObjectFactory>();
        }

        private void InstallRepositoryFactories()
        {
            Container.BindInterfacesAndSelfTo<InMemoryRepositoryFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerPrefsRepositoryFactory>().AsSingle();

            ConfigureInMemoryRepositories();
            ConfigurePlayerPrefsRepositories();
        }

        private void ConfigureInMemoryRepositories()
        {
            var inMemoryRepoFactory = Container.Resolve<InMemoryRepositoryFactory>();

            inMemoryRepoFactory.AddRepositoryConfig(
                new RepositoryConfig(typeof(GameSessionData),
                new InitializeGameSessionDataAction(inMemoryRepoFactory)
                ));
            inMemoryRepoFactory.AddRepositoryConfig(
                new RepositoryConfig(typeof(EnemyData)
                ));
        }

        private void ConfigurePlayerPrefsRepositories()
        {
            var playerPrefRepoFactory = Container.Resolve<PlayerPrefsRepositoryFactory>();

            playerPrefRepoFactory.AddRepositoryConfig(
                new RepositoryConfig(typeof(HiScoreData),
                new InitializeHiScoreDataAction(playerPrefRepoFactory)
                ));
        }
    }
}
