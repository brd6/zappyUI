using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zappy
{

    public class PlayerInfo
    {
        public int id = 0;
        public Vector2 position = Vector2.zero;
        public PlayerDirection direction = PlayerDirection.LEFT;
        public int level = 0;
        public string teamName = string.Empty;
    }

    public class PlayerManager : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> players;

        [SerializeField]
        private GameObject playerPrefab;

        [SerializeField]
        private TileMap tileMap;

        [SerializeField]
        private Transform playerContainer;

        private Dictionary<string, Action<PacketData>> packetParsers;

        private Dictionary<string, PlayerDirection> playerDirections;

        private Queue<PlayerInfo> playersToCreate;
        private Queue<int> playersToDeleteById;
        private Queue<PlayerInfo> playersToMove;

        // Use this for initialization
        void Start()
        {
            players = new List<GameObject>();
            InitializePacketParsers();
            InitializePlayerDirections();
            playersToCreate = new Queue<PlayerInfo>();
            playersToDeleteById = new Queue<int>();
            playersToMove = new Queue<PlayerInfo>();
            ZappyNetworkClient.PacketPlayerEvent += ZappyNetworkClientPacketPlayerEvent;
        }

        private void InitializePlayerDirections()
        {
            playerDirections = new Dictionary<string, PlayerDirection>();

            playerDirections["0"] = PlayerDirection.TOP;
            playerDirections["1"] = PlayerDirection.TOP;
            playerDirections["2"] = PlayerDirection.RIGHT;
            playerDirections["3"] = PlayerDirection.BOTTOM;
            playerDirections["4"] = PlayerDirection.LEFT;
        }

        private void ZappyNetworkClientPacketPlayerEvent(PacketData data)
        {
            if (packetParsers.ContainsKey(data.command))
                packetParsers[data.command](data);
        }

        private void InitializePacketParsers()
        {
            packetParsers = new Dictionary<string, Action<PacketData>>();

            packetParsers["pnw"] = ParsePlayerConnection;
            packetParsers["pdi"] = ParsePlayerDead;
            packetParsers["ppo"] = ParsePlayerMovement;
        }

        #region Packet Parsers
        private void ParsePlayerConnection(PacketData data)
        {
            Vector2 position = Vector2.zero;
            int level = 0;
            int id = 0;
            var parametersSplited = data.parameters.Split(' ');

            int.TryParse(parametersSplited[0], out id);
            float.TryParse(parametersSplited[1], out position.x);
            float.TryParse(parametersSplited[2], out position.y);
            int.TryParse(parametersSplited[4], out level);

            PlayerInfo playerInfo = new PlayerInfo();

            playerInfo.position = position;
            playerInfo.direction = playerDirections[parametersSplited[3]];
            playerInfo.level = level;
            playerInfo.teamName = parametersSplited[5];
            playerInfo.id = id;

            playersToCreate.Enqueue(playerInfo);
        }

        private void ParsePlayerDead(PacketData data)
        {
            int id = 0;
            var parametersSplited = data.parameters.Split(' ');

            int.TryParse(parametersSplited[0], out id);

            playersToDeleteById.Enqueue(id);
        }

        private void ParsePlayerMovement(PacketData data)
        {
            Vector2 position = Vector2.zero;
            int id = 0;
            var parametersSplited = data.parameters.Split(' ');

            int.TryParse(parametersSplited[0], out id);
            float.TryParse(parametersSplited[1], out position.x);
            float.TryParse(parametersSplited[2], out position.y);

            PlayerInfo playerInfo = new PlayerInfo();

            playerInfo.position = position;
            playerInfo.direction = playerDirections[parametersSplited[3]];
            playerInfo.id = id;

            playersToMove.Enqueue(playerInfo);
        }

        #endregion

        // Update is called once per frame
        void Update()
        {
            ProcessPlayersCreation();
            ProcessPlayersRemove();
            ProcessPlayersMovement();
        }

        void ProcessPlayersCreation()
        {
            if (playersToCreate.Count < 1)
                return;

            var playerInfo = playersToCreate.Dequeue();
            var player = CreatePlayer(playerInfo).GetComponent<Player>();

            var tile = tileMap.GetTileAtPosition(playerInfo.position);

            player.SetPosition(tile.GetPosition());
            player.SetCurrentTile(tile);
            player.SetLevel(playerInfo.level);
            player.SetTeamName(playerInfo.teamName);
            player.SetDirection(playerInfo.direction);
            player.SetId(playerInfo.id);

            players.Add(player.gameObject);
        }

        private GameObject CreatePlayer(PlayerInfo playerInfo)
        {
            var player = Instantiate(playerPrefab, playerContainer);

            Debug.LogWarning("CreatePlayer");

            return player;
        }

        void ProcessPlayersRemove()
        {
            if (playersToDeleteById.Count < 1)
                return;
            int id = playersToDeleteById.Dequeue();
            var player = players.Find(e => e.GetComponent<Player>().GetId() == id);
            if (player == null)
                return;
            player.GetComponent<Player>().Remove();
            players.Remove(player);
        }

        void ProcessPlayersMovement()
        {
            if (playersToMove.Count < 1)
                return;
            var playerInfo = playersToMove.Dequeue();
            var playerGameObject = players.Find(e => e.GetComponent<Player>().GetId() == playerInfo.id);
            if (playerGameObject == null)
                return;
            var player = playerGameObject.GetComponent<Player>();
            var tile = tileMap.GetTileAtPosition(playerInfo.position);
            player.SetDirection(playerInfo.direction);
            player.Move(playerInfo.position, tile);
        }

    }
}