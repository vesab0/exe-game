using UnityEngine;
using UnityEngine.InputSystem;


public class gameManager : MonoBehaviour
{
void Update(){

    if (Keyboard.current.rKey.wasPressedThisFrame){

        // Reset the game
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
}