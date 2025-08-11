using NaughtyAttributes;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEntryPoint : MonoBehaviour
{
  [SerializeField, Scene] private int _sceneToLoadAfterInitialization;

  private void Awake()
  {
    SceneManager.LoadScene(_sceneToLoadAfterInitialization, LoadSceneMode.Additive);
  }
}
