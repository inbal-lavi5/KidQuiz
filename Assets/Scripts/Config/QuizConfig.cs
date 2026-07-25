using KidQuiz.Domain;
using UnityEngine;

namespace KidQuiz.Config
{
    [CreateAssetMenu(menuName = "KidQuiz/Quiz Config", fileName = "QuizConfig")]
    public sealed class QuizConfig : ScriptableObject
    {
        [SerializeField] private int questionsPerRound = 10;
        [SerializeField] private float secondsPerQuestion = 20f;
        [SerializeField] private int basePoints = 100;
        [SerializeField] private float timeBonusMultiplier = 1f;
        [SerializeField] private Difficulty difficulty = Difficulty.Easy;
        [SerializeField] private int triviaCategoryId = 9;

        public int QuestionsPerRound => questionsPerRound;
        public float SecondsPerQuestion => secondsPerQuestion;
        public int BasePoints => basePoints;
        public float TimeBonusMultiplier => timeBonusMultiplier;
        public Difficulty Difficulty => difficulty;
        public int TriviaCategoryId => triviaCategoryId;

        public QuizRoundSettings ToRoundSettings()
        {
            return new QuizRoundSettings(questionsPerRound, difficulty, triviaCategoryId);
        }
    }
}
