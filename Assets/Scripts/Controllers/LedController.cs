// ===== SCRIPT 2: LEDController.cs =====
// Adjunta este script al GameObject que representa tu LED (puede ser un Cube o Sphere)

using UnityEngine;

public class LEDController : MonoBehaviour
{
    [Header("LED Visual Settings")]
    public Color offColor = Color.gray;
    public Color onColor = Color.red;
    public float glowIntensity = 2.0f;
    
    [Header("Components")]
    private Renderer ledRenderer;
    private Light ledLight;
    
    [Header("Animation")]
    public bool smoothTransition = true;
    public float transitionSpeed = 5.0f;
    
    private bool currentState = false;
    private Color targetColor;
    
    void Start()
    {
        // Obtener componentes
        ledRenderer = GetComponent<Renderer>();
        ledLight = GetComponent<Light>();
        
        // Crear luz si no existe
        if (ledLight == null)
        {
            ledLight = gameObject.AddComponent<Light>();
            ledLight.type = LightType.Point;
            ledLight.range = 3.0f;
            ledLight.intensity = 0;
        }
        
        // Configurar material emisivo si no existe
        if (ledRenderer != null && ledRenderer.material != null)
        {
            // Habilitar emisión en el material
            ledRenderer.material.EnableKeyword("_EMISSION");
        }
        
        // Inicializar LED apagado
        SetLEDState(false);
        targetColor = offColor;
    }
    
    void Update()
    {
        // Transición suave de colores
        if (smoothTransition && ledRenderer != null)
        {
            Color currentColor = ledRenderer.material.color;
            Color newColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * transitionSpeed);
            
            ledRenderer.material.color = newColor;
            
            if (ledRenderer.material.HasProperty("_EmissionColor"))
            {
                Color emissionColor = currentState ? onColor * glowIntensity : Color.black;
                ledRenderer.material.SetColor("_EmissionColor", 
                    Color.Lerp(ledRenderer.material.GetColor("_EmissionColor"), emissionColor, 
                    Time.deltaTime * transitionSpeed));
            }
        }
    }
    
    public void SetLEDState(bool state)
    {
        currentState = state;
        targetColor = state ? onColor : offColor;
        
        // Actualizar renderer
        if (ledRenderer != null)
        {
            if (!smoothTransition)
            {
                ledRenderer.material.color = targetColor;
                
                if (ledRenderer.material.HasProperty("_EmissionColor"))
                {
                    Color emissionColor = state ? onColor * glowIntensity : Color.black;
                    ledRenderer.material.SetColor("_EmissionColor", emissionColor);
                }
            }
        }
        
        // Actualizar luz
        if (ledLight != null)
        {
            ledLight.enabled = state;
            ledLight.color = onColor;
            ledLight.intensity = state ? 1.0f : 0.0f;
        }
        
        Debug.Log("LED " + (state ? "ENCENDIDO" : "APAGADO"));
    }
    
    public bool GetLEDState()
    {
        return currentState;
    }
    
    // Método para cambiar colores desde el inspector o código
    public void SetLEDColors(Color newOffColor, Color newOnColor)
    {
        offColor = newOffColor;
        onColor = newOnColor;
        targetColor = currentState ? onColor : offColor;
    }
}

