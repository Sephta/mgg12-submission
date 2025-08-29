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

  // private void OnEnable() {}
  // private void OnDisable() {}
  // private void Update() {}
  // private void FixedUpdate() {}

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */
}
