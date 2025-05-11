using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Api : MonoBehaviour
{
    public string apiURL = "http://127.0.0.1:5000"; // Altere aqui
    public CameraFeed cameraFeed;
    public GameObject ramObject;
    public float liftAmount = 0.1f;

    private Vector3 originalRamPosition;
    private bool isHolding = false;

    void Start()
    {
        if (ramObject != null)
            originalRamPosition = ramObject.transform.position;

        StartCoroutine(SendToApiRoutine());
    }

    IEnumerator SendToApiRoutine()
    {
        while (true)
        {
            if (cameraFeed == null) yield return new WaitForSeconds(0.5f);

            Texture2D frame = cameraFeed.GetCurrentFrame();
            if (frame == null)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            byte[] imageBytes = frame.EncodeToJPG();
            Destroy(frame); // libera a memória

            UnityWebRequest www = new UnityWebRequest(apiURL, "POST");
            www.uploadHandler = new UploadHandlerRaw(imageBytes);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/octet-stream");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success){
                string response = www.downloadHandler.text.Trim();
                Debug.Log("Resposta da API: " + response);
                isHolding = response.ToLower() == "hold";
            }
            else
            {
                Debug.LogWarning("Erro na API: " + www.error);
            }

            yield return new WaitForSeconds(0.5f); // intervalo entre requisições
        }
    }

    void Update()
    {
        if (ramObject == null) return;

        Vector3 targetPos = isHolding
            ? new Vector3(originalRamPosition.x, originalRamPosition.y + liftAmount, originalRamPosition.z)
            : originalRamPosition;

        ramObject.transform.position = Vector3.Lerp(ramObject.transform.position, targetPos, Time.deltaTime * 5f);
    }
}
