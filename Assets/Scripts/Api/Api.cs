using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Api : MonoBehaviour
{
    [Header("Configurações API")]
    public string apiURL = "http://127.0.0.1:5000"; // URL da API
    public CameraFeed cameraFeed;

    [Header("Objeto a controlar")]
    public GameObject objectToMove; // Pode ser RAM, placa-mãe, etc.
    public enum SceneType { RAM, Gabinete, OutraCena }
    public SceneType currentScene;

    // Posição e rotação originais do objeto
    private Vector3 originalObjectPosition;
    private Quaternion originalObjectRotation;

    // Controle do gesto
    private bool isHolding = false;
    private string currentSide = "center";
    public bool IsHolding => isHolding;
    public string CurrentSide => currentSide;

    // Deslocamento acumulado no eixo Z (ou eixo de movimento)
    private float accumulatedZ;

    // Flag de snap
    private bool isSnapped = false;

    void Start()
    {
        if (objectToMove != null)
        {
            // Define a posição e rotação inicial de acordo com a cena
            SetPositionBasedOnScene(currentScene);

            // Agora salva a posição e rotação originais
            originalObjectPosition = objectToMove.transform.position;
            originalObjectRotation = objectToMove.transform.rotation;

            // Inicializa o eixo de movimento corretamente
            accumulatedZ = originalObjectPosition.z;
        }

        StartCoroutine(SendToApiRoutine());
    }

    // Ajusta a posição do objeto dependendo da cena
    void SetPositionBasedOnScene(SceneType scene)
    {
        switch (scene)
        {
            case SceneType.RAM:
                objectToMove.transform.position = new Vector3(10f, 0f, 5f); // ajuste conforme necessário
                objectToMove.transform.rotation = Quaternion.identity; 
                Debug.Log("Posição da RAM: " + objectToMove.transform.position);
                break;
            case SceneType.Gabinete:
                objectToMove.transform.position = new Vector3(-61.99f, 4.61f, -13.35f);
                objectToMove.transform.rotation = Quaternion.identity; 
                Debug.Log("Posição do Gabinete: " + objectToMove.transform.position);
                break;
            case SceneType.OutraCena:
                objectToMove.transform.position = new Vector3(0f, 0f, 0f);
                objectToMove.transform.rotation = Quaternion.identity; 
                break;
        }
    }

    IEnumerator SendToApiRoutine()
    {
        while (true)
        {
            if (cameraFeed == null)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            Texture2D frame = cameraFeed.GetCurrentFrame();
            if (frame == null)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            byte[] imageBytes = frame.EncodeToJPG();
            Destroy(frame);

            UnityWebRequest www = new UnityWebRequest(apiURL, "POST");
            www.uploadHandler = new UploadHandlerRaw(imageBytes);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/octet-stream");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string response = www.downloadHandler.text.Trim().ToLower();
                Debug.Log("Resposta da API: " + response);

                string[] parts = response.Split(' ');
                if (parts.Length >= 2)
                {
                    isHolding = parts[0] == "hold";
                    string newSide = parts[1];
                    currentSide = newSide;

                    if (newSide == "right") accumulatedZ += 0.3f;
                    else if (newSide == "left") accumulatedZ -= 0.3f;
                }
                else
                {
                    isHolding = response == "hold";
                    currentSide = "center";
                }
            }
            else
            {
                Debug.LogWarning("Erro na API: " + www.error);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    void Update()
    {
        if (objectToMove == null || isSnapped) return;

        // Se a mão estiver "center", suaviza o retorno da posição
        if (currentSide == "center")
        {
            accumulatedZ = Mathf.Lerp(accumulatedZ, originalObjectPosition.z, Time.deltaTime * 5f);
        }

        float targetY = isHolding ? 5.5f : originalObjectPosition.y;
        Vector3 targetPos = new Vector3(originalObjectPosition.x, targetY, accumulatedZ);

        // Snap automático quando soltar a mão (opcional: ajuste limites)
        if (!isHolding && targetPos.z >= -17f && targetPos.z <= -12f)
        {
            Vector3 snapPos = new Vector3(-38.5f, targetPos.y, -16f);
            Quaternion snapRot = Quaternion.Euler(-90f, originalObjectRotation.eulerAngles.y, originalObjectRotation.eulerAngles.z);
            objectToMove.transform.position = snapPos;
            objectToMove.transform.rotation = snapRot;
            isSnapped = true;
            Debug.Log("Snap acionado automaticamente: posição X = -38.5 e Z = -16.");
            return;
        }

        // Transição suave para a posição alvo
        objectToMove.transform.position = Vector3.Lerp(objectToMove.transform.position, targetPos, Time.deltaTime * 5f);
        Quaternion targetRot = isHolding
            ? Quaternion.Euler(-90f, originalObjectRotation.eulerAngles.y, originalObjectRotation.eulerAngles.z)
            : originalObjectRotation;
        objectToMove.transform.rotation = Quaternion.Lerp(objectToMove.transform.rotation, targetRot, Time.deltaTime * 5f);
    }
}