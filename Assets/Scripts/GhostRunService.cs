using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

public class GhostRunService : IStartable, IFixedTickable, IDisposable
{
    private const string BestRunRecordingKey = "best_run_recording_v1";
    private const string BaseColorProperty = "_BaseColor";
    private const string ColorProperty = "_Color";

    private readonly GameTimerService _gameTimerService;
    private readonly FrogInputStateService _inputStateService;
    private readonly PlayerFrogMovement _playerFrogMovement;
    private readonly List<GhostRunFrameData> _recordingFrames = new();

    private GhostRunRecordingData _persistedRecording;
    private GameObject _ghostObject;
    private bool _isRecording;
    private bool _isPlaying;
    private float _recordingTime;
    private float _playbackTime;
    private int _playbackFrameIndex;

    public GhostRunService(
        GameTimerService gameTimerService,
        FrogInputStateService inputStateService,
        PlayerFrogMovement playerFrogMovement)
    {
        _gameTimerService = gameTimerService;
        _inputStateService = inputStateService;
        _playerFrogMovement = playerFrogMovement;
    }

    public void Start()
    {
        _persistedRecording = LoadRecording();
        CleanupAndAdoptExistingGhost();

        if (HasPlayableRecording(_persistedRecording))
        {
            if (_ghostObject == null)
            {
                CreateGhostObject();
            }
            else
            {
                _ghostObject.SetActive(false);
            }
        }
        else
        {
            DestroyGhostObject();
        }

        _gameTimerService.RunStarted += OnRunStarted;
        _gameTimerService.RunCompleted += OnRunCompleted;
    }

    public void FixedTick()
    {
        if (_isRecording)
        {
            RecordCurrentFrame();
        }

        if (_isPlaying)
        {
            TickPlayback();
        }
    }

    public void Dispose()
    {
        _gameTimerService.RunStarted -= OnRunStarted;
        _gameTimerService.RunCompleted -= OnRunCompleted;

        DestroyGhostObject();
    }

    private void OnRunStarted()
    {
        _recordingFrames.Clear();
        _recordingTime = 0f;
        _isRecording = true;

        if (HasPlayableRecording(_persistedRecording) && _ghostObject != null)
        {
            _playbackFrameIndex = 0;
            _playbackTime = 0f;
            _isPlaying = true;
            _ghostObject.SetActive(true);
            ApplyFrame(_persistedRecording.frames[0]);
        }
        else if (_ghostObject != null)
        {
            _isPlaying = false;
            _ghostObject.SetActive(false);
        }
    }

    private void OnRunCompleted(RunCompletionResult result)
    {
        _isRecording = false;
        _isPlaying = false;

        if (!result.IsNewBest)
        {
            return;
        }

        if (_recordingFrames.Count == 0)
        {
            return;
        }

        GhostRunRecordingData newRecording = BuildRecordingData(_recordingFrames, result.FinalTimeSeconds);
        SaveRecording(newRecording);
        _persistedRecording = newRecording;

        if (_ghostObject == null)
        {
            CreateGhostObject();
        }
    }

    private void RecordCurrentFrame()
    {
        if (_playerFrogMovement == null)
        {
            return;
        }

        Transform playerTransform = _playerFrogMovement.transform;
        Vector2 moveInput = _inputStateService.MoveInput.CurrentValue;

        _recordingFrames.Add(new GhostRunFrameData
        {
            timeSeconds = _recordingTime,
            moveX = moveInput.x,
            moveY = moveInput.y,
            isJumpPressed = _inputStateService.IsJumpPressed.CurrentValue,
            mouseTurnInput = _inputStateService.PeekMouseTurnInput(),
            posX = playerTransform.position.x,
            posY = playerTransform.position.y,
            posZ = playerTransform.position.z,
            rotX = playerTransform.rotation.x,
            rotY = playerTransform.rotation.y,
            rotZ = playerTransform.rotation.z,
            rotW = playerTransform.rotation.w
        });

        _recordingTime += Time.fixedDeltaTime;
    }

    private void TickPlayback()
    {
        if (_ghostObject == null || !HasPlayableRecording(_persistedRecording))
        {
            _isPlaying = false;
            return;
        }

        GhostRunFrameData[] frames = _persistedRecording.frames;

        _playbackTime += Time.fixedDeltaTime;

        while (_playbackFrameIndex < frames.Length - 1
               && frames[_playbackFrameIndex + 1].timeSeconds <= _playbackTime)
        {
            _playbackFrameIndex++;
        }

        ApplyFrame(frames[_playbackFrameIndex]);

        if (_playbackFrameIndex >= frames.Length - 1)
        {
            _isPlaying = false;
        }
    }

    private void ApplyFrame(GhostRunFrameData frame)
    {
        if (_ghostObject == null)
        {
            return;
        }

        Transform ghostTransform = _ghostObject.transform;
        ghostTransform.SetPositionAndRotation(
            new Vector3(frame.posX, frame.posY, frame.posZ),
            new Quaternion(frame.rotX, frame.rotY, frame.rotZ, frame.rotW));
    }

    private void CreateGhostObject()
    {
        if (_playerFrogMovement == null)
        {
            return;
        }

        CleanupAndAdoptExistingGhost();
        if (_ghostObject != null)
        {
            return;
        }

        _ghostObject = UnityEngine.Object.Instantiate(_playerFrogMovement.gameObject);
        _ghostObject.name = "GhostRunner";
        _ghostObject.AddComponent<GhostRunnerMarker>();

        foreach (var behaviour in _ghostObject.GetComponentsInChildren<Behaviour>(true))
        {
            behaviour.enabled = false;
        }

        foreach (var rigidbody in _ghostObject.GetComponentsInChildren<Rigidbody>(true))
        {
            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
            rigidbody.detectCollisions = false;
        }

        foreach (var collider in _ghostObject.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
        }

        ApplyGhostVisualStyle(_ghostObject);
        _ghostObject.SetActive(false);
    }

    private void CleanupAndAdoptExistingGhost()
    {
        GhostRunnerMarker[] markers = UnityEngine.Object.FindObjectsByType<GhostRunnerMarker>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        GameObject firstFoundGhost = null;
        for (int i = 0; i < markers.Length; i++)
        {
            if (markers[i] == null)
            {
                continue;
            }

            GameObject ghost = markers[i].gameObject;
            if (firstFoundGhost == null)
            {
                firstFoundGhost = ghost;
                continue;
            }

            UnityEngine.Object.Destroy(ghost);
        }

        if (firstFoundGhost != null)
        {
            _ghostObject = firstFoundGhost;
        }
    }

    private void DestroyGhostObject()
    {
        if (_ghostObject == null)
        {
            return;
        }

        UnityEngine.Object.Destroy(_ghostObject);
        _ghostObject = null;
    }

    private static void ApplyGhostVisualStyle(GameObject ghostObject)
    {
        foreach (var renderer in ghostObject.GetComponentsInChildren<Renderer>(true))
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            var propertyBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propertyBlock);

            if (renderer.sharedMaterial != null)
            {
                if (renderer.sharedMaterial.HasProperty(BaseColorProperty))
                {
                    propertyBlock.SetColor(BaseColorProperty, new Color(0.35f, 0.9f, 1f, 0.65f));
                }
                else if (renderer.sharedMaterial.HasProperty(ColorProperty))
                {
                    propertyBlock.SetColor(ColorProperty, new Color(0.35f, 0.9f, 1f, 0.65f));
                }
            }

            renderer.SetPropertyBlock(propertyBlock);
        }
    }

    private static bool HasPlayableRecording(GhostRunRecordingData recording)
    {
        return recording != null
               && recording.frames != null
               && recording.frames.Length > 0;
    }

    private static GhostRunRecordingData BuildRecordingData(
        List<GhostRunFrameData> frames,
        float finalTimeSeconds)
    {
        return new GhostRunRecordingData
        {
            finalTimeSeconds = finalTimeSeconds,
            frames = frames.ToArray()
        };
    }

    private static GhostRunRecordingData LoadRecording()
    {
        if (!PlayerPrefs.HasKey(BestRunRecordingKey))
        {
            return null;
        }

        string json = PlayerPrefs.GetString(BestRunRecordingKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            var loaded = JsonUtility.FromJson<GhostRunRecordingData>(json);
            return HasPlayableRecording(loaded) ? loaded : null;
        }
        catch
        {
            return null;
        }
    }

    private static void SaveRecording(GhostRunRecordingData recording)
    {
        string json = JsonUtility.ToJson(recording);
        PlayerPrefs.SetString(BestRunRecordingKey, json);
        PlayerPrefs.Save();
    }
}

[Serializable]
public class GhostRunRecordingData
{
    public float finalTimeSeconds;
    public GhostRunFrameData[] frames;
}

[Serializable]
public struct GhostRunFrameData
{
    public float timeSeconds;
    public float moveX;
    public float moveY;
    public bool isJumpPressed;
    public float mouseTurnInput;
    public float posX;
    public float posY;
    public float posZ;
    public float rotX;
    public float rotY;
    public float rotZ;
    public float rotW;
}

public class GhostRunnerMarker : MonoBehaviour
{
}