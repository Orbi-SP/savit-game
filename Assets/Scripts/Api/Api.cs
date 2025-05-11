using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Api : MonoBehaviour
{
    public string apiURL = "http://127.0.0.1:5000"; // Altere aqui
    public CameraFeed cameraFeed;
    public GameObject ramObject;

    // Variáveis para armazenar a posição e rotação originais do objeto
    private Vector3 originalRamPosition;
    private Quaternion originalRamRotation;

    // Variável que controla se o gesto é HOLD ou FREE
    private bool isHolding = false;

    void Start()
    {
        if (ramObject != null)
        {
            originalRamPosition = ramObject.transform.position;
            originalRamRotation = ramObject.transform.rotation;
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
                string response = www.downloadHandler.text.Trim();
                Debug.Log("Resposta da API: " + response);
                isHolding = response.ToLower() == "hold";
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
        if (ramObject == null) return;

        // Define posição e rotação alvo com base na resposta da API:
        // Se HOLD: posição Y = 5.5 e rotação X = 90 graus (mantendo os demais componentes originais)
        // Se FREE: mantém a posição e rotação originais.
        Vector3 targetPos = isHolding 
            ? new Vector3(originalRamPosition.x, 5.5f, originalRamPosition.z)
            : originalRamPosition;

        Quaternion targetRot = isHolding 
            ? Quaternion.Euler(-90f, originalRamRotation.eulerAngles.y, originalRamRotation.eulerAngles.z)
            : originalRamRotation;

        // Transição suave para a nova posição e rotação
        ramObject.transform.position = Vector3.Lerp(ramObject.transform.position, targetPos, Time.deltaTime * 5f);
        ramObject.transform.rotation = Quaternion.Lerp(ramObject.transform.rotation, targetRot, Time.deltaTime * 5f);
    }
}
