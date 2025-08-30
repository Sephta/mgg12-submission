// Thank you Unity: https://www.youtube.com/watch?v=WLDgtRNK2VE

using UnityEngine;

public class SingletonMonoBehavior<T> : MonoBehaviour where T : MonoBehaviour
{
  private static T _instance = null;

  public static T Instance
  {
    get
    {
      if (_instance == null)
      {
        _instance = FindFirstObjectByType<T>();
        if (_instance == null)
        {
          GameObject newSingletonObject = new();
          newSingletonObject.name = typeof(T).ToString();
          _instance = newSingletonObject.AddComponent<T>();
        }
      }
      return _instance;
    }
  }

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  public virtual void Awake()
  {
    if (_instance != null)
    {
      Destroy(gameObject);
      return;
    }

    _instance = GetComponent<T>();

    DontDestroyOnLoad(gameObject);

    if (_instance == null)
    {
      return;
    }
  }
}
