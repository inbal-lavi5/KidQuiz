using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KidQuiz.Domain;

namespace KidQuiz.Data
{
    // Firebase Realtime Database over REST.
    // Write: PUT {dbUrl}/scores/{guid}.json
    // Read:  GET {dbUrl}/scores.json?orderBy="score"&limitToLast={count}, sorted descending client-side.
    public sealed class FirebaseScoreRepository : IScoreRepository
    {
        private readonly ApiClient _apiClient;
        private readonly string _databaseUrl;

        public FirebaseScoreRepository(ApiClient apiClient, string databaseUrl)
        {
            _apiClient = apiClient;
            _databaseUrl = databaseUrl.TrimEnd('/');
        }

        public async Task<bool> SubmitAsync(ScoreEntry entry, CancellationToken ct)
        {
            string url = $"{_databaseUrl}/scores/{Guid.NewGuid()}.json";
            var body = new ScoreEntryDto
            {
                playerName = entry.PlayerName,
                score = entry.Score,
                timestamp = entry.TimestampUnixSeconds
            };

            return await _apiClient.PutAsync(url, body, ct);
        }

        public async Task<IReadOnlyList<ScoreEntry>> GetTopAsync(int count, CancellationToken ct)
        {
            string url = $"{_databaseUrl}/scores.json?orderBy=%22score%22&limitToLast={count}";
            ApiResult<Dictionary<string, ScoreEntryDto>> result =
                await _apiClient.GetAsync<Dictionary<string, ScoreEntryDto>>(url, ct);

            if (!result.IsSuccess)
            {
                return null;
            }

            if (result.Value == null)
            {
                return Array.Empty<ScoreEntry>();
            }

            return result.Value.Values
                .Select(dto => new ScoreEntry(dto.playerName, dto.score, dto.timestamp))
                .OrderByDescending(e => e.Score)
                .ToList();
        }
    }
}
