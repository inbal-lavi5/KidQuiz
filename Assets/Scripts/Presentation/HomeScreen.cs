using System;
using KidQuiz.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KidQuiz.Presentation
{
    public sealed class HomeScreen : UiScreen
    {
        [SerializeField] private TMP_InputField playerNameInput;
        [SerializeField] private Button easyButton;
        [SerializeField] private Button mediumButton;
        [SerializeField] private Button hardButton;
        [SerializeField] private GameObject easySelectionRing;
        [SerializeField] private GameObject mediumSelectionRing;
        [SerializeField] private GameObject hardSelectionRing;
        [SerializeField] private Button startButton;

        private Difficulty _selectedDifficulty = Difficulty.Easy;
        private Action<string, Difficulty> _onStart;

        public void Initialize(Action<string, Difficulty> onStart)
        {
            _onStart = onStart;
        }

        public override void Show()
        {
            base.Show();
            SelectDifficulty(Difficulty.Easy);
        }

        private void OnEnable()
        {
            easyButton.onClick.AddListener(HandleEasyClicked);
            mediumButton.onClick.AddListener(HandleMediumClicked);
            hardButton.onClick.AddListener(HandleHardClicked);
            startButton.onClick.AddListener(HandleStart);
        }

        private void OnDisable()
        {
            easyButton.onClick.RemoveListener(HandleEasyClicked);
            mediumButton.onClick.RemoveListener(HandleMediumClicked);
            hardButton.onClick.RemoveListener(HandleHardClicked);
            startButton.onClick.RemoveListener(HandleStart);
        }

        private void HandleEasyClicked() => SelectDifficulty(Difficulty.Easy);
        private void HandleMediumClicked() => SelectDifficulty(Difficulty.Medium);
        private void HandleHardClicked() => SelectDifficulty(Difficulty.Hard);

        private void SelectDifficulty(Difficulty difficulty)
        {
            _selectedDifficulty = difficulty;

            if (easySelectionRing != null)
            {
                easySelectionRing.SetActive(difficulty == Difficulty.Easy);
            }
            if (mediumSelectionRing != null)
            {
                mediumSelectionRing.SetActive(difficulty == Difficulty.Medium);
            }
            if (hardSelectionRing != null)
            {
                hardSelectionRing.SetActive(difficulty == Difficulty.Hard);
            }
        }

        private void HandleStart()
        {
            string playerName = string.IsNullOrWhiteSpace(playerNameInput.text)
                ? "Player"
                : playerNameInput.text.Trim();

            _onStart?.Invoke(playerName, _selectedDifficulty);
        }
    }
}
