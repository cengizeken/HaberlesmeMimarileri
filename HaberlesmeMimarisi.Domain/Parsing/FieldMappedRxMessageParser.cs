using HaberlesmeMimarisi.Domain.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaberlesmeMimarisi.Domain.Parsing
{
    /// <summary>
    /// Çerçevede alan ofsetlerini konfigürasyonla belirleyerek RxMessage parse eder.
    /// Örn: preamble(2) + header(2) + RxId(1) + CardId(1) + DataLo(1) + DataHi(1) ...
    /// </summary>
    public sealed class FieldMappedRxMessageParser : IRxMessageParser
    {
        public sealed class Map
        {
            public int RxIdOffset { get; set; }     // buffer içindeki absolute ofset (offset'e göre relatif düşünülür)
            public int CardIdOffset { get; set; }
            public int DataLoOffset { get; set; }
            public int DataHiOffset { get; set; }

            /// <summary>Parse edebilmek için gereken minimum uzunluk (RxId/CardId/DataLo/DataHi'ye kadar erişim için).</summary>
            public int MinCountRequired =>
                Math.Max(Math.Max(RxIdOffset, CardIdOffset), Math.Max(DataLoOffset, DataHiOffset)) + 1;
        }

        private readonly Map _map;

        public FieldMappedRxMessageParser(Map map)
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
            if (_map.MinCountRequired <= 0) throw new ArgumentException("Invalid mapping");
        }

        public RxMessage Parse(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (count < _map.MinCountRequired)
                throw new ArgumentException($"Insufficient frame length. Need >= {_map.MinCountRequired}, got {count}");

            byte rxId = buffer[offset + _map.RxIdOffset];
            byte card = buffer[offset + _map.CardIdOffset];
            byte lo = buffer[offset + _map.DataLoOffset];
            byte hi = buffer[offset + _map.DataHiOffset];
            ushort data = (ushort)(lo | (hi << 8));

            return new RxMessage(rxId, card, data);
        }
    }
}
