using System.Collections.Generic;
using KidQuiz.Domain;
using NUnit.Framework;

namespace KidQuiz.Domain.Tests
{
    public class QuizSessionTests
    {
        private const float SecondsPerQuestion = 20f;

        private static List<Question> CreateQuestions(int count)
        {
            var randomizer = new FakeRandomizer();
            var questions = new List<Question>(count);
            for (int i = 0; i < count; i++)
            {
                questions.Add(new Question(
                    $"q{i}",
                    $"Question {i}",
                    Difficulty.Easy,
                    "Correct",
                    new[] { "Wrong1", "Wrong2", "Wrong3" },
                    randomizer));
            }
            return questions;
        }

        [Test]
        public void FullTenQuestionRun_AllCorrectAtFullTime_ProducesExpectedFinalScore()
        {
            var questions = CreateQuestions(10);
            var session = new QuizSession(questions, SecondsPerQuestion);

            while (!session.IsComplete)
            {
                session.Submit("Correct", SecondsPerQuestion);
                session.Advance();
            }

            int perQuestionScore = ScoringRules.CalculatePoints(true, Difficulty.Easy, SecondsPerQuestion, SecondsPerQuestion);
            Assert.AreEqual(perQuestionScore * 10, session.Score);
            Assert.AreEqual(10, session.QuestionsAnswered);
            Assert.IsTrue(session.IsComplete);
        }

        [Test]
        public void Submit_CalledTwiceOnSameQuestion_AwardsPointsOnce()
        {
            var questions = CreateQuestions(1);
            var session = new QuizSession(questions, SecondsPerQuestion);

            var first = session.Submit("Correct", SecondsPerQuestion);
            var second = session.Submit("Correct", SecondsPerQuestion);

            int expectedPoints = ScoringRules.CalculatePoints(true, Difficulty.Easy, SecondsPerQuestion, SecondsPerQuestion);
            Assert.AreEqual(expectedPoints, first.PointsAwarded);
            Assert.AreEqual(0, second.PointsAwarded);
            Assert.AreEqual(expectedPoints, session.Score);
            Assert.AreEqual(1, session.QuestionsAnswered);
        }
    }
}
