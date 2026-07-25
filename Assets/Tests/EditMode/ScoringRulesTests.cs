using KidQuiz.Domain;
using NUnit.Framework;

namespace KidQuiz.Domain.Tests
{
    public class ScoringRulesTests
    {
        [Test]
        public void CalculatePoints_IncorrectAnswer_ReturnsZero()
        {
            int points = ScoringRules.CalculatePoints(false, 20f, 20f);
            Assert.AreEqual(0, points);
        }

        [Test]
        public void CalculatePoints_ZeroTimeLeft_AwardsBasePointsOnly()
        {
            int points = ScoringRules.CalculatePoints(true, 0f, 20f);
            Assert.AreEqual(ScoringRules.BasePoints, points);
        }

        [Test]
        public void CalculatePoints_FullTimeLeft_AwardsBasePointsPlusMaxBonus()
        {
            int points = ScoringRules.CalculatePoints(true, 20f, 20f);
            int expected = (int)System.Math.Round(ScoringRules.BasePoints + ScoringRules.MaxTimeBonus);
            Assert.AreEqual(expected, points);
        }
    }
}
