using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KidQuiz.Domain;

namespace KidQuiz.Data
{
    // Firebase Realtime Database over REST. Each category keeps its own top-10 list:
    // Write: PUT {dbUrl}/scores/{category}/{guid}.json
    // Read:  GET {dbUrl}/scores/{category}.json?orderBy="score"&limitToLast={count}
    // On submit, the existing entries for that category are re-read, combined with the
    // new one, and anything outside the top 10 is deleted - so a category never grows
    // past MaxEntriesPerCategory stored entries.
    public sealed class FirebaseScoreRepository : IScoreRepository
    {
        private const int MaxEntriesPerCategory = 10;

        private readonly ApiClient _apiClient;
        private readonly string _databaseUrl;

        public FirebaseScoreRepository(ApiClient apiClient, string databaseUrl)
        {
            _apiClient = apiClient;
            _databaseUrl = databaseUrl.TrimEnd('/');
        }

        public async Task<bool> SubmitAsync(ScoreEntry entry, CancellationToken ct)
        {
            string categoryUrl = CategoryUrl(entry.Category);

            ApiResult<Dictionary<string, ScoreEntryDto>> existingResult =
                await _apiClient.GetAsync<Dictionary<string, ScoreEntryDto>>($"{categoryUrl}.json", ct);

            List<KeyValuePair<string, ScoreEntryDto>> existing = existingResult.IsSuccess && existingResult.Value != null
                ? existingResult.Value.ToList()
                : new List<KeyValuePair<string, ScoreEntryDto>>();

            string newId = Guid.NewGuid().ToString();

            var ranked = existing
                .Select(kv => (id: kv.Key, score: kv.Value.score))
                .Append((id: newId, score: entry.Score))
                .OrderByDescending(x => x.score)
                .ToList();

            var keepIds = new HashSet<string>(ranked.Take(MaxEntriesPerCategory).Select(x => x.id));
            bool newEntryKept = keepIds.Contains(newId);

            if (newEntryKept)
            {
                var body = new ScoreEntryDto
                {
                    playerName = entry.PlayerName,
                    score = entry.Score,
                    timestamp = entry.TimestampUnixSeconds
                };

                await _apiClient.PutAsync($"{categoryUrl}/{newId}.json", body, ct);
            }

            foreach (KeyValuePair<string, ScoreEntryDto> kv in existing)
            {
                if (!keepIds.Contains(kv.Key))
                {
                    await _apiClient.DeleteAsync($"{categoryUrl}/{kv.Key}.json", ct);
                }
            }

            return newEntryKept;
        }

        public async Task<IReadOnlyList<ScoreEntry>> GetTopAsync(string category, int count, CancellationToken ct)
        {
            string url = $"{CategoryUrl(category)}.json?orderBy=%22score%22&limitToLast={count}";
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
                .Select(dto => new ScoreEntry(dto.playerName, dto.score, dto.timestamp, category))
                .OrderByDescending(e => e.Score)
                .ToList();
        }

        private string CategoryUrl(string category)
        {
            return $"{_databaseUrl}/scores/{Uri.EscapeDataString(category)}";
        }
    }
}
