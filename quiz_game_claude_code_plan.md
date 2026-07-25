# Kid Quiz — Unity Portfolio Project
## Build plan for Claude Code + Unity MCP

A quiz game for kids. Questions come from a remote API, scores go to a Firebase leaderboard.
Ships as **WebGL on itch.io** and as an **APK on your Android phone**.

**Scope:** 6 phases, ~12–15 hours. Every phase ends in a commit.

---

## Requirement coverage

| Job requirement | Where it shows |
|---|---|
| Unity + C# | Whole project |
| OOP | `QuestionBase` abstract class, provider/repository interfaces, encapsulated `QuizSession` |
| Backend / APIs | `ApiClient` over `UnityWebRequest`, Open Trivia DB |
| Git | Commit per phase |
| Cloud + NoSQL | Firebase Realtime Database over REST |
| QA | Unit tests on the domain layer + a short manual test plan |
| Performance | Object pooling, Profiler pass |

---

## Phase 0 — Setup

**Before opening Claude Code:**

1. Unity 6 LTS, new **2D** project named `KidQuiz`. Install **Android Build Support** and **WebGL Build Support** modules in Unity Hub.
2. Git repo with GitHub's `Unity.gitignore`.
3. Firebase project → Realtime Database → test mode. Copy the database URL.
4. Package Manager: install **Test Framework** and **Newtonsoft Json** (`com.unity.nuget.newtonsoft-json`).
5. Unity MCP connected to Claude Code — verify by asking Claude to list the project's scenes.

**Player Settings — do this now, not at the end.** Building the UI against the wrong aspect ratio is the most annoying thing to redo.

- **Orientation: Portrait, locked.** One layout for both targets. On itch you embed it at 540×960; on the phone it's native. This is the single biggest simplification available to you — take it.
- Canvas Scaler on every canvas: `Scale With Screen Size`, reference resolution `1080×1920`, Match `0.5`.
- **WebGL** → Publishing Settings → Compression Format `Gzip`, and tick **Decompression Fallback**. This is the config that reliably works on itch.io. Also enable Strip Engine Code.
- **Android** → Other Settings → Internet Access `Require`, Minimum API Level 24, ARM64, IL2CPP.

**Then, first prompt — create `CLAUDE.md` at the repo root:**

```
Create CLAUDE.md with these project rules:

- Unity 6 LTS, 2D, C#. Targets: WebGL (itch.io) and Android. Portrait only.
- Three layers:
  1. Domain — pure C#, no UnityEngine reference, own assembly definition.
  2. Data — HTTP clients, JSON, repositories.
  3. Presentation — MonoBehaviours, UI.
  Dependencies point inward. Never the reverse.
- WebGL has no threads: no Task.Run, no Thread, no blocking waits.
  UnityWebRequest with async/await only.
- async/await for network. Coroutines only for frame-based timing and animation.
- Cache component references in Awake. No GetComponent in Update.
- Every network call handles success, HTTP error, timeout, malformed JSON.
- Keep it small. Do not add features, abstractions, or dependencies that
  aren't explicitly requested. No tweening libraries, no DI frameworks.
- English for code, comments, and commits.
```

That last rule matters more than it looks. Without it Claude will happily add a save system and an achievement framework you didn't ask for.

---

## Phase 1 — Domain layer

Pure C#, no Unity. Testable in milliseconds without entering Play mode.

```
Create Assets/Scripts/Domain with an assembly definition KidQuiz.Domain
that does NOT reference UnityEngine.

1. Question — a sealed class. Id, Prompt, Difficulty (enum Easy/Medium/Hard),
   IReadOnlyList<string> Options, and bool IsCorrect(string answer)
   that is case-insensitive and trims whitespace.
   Options are shuffled on construction via an injected IRandomizer.

2. IRandomizer with Shuffle<T>(IList<T>), plus SystemRandomizer.
   Never call UnityEngine.Random in this layer — that's what makes tests
   deterministic.

3. QuizSession — one playthrough. Private fields, read-only public properties
   (CurrentQuestion, Score, QuestionsAnswered, IsComplete). Methods:
   AnswerResult Submit(string answer, float secondsRemaining), and Advance().
   AnswerResult is a readonly struct: WasCorrect, PointsAwarded, CorrectAnswer.
   No public setters. A second Submit on the same question is ignored.

4. ScoringRules — points from correctness, difficulty, and time remaining.
   Separate class, single responsibility.

Keep it minimal. No inheritance hierarchy — one Question type is enough.
```

Then:

```
Add a Tests/EditMode assembly with NUnit tests for:
- a full 10-question run and its final score
- Submit called twice on one question awards points once
- IsCorrect handles casing and whitespace
- ScoringRules at boundaries: zero time left, full time left
Use a FakeRandomizer that doesn't shuffle.
```

**Verify:** Test Runner → EditMode → Run All. Green before continuing.

**Commit:** `feat(domain): quiz session, question model, scoring rules + tests`

> Note the change from the earlier draft: no `QuestionBase`/`MultipleChoiceQuestion`/`TrueFalseQuestion` hierarchy. One concrete `Question` class. An abstract base with a single subclass is inheritance for its own sake, and an interviewer will notice. You still have plenty of polymorphism to discuss in the provider and repository interfaces, where it's actually earning its place.

---

## Phase 2 — Config

```
Create Assets/Scripts/Config/QuizConfig.cs — a ScriptableObject with
[CreateAssetMenu(menuName = "KidQuiz/Quiz Config")].

Serialized private fields with public read-only properties:
questionsPerRound (10), secondsPerQuestion (20), basePoints (100),
timeBonusMultiplier, difficulty, triviaCategoryId.

Create three assets in Assets/Resources/Configs: Easy, Medium, Hard.
```

**Commit:** `feat(config): quiz settings as ScriptableObjects`

---

## Phase 3 — API layer, and an early WebGL smoke test

```
Create Assets/Scripts/Data:

1. ApiClient — async wrapper over UnityWebRequest.
   Task<ApiResult<T>> GetAsync<T>(string url, CancellationToken ct)
   Task<bool> PutAsync(string url, object body, CancellationToken ct)
   ApiResult<T> is a readonly struct: IsSuccess, Value, ErrorKind
   (None/Network/HttpError/Timeout/ParseError), ErrorMessage.
   Never throws — failures come back as a failed ApiResult.
   10s timeout. One retry on Network/Timeout only, never on 4xx.
   Await the AsyncOperation via TaskCompletionSource. No polling loops.

2. IQuestionProvider (in the Domain assembly — it's a boundary contract):
   Task<IReadOnlyList<Question>> FetchAsync(QuizConfig config, CancellationToken ct)

3. TriviaApiProvider — GET https://opentdb.com/api.php?amount={n}&difficulty={d}&type=multiple
   Separate DTO classes, mapped explicitly to domain Questions.
   The API HTML-encodes text — decode entities. Treat response_code != 0 as failure.

4. LocalQuestionProvider — reads a QuestionBank ScriptableObject with 15
   hardcoded kid-friendly questions. The offline fallback.

5. FallbackQuestionProvider — takes a primary and a fallback, tries primary,
   falls back silently on failure. Decorator pattern, ~20 lines.
```

**Now build to WebGL and load it once.** Do not wait until phase 6.

The reason: your game makes cross-origin requests from a browser, and whether that works depends on CORS headers the third-party trivia API sends — which you don't control and I can't verify for you. Find out now, while the fix is cheap. If opentdb.com doesn't allow it, your options are to lean on `LocalQuestionProvider` as the primary source, or proxy the request through Firebase. Either is a five-minute change at this stage and a painful one at phase 6.

Firebase's REST API does support browser requests, so the leaderboard should be fine — but confirm that in the same test build.

**Commit:** `feat(data): async api client, trivia provider, offline fallback`

---

## Phase 4 — UI

```
Single scene named Main. Three screens as sibling canvases under a
ScreenManager that activates one at a time. No additive scene loading.

1. GameManager — singleton with a duplicate guard in Awake and
   DontDestroyOnLoad. It is the composition root: it constructs the
   providers and repository and passes them to screens. Screens do not
   reach for GameManager.Instance themselves.

2. HomeScreen — TMP_InputField for player name, three difficulty buttons,
   Start button.

3. QuizScreen — question text, answer button container, timer bar,
   score label. On answer: show correct/incorrect colour for 1.2s, advance.
   Coroutine for the timer and the feedback delay. async/await for network.
   Cache all references in Awake.

4. AnswerButton prefab — Bind(string text, Action<string> onClick) and
   SetState(Neutral/Correct/Incorrect).

5. ObjectPool<T> where T : Component — prewarm, Get, Release.
   Use it for AnswerButton instances.

6. ResultsScreen — score, correct/total, Play Again, leaderboard container.

Wire button listeners in code: AddListener in OnEnable, RemoveListener in
OnDisable. Not in the inspector — it should be visible in version control.

Touch input needs no special handling; Unity UI buttons handle mouse and
touch identically. Anchor everything for portrait 1080x1920.
```

**Verify on the phone.** Build an APK and install it before moving on. Text that's readable in the editor is often too small in your hand, and that's easier to fix now than after the results screen exists.

**Commit:** `feat(ui): quiz flow with pooled answer buttons`

> On pooling: reusing four buttons is not a meaningful performance win, and you should say so if asked rather than overselling it. What it demonstrates is that you know the pattern and when it *would* matter — a particle-heavy game, a bullet hell, a scrolling list of hundreds of items. Claiming a real optimisation here would be an easy thing for an interviewer to catch.

---

## Phase 5 — Firebase leaderboard

```
1. IScoreRepository (Domain assembly):
   Task<bool> SubmitAsync(ScoreEntry entry, CancellationToken ct)
   Task<IReadOnlyList<ScoreEntry>> GetTopAsync(int count, CancellationToken ct)

2. FirebaseScoreRepository — uses ApiClient.
   Write: PUT {dbUrl}/scores/{guid}.json
   Read:  GET {dbUrl}/scores.json?orderBy="score"&limitToLast={count}
          then sort descending client-side.
   The database URL comes from a FirebaseConfig ScriptableObject.
   Gitignore that asset; commit a FirebaseConfig.example and document it
   in the README.

3. ResultsScreen shows three states: loading, loaded, unavailable.
   Never show a raw error or an empty screen to a child.
```

Set these database rules in the Firebase console:

```json
{
  "rules": {
    "scores": {
      ".read": true,
      ".write": true,
      ".indexOn": "score"
    }
  }
}
```

These are permissive. Say so in the README, and say that production would put an authenticated backend in front of it. Naming your own limitation is worth more in an interview than quietly hoping nobody asks.

**Commit:** `feat(cloud): firebase leaderboard over REST`

---

## Phase 6 — Ship it

**Performance pass:**

```
Open the Profiler during a full round. Find and remove per-frame
allocations: no LINQ in Update, no string concatenation for the timer
(cache the second strings). Confirm the pool prevents Instantiate calls
after the first question.
```

**Manual QA** — short table in `TEST_PLAN.md`, then actually run it:

| Airplane mode at launch | Connection dropped mid-round | Double-tap an answer | Timer expires with nothing selected | Empty player name | Back button on Android |

**WebGL → itch.io:** build, zip the output folder, upload, tick "This file will be played in the browser", set embed to 540×960 with fullscreen enabled. If it hangs on the loading bar, it's almost always the compression setting from phase 0.

**Android:** build APK, transfer to phone, allow install from unknown sources.

**README** — keep it to one screen plus screenshots:

```
- One paragraph on what it is, plus a playable itch.io link and 2 screenshots
- Mermaid diagram: three layers, arrows pointing inward
- Setup: the FirebaseConfig step
- Technical decisions, 2 sentences each: why the domain layer has no
  UnityEngine reference; why async/await for network and coroutines for
  timing; why the decorator for offline fallback
- Known limitations: client-authoritative scoring, permissive database rules
- What's next: auth, server-side scoring, Hebrew RTL localisation
```

**Commit:** `docs: readme, test plan, build config`

---

## Working with Claude Code + Unity MCP

**Use the MCP for verification, not authorship.** Let Claude write scripts to disk normally, then use the MCP to attach components, enter Play mode, and read the console back. The verify loop is where it earns its keep.

**One phase per session.** A fresh session with a good `CLAUDE.md` beats a four-hour one that's drifted.

**Read every diff before committing.** You will be asked why something is written the way it is. If you can't answer, it shouldn't be in the repo.

---

## Deliberately not doing

Worth saying out loud in the interview — knowing what you cut reads as judgement.

- **No accounts.** Player name only.
- **No custom art.** Clean palette, Unity UI defaults. It's a code sample.
- **Portrait only.** One layout for two platforms.
- **Client-side scoring.** Trivially cheatable, noted in the README with the fix.
- **No Docker, no CI.** Nothing here needs them.

---

## Checklist

- [ ] Phase 0 — project, git, Firebase, MCP, `CLAUDE.md`, player settings for both targets
- [ ] Phase 1 — domain + green tests
- [ ] Phase 2 — QuizConfig assets
- [ ] Phase 3 — API client + providers + **WebGL smoke test**
- [ ] Phase 4 — UI + pooling + **APK on your phone**
- [ ] Phase 5 — Firebase leaderboard
- [ ] Phase 6 — Profiler, test plan, itch.io upload, README
