using UnityEngine;

public class CameraFeed : MonoBehaviour
{
    private WebCamTexture webcamTexture;
    public Vector2 size = new Vector2(0.9f, 0.6f); // Largura x Altura em "tela"
    private Camera mainCamera;

    void Start()
{
    mainCamera = Camera.main;

    // Verifica se há câmera disponível
    if (WebCamTexture.devices.Length == 0)
    {
        Debug.LogError("Nenhuma webcam detectada!");
        return;
    }

    webcamTexture = new WebCamTexture();
    webcamTexture.Play();

    Debug.Log("Webcam iniciada: " + webcamTexture.deviceName);

    Renderer renderer = GetComponent<Renderer>();
    renderer.material.mainTexture = webcamTexture;

    PositionInCorner();
}


    void PositionInCorner()
{
    if (mainCamera == null) return;

    Vector3 viewportPosition = new Vector3(0f, 1f, 1f);
    Vector3 worldPosition = mainCamera.ViewportToWorldPoint(viewportPosition);

    transform.position = worldPosition;

    // Aplica inversão horizontal (eixo X negativo)
    transform.localScale = new Vector3(-size.x, size.y, 1f); 

    transform.rotation = mainCamera.transform.rotation;
}

    void OnDisable()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }
    }
}
