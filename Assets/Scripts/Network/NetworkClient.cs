using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Zappy
{
    public enum NetworkClientState
    {
        None,
        Connecting,
        Connected,
        Error,
        Closed
    }

    public class NetworkClient
    {
        private string serverAddr;
        private int serverPort;

        private TcpClient tcpClient;

        private NetworkClientState state;

        private NetworkData data;

        private bool canReceiveNewData;

        public delegate void StateChangedDelegate(NetworkClientState newState);
        public event StateChangedDelegate StateChangedEvent;

        public NetworkClient(string serverAddr, int serverPort)
        {
            this.serverAddr = serverAddr;
            this.serverPort = serverPort;
            tcpClient = null;
            data = null;
            //StateChangedEvent = null;
            //DataReceivedEvent = null;
            //DataSendedEvent = null;
            canReceiveNewData = true;
            SetState(NetworkClientState.None);
        }

        #region ConnectToServer
        public virtual void ConnectToServer()
        {
            if (state == NetworkClientState.Connected)
                return;
            try
            {
                tcpClient = new TcpClient();
                tcpClient.BeginConnect(serverAddr, serverPort, new System.AsyncCallback(ConnectToServerCallback), tcpClient);
                SetState(NetworkClientState.Connecting);
                Debug.Log("NetworkClient - Connecting");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("NetworkClient - " + e.Message);
            }
        }

        private void ConnectToServerCallback(IAsyncResult ar)
        {
            try
            {
                //tcpClient = (TcpClient)ar.AsyncState;
                tcpClient.EndConnect(ar);

                data = new NetworkData();
                Debug.Log("NetworkClient - Connected!");
                SetState(NetworkClientState.Connected);
                Receive();
            }
            catch (Exception e)
            {
                Debug.LogWarning("NetworkClient - " + e.Message);
                SetState(NetworkClientState.Error);
            }
        }

        #endregion

        #region Receive
        protected virtual void Receive()
        {
            if (state != NetworkClientState.Connected)
                return;
            if (canReceiveNewData == false)
                return;
            try
            {
                canReceiveNewData = false;
                tcpClient.GetStream().BeginRead(data.buffer, 0, NetworkData.bufferSize, new AsyncCallback(ReceiveCallback), data);
            }
            catch (Exception e)
            {
                Debug.LogWarning("NetworkClient / Receive - " + e.Message);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                if (state != NetworkClientState.Connected)
                    return;

                //data = (NetworkData)ar.AsyncState;

                int bytesRead = tcpClient.GetStream().EndRead(ar);

                if (bytesRead < 1)
                {
                    Close();
                    return;
                }
                data.stringBuilder.Append(Encoding.ASCII.GetString(data.buffer, 0, bytesRead));
                string dataTmp = data.ToString();
                var dataSplited = dataTmp.Split('\n');
                for (int i = 0; i < dataSplited.Length; i++)
                {
                    OnDataReceived(dataSplited[i]);
                }
                canReceiveNewData = true;
                data.Reset();
            }
            catch (Exception e)
            {
                Debug.LogWarning("NetworkClient / Receive - " + e.Message + " | " + e.GetType());
                //Close();
            }
        }

        #endregion

        #region Send
        protected virtual void Send(string data)
        {
            if (state != NetworkClientState.Connected)
                return;
            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes(data);

                tcpClient.GetStream().BeginWrite(bytes, 0, bytes.Length, new AsyncCallback(SendCallback), tcpClient);
            }
            catch (Exception e)
            {
                Debug.LogWarning("NetworkClient / Send - " + e.Message);
                //Close();
            }

        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                tcpClient.GetStream().EndWrite(ar);
                tcpClient.GetStream().Flush();
                OnDataSended();
            }
            catch (Exception e)
            {
                Debug.LogWarning("NetworkClient / Write - " + e.Message + " | " + e.GetType());
            }
        }
        #endregion

        public virtual void Close()
        {
            if (state == NetworkClientState.Closed)
                return;

            tcpClient.Close();
            tcpClient = null;
            SetState(NetworkClientState.Closed);
            Debug.Log("NetworkClient - Close!");
        }

        #region Events - State
        private void SetState(NetworkClientState state)
        {
            this.state = state;
            InvokeStateChangedEvent(state);
        }

        private void InvokeStateChangedEvent(NetworkClientState newState)
        {
            if (StateChangedEvent != null)
            {
                StateChangedEvent(newState);
            }
        }
        #endregion

        #region Events - Receive

        protected virtual void OnDataReceived(string data)
        {

        }
        #endregion

        #region Events - Send

        protected virtual void OnDataSended()
        {

        }
        #endregion

    }
}