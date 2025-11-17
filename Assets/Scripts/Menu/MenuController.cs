using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Login Configuration")]
    [SerializeField] private ArduinoLoginController arduinoLoginController;

    private void Start()
    {
        // Verifica se o ArduinoLoginController está configurado
        if (arduinoLoginController == null)
        {
            Debug.LogWarning("[MenuController] ArduinoLoginController não está atribuído! Procurando na cena...");
            arduinoLoginController = FindObjectOfType<ArduinoLoginController>();
            
            if (arduinoLoginController == null)
            {
                Debug.LogError("[MenuController] ArduinoLoginController não encontrado na cena!");
            }
            else
            {
                Debug.Log("[MenuController] ArduinoLoginController encontrado automaticamente.");
            }
        }
    }

    public void StartGame()
    {
        Debug.Log("[MenuController] Botão 'Jogar' clicado!");
        
        // Ativa o sistema de login com Arduino ao invés de carregar a cena diretamente
        if (arduinoLoginController != null)
        {
            Debug.Log("[MenuController] Iniciando sistema de login Arduino...");
            arduinoLoginController.StartListening();
        }
        else
        {
            Debug.LogError("[MenuController] ArduinoLoginController não está atribuído no Inspector!");
            Debug.LogWarning("[MenuController] Carregando jogo diretamente como fallback...");
            // Fallback: carrega a cena diretamente se o controller não estiver configurado
            SceneManager.LoadScene("BRP Sample SceneGabinete");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("[MenuController] Jogo encerrado"); // Para testes no editor
    }
}
