using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using KidQuiz.Domain;

namespace KidQuiz.Data
{
    // Open Trivia DB. HTML-encodes text, so every field needs decoding.
    // response_code != 0 means the API itself reports a failure.
    public sealed class TriviaApiProvider : IQuestionProvider
    {
        private const string BaseUrl = "https://opentdb.com/api.php";

        private readonly ApiClient _apiClient;
        private readonly IRandomizer _randomizer;

        public TriviaApiProvider(ApiClient apiClient, IRandomizer randomizer)
        {
            _apiClient = apiClient;
            _randomizer = randomizer;
        }

        public async Task<IReadOnlyList<Question>> FetchAsync(QuizRoundSettings settings, CancellationToken ct)
        {
            // Every topic in this kids' game is Easy - see QuizConfig assets under Resources/Configs.
            string url = $"{BaseUrl}?amount={settings.QuestionCount}&category={settings.CategoryId}&difficulty=easy&type=multiple";

            ApiResult<TriviaResponseDto> result = await _apiClient.GetAsync<TriviaResponseDto>(url, ct);

            if (!result.IsSuccess || result.Value?.results == null || result.Value.response_code != 0)
            {
                return Array.Empty<Question>();
            }

            var questions = new List<Question>(result.Value.results.Count);
            foreach (TriviaQuestionDto dto in result.Value.results)
            {
                string prompt = WebUtility.HtmlDecode(dto.question);
                string correctAnswer = WebUtility.HtmlDecode(dto.correct_answer);
                List<string> incorrectAnswers = dto.incorrect_answers.Select(WebUtility.HtmlDecode).ToList();

                questions.Add(new Question(
                    Guid.NewGuid().ToString(),
                    prompt,
                    correctAnswer,
                    incorrectAnswers,
                    _randomizer));
            }

            return questions;
        }
    }
}
