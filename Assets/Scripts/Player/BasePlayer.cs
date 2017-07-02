using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Zappy
{
    public class BasePlayer : MonoBehaviour
    {
        [SerializeField]
        private BasePlayerState state;

        [SerializeField]
        private Dictionary<int, int> resourcesQuantity;

        protected BasePlayerState CurrentState { get; private set; }

        private void Awake()
        {
            SetPlayerIdle();
        }

        private void Start()
        {
            GameStateManager.StateChangedEvent += StateChanged;
        }

        private void StateChanged(GameState newState)
        {
            if (newState == GameState.GAMEOVER)
            {

            }
        }

        #region StateManager
        protected virtual void SetPlayerDead()
        {
            SetState(BasePlayerState.DEAD);
        }

        protected virtual void SetPlayerWalking()
        {
            SetState(BasePlayerState.WALKING);
        }

        protected virtual void SetPlayerNone()
        {
            SetState(BasePlayerState.NONE);
        }

        protected virtual void SetPlayerIdle()
        {
            SetState(BasePlayerState.IDLE);
        }

        private void SetState(BasePlayerState _state)
        {
            CurrentState = _state;
            state = _state;
        }

        #endregion

    }
}