using HaberlesmeMimarisi.Domain.Evaluation;
using HaberlesmeMimarisi.Domain.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaberlesmeMimarisi.Repository.Json
{
    /// <summary>
    /// JSON dosyasından MessageDefinition listesi yükler.
    /// </summary>
    public sealed class FileBackedMessageDefinitionRepository : IMessageDefinitionRepository
    {
        private readonly string _jsonPath;

        public FileBackedMessageDefinitionRepository(string jsonPath)
        {
            if (string.IsNullOrWhiteSpace(jsonPath))
                throw new ArgumentException("jsonPath is null/empty", nameof(jsonPath));

            _jsonPath = jsonPath;
        }
        
        public IReadOnlyList<MessageDefinition> GetAll()
        {
            if (!File.Exists(_jsonPath))
                throw new FileNotFoundException("Message definition JSON not found", _jsonPath);

            var json = File.ReadAllText(_jsonPath);
            var items = JsonConvert.DeserializeObject<List<MessageDefinitionDto>>(json)
                        ?? new List<MessageDefinitionDto>();

            var list = new List<MessageDefinition>(items.Count);
            foreach (var dto in items)
            {
                Validate(dto);

                byte txId = ParseByte(dto.TxId);
                byte rxId = ParseByte(dto.ExpectedRxId);

                IEvaluator evaluator = CreateEvaluator(dto.Evaluator);

                uint? lower = dto.Lower;
                uint? upper = dto.Upper;
                int? byteIndex = dto.ByteIndex;

                list.Add(new MessageDefinition(
                    txName: dto.TxName,
                    txId: txId,
                    rxName: dto.RxName,
                    expectedRxId: rxId,
                    evaluator: evaluator,
                    lower: lower,
                    upper: upper,
                    byteIndex: byteIndex
                ));
            }

            return list;
        }

        private static void Validate(MessageDefinitionDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.TxName))
                throw new InvalidDataException("TxName boş olamaz");
            if (string.IsNullOrWhiteSpace(dto.TxId))
                throw new InvalidDataException("TxId boş olamaz");
            if (string.IsNullOrWhiteSpace(dto.RxName))
                throw new InvalidDataException("RxName boş olamaz");
            if (string.IsNullOrWhiteSpace(dto.ExpectedRxId))
                throw new InvalidDataException("ExpectedRxId boş olamaz");
            if (string.IsNullOrWhiteSpace(dto.Evaluator))
                throw new InvalidDataException("Evaluator boş olamaz");

            // Evaluator’a göre alan kontrolü
            var eval = dto.Evaluator.Trim().ToLowerInvariant();
            if (eval == "uintrange")
            {
                if (!dto.Lower.HasValue || !dto.Upper.HasValue)
                    throw new InvalidDataException("UIntRange için Lower/Upper zorunludur.");
            }
            else if (eval == "dualbyterange")
            {
                if (!dto.Lower.HasValue || !dto.Upper.HasValue || !dto.ByteIndex.HasValue)
                    throw new InvalidDataException("DualByteRange için Lower/Upper/ByteIndex zorunludur.");
                if (dto.ByteIndex.Value != 0 && dto.ByteIndex.Value != 1)
                    throw new InvalidDataException("DualByteRange.ByteIndex sadece 0 (low) veya 1 (high) olabilir.");
            }
            else if (eval != "idonly")
            {
                throw new InvalidDataException($"Bilinmeyen Evaluator: {dto.Evaluator}");
            }
        }

        private static IEvaluator CreateEvaluator(string evaluatorName)
        {
            switch (evaluatorName.Trim().ToLowerInvariant())
            {
                case "idonly": return new IdOnlyEvaluator();
                case "uintrange": return new UIntRangeEvaluator();
                case "dualbyterange": return new DualByteRangeEvaluator();
                default:
                    throw new InvalidDataException($"Desteklenmeyen evaluator: {evaluatorName}");
            }
        }

        private static byte ParseByte(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new InvalidDataException("Boş byte değeri");

            s = s.Trim();

            // "0x33" / "33" / "0XCB"
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                var hex = s.Substring(2);
                return Convert.ToByte(hex, 16);
            }
            // decimal
            return Convert.ToByte(s, 10);
        }

        // ---------------- DTO ----------------
        private sealed class MessageDefinitionDto
        {
            public string TxName { get; set; }
            public string TxId { get; set; }              // "0x33" veya "51"
            public string RxName { get; set; }
            public string ExpectedRxId { get; set; }      // "0x34" gibi
            public string Evaluator { get; set; }         // "IdOnly" | "UIntRange" | "DualByteRange"
            public uint? Lower { get; set; }              // UIntRange/DualByteRange için
            public uint? Upper { get; set; }              // UIntRange/DualByteRange için
            public int? ByteIndex { get; set; }           // DualByteRange için: 0(low),1(high)
        }
    }
}
