using UnityEngine;

namespace KidQuiz.Presentation
{
    public sealed class ScreenManager : MonoBehaviour
    {
        [SerializeField] private HomeScreen homeScreen;
        [SerializeField] private QuizScreen quizScreen;
        [SerializeField] private ResultsScreen resultsScreen;

        public HomeScreen Home => homeScreen;
        public QuizScreen Quiz => quizScreen;
        public ResultsScreen Results => resultsScreen;

        public void ShowHome() => ShowOnly(homeScreen);
        public void ShowQuiz() => ShowOnly(quizScreen);
        public void ShowResults() => ShowOnly(resultsScreen);

        private void ShowOnly(UiScreen screen)
        {
            homeScreen.Hide();
            quizScreen.Hide();
            resultsScreen.Hide();
            screen.Show();
        }
    }
}
