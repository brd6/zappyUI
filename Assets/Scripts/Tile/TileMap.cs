using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zappy
{

    public class TileMap : MonoBehaviour
    {

        [SerializeField]
        private Vector2 mapSize;

        [SerializeField]
        private List<GameObject> groundPrefabs;

        [SerializeField]
        private Vector3 firstGroundPosition;

        [SerializeField]
        public List<GameObject> resourcePrefabs;

        private List<Tile> tiles;

        private Dictionary<string, Action<PacketData>> packetParsers;

        private int canCreateMapCpt;

        // Use this for initialization
        void Start()
        {
            canCreateMapCpt = 0;
            InitializePacketParsers();
            ZappyNetworkClient.PacketMapEvent += ZappyNetworkClientPacketMapEvent;
            tiles = null;
        }

        private void InitializePacketParsers()
        {
            packetParsers = new Dictionary<string, Action<PacketData>>();

            packetParsers["msz"] = ParseMapSize;
            packetParsers["bct"] = ParseMapCaseContent;
        }

        #region Packet Parsers

        private void ParseMapSize(PacketData data)
        {
            var parametersSplited = data.parameters.Split(' ');

            mapSize.x = Int32.Parse(parametersSplited[0]);
            mapSize.y = Int32.Parse(parametersSplited[1]);
            canCreateMapCpt++;
        }

        private void ParseMapCaseContent(PacketData data)
        {
            //Debug.Log("tile " + data.rawData);

            int resourceId;
            int resourceQuantity;

            Vector2 position = Vector2.zero;
            var parametersSplited = data.parameters.Split(' ');

            float.TryParse(parametersSplited[0], out position.x);
            float.TryParse(parametersSplited[1], out position.y);

            Debug.Log(position);

            var tile = GetTileAtPosition(position);
            if (tile == null)
                return;

            Debug.Log(">> " + tile.GetPosition());

            resourceId = 0;
            resourceQuantity = 0;
            for (int i = 2; i < parametersSplited.Length; i++)
            {
                Int32.TryParse(parametersSplited[i], out resourceQuantity);
                tile.UpdateResourceQuantity(resourceId, resourceQuantity, this);
                resourceId++;
            }
        }

        #endregion

        private void ZappyNetworkClientPacketMapEvent(PacketData data)
        {
            if (packetParsers.ContainsKey(data.command))
                packetParsers[data.command](data);
        }

        // Update is called once per frame
        void Update()
        {
            if (canCreateMapCpt > 0)
            {
                CreateMap();
                canCreateMapCpt = 0;
            }
        }

        void CreateMap()
        {
            tiles = new List<Tile>();

            Vector2 tileSize = Vector2.zero;
            Vector3 tilePosition = Vector3.zero; //firstGroundPosition;

            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    var tile = InstantiateTile();
                    SetTileInstancePosition(x, y, tile);
                    SetTileInstanceName(x, y, tile);
                }
            }
        }

        Tile InstantiateTile()
        {
            int groundPrefabIndex = UnityEngine.Random.Range(0, groundPrefabs.Count - 1);
            var groundPrefab = groundPrefabs[groundPrefabIndex];
            var tile = Instantiate(groundPrefab, transform).GetComponent<Tile>();
            tile.SetTileMap(this);
            tiles.Add(tile);
            return tile;
        }

        void SetTileInstancePosition(int x, int y, Tile currentTile)
        {
            var tilePosition = Vector3.zero;
            Vector2 tileSize = currentTile.GetComponent<SpriteRenderer>().bounds.size;

            tilePosition.x = x * tileSize.x;
            tilePosition.y = -y * tileSize.y;
            tilePosition.z = 10;

            currentTile.transform.localPosition = tilePosition;
            currentTile.SetTilePosition(x, y);
        }

        void SetTileInstanceName(int x, int y, Tile currentTile)
        {
            currentTile.name = "tile" + x + "-" + y;
        }

        public Tile GetTileAtPosition(Vector2 position)
        {
            if (tiles == null)
                return null;
            foreach (var tile in tiles.ToArray())
            {
                var tilePos = tile.GetPosition();
                if (tilePos.x == position.x && tilePos.y == position.y)
                {
                    Debug.Log("tileOK");
                    return tile;
                }
            }
            return null;
        }

        private void OnDestroy()
        {
            ZappyNetworkClient.PacketMapEvent -= ZappyNetworkClientPacketMapEvent;
        }

    }
}