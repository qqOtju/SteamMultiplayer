using Mirror;
using Project.Scripts.GameLogic;
using Project.Scripts.UI;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

namespace Project.Scripts.Infrastructure
{
    public class GameInstaller: MonoInstaller
    {
        [SerializeField] private CinemachineCamera _camera;
        [SerializeField] private MatchController _matchController;
        [SerializeField] private LobbyChat _lobbyChat;
        
        public static DiContainer DiContainer { get; private set; }

        public override void InstallBindings()
        {
            BindCinemachineCamera();
            BindMatchController();
            DiContainer = Container;
        }

        private void BindCinemachineCamera()
        {
            Container.Bind<CinemachineCamera>().FromInstance(_camera).AsSingle();
        }

        private void BindMatchController()
        {
            Container.Bind<MatchController>().FromInstance(_matchController).AsSingle();
        }
    }
}