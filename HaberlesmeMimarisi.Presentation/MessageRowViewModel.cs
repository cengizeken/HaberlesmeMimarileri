using HaberlesmeMimarisi.Core.Utils;
using HaberlesmeMimarisi.Domain.Evaluation;
using HaberlesmeMimarisi.Domain.Messages;

namespace HaberlesmeMimarisi.Presentation
{
    public sealed class MessageRowViewModel
    {
        public string TxMessageName { get; set; }
        public string TxMessageId { get; set; }
        public string TxMessageHex { get; set; }

        public string RxMessageName { get; set; }
        public string RxMessageId { get; set; }
        public string RxMessageHex { get; set; }

        public string RxData { get; set; }
        public string AltLimit { get; set; }
        public string AnlamliRxData { get; set; }
        public string UstLimit { get; set; }
        public string GectiKaldi { get; set; }

        public HaberlesmeMimarisi.Domain.Evaluation.MessageDefinition Definition { get; set; }
        public ushort PendingTxData { get; set; } = 0x5555;

        public void ApplyEvaluation(TxMessage tx, RxMessage rx, EvaluationResult eval)
        {
            TxMessageHex = tx.ToString();
            RxMessageHex = rx.ToString();
            TxMessageId  = Bytes.Hex(tx.TxMessageId);
            RxMessageId  = Bytes.Hex(rx.RxMessageId);
            RxData       = Bytes.Hex(rx.RxData);

            AltLimit = eval.LowerLimitText;
            UstLimit = eval.UpperLimitText;
            AnlamliRxData = eval.MeaningfulText;
            GectiKaldi = eval.Result == PassFail.Gecti ? "Geçti" :
                         eval.Result == PassFail.Kaldi  ? "Kaldı" : "";
        }
    }
}
