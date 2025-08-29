using System.Collections.Generic;
using stal.HSM.Drivers;
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

  [SerializeField] private VoidEventChannelSO _playerRespawnEvent;
  [SerializeField] private TransformEventChannelSO _assignPlayerToCamera;
  [SerializeField, ReadOnly] private List<PlayerSpawnPoint> _spawnPoints = new();

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
    PlayerHSMDriver[] playerHSMDrivers = FindObjectsByType<PlayerHSMDriver>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    if (playerHSMDrivers.Length > 0)
    {
      PlayerHSMDriver player = playerHSMDrivers[0];

      float closestDistance = float.MaxValue;
      PlayerSpawnPoint closestSpawn = null;

      foreach (PlayerSpawnPoint spawnPoint in _spawnPoints)
      {
        float newDistance = Vector3.Distance(spawnPoint.transform.position, player.transform.position);
        if (newDistance < closestDistance)
        {
          closestDistance = newDistance;
          closestSpawn = spawnPoint;
        }
      }

      if (closestSpawn != null)
      {
        player.transform.position = closestSpawn.transform.position;
        player.gameObject.SetActive(true);
        if (_assignPlayerToCamera != null) _assignPlayerToCamera.RaiseEvent(player.transform);
      }
      else
      {
        Debug.LogError("Unable to spawn new player prefab.");
      }
    }
    else
    {
      Debug.LogError("Player not found");
    }

  }
}
