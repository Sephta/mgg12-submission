using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{

  [SerializeField] private GameObject _pausePanel;
  [SerializeField] private GameObject _controlsPanel;
  [SerializeField] private GameObject _gamepadControls;
  [SerializeField] private GameObject _keyboardControls;
  [SerializeField] private GameObject _gamepadCheck;
  [SerializeField] private GameObject _gamepadX;
  [SerializeField] private GameObject _keyboardCheck;
  [SerializeField] private GameObject _keyboardX;

  [SerializeField, Expandable] private PlayerEventDataSO _playerEventData;
  [SerializeField] private PlayerAbilityDataSO _playerArmsSO;
  // [SerializeField] private GameObject _player;


  [SerializeField, ReadOnly] bool _isPaused = false;

  private bool _isShowingGamepadControls = true;
  private bool _isShowingKeyboardControls = false;

  private void Awake()
  {
    if (_playerEventData == null)
    {
      gameObject.SetActive(false);
    }
  }

  private void OnEnable()
  {
    if (_playerEventData != null)
    {
      _playerEventData.Pause.OnEventRaised += OnPause;
    }

    _isPaused = false;  // I know redundant, but I'm paranoid
    _pausePanel.SetActive(false);
    _controlsPanel.SetActive(false);
  }

  // Update is called once per frame
  private void OnDisable()
  {
    if (_playerEventData != null)
    {
      _playerEventData.Pause.OnEventRaised -= OnPause;
    }

    _isPaused = false;
    _pausePanel.SetActive(false);
    _controlsPanel.SetActive(false);
  }



  private void OnPause(InputAction.CallbackContext context)
  {
    if (context.started)
    {
      // if we're already paused, they probably want to unpause if they pressed it again
      if (_isPaused)
      {
        Resume();
      }
      else
      {
        Pause();
      }
    }
  }

  public void Pause()
  {
    Time.timeScale = 0;
    _pausePanel.SetActive(true);
    _isPaused = true;
  }

  public void Resume()
  {
    Time.timeScale = 1;
    _pausePanel.SetActive(false);
    _isPaused = false;
    if (_controlsPanel.activeSelf) _controlsPanel.SetActive(false);
  }

  public void Restart()
  {
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    Time.timeScale = 1;
    _pausePanel.SetActive(false);
    _isPaused = false;
    ResetPlayerArms();
  }

  public void ShowControls()
  {
    _pausePanel.SetActive(false);
    _controlsPanel.SetActive(true);
    SwapGamepadControlsVisibility(_isShowingGamepadControls);
    SwapMKBControlsVisibility(_isShowingKeyboardControls);
  }

  public void ExitControls()
  {
    _controlsPanel.SetActive(false);
    _pausePanel.SetActive(true);
  }


  // look this is how I made the UI look whatever lol
  public void ShowGamepadControls()
  {
    if (!_isShowingGamepadControls)
    {
      SwapGamepadControlsVisibility(true);
      SwapMKBControlsVisibility(false);
      _isShowingGamepadControls = true;
      _isShowingKeyboardControls = false;

    }
    else
    {
      SwapGamepadControlsVisibility(false);
      SwapMKBControlsVisibility(true);
      _isShowingGamepadControls = false;
      _isShowingKeyboardControls = true;
    }
  }

  public void ShowMKBControls()
  {
    Debug.Log("HELLO MKB");

    if (!_isShowingKeyboardControls)
    {
      SwapGamepadControlsVisibility(false);
      SwapMKBControlsVisibility(true);
      _isShowingGamepadControls = false;
      _isShowingKeyboardControls = true;

    }
    else
    {
      SwapGamepadControlsVisibility(true);
      SwapMKBControlsVisibility(false);
      _isShowingGamepadControls = true;
      _isShowingKeyboardControls = false;
    }
  }

  public void ReturnToStart()
  {
    ResetPlayerArms();
    SceneManager.LoadScene(0);
  }


  public void Quit()
  {
    Debug.Log("Quitting the game :C");
    Application.Quit();
  }


  private void SwapGamepadControlsVisibility(bool selectShow)
  {
    _gamepadControls.SetActive(selectShow);
    _gamepadCheck.SetActive(selectShow);
    _gamepadX.SetActive(!selectShow);
  }

  private void SwapMKBControlsVisibility(bool selectShow)
  {
    _keyboardControls.SetActive(selectShow);
    _keyboardCheck.SetActive(selectShow);
    _keyboardX.SetActive(!selectShow);
  }

  // private void ResetPlayerAttributes()
  // {
  //   _player.GetComponent<PlayerAbilityDataSO>().ResetArms();
  //   PlayerHealthSO playerHealth = _player.GetComponent<PlayerHealthSO>();
  //   playerHealth.SetCurrentHealth(playerHealth.MaxHealth);
  // }

  private void ResetPlayerArms()
  {
    _playerArmsSO.ResetArms();
  }




}
