using System;

namespace HaberlesmeMimarisi.Domain.Messages
{
    // 4-byte: txMessageID | cardID | dataLo | dataHi
    public sealed class TxMessage
    {
        public byte TxMessageId { get; }
        public byte CardId { get; }
        public ushort TxData { get; }

        public TxMessage(byte txId, byte cardId, ushort data = 0x5555)
        {
            TxMessageId = txId;
            CardId = cardId;
            TxData = data;
        }

        public byte[] ToBytes()
        {
            var lo = (byte)(TxData & 0xFF);
            var hi = (byte)(TxData >> 8);
            return new[] { TxMessageId, CardId, lo, hi };
        }

        public override string ToString() => BitConverter.ToString(ToBytes());
    }
}
