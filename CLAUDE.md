# KidQuiz — Project Rules

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
