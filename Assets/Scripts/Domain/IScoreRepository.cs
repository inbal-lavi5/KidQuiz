using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KidQuiz.Domain
{
    // Boundary contract: implemented in the Data layer, consumed by Presentation.
    // GetTopAsync returns null on failure and an empty (possibly zero-length) list on success,
    // so callers can tell "unavailable" apart from "no scores yet".
    public interface IScoreRepository
    {
        Task<bool> SubmitAsync(ScoreEntry entry, CancellationToken ct);
        Task<IReadOnlyList<ScoreEntry>> GetTopAsync(int count, CancellationToken ct);
    }
}
