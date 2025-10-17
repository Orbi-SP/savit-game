using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Api : MonoBehaviour
{
    [Header("Configurações API")]
    public string apiURL = "http://127.0.0.1:5000";
    public CameraFeed cameraFeed;

    [Header("Objeto a controlar (apenas se a cena exigir)")]
    public GameObject objectToMove; // use apenas em cenas que o Api controla (ex.: RAM)
    public enum SceneType { RAM, Gabinete, OutraCena }
    public SceneType currentScene = SceneType.Gabinete;

    // ==== Saídas de gesto para outros scripts ====
    private bool isHolding = false;
    private string currentSide = "center";
    public bool IsHolding => isHolding;          // true = mão fechada
    public string CurrentSide => currentSide;    // "left" | "center" | "right"

    // ==== Estado interno (só usado se o Api controla o objeto) ====
    private Vector3 originalPos;
    private Quaternion originalRot;
    private float accumulatedZ;
    private bool driveTransform;   // se o Api deve mexer no transform (ex.: cena RAM)

    void Start()
    {
        // Define se o Api vai dirigir o transform nesta cena
        driveTransform = (currentScene == SceneType.RAM);

        if (driveTransform && objectToMove != null)
        {
            originalPos = objectToMove.transform.position;
            originalRot = objectToMove.transform.rotation;
            accumulatedZ = originalPos.z;
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

            var frame = cameraFeed.GetCurrentFrame();
            if (frame == null)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            byte[] imageBytes = frame.EncodeToJPG();
            Destroy(frame);
            Destroy(frame);

            var www = new UnityWebRequest(apiURL, "POST");
            www.uploadHandler = new UploadHandlerRaw(imageBytes);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/octet-stream");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string response = www.downloadHandler.text.Trim().ToLower();
                // exemplos: "hold left", "free right", "free center"
                string[] parts = response.Split(' ');

                if (parts.Length >= 2)
                {
                    isHolding = (parts[0] == "hold");
                    currentSide = parts[1]; // "left"|"center"|"right"
                }
                else
                {
                    isHolding = (response == "hold");
                    currentSide = "center";
                }
            }
            else
            {
                Debug.LogWarning("Erro na API: " + www.error);
            }

            yield return new WaitForSeconds(0.5f);
            yield return new WaitForSeconds(0.5f);
        }
    }

    void Update()
    {
        // 👉 Se NÃO for cena RAM, o Api NÃO mexe em transform (evita briga com MotherboardPlacer)
        if (!driveTransform) return;
        if (objectToMove == null) return;

        // Daqui pra baixo é só pra cena RAM (exemplo antigo)
        // Movimento simples no eixo Z + elevação quando segurar
        if (currentSide == "center")
            accumulatedZ = Mathf.Lerp(accumulatedZ, originalPos.z, Time.deltaTime * 5f);
        else if (currentSide == "right")
            accumulatedZ += 0.3f;
        else if (currentSide == "left")
            accumulatedZ -= 0.3f;

        float targetY = isHolding ? 5.5f : originalPos.y;
        Vector3 targetPos = new Vector3(originalPos.x, targetY, accumulatedZ);

        objectToMove.transform.position =
            Vector3.Lerp(objectToMove.transform.position, targetPos, Time.deltaTime * 5f);

        // Se quiser rotação especial só na cena RAM, deixe aqui.
        Quaternion targetRot = isHolding
            ? Quaternion.Euler(-90f, originalRot.eulerAngles.y, originalRot.eulerAngles.z)
            : originalRot;

        objectToMove.transform.rotation =
            Quaternion.Lerp(objectToMove.transform.rotation, targetRot, Time.deltaTime * 5f);
    }
}