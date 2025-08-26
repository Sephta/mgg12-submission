using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrapper : MonoBehaviour
{
  [SerializeField, Scene] private int _bootstrapScene = 1;

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    Scene bootstrapScene = SceneManager.GetSceneByBuildIndex(_bootstrapScene);
    if (!SceneManager.GetSceneByName(bootstrapScene.name).IsValid())
    {
      SceneManager.LoadScene(_bootstrapScene, LoadSceneMode.Additive);
    }
  }

  // private void Start() {}
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
