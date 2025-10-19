using HaberlesmeMimarisi.Core.Utils;
using HaberlesmeMimarisi.Domain.Messages;


namespace HaberlesmeMimarisi.Domain.Evaluation
{
    // YGÖ8
    public sealed class IdOnlyEvaluator : IEvaluator
    {
        public EvaluationResult Evaluate(MessageDefinition def, TxMessage tx, RxMessage rx)
        {
            var ok = (rx.RxMessageId == def.ExpectedRxId);
            return new EvaluationResult(ok ? PassFail.Gecti : PassFail.Kaldi, $"RX={Bytes.Hex(rx.RxMessageId)}");
        }
    }

    // YGÖ9
    public sealed class UIntRangeEvaluator : IEvaluator
    {
        public EvaluationResult Evaluate(MessageDefinition def, TxMessage tx, RxMessage rx)
        {
            if (rx.RxMessageId != def.ExpectedRxId)
                return new EvaluationResult(PassFail.Kaldi, $"Yanlis RxID: {Bytes.Hex(rx.RxMessageId)}", def.LowerLimit?.ToString(), def.UpperLimit?.ToString());

            uint val = (uint)rx.RxData;
            if (def.LowerLimit.HasValue && def.UpperLimit.HasValue)
            {
                bool pass = def.LowerLimit.Value <= val && val <= def.UpperLimit.Value;
                return new EvaluationResult(pass ? PassFail.Gecti : PassFail.Kaldi,
                    val.ToString(), def.LowerLimit.Value.ToString(), def.UpperLimit.Value.ToString());
            }
            return EvaluationResult.Unknown(val.ToString());
        }
    }

    // YGÖ10
    public sealed class DualByteRangeEvaluator : IEvaluator
    {
        public EvaluationResult Evaluate(MessageDefinition def, TxMessage tx, RxMessage rx)
        {
            if (rx.RxMessageId != def.ExpectedRxId)
                return new EvaluationResult(PassFail.Kaldi, $"Yanlis RxID: {Bytes.Hex(rx.RxMessageId)}", def.LowerLimit?.ToString(), def.UpperLimit?.ToString());

            byte low = (byte)(rx.RxData & 0xFF);
            byte high = (byte)(rx.RxData >> 8);
            byte value = (def.ByteIndex == 0) ? low : high;

            if (def.LowerLimit.HasValue && def.UpperLimit.HasValue)
            {
                bool pass = def.LowerLimit.Value <= value && value <= def.UpperLimit.Value;
                return new EvaluationResult(pass ? PassFail.Gecti : PassFail.Kaldi,
                    value.ToString(), def.LowerLimit.Value.ToString(), def.UpperLimit.Value.ToString());
            }
            return EvaluationResult.Unknown(value.ToString());
        }
    }
}
