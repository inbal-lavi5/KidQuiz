using System;
using System.Collections.Generic;

namespace KidQuiz.Domain
{
    public sealed class Question
    {
        public string Id { get; }
        public string Prompt { get; }
        public IReadOnlyList<string> Options { get; }
        public string CorrectAnswer => _correctAnswer;

        private readonly string _correctAnswer;

        public Question(
            string id,
            string prompt,
            string correctAnswer,
            IReadOnlyList<string> incorrectAnswers,
            IRandomizer randomizer)
        {
            Id = id;
            Prompt = prompt;
            _correctAnswer = correctAnswer;

            var options = new List<string>(incorrectAnswers.Count + 1) { correctAnswer };
            options.AddRange(incorrectAnswers);
            randomizer.Shuffle(options);
            Options = options;
        }

        public bool IsCorrect(string answer)
        {
            if (answer == null)
            {
                return false;
            }

            return string.Equals(_correctAnswer.Trim(), answer.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
