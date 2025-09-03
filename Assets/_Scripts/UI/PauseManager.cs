using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [SerializeField, Expandable] private PlayerEventDataSO _playerEventData;


    void OnEnable()
    {
        _playerEventData.Pause.OnEventRaised += OnPause;
    }

    // Update is called once per frame
    void OnDisable()
    {

    }



    private void OnPause(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // stuff do when paused
        }
    }


}
