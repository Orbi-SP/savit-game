using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("BRP Sample Scene");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Jogo encerrado"); // Para testes no editor
    }
}
