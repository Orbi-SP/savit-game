using System;
using System.Collections;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArduinoLoginController : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private string listenPort = "5000";
    [SerializeField] private string expectedResult = "abc123"; // Resultado esperado do Arduino
    
    [Header("UI Elements (Opcional)")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private UnityEngine.UI.Text statusText;
    
    private HttpListener listener;
    private Thread listenerThread;
    private bool isListening = false;
    private bool loginAuthorized = false;

    public void StartListening()
    {
        if (isListening) return;

        if (loginPanel != null)
        {
            loginPanel.SetActive(true);
        }

        loginAuthorized = false;
        UpdateStatusText("Aguardando autorização do Arduino...");

        try
        {
            listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{listenPort}/");
            listener.Start();
            isListening = true;

            listenerThread = new Thread(ListenForRequests);
            listenerThread.IsBackground = true;
            listenerThread.Start();

            Debug.Log($"[ArduinoLogin] Servidor HTTP iniciado na porta {listenPort}");
            Debug.Log($"[ArduinoLogin] Aguardando requisição POST em http://localhost:{listenPort}/login");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ArduinoLogin] Erro ao iniciar servidor: {e.Message}");
            UpdateStatusText("Erro ao iniciar servidor!");
        }
    }

    private void ListenForRequests()
    {
        while (isListening && listener != null && listener.IsListening)
        {
            try
            {
                HttpListenerContext context = listener.GetContext();
                ProcessRequest(context);
            }
            catch (Exception e)
            {
                if (isListening)
                {
                    Debug.LogError($"[ArduinoLogin] Erro ao processar requisição: {e.Message}");
                }
            }
        }
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        string responseString = "";

        try
        {
            if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/login")
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string body = reader.ReadToEnd();
                    Debug.Log($"[ArduinoLogin] Requisição recebida: {body}");

                    // Parseia o JSON e verifica se contém "abc123"
                    string result = ParseResult(body);
                    
                    if (result == expectedResult)
                    {
                        loginAuthorized = true;
                        responseString = "{\"status\":\"authorized\",\"message\":\"Login autorizado\"}";
                        Debug.Log("[ArduinoLogin] ✓ Login autorizado!");
                    }
                    else
                    {
                        responseString = "{\"status\":\"unauthorized\",\"message\":\"Resultado inválido\"}";
                        Debug.Log($"[ArduinoLogin] ✗ Login não autorizado. Recebido: {result}, Esperado: {expectedResult}");
                    }
                }
            }
            else if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/status")
            {
                responseString = "{\"status\":\"listening\",\"ready\":true}";
            }
            else
            {
                responseString = "{\"status\":\"error\",\"message\":\"Endpoint inválido\"}";
            }

            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            response.ContentType = "application/json";
            response.StatusCode = 200;
            
            // Adiciona headers CORS para permitir requisições externas
            response.AddHeader("Access-Control-Allow-Origin", "*");
            response.AddHeader("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
            response.AddHeader("Access-Control-Allow-Headers", "Content-Type");
            
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }
        catch (Exception e)
        {
            Debug.LogError($"[ArduinoLogin] Erro ao processar request: {e.Message}");
        }
        finally
        {
            response.Close();
        }
    }

    private string ParseResult(string json)
    {
        try
        {
            // Aceita tanto "result" quanto "password" como chave
            // Exemplo: {"result": "abc123"} ou {"password": "abc123"}
            
            // Tenta encontrar "result" primeiro
            int startIndex = json.IndexOf("\"result\"");
            
            // Se não encontrar "result", tenta "password"
            if (startIndex == -1)
            {
                startIndex = json.IndexOf("\"password\"");
            }
            
            if (startIndex == -1) return "";

            startIndex = json.IndexOf(":", startIndex) + 1;
            startIndex = json.IndexOf("\"", startIndex) + 1;
            
            int endIndex = json.IndexOf("\"", startIndex);
            
            if (endIndex > startIndex)
            {
                return json.Substring(startIndex, endIndex - startIndex);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[ArduinoLogin] Erro ao parsear JSON: {e.Message}");
        }
        return "";
    }

    private void Update()
    {
        // Verifica se foi autorizado e carrega o jogo
        if (loginAuthorized)
        {
            loginAuthorized = false; // Previne múltiplas execuções
            UpdateStatusText("Autorizado! Carregando jogo...");
            StartCoroutine(LoadGameAfterDelay(1.0f));
        }
    }

    private IEnumerator LoadGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StopListening();
        SceneManager.LoadScene("BRP Sample SceneGabinete");
    }

    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[ArduinoLogin] {message}");
    }

    public void StopListening()
    {
        isListening = false;

        if (listener != null && listener.IsListening)
        {
            listener.Stop();
            listener.Close();
        }

        if (listenerThread != null && listenerThread.IsAlive)
        {
            listenerThread.Join(1000);
        }

        Debug.Log("[ArduinoLogin] Servidor HTTP parado");
    }

    public void CancelLogin()
    {
        StopListening();
        if (loginPanel != null)
        {
            loginPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        StopListening();
    }

    private void OnApplicationQuit()
    {
        StopListening();
    }
}
