using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KidQuiz.Domain;

namespace KidQuiz.Data
{
    // Decorator: tries the primary source, falls back silently on failure.
    public sealed class FallbackQuestionProvider : IQuestionProvider
    {
        private readonly IQuestionProvider _primary;
        private readonly IQuestionProvider _fallback;

        public FallbackQuestionProvider(IQuestionProvider primary, IQuestionProvider fallback)
        {
            _primary = primary;
            _fallback = fallback;
        }

        public async Task<IReadOnlyList<Question>> FetchAsync(QuizRoundSettings settings, CancellationToken ct)
        {
            IReadOnlyList<Question> questions = await _primary.FetchAsync(settings, ct);

            if (questions == null || questions.Count == 0)
            {
                questions = await _fallback.FetchAsync(settings, ct);
            }

            return questions;
        }
    }
}
