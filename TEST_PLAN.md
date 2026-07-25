# KidQuiz — Manual Test Plan

Run this after any change to the Data or Presentation layers, before shipping a build.

| # | Scenario | Steps | Expected | Result |
|---|----------|-------|----------|--------|
| 1 | Airplane mode at launch | Enable airplane mode, launch the app, start a round | Trivia API fetch fails fast, `FallbackQuestionProvider` silently falls back to the offline `QuestionBank` for the chosen topic. Round plays normally. Leaderboard on the Results screen shows "Leaderboard unavailable right now." | Pass — verified by code path (`ApiClient` never throws; `FallbackQuestionProvider` falls back on empty/null result) and by disabling the network mid-fetch in the Editor. |
| 2 | Connection dropped mid-round | Start a round on Wi-Fi/data, disable networking after the questions have loaded, finish the round | Round is unaffected (questions already fetched). Score submission and leaderboard fetch fail gracefully; Results screen shows "Leaderboard unavailable right now." instead of hanging or crashing. | Pass — `FirebaseScoreRepository` returns `null`/`false` on failure, `LeaderboardView.ShowLoaded(null)` renders the unavailable message. |
| 3 | Double-tap an answer | Tap an answer button twice in quick succession (or spam-tap) | Only the first tap registers. Score changes once. No double-counted points, no duplicate feedback animation. | Pass — `QuizScreen.HandleAnswerSelected` returns immediately if `_acceptingAnswers` is already `false`, which is set on the very first accepted tap. |
| 4 | Timer expires with nothing selected | Let the countdown reach zero without tapping any answer | Round auto-advances as a wrong answer (0 points), correct answer is highlighted, then the next question loads. | Pass — `RunTimer` calls `HandleAnswerSelected(null)` on expiry; `Question.IsCorrect(null)` returns `false` explicitly. |
| 5 | Empty player name | Leave the nickname field blank (or whitespace-only) on Home | Start button stays disabled and a hint ("Type your name to start!") is shown instead of a silently-inert button. | Pass — `HomeScreen.UpdateStartButtonState` gates `startButton.interactable` on `!string.IsNullOrWhiteSpace`. |
| 6 | Back button on Android | On a physical Android device/emulator, press the hardware back button from any screen | From Home: quits the app. From any other screen: returns to Home, same as that screen's own Back button. | Pass — `GameManager.Update()` listens for `KeyCode.Escape` (Android maps the hardware back button to it) and routes to the same handlers the on-screen Back buttons use. |

## Notes

- Rows 1–5 also apply to WebGL; row 6 is Android-only (in WebGL, `Escape` does nothing since there's no hardware back button to map).
- "Result" reflects the state of the code as of this test plan's last update — re-verify manually in the actual target build (Editor Play mode does not fully represent WebGL/Android runtime behavior, especially for networking).
