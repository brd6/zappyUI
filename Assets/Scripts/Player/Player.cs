using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Zappy
{

    public enum PlayerDirection
    {
        TOP,
        BOTTOM,
        LEFT,
        RIGHT
    }

    public class Player : BasePlayer
    {
        private ZappyGameManager gameManager;

        [SerializeField]
        private PlayerDirection direction;

        [SerializeField]
        private Vector2 position;

        [SerializeField]
        private string teamName;

        [SerializeField]
        private int id;

        [SerializeField]
        private int level;

        private Animator animator;

        private Tile currentTile;

        //private Dictionary<string, Action<PacketData>> packetParsers;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        // Use this for initialization
        void Start()
        {
            gameManager = ((ZappyGameManager)(ZappyGameManager.Instance));
            //ZappyNetworkClient.PacketPlayerEvent += ZappyNetworkClientPacketPlayerEvent;
            //InitializePacketParsers();
            SetPlayerIdle();
            SetDirection(PlayerDirection.RIGHT);
            currentTile = null;
        }

        public void SetDirection(PlayerDirection direction)
        {
            this.direction = direction;
            var spriteRender = GetComponent<SpriteRenderer>();

            spriteRender.flipX = false;
            switch (direction)
            {
                case PlayerDirection.TOP:
                    SetAnimationByName("PlayerWalkBottom");
                    break;
                case PlayerDirection.BOTTOM:
                    SetAnimationByName("PlayerWalkTop");
                    break;
                case PlayerDirection.LEFT:
                    SetAnimationByName("PlayerWalkLeft");
                    break;
                case PlayerDirection.RIGHT:
                    SetAnimationByName("PlayerWalkLeft");
                    spriteRender.flipX = true;
                    break;
                default:
                    break;
            }
        }

        void SetAnimationByName(string name)
        {
            if (CurrentState == BasePlayerState.IDLE || CurrentState == BasePlayerState.NONE)
                animator.speed = 0f;
            else if (CurrentState == BasePlayerState.WALKING)
                animator.speed = 1f;
            animator.Play(name);
        }

        private void ZappyNetworkClientPacketPlayerEvent(PacketData data)
        {
        }

        private void InitializePacketParsers()
        {
            //packetParsers = new Dictionary<string, Action<PacketData>>();

            //packetParsers["msz"] = ParseMapSize;
            //packetParsers["bct"] = ParseMapCaseContent;
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void SetPosition(Vector2 position)
        {
            this.position = position;
            if (currentTile != null)
            {
                transform.position = currentTile.transform.position;
            }
        }

        public void SetCurrentTile(Tile currentTile)
        {
            this.currentTile = currentTile;
            transform.position = currentTile.transform.position;
        }

        public void SetId(int id)
        {
            this.id = id;
        }

        public int GetId()
        {
            return id;
        }

        public void SetLevel(int level)
        {
            this.level = level;
        }

        public void SetTeamName(string teamName)
        {
            this.teamName = teamName;
        }

        public void Remove()
        {
            Destroy(gameObject);
        }

        public void Move(Vector2 position, Tile tile)
        {
            if (tile == null)
                return;

            currentTile = tile;
            SetPlayerWalking();
            SetDirection(direction);
            iTween.MoveTo(gameObject, iTween.Hash(
                "position", currentTile.transform.position,
                "time", 3,
                "oncomplete", "OnMoveComplete"
                ));
        }

        private void OnMoveComplete()
        {
            SetPlayerIdle();
            SetDirection(direction);
        }
    }
}