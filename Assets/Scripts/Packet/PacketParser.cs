using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zappy
{
    public class PacketParser
    {
        public static PacketData Parse(string data)
        {
            PacketData packetData = new PacketData();
            char[] seps = new char[1];
            seps[0] = ' ';
            var dataSplited = data.Split(seps, 2);

            if (dataSplited.Length < 1 || dataSplited[0] == string.Empty)
                return null;

            packetData.rawData = data;
            packetData.command = dataSplited[0];
            packetData.parameters = dataSplited.Length > 1 ? dataSplited[1] : string.Empty;

            return packetData;
        }
    }
}