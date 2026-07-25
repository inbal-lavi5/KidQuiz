using System;
using System.Threading;
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

        private const int LeaderboardSize = 10;

        private IQuestionProvider _questionProvider;
        private IScoreRepository _scoreRepository;
        private string _playerName;
        private QuizConfig _lastConfig;

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

            // Not committed - see Assets/Resources/FirebaseConfig.example.asset and the README.
            var firebaseConfig = Resources.Load<FirebaseConfig>("FirebaseConfig");
            _scoreRepository = firebaseConfig != null && !string.IsNullOrWhiteSpace(firebaseConfig.DatabaseUrl)
                ? new FirebaseScoreRepository(apiClient, firebaseConfig.DatabaseUrl)
                : null;
        }

        private void Start()
        {
            screenManager.Home.Initialize(HandleStartRequested);
            screenManager.Quiz.Initialize(_questionProvider, HandleQuizExit);
            screenManager.Results.Initialize(HandlePlayAgain, HandleGoHome);
            screenManager.ShowHome();
        }

        private void HandleStartRequested(string playerName, Difficulty difficulty)
        {
            _playerName = playerName;
            _lastConfig = SelectConfig(difficulty);
            BeginRound(_lastConfig);
        }

        private void BeginRound(QuizConfig config)
        {
            screenManager.ShowQuiz();
            screenManager.Quiz.BeginRound(config, HandleRoundComplete);
        }

        private void HandleQuizExit()
        {
            screenManager.ShowHome();
        }

        private async void HandleRoundComplete(QuizResult result)
        {
            screenManager.ShowResults();
            screenManager.Results.ShowResult(result, _playerName);

            if (_scoreRepository == null)
            {
                screenManager.Results.ShowLeaderboardUnavailable();
                return;
            }

            var entry = new ScoreEntry(_playerName, result.Score, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            using var cts = new CancellationTokenSource();

            await _scoreRepository.SubmitAsync(entry, cts.Token);
            var topScores = await _scoreRepository.GetTopAsync(LeaderboardSize, cts.Token);

            screenManager.Results.ShowLeaderboardLoaded(topScores);
        }

        private void HandlePlayAgain()
        {
            BeginRound(_lastConfig);
        }

        private void HandleGoHome()
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
