using UnityEngine;
using UnityEngine.SceneManagement;

public class EndLevelTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            Debug.Log("Congratulations!");
            SceneManager.LoadScene("EndScreen");
        }
    }

}
