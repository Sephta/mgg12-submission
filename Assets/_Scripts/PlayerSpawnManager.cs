using System.Collections.Generic;
using stal.HSM.Drivers;
using UnityEngine;

public class PlayerSpawnManager : SingletonMonoBehavior<PlayerSpawnManager>
{
  [SerializeField] private PlayerHealthSO _playerHealth;
  [SerializeField] private VoidEventChannelSO _playerRespawnEvent;
  [SerializeField] private TransformEventChannelSO _assignPlayerToCamera;
  [SerializeField, ReadOnly] private List<PlayerSpawnPoint> _spawnPoints = new();

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Start()
  {
    if (Instance == null) return;

    if (_playerHealth == null)
    {
      Debug.LogError(name + " needs a reference to PlayerHealthSO plugged into the inspector for this class.");
    }

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
        _playerHealth.SetCurrentHealth(_playerHealth.MaxHealth);
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
