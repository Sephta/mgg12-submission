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
  [SerializeField] private GameObject _startMenuButtons;
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
    _howToPlayScreen.SetActive(true);
    _startMenuButtons.SetActive(false);
    if (_splashImage.activeSelf) _splashImage.SetActive(false);
  }

  public void ExitControls()
  {
    _howToPlayScreen.SetActive(false);
    _startMenuButtons.SetActive(true);
    _splashImage.SetActive(_isSplashImagePresent);
  }


  public void ExitGame()
  {
    Debug.Log("Quitting the game :C");
    Application.Quit();
  }

  public void LoadSethScene()
  {
    SceneManager.LoadScene("stal_Sandbox");
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
