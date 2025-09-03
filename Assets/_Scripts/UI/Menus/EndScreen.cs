using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
  public void StartGame()
  {
    SceneManager.LoadScene("World Map");
  }

  public void ExitGame()
  {
    Debug.Log("Quitting the game :C");
    Application.Quit();
  }
}
