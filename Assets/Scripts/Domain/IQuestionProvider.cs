using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KidQuiz.Domain
{
    // Boundary contract: implemented in the Data layer, consumed by Presentation.
    public interface IQuestionProvider
    {
        Task<IReadOnlyList<Question>> FetchAsync(QuizRoundSettings settings, CancellationToken ct);
    }
}
