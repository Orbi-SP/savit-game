using UnityEngine;

public class CameraFeed : MonoBehaviour
{
    private WebCamTexture webcamTexture;

    void Start()
    {
        // Inicia a câmera padrão do dispositivo
        webcamTexture = new WebCamTexture();

        // Aplica o vídeo da webcam como textura no material do objeto
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.mainTexture = webcamTexture;
        }

        webcamTexture.Play();
    }

    void OnDisable()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }
    }
}
