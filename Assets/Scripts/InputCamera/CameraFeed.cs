using UnityEngine;

public class CameraFeed : MonoBehaviour
{
    private WebCamTexture webcamTexture;
    public Vector2 size = new Vector2(0.6f, 0.4f); // Largura x Altura em "tela"
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

        // Ponto no canto superior esquerdo (X=0, Y=1, Z=distância da câmera)
        Vector3 viewportPosition = new Vector3(0f, 1f, 1f); // z=1f é a profundidade

        // Converte para coordenadas do mundo
        Vector3 worldPosition = mainCamera.ViewportToWorldPoint(viewportPosition);

        // Ajusta posição do Quad
        transform.position = worldPosition;

        // Redimensiona para a tela
        transform.localScale = new Vector3(size.x, size.y, 1f);

        // Gira o Quad para ficar virado para a câmera
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
