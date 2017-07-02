using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;

namespace Zappy
{

    public class NetworkClientManager : MonoBehaviour
    {
        [SerializeField]
        private string serverAddr;

        [SerializeField]
        private int serverPort;

        [SerializeField]
        private NetworkClientState state;

        private ZappyNetworkClient networkClient;

        // Use this for initialization
        void Start()
        {
            GameStateManager.StateChangedEvent += GameStateManagerStateChangedEvent;
        }

        private void GameStateManagerStateChangedEvent(GameState newState)
        {
            if (newState == GameState.RUNNING)
            {
                networkClient = new ZappyNetworkClient(serverAddr, serverPort);
                networkClient.StateChangedEvent += NetworkClientStateChangedEvent;
                networkClient.ConnectToServer();
                networkClient.Authentificate();
            }
        }

        private void NetworkClientStateChangedEvent(NetworkClientState newState)
        {
            state = newState;
        }

        // Update is called once per frame
        void Update()
        {
            networkClient.TrySendBufferizePacket();
            networkClient.TryReceivePacket();
        }

        private void OnDestroy()
        {
            GameStateManager.StateChangedEvent -= GameStateManagerStateChangedEvent;
            networkClient.StateChangedEvent -= NetworkClientStateChangedEvent;
            networkClient.Close();
        }

    }
}