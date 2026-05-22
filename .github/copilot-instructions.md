Terse like caveman. Technical substance exact. Only fluff die.
Drop: articles, filler (just/really/basically), pleasantries, hedging.
Fragments OK. Short synonyms. Code unchanged.
Pattern: [thing] [action] [reason]. [next step].
ACTIVE EVERY RESPONSE. No revert after many turns. No filler drift.
Code/commits/PRs: normal. Off: "stop caveman" / "normal mode".

# Unity Project Instructions

Tech Stack:

- Unity
- VContainer
- UniTask
- R3
- MessagePipe
- UI Toolkit

---

# Core Principles

Prefer modern Unity architecture focused on:

- Dependency Injection
- Reactive State
- Event-Driven Communication
- Async Workflows
- Scene-Based Composition

Avoid:

- Singletons
- FindObjectOfType
- GameObject.Find
- Resources.Load
- Static gameplay state
- Service Locator patterns
- God Objects / GameManager patterns

Prefer:

- Constructor Injection
- LifetimeScopes
- Small focused services
- ScriptableObjects for configuration
- MessagePipe for events
- R3 for state
- UniTask for async operations

---

# Architecture

Separate code into:

```text
Bootstrap
Core
Features
UI
```

Business logic belongs in services.

Views display state and forward user input.

Gameplay systems communicate through events and state.

---

# Scene Architecture

Use a Bootstrap Scene that remains loaded for the entire application lifetime.

Example:

```text
BootstrapScene
├── AppLifetimeScope
├── LoadingScreenView
└── App Entry Point

MenuScene
├── MenuLifetimeScope
└── MainMenuView

GameplayScene
├── GameplayLifetimeScope
├── Player
├── UI
└── Gameplay Objects
```

Bootstrap Scene owns:

- SceneService
- LoadingScreenService
- AudioService
- SaveService
- SettingsService

Gameplay Scene owns:

- CoinService
- EnemyService
- WaveService
- WinConditionService

Menu Scene owns:

- Menu UI
- Menu-specific services

Use additive scene loading.

Avoid putting all services in a single global LifetimeScope.

---

# Dependency Injection

Use VContainer.

Register services inside LifetimeScopes.

```csharp
builder.Register<CoinService>(Lifetime.Scoped);
builder.Register<SceneService>(Lifetime.Singleton);
```

Inject dependencies through constructors.

```csharp
public class WinConditionService
{
    private readonly CoinService _coinService;

    public WinConditionService(
        CoinService coinService)
    {
        _coinService = coinService;
    }
}
```

Use `[Inject]` only for MonoBehaviours.

```csharp
[Inject]
public void Construct(
    CoinService coinService)
{
    _coinService = coinService;
}
```

Prefer constructor injection everywhere else.

---

# Entry Points

Prefer VContainer EntryPoints over MonoBehaviour bootstrap logic.

```csharp
builder.RegisterEntryPoint<GameEntryPoint>();
```

Use:

- IStartable
- ITickable
- IFixedTickable
- IDisposable

for application services.

Avoid MonoBehaviour Update loops when a service is more appropriate.

---

# Services

Services contain business logic.

Services should:

- Be plain C# classes
- Not inherit from MonoBehaviour
- Be testable
- Own game state

Example:

```csharp
public class CoinService
{
}
```

Naming:

```text
SomethingService
```

Examples:

```text
CoinService
EnemyService
WaveService
SceneService
AudioService
```

---

# Views

Views are thin.

Views should:

- Display state
- Forward user input
- Not contain business logic

Example:

```csharp
public class GameHudView : MonoBehaviour
{
}
```

Naming:

```text
SomethingView
```

Examples:

```text
GameHudView
MainMenuView
LoadingScreenView
```

---

# UniTask

Prefer UniTask over Coroutines.

```csharp
await UniTask.Delay(
    TimeSpan.FromSeconds(1));
```

Use:

- UniTask
- UniTaskVoid only when appropriate
- CancellationToken

Prefer:

```csharp
this.GetCancellationTokenOnDestroy();
```

Avoid:

```csharp
IEnumerator
StartCoroutine
```

unless Unity APIs require them.

Avoid async void except Unity entry points.

---

# R3

Use R3 for state.

State changes over time and has a current value.

Examples:

```text
Coins
Health
CurrentWave
HasWon
LoadingProgress
```

Use:

```csharp
private readonly ReactiveProperty<int> _coins = new(0);
```

Expose:

```csharp
public ReadOnlyReactiveProperty<int> Coins => _coins;
```

Views subscribe to state.

```csharp
_coinService.Coins
    .Subscribe(UpdateCoins)
    .AddTo(this);
```

Do not use R3 as the primary event bus.

---

# MessagePipe

Use MessagePipe for events.

Events describe facts that happened.

Examples:

```text
CoinCollectedEvent
EnemyKilledEvent
PlayerDiedEvent
WaveStartedEvent
```

Publish:

```csharp
_publisher.Publish(
    new CoinCollectedEvent(1));
```

Subscribe:

```csharp
_subscriber.Subscribe(OnCoinCollected);
```

Use MessagePipe when multiple systems react to the same event.

Examples:

```text
CoinCollectedEvent
    ↓
CoinService

CoinCollectedEvent
    ↓
AudioService

CoinCollectedEvent
    ↓
StatisticsService

CoinCollectedEvent
    ↓
AchievementService
```

Prefer MessagePipe over manually building event buses.

---

# State vs Events

Important distinction:

State:

```text
Current Coins
Current Health
Current Wave
Has Won
```

→ R3

Events:

```text
Coin Collected
Enemy Killed
Player Died
Level Completed
```

→ MessagePipe

Never model state as events.

Never model events as ReactiveProperties.

---

# UI Toolkit

Use UI Toolkit.

Prefer:

- UIDocument
- UXML
- USS

Views should query elements once.

```csharp
_button = root.Q<Button>("start-button");
```

Wire events inside views.

Prefer programmatic setup through injected configuration.

Example:

```csharp
UIDocumentConfig
```

ScriptableObject contains:

- PanelSettings
- VisualTreeAsset

Inject configuration into views.

---

# ScriptableObjects

Use ScriptableObjects for configuration only.

Examples:

```text
GameSettings
AudioSettings
EnemyConfig
SkillConfig
UIDocumentConfig
```

Avoid putting runtime state inside ScriptableObjects.

Naming:

```text
SomethingConfig
SomethingSettings
```

---

# Scene Loading

Use SceneService.

Prefer:

```csharp
await sceneService.LoadSceneAsync(
    "GameplayScene");
```

Use additive loading.

Show loading screens during scene transitions.

Bootstrap Scene should remain loaded.

Loading flow:

```text
BootstrapScene
    ↓
Loading Screen
    ↓
Unload Current Scene
    ↓
Load New Scene
    ↓
Hide Loading Screen
```

---

# Naming

Use consistent naming.

```text
SomethingService
SomethingView
SomethingConfig
SomethingSettings
SomethingState
SomethingEvent
SomethingLifetimeScope
```

Examples:

```text
CoinService
GameHudView
EnemyConfig
CoinCollectedEvent
GameplayLifetimeScope
```

---

# Generated Code Rules

When generating code:

- Prefer composition over inheritance
- Prefer constructor injection
- Prefer VContainer
- Prefer UniTask
- Prefer MessagePipe for events
- Prefer R3 for state
- Prefer UI Toolkit over uGUI
- Keep MonoBehaviours thin
- Keep business logic in services
- Use additive scene loading
- Use Bootstrap Scene architecture
- Use ScriptableObjects for configuration
- Use scoped services for gameplay state
- Use singleton services only for application-wide systems