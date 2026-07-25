using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KidQuiz.Domain
{
    // Boundary contract: implemented in the Data layer, consumed by Presentation.
    // GetTopAsync returns null on failure and an empty (possibly zero-length) list on success,
    // so callers can tell "unavailable" apart from "no scores yet". Scores are scoped per
    // category - each category keeps its own top list.
    public interface IScoreRepository
    {
        Task<bool> SubmitAsync(ScoreEntry entry, CancellationToken ct);
        Task<IReadOnlyList<ScoreEntry>> GetTopAsync(string category, int count, CancellationToken ct);
    }
}
