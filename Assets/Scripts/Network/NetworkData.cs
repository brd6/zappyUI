using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Zappy
{
    class NetworkData
    {
        public const int bufferSize = 2048;
        public byte[] buffer;
        public StringBuilder stringBuilder;

        public NetworkData()
        {
            Reset();
        }

        public void Reset()
        {
            buffer = new byte[bufferSize];
            stringBuilder = new StringBuilder();
        }

        public override string ToString()
        {
            return stringBuilder.ToString();
        }
    }
}