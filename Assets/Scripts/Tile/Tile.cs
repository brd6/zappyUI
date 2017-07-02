using System.Collections.Generic;
using UnityEngine;

namespace Zappy
{

    public class Tile : MonoBehaviour
    {
        [SerializeField]
        private Vector2 position;

        [SerializeField]
        private Dictionary<int, int> resourcesQuantity;

        private List<Resource> resources;

        private TileMap tileMap;

        private int canUpdateResourceCpt;

        private readonly object dicSyncLock = new object();

        private void Awake()
        {
            lock (dicSyncLock)
            {
                resourcesQuantity = new Dictionary<int, int>();
            }
            resources = new List<Resource>();
            canUpdateResourceCpt = 0;
            tileMap = null;
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (canUpdateResourceCpt > 0)
            {
                UpdateAllResourceQuantity();
                canUpdateResourceCpt = 0;
            }
        }

        void UpdateAllResourceQuantity()
        {
            lock (dicSyncLock)
            {
                DeleteCurrentResources();
                foreach (var item in resourcesQuantity)
                {
                    InstantiateResource(item.Key);
                }
            }
        }

        void InstantiateResource(int id)
        {
            var resource = GetResourceById(id);
            if (resource == null)
                return;
            var resourceGameObj = Instantiate(resource, transform);

            var tileBound = GetComponent<SpriteRenderer>().bounds;

            Vector3 pos = transform.position;

            pos.x = Random.Range(pos.x - tileBound.extents.x, pos.x + tileBound.extents.x);
            pos.y = Random.Range(pos.y - tileBound.extents.y, pos.y + tileBound.extents.y);

            resourceGameObj.transform.position = pos;

            resources.Add(resourceGameObj.GetComponent<Resource>());
        }

        Resource GetResourceById(int id)
        {
            if (tileMap == null)
                return null;
            foreach (var item in tileMap.resourcePrefabs.ToArray())
            {
                var resource = item.GetComponent<Resource>();
                if (resource.rId == id)
                {
                    return resource;
                }
            }
            return null;
        }

        void DeleteCurrentResources()
        {
            foreach (var item in resources  )
            {
                item.Remove();
            }
            resources.Clear();
        }

        public void UpdateResourceQuantity(int id, int quantity, TileMap tileMap)
        {
            lock (dicSyncLock)
            {
                resourcesQuantity[id] = quantity;
            }
            canUpdateResourceCpt++;
            this.tileMap = tileMap;
        }

        public void SetTilePosition(Vector2 position)
        {
            this.position = position;
        }

        public void SetTilePosition(int x, int y)
        {
            this.position.x = x;
            this.position.y = y;
        }

        public Vector2 GetPosition()
        {
            return position;
        }

        public void SetTileMap(TileMap tileMap)
        {
            this.tileMap = tileMap;
        }


    }
}