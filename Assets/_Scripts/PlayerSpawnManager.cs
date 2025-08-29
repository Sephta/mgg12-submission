using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerSpawnManager))]
public class PlayerSpawnManager : MonoBehaviour
{
  private static PlayerSpawnManager _instance = null;

  public static PlayerSpawnManager Instance
  {
    get
    {
      if (_instance == null)
      {
        _instance = FindFirstObjectByType<PlayerSpawnManager>();
        if (_instance == null)
        {
          GameObject singletonObject = new()
          {
            name = typeof(PlayerSpawnManager).ToString()
          };

          _instance = singletonObject.AddComponent<PlayerSpawnManager>();
        }
      }

      return _instance;
    }
  }

  [SerializeField] private VoidEventChannelSO _playerDeathEvent;
  [SerializeField] private VoidEventChannelSO _playerRespawnEvent;
  [SerializeField] private TransformEventChannelSO _assignPlayerToCamera;
  [SerializeField] private GameObject _playerPrefab;
  [SerializeField, ReadOnly] private List<PlayerSpawnPoint> _spawnPoints = new();

  [Space(5f)]

  [Header("Debug")]

  [Space(5f)]

  [ReadOnly] public Vector3 PlayerPositionOnDeath = Vector3.negativeInfinity;

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (_instance != null)
    {
      Destroy(gameObject);
      return;
    }

    _instance = GetComponent<PlayerSpawnManager>();

    DontDestroyOnLoad(gameObject);

    if (_instance == null) return;
  }

  private void Start()
  {
    if (_instance == null) return;

    PlayerSpawnPoint[] playerSpawns = FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.None);

    _spawnPoints = new(playerSpawns);
  }

  private void OnEnable()
  {
    if (_playerRespawnEvent == null) return;

    _playerRespawnEvent.OnEventRaised += OnPlayerRespawn;
  }

  private void OnDisable()
  {
    if (_playerRespawnEvent == null) return;

    _playerRespawnEvent.OnEventRaised -= OnPlayerRespawn;
  }

  // private void Update() {}
  // private void FixedUpdate() {}

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  private void OnPlayerRespawn()
  {
    if (_playerPrefab == null) return;
    if (PlayerPositionOnDeath == Vector3.negativeInfinity) return;

    GameObject newPlayer = Instantiate(_playerPrefab);


    float closestDistance = float.MaxValue;
    PlayerSpawnPoint closestSpawn = null;

    foreach (PlayerSpawnPoint spawnPoint in _spawnPoints)
    {
      float newDistance = Vector3.Distance(spawnPoint.transform.position, PlayerPositionOnDeath);
      if (newDistance < closestDistance)
      {
        closestDistance = newDistance;
        closestSpawn = spawnPoint;
        Debug.Log("closestSpawn is a new point");
      }
    }

    if (closestSpawn != null)
    {
      newPlayer.transform.position = closestSpawn.transform.position;
      if (_assignPlayerToCamera != null) _assignPlayerToCamera.RaiseEvent(newPlayer.transform);
    }
    else
    {
      Destroy(newPlayer);
      Debug.LogError("Unable to spawn new player prefab.");
    }
  }
}
