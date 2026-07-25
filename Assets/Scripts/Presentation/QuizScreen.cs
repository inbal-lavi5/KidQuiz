using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using KidQuiz.Config;
using KidQuiz.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KidQuiz.Presentation
{
    public sealed class QuizScreen : UiScreen
    {
        [SerializeField] private TMP_Text questionText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text questionCounterText;
        [SerializeField] private TMP_Text categoryText;
        [SerializeField] private RectTransform timerFillRect;
        [SerializeField] private Transform answerButtonParent;
        [SerializeField] private AnswerButton answerButtonPrefab;
        [SerializeField] private Button backButton;
        [SerializeField] private GameObject questionCard;
        [SerializeField] private GameObject answerButtonContainer;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private float feedbackDelaySeconds = 1.2f;

        private ObjectPool<AnswerButton> _buttonPool;
        private readonly List<AnswerButton> _activeButtons = new();

        private IQuestionProvider _questionProvider;
        private AudioManager _audioManager;
        private QuizConfig _config;
        private string _topicLabel;
        private Action<QuizResult> _onComplete;
        private Action _onExit;

        private QuizSession _session;
        private CancellationTokenSource _fetchCts;
        private Coroutine _activeRoutine;
        private bool _acceptingAnswers;
        private float _questionStartTime;
        private int _correctCount;
        private int _questionNumber;
        private int _totalQuestions;

        private void Awake()
        {
            _buttonPool = new ObjectPool<AnswerButton>(answerButtonPrefab, answerButtonParent, prewarmCount: 4);
        }

        public void Initialize(IQuestionProvider questionProvider, Action onExit, AudioManager audioManager)
        {
            _questionProvider = questionProvider;
            _onExit = onExit;
            _audioManager = audioManager;
        }

        private void OnEnable()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(HandleBackClicked);
            }
        }

        private void OnDisable()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(HandleBackClicked);
            }
        }

        private void HandleBackClicked()
        {
            _onExit?.Invoke();
        }

        public override void Hide()
        {
            base.Hide();
            _fetchCts?.Cancel();
            StopActiveRoutine();
        }

        public async void BeginRound(QuizConfig config, string topicLabel, Action<QuizResult> onComplete)
        {
            _config = config;
            _topicLabel = topicLabel;
            _onComplete = onComplete;
            _correctCount = 0;
            _questionNumber = 0;
            scoreText.text = "0";
            ClearAnswerButtons();
            SetLoading(true);

            _fetchCts?.Cancel();
            _fetchCts = new CancellationTokenSource();
            CancellationToken ct = _fetchCts.Token;

            IReadOnlyList<Question> questions = await _questionProvider.FetchAsync(config.ToRoundSettings(), ct);

            if (ct.IsCancellationRequested)
            {
                return;
            }

            if (questions == null || questions.Count == 0)
            {
                SetLoading(false);
                questionText.text = "No questions available. Please try again.";
                return;
            }

            _totalQuestions = questions.Count;
            _session = new QuizSession(questions, config.SecondsPerQuestion);
            SetLoading(false);
            ShowCurrentQuestion();
        }

        private void SetLoading(bool isLoading)
        {
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(isLoading);
            }
            if (questionCard != null)
            {
                questionCard.SetActive(!isLoading);
            }
            if (answerButtonContainer != null)
            {
                answerButtonContainer.SetActive(!isLoading);
            }
            if (isLoading && timerFillRect != null)
            {
                timerFillRect.anchorMax = new Vector2(0f, timerFillRect.anchorMax.y);
            }
        }

        private void ShowCurrentQuestion()
        {
            ClearAnswerButtons();

            Question question = _session.CurrentQuestion;
            questionText.text = question.Prompt;
            _questionNumber++;

            if (questionCounterText != null)
            {
                questionCounterText.text = $"Question {_questionNumber}/{_totalQuestions}";
            }
            if (categoryText != null)
            {
                categoryText.text = _topicLabel.ToUpperInvariant();
            }

            int slotIndex = 0;
            foreach (string option in question.Options)
            {
                AnswerButton button = _buttonPool.Get();
                button.Bind(option, HandleAnswerSelected, slotIndex);
                _activeButtons.Add(button);
                slotIndex++;
            }

            _acceptingAnswers = true;
            _questionStartTime = Time.time;

            StopActiveRoutine();
            _activeRoutine = StartCoroutine(RunTimer());
        }

        private IEnumerator RunTimer()
        {
            float duration = _config.SecondsPerQuestion;
            float elapsed = 0f;

            while (elapsed < duration && _acceptingAnswers)
            {
                elapsed += Time.deltaTime;
                if (timerFillRect != null)
                {
                    float ratio = 1f - Mathf.Clamp01(elapsed / duration);
                    timerFillRect.anchorMax = new Vector2(ratio, timerFillRect.anchorMax.y);
                }
                yield return null;
            }

            if (_acceptingAnswers)
            {
                HandleAnswerSelected(null);
            }
        }

        private void HandleAnswerSelected(string answer)
        {
            if (!_acceptingAnswers)
            {
                return;
            }

            _acceptingAnswers = false;

            float secondsRemaining = Mathf.Max(0f, _config.SecondsPerQuestion - (Time.time - _questionStartTime));
            AnswerResult result = _session.Submit(answer, secondsRemaining);

            if (result.WasCorrect)
            {
                _correctCount++;
                _audioManager?.PlayCorrect();
            }
            else
            {
                _audioManager?.PlayWrong();
            }

            scoreText.text = _session.Score.ToString();
            ShowAnswerFeedback(answer, result);

            StopActiveRoutine();
            _activeRoutine = StartCoroutine(AdvanceAfterDelay());
        }

        private void ShowAnswerFeedback(string selectedAnswer, AnswerResult result)
        {
            foreach (AnswerButton button in _activeButtons)
            {
                if (button.AnswerText == result.CorrectAnswer)
                {
                    button.SetState(AnswerButtonState.Correct);
                }
                else if (button.AnswerText == selectedAnswer)
                {
                    button.SetState(AnswerButtonState.Incorrect);
                }
            }
        }

        private IEnumerator AdvanceAfterDelay()
        {
            yield return new WaitForSeconds(feedbackDelaySeconds);

            _session.Advance();

            if (_session.IsComplete)
            {
                CompleteRound();
            }
            else
            {
                ShowCurrentQuestion();
            }
        }

        private void CompleteRound()
        {
            ClearAnswerButtons();
            var result = new QuizResult(_session.Score, _correctCount, _config.QuestionsPerRound);
            _onComplete?.Invoke(result);
        }

        private void ClearAnswerButtons()
        {
            foreach (AnswerButton button in _activeButtons)
            {
                _buttonPool.Release(button);
            }
            _activeButtons.Clear();
        }

        private void StopActiveRoutine()
        {
            if (_activeRoutine != null)
            {
                StopCoroutine(_activeRoutine);
                _activeRoutine = null;
            }
        }
    }
}
