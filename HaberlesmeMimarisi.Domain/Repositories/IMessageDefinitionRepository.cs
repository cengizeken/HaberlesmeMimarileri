using System.Collections.Generic;

namespace HaberlesmeMimarisi.Domain.Repositories
{
    using HaberlesmeMimarisi.Domain.Evaluation;

    public interface IMessageDefinitionRepository
    {
        IReadOnlyList<MessageDefinition> GetAll();
    }
}
