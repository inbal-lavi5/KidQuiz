using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KidQuiz.Presentation
{
    public enum AnswerButtonState
    {
        Neutral,
        Correct,
        Incorrect
    }

    [RequireComponent(typeof(Button))]
    public sealed class AnswerButton : MonoBehaviour
    {
        private static readonly Color[] BadgeColors =
        {
            UiPalette.SkyBlue, UiPalette.Coral, UiPalette.SunYellow, UiPalette.GrassGreen
        };

        private static readonly string[] BadgeLetters = { "A", "B", "C", "D" };

        [SerializeField] private Image cardBorder;
        [SerializeField] private Image cardFill;
        [SerializeField] private Image badgeBorder;
        [SerializeField] private Image badgeFill;
        [SerializeField] private TMP_Text badgeLabel;
        [SerializeField] private TMP_Text answerLabel;

        private Button _button;
        private Action<string> _onClick;

        public string AnswerText { get; private set; }

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(HandleClick);
        }

        public void Bind(string text, Action<string> onClick, int slotIndex)
        {
            AnswerText = text;
            _onClick = onClick;
            answerLabel.text = text;

            int i = Mathf.Clamp(slotIndex, 0, BadgeColors.Length - 1);
            badgeFill.color = BadgeColors[i];
            badgeLabel.text = BadgeLetters[i];
            badgeLabel.color = Color.white;

            SetState(AnswerButtonState.Neutral);
        }

        public void SetState(AnswerButtonState state)
        {
            switch (state)
            {
                case AnswerButtonState.Correct:
                    cardBorder.color = UiPalette.CorrectBorder;
                    cardFill.color = UiPalette.Correct;
                    answerLabel.color = Color.white;
                    badgeBorder.color = UiPalette.CorrectBorder;
                    badgeFill.color = Color.white;
                    badgeLabel.text = "V";
                    badgeLabel.color = UiPalette.CorrectBorder;
                    break;

                case AnswerButtonState.Incorrect:
                    cardBorder.color = UiPalette.IncorrectBorder;
                    cardFill.color = UiPalette.Incorrect;
                    answerLabel.color = Color.white;
                    badgeBorder.color = UiPalette.IncorrectBorder;
                    badgeFill.color = Color.white;
                    badgeLabel.text = "X";
                    badgeLabel.color = UiPalette.IncorrectBorder;
                    break;

                default:
                    cardBorder.color = UiPalette.Ink;
                    cardFill.color = Color.white;
                    answerLabel.color = UiPalette.Ink;
                    badgeBorder.color = UiPalette.Ink;
                    break;
            }
        }

        private void HandleClick()
        {
            _onClick?.Invoke(AnswerText);
        }
    }
}
