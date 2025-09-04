using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
  [SerializeField, OnValueChanged(nameof(OnSettingsChanged))]
  private bool _isSplashImagePresent = false;

  [SerializeField, OnValueChanged(nameof(OnSettingsChanged))]
  private bool _isTesting = false;

  [SerializeField] private GameObject _howToPlayScreen;
  [SerializeField] private GameObject _splashImage;
  [SerializeField] private GameObject _debugButtons;

  private void OnEnable()
  {
    _howToPlayScreen.SetActive(false);
    _splashImage.SetActive(_isSplashImagePresent);
    _debugButtons.SetActive(_isTesting);
  }

  public void StartGame()
  {
    SceneManager.LoadScene("World Map");
  }

  public void HowToPlay()
  {
    if (!_howToPlayScreen.activeSelf)
    {
      _howToPlayScreen.SetActive(true);
      if (_splashImage.activeSelf) _splashImage.SetActive(false);
    }
    else
    {
      _howToPlayScreen.SetActive(false);
      _splashImage.SetActive(_isSplashImagePresent);
    }

  }
  public void ExitGame()
  {
    Debug.Log("Quitting the game :C");
    Application.Quit();
  }

  public void LoadSethScene()
  {
    SceneManager.LoadScene("Stal_prototyping_bramble_system");
  }

  public void LoadAshScene()
  {
    SceneManager.LoadScene("Ash_Sandbox");
  }

  private void OnSettingsChanged()
  {
    _splashImage.SetActive(_isSplashImagePresent);
    _debugButtons.SetActive(_isTesting);
  }
}
