using UnityEngine;

namespace SavitGame.OS {
    public class OSCameraDebug : MonoBehaviour {
        public Camera osCamera;
        public RenderTexture renderTexture;
        
        private void Update() {
            if (Input.GetKeyDown(KeyCode.F1)) {
                DebugCameraInfo();
            }
        }
        
        private void DebugCameraInfo() {
            if (osCamera == null) {
                Debug.LogError("OSCamera não atribuída!");
                return;
            }
            
            Debug.Log($"=== DEBUG OSCamera ===");
            Debug.Log($"Camera habilitada: {osCamera.enabled}");
            Debug.Log($"Target Texture: {osCamera.targetTexture?.name ?? "NONE"}");
            Debug.Log($"Culling Mask: {LayerMask.LayerToName(osCamera.cullingMask)}");
            
            if (renderTexture != null) {
                Debug.Log($"RenderTexture: {renderTexture.name}");
                Debug.Log($"RenderTexture tamanho: {renderTexture.width}x{renderTexture.height}");
                Debug.Log($"RenderTexture criada: {renderTexture.IsCreated()}");
            }
            
            // Forçar renderização
            osCamera.Render();
            Debug.Log("Render() forçado!");
        }
    }
}
