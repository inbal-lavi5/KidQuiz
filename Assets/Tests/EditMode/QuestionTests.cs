using KidQuiz.Domain;
using NUnit.Framework;

namespace KidQuiz.Domain.Tests
{
    public class QuestionTests
    {
        private static Question CreateQuestion(string correctAnswer)
        {
            return new Question(
                "q1",
                "What color is the sky?",
                correctAnswer,
                new[] { "Red", "Green", "Yellow" },
                new FakeRandomizer());
        }

        [Test]
        public void IsCorrect_ExactMatch_ReturnsTrue()
        {
            var question = CreateQuestion("Blue");
            Assert.IsTrue(question.IsCorrect("Blue"));
        }

        [TestCase("blue")]
        [TestCase("BLUE")]
        [TestCase("BlUe")]
        public void IsCorrect_IgnoresCasing(string answer)
        {
            var question = CreateQuestion("Blue");
            Assert.IsTrue(question.IsCorrect(answer));
        }

        [TestCase("  Blue")]
        [TestCase("Blue  ")]
        [TestCase("  Blue  ")]
        public void IsCorrect_TrimsWhitespace(string answer)
        {
            var question = CreateQuestion("Blue");
            Assert.IsTrue(question.IsCorrect(answer));
        }

        [Test]
        public void IsCorrect_WrongAnswer_ReturnsFalse()
        {
            var question = CreateQuestion("Blue");
            Assert.IsFalse(question.IsCorrect("Red"));
        }

        [Test]
        public void IsCorrect_NullAnswer_ReturnsFalse()
        {
            var question = CreateQuestion("Blue");
            Assert.IsFalse(question.IsCorrect(null));
        }
    }
}
