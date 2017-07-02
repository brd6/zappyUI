using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zappy
{
    public enum ZappyAuthenticateState
    {
        NONE,
        CONNECTED,
        NOREGISTER
    }

    public enum PacketEventType
    {
        ALL,
        MAP,
        PLAYER
    }

    public class PacketEventTypeData
    {
        public List<string> commands;
        public PacketEventType type;
        public Action<PacketData> invoker;

        public PacketEventTypeData()
        {
            commands = new List<string>();
            type = PacketEventType.ALL;
        }
    }

    public class ZappyNetworkClient : NetworkClient
    {
        private ZappyAuthenticateState authenticateState;

        private Queue<string> packetToSend;

        private List<PacketEventTypeData> packetEventTypeDatas;

        public delegate void PacketDelegate(PacketData data);
        public static event PacketDelegate PacketEvent;

        public delegate void PacketMapDelegate(PacketData data);
        public static event PacketMapDelegate PacketMapEvent;

        public delegate void PacketPlayerDelegate(PacketData data);
        public static event PacketPlayerDelegate PacketPlayerEvent;


        public ZappyNetworkClient(string serverAddr, int serverPort) : 
            base(serverAddr, serverPort)
        {
            packetToSend = new Queue<string>();
            authenticateState = ZappyAuthenticateState.NONE;
            InitializePacketEventTypeDatas();
        }

        private void InitializePacketEventTypeDatas()
        {
            packetEventTypeDatas = new List<PacketEventTypeData>();

            PacketEventTypeData eventTypeData = new PacketEventTypeData();

            eventTypeData.type = PacketEventType.MAP;
            eventTypeData.invoker = InvokePacketMapEvent;
            eventTypeData.commands.Add("msz");
            eventTypeData.commands.Add("bct");
            packetEventTypeDatas.Add(eventTypeData);

            eventTypeData = new PacketEventTypeData();
            eventTypeData.type = PacketEventType.PLAYER;
            eventTypeData.invoker = InvokePacketPlayerEvent;
            eventTypeData.commands.Add("pnw");
            eventTypeData.commands.Add("ppo");
            eventTypeData.commands.Add("plv");
            eventTypeData.commands.Add("pin");
            eventTypeData.commands.Add("pex");
            eventTypeData.commands.Add("pbc");
            eventTypeData.commands.Add("pic");
            eventTypeData.commands.Add("pfk");
            eventTypeData.commands.Add("pdr");
            eventTypeData.commands.Add("pgt");
            eventTypeData.commands.Add("pdi");
            eventTypeData.commands.Add("ebo");
            packetEventTypeDatas.Add(eventTypeData);
        }

        protected override void OnDataReceived(string data)
        {
            if (authenticateState == ZappyAuthenticateState.NONE && data.ToLower().CompareTo("WELCOME") == 1)
            {
                authenticateState = ZappyAuthenticateState.NOREGISTER;
            }
            else
            {
                var packet = PacketParser.Parse(data);
                if (packet != null)
                    InvokeAllPacketEvent(packet);
            }
        }

        private void InvokeAllPacketEvent(PacketData packet)
        {
            var packtEventTypeData = GetPacktEventTypeData(packet);
            if (packtEventTypeData != null)
                packtEventTypeData.invoker(packet);
            InvokePacketEvent(packet);
            //Debug.Log(data);
        }

        protected override void OnDataSended()
        {
            if (packetToSend.Count > 0)
                packetToSend.Dequeue();
        }

        public void Authentificate()
        {
            SendPacket("GRAPHIC");
        }

        public void GetMapSize()
        {
            SendPacket("msz");
        }

        private void SendPacket(string data)
        {
            packetToSend.Enqueue(data);
        }

        public void TrySendBufferizePacket()
        {
            if (packetToSend.Count > 0)
                Send(packetToSend.Peek() + "\n");
        }

        public void TryReceivePacket()
        {
            Receive();
        }

        public override void Close()
        {
            base.Close();
        }

        #region Events
        private void InvokePacketEvent(PacketData data)
        {
            if (PacketEvent != null)
            {
                PacketEvent(data);
            }
        }

        private void InvokePacketMapEvent(PacketData data)
        {
            if (PacketMapEvent != null)
            {
                PacketMapEvent(data);
            }
        }

        private void InvokePacketPlayerEvent(PacketData data)
        {
            if (PacketPlayerEvent != null)
            {
                PacketPlayerEvent(data);
            }
        }
        #endregion

        private PacketEventTypeData GetPacktEventTypeData(PacketData data)
        {
            foreach (var item in packetEventTypeDatas.ToArray())
            {
                if (item.commands.Exists(e => e.Equals(data.command)))
                {
                    return item;
                }
            }
            return null;
        }
    }
}