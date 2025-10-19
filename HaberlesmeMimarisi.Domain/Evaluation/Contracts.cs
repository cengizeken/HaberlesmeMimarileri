namespace HaberlesmeMimarisi.Domain.Evaluation
{
    public enum PassFail { Unknown, Gecti, Kaldi }

    public sealed class EvaluationResult
    {
        public PassFail Result { get; }
        public string MeaningfulText { get; }
        public string LowerLimitText { get; }
        public string UpperLimitText { get; }

        public EvaluationResult(PassFail res, string meaningful, string lower = "", string upper = "")
        {
            Result = res;
            MeaningfulText = meaningful;
            LowerLimitText = lower;
            UpperLimitText = upper;
        }

        public static EvaluationResult Unknown(string text = "") => new EvaluationResult(PassFail.Unknown, text);
    }

    public interface IEvaluator
    {
        EvaluationResult Evaluate(MessageDefinition def, HaberlesmeMimarisi.Domain.Messages.TxMessage tx, HaberlesmeMimarisi.Domain.Messages.RxMessage rx);
    }

    public interface IEvaluationContext
    {
        byte ExpectedRxId { get; }
        int? ByteIndex { get; }
        uint? LowerLimit { get; }
        uint? UpperLimit { get; }
    }
}
