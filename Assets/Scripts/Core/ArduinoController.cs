// ===== SCRIPT 1: ArduinoSimulator.cs =====
// Adjunta este script a un GameObject vacío llamado "ArduinoController"

using UnityEngine;
using System.Collections;

public class ArduinoSimulator : MonoBehaviour
{
    [Header("LED Configuration")]
    public LEDController ledController;
    public int ledPin = 13;
    
    [Header("Simulation Settings")]
    public bool autoMode = false;
    public float blinkInterval = 1.0f;
    
    [Header("Manual Controls")]
    public KeyCode toggleKey = KeyCode.Space;
    public KeyCode blinkKey = KeyCode.B;
    
    private bool ledState = false;
    private bool isBlinking = false;
    
    void Start()
    {
        // Inicializar el LED
        if (ledController == null)
        {
            ledController = FindObjectOfType<LEDController>();
        }
        
        if (ledController != null)
        {
            ledController.SetLEDState(false);
        }
        
        Debug.Log("Arduino Simulator iniciado - Pin LED: " + ledPin);
        Debug.Log("Controles: [ESPACIO] = Toggle LED, [B] = Blink, [A] = Auto Mode");
        
        // Iniciar modo automático si está habilitado
        if (autoMode)
        {
            StartCoroutine(AutoBlinkRoutine());
        }
    }
    
    void Update()
    {
        // Control manual con teclado
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleLED();
        }
        
        if (Input.GetKeyDown(blinkKey))
        {
            StartCoroutine(BlinkLED());
        }
        
        if (Input.GetKeyDown(KeyCode.A))
        {
            ToggleAutoMode();
        }
    }
    
    // Simula digitalWrite(pin, HIGH/LOW)
    public void DigitalWrite(int pin, bool state)
    {
        if (pin == ledPin && ledController != null)
        {
            ledController.SetLEDState(state);
            ledState = state;
            Debug.Log($"digitalWrite({pin}, {(state ? "HIGH" : "LOW")})");
        }
    }
    
    // Simula digitalRead(pin)
    public bool DigitalRead(int pin)
    {
        if (pin == ledPin)
        {
            return ledState;
        }
        return false;
    }
    
    public void ToggleLED()
    {
        if (!isBlinking)
        {
            ledState = !ledState;
            DigitalWrite(ledPin, ledState);
        }
    }
    
    public IEnumerator BlinkLED()
    {
        if (isBlinking) yield break;
        
        isBlinking = true;
        
        // Parpadear 5 veces
        for (int i = 0; i < 5; i++)
        {
            DigitalWrite(ledPin, true);
            yield return new WaitForSeconds(0.2f);
            DigitalWrite(ledPin, false);
            yield return new WaitForSeconds(0.2f);
        }
        
        isBlinking = false;
    }
    
    public void ToggleAutoMode()
    {
        autoMode = !autoMode;
        Debug.Log("Auto Mode: " + (autoMode ? "ON" : "OFF"));
        
        if (autoMode)
        {
            StartCoroutine(AutoBlinkRoutine());
        }
    }
    
    private IEnumerator AutoBlinkRoutine()
    {
        while (autoMode)
        {
            DigitalWrite(ledPin, true);
            yield return new WaitForSeconds(blinkInterval);
            DigitalWrite(ledPin, false);
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
