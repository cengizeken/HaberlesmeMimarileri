using System;

namespace HaberlesmeMimarisi.Domain.Messages
{
    public sealed class RxMessage
    {
        public byte RxMessageId { get; }
        public byte CardId { get; }
        public ushort RxData { get; }

        public RxMessage(byte rxId, byte cardId, ushort data)
        {
            RxMessageId = rxId;
            CardId = cardId;
            RxData = data;
        }

        public static RxMessage FromBytes(byte[] buf, int offset = 0)
        {
            if (buf == null || buf.Length - offset < 4)
                throw new ArgumentException("RX requires 4 bytes");
            var rxId = buf[offset];
            var card = buf[offset + 1];
            var lo = buf[offset + 2];
            var hi = buf[offset + 3];
            ushort data = (ushort)(lo | (hi << 8));
            return new RxMessage(rxId, card, data);
        }

        public override string ToString()
        {
            var b = new[] { RxMessageId, CardId, (byte)(RxData & 0xFF), (byte)(RxData >> 8) };
            return BitConverter.ToString(b);
        }
    }
}
