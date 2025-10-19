using HaberlesmeMimarisi.Domain.Messages;

namespace HaberlesmeMimarisi.Domain.Evaluation
{
    public sealed class MessageDefinition : IEvaluationContext
    {
        public string TxName { get; }
        public byte TxId { get; }
        public string RxName { get; }
        public byte ExpectedRxId { get; }
        public int? ByteIndex { get; }
        public uint? LowerLimit { get; }
        public uint? UpperLimit { get; }
        public IEvaluator Evaluator { get; }

        public MessageDefinition(string txName, byte txId, string rxName, byte expectedRxId, IEvaluator evaluator, uint? lower = null, uint? upper = null, int? byteIndex = null)
        {
            TxName = txName;
            TxId = txId;
            RxName = rxName;
            ExpectedRxId = expectedRxId;
            Evaluator = evaluator;
            LowerLimit = lower;
            UpperLimit = upper;
            ByteIndex = byteIndex;
        }
    }
}
