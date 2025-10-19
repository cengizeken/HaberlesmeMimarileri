using HaberlesmeMimarisi.Domain.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaberlesmeMimarisi.Domain.Parsing
{
    /// <summary>
    /// 4 byte'lık sabit frame: RxId | CardId | DataLo | DataHi
    /// </summary>
    public sealed class Fixed4ByteRxMessageParser : IRxMessageParser
    {
        public RxMessage Parse(byte[] buffer, int offset, int count)
        {
            if (buffer == null || count < 4)
                throw new ArgumentException("Fixed4Byte parser requires >= 4 bytes");

            var rxId = buffer[offset + 0];
            var card = buffer[offset + 1];
            var lo = buffer[offset + 2];
            var hi = buffer[offset + 3];
            ushort data = (ushort)(lo | (hi << 8));

            return new RxMessage(rxId, card, data);
        }
    }
}
