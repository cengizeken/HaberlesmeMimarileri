using System.Collections.Generic;
using HaberlesmeMimarisi.Domain.Repositories;
using HaberlesmeMimarisi.Domain.Evaluation;

namespace HaberlesmeMimarisi.Repository.Json
{
    // For demo: we build the same catalog in-memory. You can later load from JSON.
    public sealed class JsonMessageDefinitionRepository : IMessageDefinitionRepository
    {
        public IReadOnlyList<MessageDefinition> GetAll()
        {
            var idOnly = new IdOnlyEvaluator();
            var u16rng = new UIntRangeEvaluator();
            var btrng  = new DualByteRangeEvaluator();

            var list = new List<MessageDefinition>();

            list.Add(new MessageDefinition("PingA", 0x23, "AckA", 0x24, idOnly));
            list.Add(new MessageDefinition("PingB", 0x76, "AckB", 0x77, idOnly));
            list.Add(new MessageDefinition("PingC", 0xBB, "AckC", 0xBC, idOnly));

            list.Add(new MessageDefinition("Meas1", 0x33, "Meas1Ack", 0x34, u16rng, lower: 100, upper: 500));
            list.Add(new MessageDefinition("Meas2", 0x86, "Meas2Ack", 0x87, u16rng, lower: 0, upper: 1000));
            list.Add(new MessageDefinition("Meas3", 0x1B, "Meas3Ack", 0x1C, u16rng, lower: 50, upper: 60));

            list.Add(new MessageDefinition("PackedA-Lo", 0x41, "PackedA", 0xCB, btrng, lower: 10, upper: 20, byteIndex: 0));
            list.Add(new MessageDefinition("PackedA-Hi", 0x41, "PackedA", 0xCB, btrng, lower: 200, upper: 220, byteIndex: 1));

            list.Add(new MessageDefinition("PackedB-Lo", 0xBA, "PackedB", 0xFA, btrng, lower: 5, upper: 15, byteIndex: 0));
            list.Add(new MessageDefinition("PackedB-Hi", 0xBA, "PackedB", 0xFA, btrng, lower: 150, upper: 160, byteIndex: 1));

            return list;
        }
    }
}
