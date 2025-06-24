// ===== SCRIPT 3: UIController.cs =====
// Adjunta este script a un Canvas para crear una interfaz de usuario

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("UI References")]
    public Button toggleButton;
    public Button blinkButton;
    public Button autoModeButton;
    public TextMeshProUGUI statusText;
    public Slider intensitySlider;
    
    [Header("Arduino Reference")]
    public ArduinoSimulator arduino;
    
    void Start()
    {
        // Encontrar Arduino si no está asignado
        if (arduino == null)
        {
            arduino = FindObjectOfType<ArduinoSimulator>();
        }
        
        // Configurar botones
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(() => arduino.ToggleLED());
        }
        
        if (blinkButton != null)
        {
            blinkButton.onClick.AddListener(() => StartCoroutine(arduino.BlinkLED()));
        }
        
        if (autoModeButton != null)
        {
            autoModeButton.onClick.AddListener(() => arduino.ToggleAutoMode());
        }
        
        if (intensitySlider != null)
        {
            intensitySlider.onValueChanged.AddListener(OnIntensityChanged);
            intensitySlider.value = arduino.ledController.glowIntensity;
        }
    }
    
    void Update()
    {
        // Actualizar texto de estado
        if (statusText != null && arduino != null && arduino.ledController != null)
        {
            bool ledState = arduino.ledController.GetLEDState();
            statusText.text = ledState ? "LED: ENCENDIDO" : "LED: APAGADO";
            statusText.color = ledState ? Color.green : Color.red;
        }
        
        // Actualizar texto del botón de auto mode
        if (autoModeButton != null && arduino != null)
        {
            Text buttonText = autoModeButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = arduino.autoMode ? "Auto: ON" : "Auto: OFF";
            }
        }
    }
    
    private void OnIntensityChanged(float value)
    {
        if (arduino != null && arduino.ledController != null)
        {
            arduino.ledController.glowIntensity = value;
        }
    }
}
