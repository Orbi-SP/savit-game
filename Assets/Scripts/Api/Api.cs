using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Api : MonoBehaviour
{
    public string apiURL = "http://127.0.0.1:5000"; // Altere conforme necessário
    public CameraFeed cameraFeed;
    public GameObject ramObject;

    // Armazena a posição e a rotação originais para referência
    private Vector3 originalRamPosition;
    private Quaternion originalRamRotation;

    // Controle do gesto: isHolding indica "hold" ou "free", currentSide indica a direção ("left", "center", "right")
    private bool isHolding = false;
    private string currentSide = "center";

    // Deslocamento acumulado no eixo Z, atualizado conforme a resposta da API
    private float accumulatedZ;

    // Flag que indica se a RAM já foi encaixada (snap) no slot
    private bool isSnapped = false;

    void Start()
    {
        if (ramObject != null)
        {
            originalRamPosition = ramObject.transform.position;
            originalRamRotation = ramObject.transform.rotation;
            accumulatedZ = originalRamPosition.z;
        }

        StartCoroutine(SendToApiRoutine());
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
            Destroy(frame); // Libera a memória

            UnityWebRequest www = new UnityWebRequest(apiURL, "POST");
            www.uploadHandler = new UploadHandlerRaw(imageBytes);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/octet-stream");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Exemplo de resposta: "hold left", "free right" ou "free center"
                string response = www.downloadHandler.text.Trim().ToLower();
                Debug.Log("Resposta da API: " + response);

                string[] parts = response.Split(' ');
                if (parts.Length >= 2)
                {
                    isHolding = parts[0] == "hold";
                    string newSide = parts[1]; // "left", "center" ou "right"
                    currentSide = newSide;

                    // Atualiza o deslocamento acumulado no eixo Z com base na direção:
                    // Se for "right": incrementa (movimenta para a direita)
                    // Se for "left": decrementa (movimenta para a esquerda)
                    if (newSide == "right")
                    {
                        accumulatedZ += 0.3f;
                    }
                    else if (newSide == "left")
                    {
                        accumulatedZ -= 0.3f;
                    }
                    // Se for "center", o Lerp no Update retornará suavemente ao valor original.
                }
                else
                {
                    // Caso a resposta seja apenas "hold" ou "free", assume "center" para a direção
                    isHolding = response == "hold";
                    currentSide = "center";
                }
            }
            else
            {
                Debug.LogWarning("Erro na API: " + www.error);
            }

            yield return new WaitForSeconds(0.5f); // Intervalo entre requisições
        }
    }

    void Update()
    {
        if (ramObject == null)
            return;

        // Se o objeto já foi encaixado, não atualizamos sua posição
        if (isSnapped)
            return;

        // Se a resposta for "center", suavemente retorna o acumulado no eixo Z ao valor original.
        if (currentSide == "center")
        {
            accumulatedZ = Mathf.Lerp(accumulatedZ, originalRamPosition.z, Time.deltaTime * 5f);
        }

        // Define a posição alvo:
        // - O eixo X permanece o da posição original.
        // - O eixo Y é definido para 5.5 se estiver em hold; caso contrário, permanece o valor original.
        // - O eixo Z utiliza o deslocamento acumulado.
        float targetY = isHolding ? 5.5f : originalRamPosition.y;
        Vector3 targetPos = new Vector3(originalRamPosition.x, targetY, accumulatedZ);

        // Verifica se a posição alvo está entre Z = -17 e Z = -12
        if (!isHolding && targetPos.z >= -17f && targetPos.z <= -12f)
        {
            // Ao detectar que o objeto está "free" e dentro do intervalo,
            // realiza o snap definindo a posição X = -38.5 e Z = -16, com rotação X = -90°.
            Vector3 snapPos = new Vector3(-38.5f, targetPos.y, -16f);
            Quaternion snapRot = Quaternion.Euler(-90f, originalRamRotation.eulerAngles.y, originalRamRotation.eulerAngles.z);
            ramObject.transform.position = snapPos;
            ramObject.transform.rotation = snapRot;
            isSnapped = true;
            Debug.Log("Snap acionado automaticamente: posição X = -38.5 e Z = -16.");
            return;
        }

        // Atualiza o objeto com transição suave para a posição e rotação alvo
        ramObject.transform.position = Vector3.Lerp(ramObject.transform.position, targetPos, Time.deltaTime * 5f);

        Quaternion targetRot = isHolding
            ? Quaternion.Euler(-90f, originalRamRotation.eulerAngles.y, originalRamRotation.eulerAngles.z)
            : originalRamRotation;

        ramObject.transform.rotation = Quaternion.Lerp(ramObject.transform.rotation, targetRot, Time.deltaTime * 5f);
    }
}
