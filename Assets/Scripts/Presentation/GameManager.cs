using KidQuiz.Config;
using KidQuiz.Data;
using KidQuiz.Domain;
using UnityEngine;

namespace KidQuiz.Presentation
{
    // Composition root: builds the providers and hands them to screens.
    // Screens never reach for GameManager.Instance themselves.
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private ScreenManager screenManager;
        [SerializeField] private QuizConfig easyConfig;
        [SerializeField] private QuizConfig mediumConfig;
        [SerializeField] private QuizConfig hardConfig;
        [SerializeField] private QuestionBank offlineQuestionBank;

        private IQuestionProvider _questionProvider;
        private string _playerName;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            var apiClient = new ApiClient();
            var randomizer = new SystemRandomizer();
            var triviaProvider = new TriviaApiProvider(apiClient, randomizer);
            var localProvider = new LocalQuestionProvider(offlineQuestionBank, randomizer);
            _questionProvider = new FallbackQuestionProvider(triviaProvider, localProvider);
        }

        private void Start()
        {
            screenManager.Home.Initialize(HandleStartRequested);
            screenManager.Quiz.Initialize(_questionProvider);
            screenManager.Results.Initialize(HandlePlayAgain);
            screenManager.ShowHome();
        }

        private void HandleStartRequested(string playerName, Difficulty difficulty)
        {
            _playerName = playerName;
            QuizConfig config = SelectConfig(difficulty);

            screenManager.ShowQuiz();
            screenManager.Quiz.BeginRound(config, HandleRoundComplete);
        }

        private void HandleRoundComplete(QuizResult result)
        {
            screenManager.ShowResults();
            screenManager.Results.ShowResult(result);
        }

        private void HandlePlayAgain()
        {
            screenManager.ShowHome();
        }

        private QuizConfig SelectConfig(Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.Medium => mediumConfig,
                Difficulty.Hard => hardConfig,
                _ => easyConfig
            };
        }
    }
}
