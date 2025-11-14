using UnityEngine;

namespace SavitGame.OS {
    public class MonitorClickInteraction : MonoBehaviour {
        [Header("References")]
        public GameObject osCanvas;
        public GameObject desktop;
        public TCPIPPropertiesController tcpipController;
        
        [Header("Interaction Settings")]
        public Camera interactionCamera;
        public float raycastDistance = 100f;
        
        private bool isWindowOpen = false;
        
        private void Start() {
            if (interactionCamera == null) {
                interactionCamera = Camera.main;
            }
            
            // Log do estado inicial
            Debug.Log("=== MONITOR CLICK INTERACTION - START ===");
            Debug.Log($"OSCanvas: {(osCanvas != null ? (osCanvas.activeSelf ? "ATIVO" : "DESATIVADO") : "NULL")}");
            Debug.Log($"Desktop: {(desktop != null ? (desktop.activeSelf ? "ATIVO" : "DESATIVADO") : "NULL")}");
            Debug.Log($"TCPIPPropertiesWindow: {(tcpipController != null && tcpipController.propertiesWindow != null ? (tcpipController.propertiesWindow.activeSelf ? "ATIVO" : "DESATIVADO") : "NULL")}");
            Debug.Log($"isWindowOpen: {isWindowOpen}");
        }
        
        private void Update() {
            // Detecta clique no monitor usando Raycast
            if (Input.GetMouseButtonDown(0) && !isWindowOpen) {
                CheckMonitorClick();
            }
            
            // Permite fechar com ESC
            if (isWindowOpen && Input.GetKeyDown(KeyCode.Escape)) {
                CloseWindow();
            }
        }
        
        private void CheckMonitorClick() {
            if (interactionCamera == null) {
                Debug.LogWarning("Camera não atribuída!");
                return;
            }
            
            Ray ray = interactionCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red, 1f);
            
            if (Physics.Raycast(ray, out hit, raycastDistance)) {
                Debug.Log($"Raycast atingiu: {hit.collider.gameObject.name}");
                
                // Verifica se acertou este monitor ou algum filho dele
                if (hit.collider.gameObject == gameObject || 
                    hit.collider.transform.IsChildOf(transform)) {
                    Debug.Log($"Monitor clicado! isWindowOpen atual: {isWindowOpen}");
                    
                    if (!isWindowOpen) {
                        OpenWindow();
                    } else {
                        Debug.Log("Janela já está aberta, ignorando clique");
                    }
                }
            }
        }
        
        private void OpenWindow() {
            if (tcpipController == null) {
                Debug.LogWarning("TCPIPPropertiesController não está atribuído!");
                return;
            }
            
            Debug.Log($"=== ANTES DE ABRIR ===");
            Debug.Log($"OSCanvas ativo: {(osCanvas != null ? osCanvas.activeSelf.ToString() : "NULL")}");
            Debug.Log($"Desktop ativo: {(desktop != null ? desktop.activeSelf.ToString() : "NULL")}");
            Debug.Log($"TCPIPPropertiesWindow ativo: {(tcpipController.propertiesWindow != null ? tcpipController.propertiesWindow.activeSelf.ToString() : "NULL")}");
            Debug.Log($"isWindowOpen: {isWindowOpen}");
            
            // PRIMEIRO ativa o Canvas
            if (osCanvas != null) {
                osCanvas.SetActive(true);
                Debug.Log($"OSCanvas ativado → agora está: {osCanvas.activeSelf}");
            }
            
            // Garantir que Desktop está ativo (caso tenha sido desativado)
            if (desktop != null) {
                desktop.SetActive(true);
                Debug.Log($"Desktop ativado → agora está: {desktop.activeSelf}");
            }
            
            // DEPOIS mostra a janela
            isWindowOpen = true;
            tcpipController.Show();
            Debug.Log($"TCPIPPropertiesWindow.Show() chamado → agora está: {tcpipController.propertiesWindow.activeSelf}");
            
            // Mostrar cursor para poder interagir com a UI
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            Debug.Log("=== Janela TCP/IP aberta - Pressione ESC para fechar ===");
        }
        
        private void CloseWindow() {
            if (tcpipController == null) return;
            
            Debug.Log($"=== FECHANDO JANELA ===");
            
            isWindowOpen = false;
            tcpipController.Hide();
            Debug.Log($"TCPIPPropertiesWindow.Hide() chamado → agora está: {tcpipController.propertiesWindow.activeSelf}");
            
            // Desativa o Canvas também
            if (osCanvas != null) {
                osCanvas.SetActive(false);
                Debug.Log($"OSCanvas desativado → agora está: {osCanvas.activeSelf}");
            }
            
            Debug.Log("=== Janela TCP/IP fechada ===");
        }
        
        // Método alternativo usando OnMouseDown (requer Collider no monitor)
        private void OnMouseDown() {
            Debug.Log($"OnMouseDown chamado! isWindowOpen: {isWindowOpen}");
            if (!isWindowOpen) {
                Debug.Log("OnMouseDown - Monitor clicado! Abrindo janela...");
                OpenWindow();
            } else {
                Debug.Log("OnMouseDown - Janela já aberta, ignorando");
            }
        }
    }
}
