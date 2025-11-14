using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SavitGame.OS.Network;

namespace SavitGame.OS {
    public class TCPIPPropertiesController : MonoBehaviour {
        [Header("Window")]
        public GameObject propertiesWindow;
        
        [Header("IP Settings")]
        public Toggle dhcpToggle;
        public Toggle staticIPToggle;
        public TMP_InputField ipAddressInput;
        public TMP_InputField subnetMaskInput;
        public TMP_InputField defaultGatewayInput;
        
        [Header("DNS Settings")]
        public Toggle autoDNSToggle;
        public Toggle manualDNSToggle;
        public TMP_InputField preferredDNSInput;
        public TMP_InputField alternateDNSInput;
        
        [Header("Buttons")]
        public Button okButton;
        public Button cancelButton;
        public Button advancedButton;
        
        [Header("Validation")]
        public TextMeshProUGUI validationMessage;
        public Color validColor = Color.green;
        public Color invalidColor = Color.red;
        
        [Header("References")]
        public WindowsNetworkConfig networkConfig;
        
        private NetworkSettings tempSettings;
        
        private void Start() {
            SetupButtons();
            SetupToggles();
            // Comentado: deixe o estado inicial ser controlado pela Hierarchy
            // Hide();
        }
        
        private void SetupButtons() {
            if (okButton != null) {
                okButton.onClick.AddListener(OnOKClicked);
            }
            
            if (cancelButton != null) {
                cancelButton.onClick.AddListener(OnCancelClicked);
            }
            
            if (advancedButton != null) {
                advancedButton.onClick.AddListener(OnAdvancedClicked);
            }
        }
        
        private void SetupToggles() {
            if (dhcpToggle != null) {
                dhcpToggle.onValueChanged.AddListener(OnDHCPToggled);
            }
            
            if (staticIPToggle != null) {
                staticIPToggle.onValueChanged.AddListener(OnStaticIPToggled);
            }
            
            if (autoDNSToggle != null) {
                autoDNSToggle.onValueChanged.AddListener(OnAutoDNSToggled);
            }
            
            if (manualDNSToggle != null) {
                manualDNSToggle.onValueChanged.AddListener(OnManualDNSToggled);
            }
            
            // Add input field listeners for real-time validation
            AddInputValidation(ipAddressInput);
            AddInputValidation(subnetMaskInput);
            AddInputValidation(defaultGatewayInput);
            AddInputValidation(preferredDNSInput);
            AddInputValidation(alternateDNSInput);
        }
        
        private void AddInputValidation(TMP_InputField inputField) {
            if (inputField != null) {
                inputField.onValueChanged.AddListener(_ => ValidateInputs());
            }
        }
        
        public void Show() {
            if (propertiesWindow != null) {
                propertiesWindow.SetActive(true);
            }
        }
        
        public void Hide() {
            if (propertiesWindow != null) {
                propertiesWindow.SetActive(false);
            }
        }
        
        public void PopulateFields(NetworkSettings settings) {
            tempSettings = new NetworkSettings(settings);
            
            if (dhcpToggle != null && staticIPToggle != null) {
                dhcpToggle.isOn = settings.useDHCP;
                staticIPToggle.isOn = !settings.useDHCP;
            }
            
            if (ipAddressInput != null) {
                ipAddressInput.text = settings.ipAddress;
                ipAddressInput.interactable = !settings.useDHCP;
            }
            
            if (subnetMaskInput != null) {
                subnetMaskInput.text = settings.subnetMask;
                subnetMaskInput.interactable = !settings.useDHCP;
            }
            
            if (defaultGatewayInput != null) {
                defaultGatewayInput.text = settings.defaultGateway;
                defaultGatewayInput.interactable = !settings.useDHCP;
            }
            
            if (preferredDNSInput != null) {
                preferredDNSInput.text = settings.preferredDNS;
            }
            
            if (alternateDNSInput != null) {
                alternateDNSInput.text = settings.alternateDNS;
            }
            
            ValidateInputs();
        }
        
        private void OnDHCPToggled(bool isOn) {
            if (!isOn) return;
            
            if (ipAddressInput != null) ipAddressInput.interactable = false;
            if (subnetMaskInput != null) subnetMaskInput.interactable = false;
            if (defaultGatewayInput != null) defaultGatewayInput.interactable = false;
        }
        
        private void OnStaticIPToggled(bool isOn) {
            if (!isOn) return;
            
            if (ipAddressInput != null) ipAddressInput.interactable = true;
            if (subnetMaskInput != null) subnetMaskInput.interactable = true;
            if (defaultGatewayInput != null) defaultGatewayInput.interactable = true;
        }
        
        private void OnAutoDNSToggled(bool isOn) {
            if (!isOn) return;
            
            if (preferredDNSInput != null) preferredDNSInput.interactable = false;
            if (alternateDNSInput != null) alternateDNSInput.interactable = false;
        }
        
        private void OnManualDNSToggled(bool isOn) {
            if (!isOn) return;
            
            if (preferredDNSInput != null) preferredDNSInput.interactable = true;
            if (alternateDNSInput != null) alternateDNSInput.interactable = true;
        }
        
        private bool ValidateInputs() {
            bool isValid = true;
            string message = "";
            
            if (staticIPToggle != null && staticIPToggle.isOn) {
                if (!NetworkSettings.ValidateIPAddress(ipAddressInput?.text)) {
                    isValid = false;
                    message = "Invalid IP Address format";
                }
                else if (!NetworkSettings.ValidateIPAddress(subnetMaskInput?.text)) {
                    isValid = false;
                    message = "Invalid Subnet Mask format";
                }
                else if (!NetworkSettings.ValidateIPAddress(defaultGatewayInput?.text)) {
                    isValid = false;
                    message = "Invalid Default Gateway format";
                }
            }
            
            if (manualDNSToggle != null && manualDNSToggle.isOn) {
                if (!NetworkSettings.ValidateIPAddress(preferredDNSInput?.text)) {
                    isValid = false;
                    message = "Invalid Preferred DNS format";
                }
                else if (!string.IsNullOrEmpty(alternateDNSInput?.text) && 
                         !NetworkSettings.ValidateIPAddress(alternateDNSInput?.text)) {
                    isValid = false;
                    message = "Invalid Alternate DNS format";
                }
            }
            
            if (validationMessage != null) {
                validationMessage.text = message;
                validationMessage.color = isValid ? validColor : invalidColor;
            }
            
            if (okButton != null) {
                okButton.interactable = isValid;
            }
            
            return isValid;
        }
        
        private void OnOKClicked() {
            if (!ValidateInputs()) return;
            
            NetworkSettings newSettings = new NetworkSettings {
                useDHCP = dhcpToggle != null && dhcpToggle.isOn,
                ipAddress = ipAddressInput?.text ?? "",
                subnetMask = subnetMaskInput?.text ?? "",
                defaultGateway = defaultGatewayInput?.text ?? "",
                preferredDNS = preferredDNSInput?.text ?? "",
                alternateDNS = alternateDNSInput?.text ?? ""
            };
            
            if (networkConfig != null) {
                networkConfig.ApplyNetworkSettings(newSettings);
            }
            
            Hide();
        }
        
        private void OnCancelClicked() {
            if (networkConfig != null) {
                networkConfig.CancelNetworkSettings();
            }
            
            Hide();
        }
        
        private void OnAdvancedClicked() {
            Debug.Log("Advanced settings clicked");
        }
    }
}