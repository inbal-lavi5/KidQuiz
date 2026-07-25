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
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private QuizConfig scienceConfig;
        [SerializeField] private QuizConfig generalKnowledgeConfig;
        [SerializeField] private QuizConfig mathConfig;
        [SerializeField] private QuestionBank offlineQuestionBank;

        private const int LeaderboardSize = 10;

        private IQuestionProvider _questionProvider;
        private IScoreRepository _scoreRepository;
        private string _playerName;
        private QuizConfig _lastConfig;
        private Topic _lastTopic;

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
            screenManager.Home.Initialize(HandleStartRequested, HandleViewLeaderboard, HandleToggleSound, audioManager);
            screenManager.Home.SetMuted(audioManager.IsMuted);
            screenManager.Quiz.Initialize(_questionProvider, HandleQuizExit, audioManager);
            screenManager.Results.Initialize(HandlePlayAgain, HandleGoHome);
            screenManager.Leaderboard.Initialize(HandleGoHome, HandleLeaderboardTopicSelected);
            screenManager.ShowHome();
            audioManager.PlayMusic();
        }

        private void HandleStartRequested(string playerName, Topic topic)
        {
            _playerName = playerName;
            _lastTopic = topic;
            _lastConfig = SelectConfig(topic);
            BeginRound(_lastConfig, _lastTopic);
        }

        private void BeginRound(QuizConfig config, Topic topic)
        {
            screenManager.ShowQuiz();
            screenManager.Quiz.BeginRound(config, TopicLabel(topic), HandleRoundComplete);
        }

        private void HandleQuizExit()
        {
            screenManager.ShowHome();
        }

        private async void HandleRoundComplete(QuizResult result)
        {
            string category = TopicLabel(_lastTopic);
            screenManager.ShowResults();
            screenManager.Results.ShowResult(result, _playerName, category);

            if (_scoreRepository == null)
            {
                screenManager.Results.ShowLeaderboardUnavailable();
                return;
            }

            var entry = new ScoreEntry(_playerName, result.Score, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), category);
            using var cts = new CancellationTokenSource();

            await _scoreRepository.SubmitAsync(entry, cts.Token);
            var topScores = await _scoreRepository.GetTopAsync(category, LeaderboardSize, cts.Token);

            screenManager.Results.ShowLeaderboardLoaded(topScores);
        }

        private void HandlePlayAgain()
        {
            BeginRound(_lastConfig, _lastTopic);
        }

        private void HandleGoHome()
        {
            screenManager.ShowHome();
        }

        private void HandleViewLeaderboard()
        {
            screenManager.ShowLeaderboard();
            screenManager.Leaderboard.SelectTopic(_lastTopic);
        }

        private async void HandleLeaderboardTopicSelected(Topic topic)
        {
            screenManager.Leaderboard.ShowLoading();

            if (_scoreRepository == null)
            {
                screenManager.Leaderboard.ShowUnavailable();
                return;
            }

            using var cts = new CancellationTokenSource();
            var topScores = await _scoreRepository.GetTopAsync(TopicLabel(topic), LeaderboardSize, cts.Token);
            screenManager.Leaderboard.ShowLoaded(topScores);
        }

        private void HandleToggleSound()
        {
            audioManager.ToggleMuted();
            screenManager.Home.SetMuted(audioManager.IsMuted);
        }

        private QuizConfig SelectConfig(Topic topic)
        {
            return topic switch
            {
                Topic.GeneralKnowledge => generalKnowledgeConfig,
                Topic.Math => mathConfig,
                _ => scienceConfig
            };
        }

        private static string TopicLabel(Topic topic)
        {
            return topic switch
            {
                Topic.GeneralKnowledge => "General Knowledge",
                Topic.Math => "Math",
                _ => "Science"
            };
        }
    }
}
