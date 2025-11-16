using UnityEngine;

namespace SavitGame.OS {
    /// <summary>
    /// Detecta cliques no monitor 3D e abre a interface em modo Overlay.
    /// MODO OVERLAY: UI aparece diretamente na tela (muito mais simples!)
    /// </summary>
    public class MonitorClickInteraction : MonoBehaviour {
        [Header("Overlay Manager")]
        [SerializeField] private OverlayUIManager overlayManager;
        
        [Header("Camera")]
        [SerializeField] private Camera mainCamera;
        
        private void Update() {
            // Detecta clique esquerdo do mouse
            if (Input.GetMouseButtonDown(0)) {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit)) {
                    // Verifica se clicou no monitor
                    if (hit.collider.gameObject == gameObject) {
                        Debug.Log($"🖱️ Clicou no monitor: {gameObject.name}");
                        OpenOverlay();
                    }
                }
            }
        }
        
        // Também funciona com OnMouseDown (mais simples)
        private void OnMouseDown() {
            Debug.Log($"🖱️ OnMouseDown no monitor: {gameObject.name}");
            OpenOverlay();
        }
        
        private void OpenOverlay() {
            if (overlayManager != null) {
                overlayManager.ShowUI();
            } else {
                Debug.LogError("❌ OverlayUIManager não está configurado!");
            }
        }
    }
}
