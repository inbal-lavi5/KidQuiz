using UnityEngine;

namespace KidQuiz.Presentation
{
    public sealed class ScreenManager : MonoBehaviour
    {
        [SerializeField] private HomeScreen homeScreen;
        [SerializeField] private QuizScreen quizScreen;
        [SerializeField] private ResultsScreen resultsScreen;
        [SerializeField] private LeaderboardScreen leaderboardScreen;

        public HomeScreen Home => homeScreen;
        public QuizScreen Quiz => quizScreen;
        public ResultsScreen Results => resultsScreen;
        public LeaderboardScreen Leaderboard => leaderboardScreen;

        public void ShowHome() => ShowOnly(homeScreen);
        public void ShowQuiz() => ShowOnly(quizScreen);
        public void ShowResults() => ShowOnly(resultsScreen);
        public void ShowLeaderboard() => ShowOnly(leaderboardScreen);

        private void ShowOnly(UiScreen screen)
        {
            homeScreen.Hide();
            quizScreen.Hide();
            resultsScreen.Hide();
            leaderboardScreen.Hide();
            screen.Show();
        }
    }
}
