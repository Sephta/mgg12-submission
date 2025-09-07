using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
  [SerializeField] private PlayerAbilityDataSO _playerArmsSO;
  [SerializeField] private PlayerHealthSO _playerHealthSO;

  public void StartGame()
  {
    ResetPlayerAttributes();
    SceneManager.LoadScene("World Map");
  }

  public void ExitGame()
  {
    Debug.Log("Quitting the game :C");
    Application.Quit();
  }

  private void ResetPlayerAttributes()
  {
    _playerArmsSO.ResetArms();
    _playerHealthSO.SetCurrentHealth(_playerHealthSO.MaxHealth);
  }


}
