using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SavitGame.OS.Network {
    public class WindowsNetworkConfig : MonoBehaviour {
        [Header("UI References")]
        public NCPAController ncpaController;
        public TCPIPPropertiesController tcpipController;
        
        [Header("Status Display")]
        public TextMeshProUGUI statusText;
        public Image statusIcon;
        public Color successColor = Color.green;
        public Color errorColor = Color.red;
        
        private NetworkSettings currentSettings;
        private NetworkSettings backupSettings;
        
        private void Start() {
            currentSettings = new NetworkSettings();
            currentSettings.LoadFromPlayerPrefs();
            backupSettings = new NetworkSettings(currentSettings);
            
            UpdateAllDisplays();
        }
        
        public void OpenNCPA() {
            if (ncpaController != null) {
                ncpaController.Show();
            }
            
            if (tcpipController != null) {
                tcpipController.Hide();
            }
        }
        
        public void OpenTCPIPProperties() {
            if (tcpipController != null) {
                tcpipController.Show();
                tcpipController.PopulateFields(currentSettings);
            }
            
            if (ncpaController != null) {
                ncpaController.Hide();
            }
        }
        
        public void ApplyNetworkSettings(NetworkSettings newSettings) {
            if (!newSettings.ValidateAll()) {
                ShowStatus("Invalid IP configuration!", true);
                return;
            }
            
            backupSettings = new NetworkSettings(currentSettings);
            currentSettings = new NetworkSettings(newSettings);
            currentSettings.SaveToPlayerPrefs();
            
            UpdateAllDisplays();
            ShowStatus("Network settings applied successfully!", false);
        }
        
        public void CancelNetworkSettings() {
            currentSettings = new NetworkSettings(backupSettings);
            UpdateAllDisplays();
            ShowStatus("Changes cancelled", false);
        }
        
        public NetworkSettings GetCurrentSettings() {
            return new NetworkSettings(currentSettings);
        }
        
        private void UpdateAllDisplays() {
            if (ncpaController != null) {
                ncpaController.UpdateNetworkInfo(currentSettings);
            }
        }
        
        private void ShowStatus(string message, bool isError) {
            if (statusText != null) {
                statusText.text = message;
                statusText.color = isError ? errorColor : successColor;
            }
            
            if (statusIcon != null) {
                statusIcon.color = isError ? errorColor : successColor;
            }
            
            CancelInvoke(nameof(ClearStatus));
            Invoke(nameof(ClearStatus), 3f);
        }
        
        private void ClearStatus() {
            if (statusText != null) {
                statusText.text = "";
            }
        }
    }
}