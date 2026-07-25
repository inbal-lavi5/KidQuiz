using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KidQuiz.Domain;

namespace KidQuiz.Data
{
    public sealed class LocalQuestionProvider : IQuestionProvider
    {
        private readonly QuestionBank _bank;
        private readonly IRandomizer _randomizer;

        public LocalQuestionProvider(QuestionBank bank, IRandomizer randomizer)
        {
            _bank = bank;
            _randomizer = randomizer;
        }

        public Task<IReadOnlyList<Question>> FetchAsync(QuizRoundSettings settings, CancellationToken ct)
        {
            List<QuestionBank.Entry> pool = _bank.Entries
                .Where(entry => entry.categoryId == settings.CategoryId)
                .ToList();

            if (pool.Count == 0)
            {
                pool = _bank.Entries.ToList();
            }

            _randomizer.Shuffle(pool);

            int count = Math.Min(settings.QuestionCount, pool.Count);
            var questions = new List<Question>(count);

            for (int i = 0; i < count; i++)
            {
                QuestionBank.Entry entry = pool[i];
                questions.Add(new Question(
                    entry.prompt,
                    entry.correctAnswer,
                    entry.incorrectAnswers,
                    _randomizer));
            }

            return Task.FromResult<IReadOnlyList<Question>>(questions);
        }
    }
}
