using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{

    [SerializeField] private GameObject _pausePanel;
    [SerializeField, Expandable] private PlayerEventDataSO _playerEventData;


    [SerializeField, ReadOnly] bool _isPaused = false;


    private void OnAwake()
    {
        if (_playerEventData == null)
        {
            Debug.LogError("PlayerEventDataSO required for pause menu.");
        }
    }

    private void OnEnable()
    {
        _playerEventData.Pause.OnEventRaised += OnPause;
        _isPaused = false;  // I know redundant, but I'm paranoid
        _pausePanel.SetActive(false);
    }

    // Update is called once per frame
    private void OnDisable()
    {
        _isPaused = false;
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
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1;
        _pausePanel.SetActive(false);
        _isPaused = false;
    }


}
