using System;
using System.Collections;
using UnityEngine;

public class AVRSimulator : MonoBehaviour
{
    [Header("Configuración del Simulador")]
    public HexFileParser hexParser;
    public LEDController ledController;
    public float clockFrequency = 16000000f; // 16 MHz como Arduino Uno
    public bool autoRun = true;
    public int instructionsPerFrame = 100;
    
    [Header("Estado del Procesador")]
    public ushort programCounter = 0;
    public byte[] registers = new byte[32]; // R0-R31
    public ushort stackPointer = 0x08FF; // Típico para Arduino Uno
    public byte statusRegister = 0; // SREG
    
    [Header("Memoria de Datos y Puertos I/O")]
    public byte[] sram = new byte[2048]; // 2KB SRAM típico
    public byte[] eeprom = new byte[1024]; // 1KB EEPROM típico
    public byte[] ioRegisters = new byte[64]; // Registros I/O (0x20-0x5F)
    
    [Header("Debug")]
    public bool enableDebug = true;
    public bool stepByStep = false;
    public int maxInstructions = 100000;
    
    private bool isRunning = false;
    private int instructionCount = 0;
    private float delayCounter = 0;
    private bool inDelay = false;
    
    // Direcciones importantes de Arduino Uno
    private const byte PORTB = 0x05; // Puerto B (pin 13 = bit 5)
    private const byte DDRB = 0x04;  // Dirección de datos Puerto B
    private const byte PINB = 0x03;  // Entrada Puerto B
    
    // Flags del Status Register (SREG)
    private const byte SREG_C = 0; // Carry
    private const byte SREG_Z = 1; // Zero
    private const byte SREG_N = 2; // Negative
    private const byte SREG_V = 3; // Overflow
    private const byte SREG_S = 4; // Sign
    private const byte SREG_H = 5; // Half Carry
    private const byte SREG_T = 6; // Transfer
    private const byte SREG_I = 7; // Interrupt Enable

    void Start()
    {
        if (hexParser == null)
            hexParser = GetComponent<HexFileParser>();
            
        if (ledController == null)
            ledController = FindObjectOfType<LEDController>();
            
        InitializeProcessor();
        
        if (autoRun && hexParser != null)
        {
            StartCoroutine(RunSimulation());
        }
    }

    void Update()
    {
        // Solo ejecutar si hay un programa cargado y el simulador está corriendo
        if (isRunning && hexParser?.programMemory != null && !inDelay)
        {
            for (int i = 0; i < instructionsPerFrame; i++)
            {
                if (!ExecuteNextInstruction())
                {
                    break;
                }
            }
        }
    }

    void InitializeProcessor()
    {
        // Inicializar registros
        for (int i = 0; i < registers.Length; i++)
            registers[i] = 0;
            
        // Inicializar SRAM
        for (int i = 0; i < sram.Length; i++)
            sram[i] = 0;
            
        // Inicializar registros I/O
        for (int i = 0; i < ioRegisters.Length; i++)
            ioRegisters[i] = 0;
            
        programCounter = 0;
        statusRegister = 0;
        instructionCount = 0;
        delayCounter = 0;
        inDelay = false;
        
        // Configurar stack pointer
        stackPointer = 0x08FF;
        
        if (enableDebug)
            Debug.Log("Procesador AVR inicializado");
    }

    IEnumerator RunSimulation()
    {
        if (hexParser?.programMemory == null)
        {
            Debug.LogError("No hay programa cargado para simular");
            yield break;
        }

        isRunning = true;
        
        while (isRunning && instructionCount < maxInstructions)
        {
            if (stepByStep)
            {
                yield return new WaitForKeyDown(KeyCode.Space);
            }
            
            // Si estamos en delay, manejar el tiempo
            if (inDelay)
            {
                delayCounter -= Time.deltaTime * 1000; // Convertir a ms
                if (delayCounter <= 0)
                {
                    inDelay = false;
                    if (enableDebug)
                        Debug.Log("Delay terminado, continuando ejecución");
                }
                yield return null;
                continue;
            }
            
            // Ejecutar múltiples instrucciones por frame para mejor rendimiento
            for (int i = 0; i < instructionsPerFrame && isRunning && !inDelay; i++)
            {
                if (!ExecuteNextInstruction())
                {
                    isRunning = false;
                    break;
                }
            }
            
            yield return null; // Esperar al siguiente frame
        }
        
        if (enableDebug)
        {
            Debug.Log($"Simulación terminada. Instrucciones ejecutadas: {instructionCount}");
        }
    }

    bool ExecuteNextInstruction()
    {
        if (hexParser?.programMemory == null)
            return false;
            
        ushort instruction = hexParser.programMemory.ReadInstruction(programCounter);
        instructionCount++;
        
        // JMP (32-bit) - 1001 010k kkkk 110k + kkkk kkkk kkkk kkkk
        if ((instruction & 0xFE0E) == 0x940C)
        {
            ushort nextWord = hexParser.programMemory.ReadInstruction((ushort)(programCounter + 2));
            uint k = (uint)(((instruction & 0x01F0) << 13) | ((instruction & 0x0001) << 16) | nextWord);
            programCounter = (ushort)(k * 2);
            
            if (enableDebug && instructionCount < 50)
                Debug.Log($"JMP 0x{k:X8}");
                
            return true;
        }
        
        // CALL (32-bit) - 1001 010k kkkk 111k + kkkk kkkk kkkk kkkk
        else if ((instruction & 0xFE0E) == 0x940E)
        {
            ushort nextWord = hexParser.programMemory.ReadInstruction((ushort)(programCounter + 2));
            uint k = (uint)(((instruction & 0x01F0) << 13) | ((instruction & 0x0001) << 16) | nextWord);
            
            // Guardar dirección de retorno en stack
            PushStack((ushort)((programCounter + 4) / 2));
            
            // Saltar a la subrutina
            programCounter = (ushort)(k * 2);
            
            if (enableDebug && instructionCount < 50)
                Debug.Log($"CALL 0x{k:X4}");
                
            return true;
        }
        else
        {
            // Instrucciones de 16 bits
            bool result = DecodeAndExecute(instruction);
            programCounter += 2;
            return result;
        }
    }

    bool DecodeAndExecute(ushort instruction)
    {
        // NOP (0x0000)
        if (instruction == 0x0000)
        {
            return true;
        }
        
        // LDI Rd, K (Load Immediate) - 1110 KKKK dddd KKKK
        if ((instruction & 0xF000) == 0xE000)
        {
            byte rd = (byte)(16 + ((instruction >> 4) & 0x0F)); // R16-R31 solamente
            byte k = (byte)(((instruction >> 4) & 0xF0) | (instruction & 0x0F));
            registers[rd] = k;
            
            if (enableDebug && instructionCount < 50)
                Debug.Log($"LDI R{rd}, 0x{k:X2}");
            
            return true;
        }
        
        // OUT A, Rr (Store Register to I/O Location) - 1011 1AAr rrrr AAAA
        if ((instruction & 0xF800) == 0xB800)
        {
            byte rr = (byte)((instruction >> 4) & 0x1F);
            byte a = (byte)(((instruction >> 5) & 0x30) | (instruction & 0x0F));
            
            // Escribir al registro I/O
            if (a < ioRegisters.Length)
            {
                ioRegisters[a] = registers[rr];
                
                // Manejar puertos específicos
                HandleIOWrite(a, registers[rr]);
            }
            
            if (enableDebug && instructionCount < 50)
                Debug.Log($"OUT 0x{a:X2}, R{rr} (valor: 0x{registers[rr]:X2})");
            
            return true;
        }
        
        // SBI A, b (Set Bit in I/O Register) - 1001 1010 AAAA Abbb
        if ((instruction & 0xFF00) == 0x9A00)
        {
            byte a = (byte)((instruction >> 3) & 0x1F);
            byte b = (byte)(instruction & 0x07);
            
            if (a < ioRegisters.Length)
            {
                ioRegisters[a] |= (byte)(1 << b);
                HandleIOWrite(a, ioRegisters[a]);
            }
            
            if (enableDebug && instructionCount < 50)
                Debug.Log($"SBI 0x{a:X2}, {b}");
            
            return true;
        }
        
        // CBI A, b (Clear Bit in I/O Register) - 1001 1000 AAAA Abbb
        if ((instruction & 0xFF00) == 0x9800)
        {
            byte a = (byte)((instruction >> 3) & 0x1F);
            byte b = (byte)(instruction & 0x07);
            
            if (a < ioRegisters.Length)
            {
                ioRegisters[a] &= (byte)~(1 << b);
                HandleIOWrite(a, ioRegisters[a]);
            }
            
            if (enableDebug && instructionCount < 50)
                Debug.Log($"CBI 0x{a:X2}, {b}");
            
            return true;
        }
        
        // RJMP k (Relative Jump) - 1100 kkkk kkkk kkkk
        if ((instruction & 0xF000) == 0xC000)
        {
            short k = (short)(instruction & 0x0FFF);
            if (k > 0x7FF) k = (short)(k - 0x1000); // Complemento a 2
            
            programCounter = (ushort)(programCounter + k * 2);
            
            if (enableDebug && instructionCount < 50)
                Debug.Log($"RJMP {k} (PC = 0x{programCounter:X4})");
            
            return true;
        }
        
        // RET (Return from Subroutine) - 1001 0101 0000 1000
        if (instruction == 0x9508)
        {
            programCounter = (ushort)(PopStack() * 2);
            
            if (enableDebug && instructionCount < 50)
                Debug.Log($"RET (PC = 0x{programCounter:X4})");
            
            return true;
        }

        // JMP (32-bit) - 1001 010k kkkk 110k + kkkk kkkk kkkk kkkk
        if ((instruction & 0xFE0E) == 0x940C)
        {
            ushort nextWord = hexParser.programMemory.ReadInstruction((ushort)(programCounter + 2));
            uint k = (uint)(((instruction & 0x01F0) << 13) | ((instruction & 0x0001) << 16) | nextWord);
            programCounter = (ushort)(k * 2); // Salta a la dirección absoluta (en bytes)
            if (enableDebug && instructionCount < 50)
                Debug.Log($"JMP 0x{k:X4}");
            return true;
        }
                
        // Simular delay usando una función especial
        // Esto es una simplificación para el programa Blink
        if (programCounter >= 0x160 && programCounter <= 0x180) // Rango aproximado de delay
        {
            SimulateDelay(1000); // 1 segundo de delay
            return true;
        }
        
        // Instrucción no implementada
        if (enableDebug && instructionCount < 100)
        {
            Debug.Log($"Instrucción no implementada: 0x{instruction:X4} en PC: 0x{(programCounter - 2):X4}");
        }
        
        return true;
    }
    
    void HandleIOWrite(byte address, byte value)
    {
        // Manejar escritura al Puerto B (donde está conectado el LED del pin 13)
        if (address == PORTB)
        {
            // Pin 13 de Arduino corresponde al bit 5 del Puerto B
            bool ledState = (value & 0x20) != 0; // Bit 5
            
            if (ledController != null)
            {
                ledController.SetLEDState(ledState);
            }
            
            if (enableDebug)
                Debug.Log($"Puerto B = 0x{value:X2}, LED Pin 13 = {(ledState ? "ON" : "OFF")}");
        }
        else if (address == DDRB)
        {
            if (enableDebug)
                Debug.Log($"DDRB (Data Direction B) = 0x{value:X2}");
        }
    }
    
    void SimulateDelay(float milliseconds)
    {
        delayCounter = milliseconds;
        inDelay = true;
        
        if (enableDebug)
            Debug.Log($"Iniciando delay de {milliseconds}ms");
    }
    
    void PushStack(ushort value)
    {
        if (stackPointer > 0)
        {
            sram[stackPointer] = (byte)(value & 0xFF);
            sram[stackPointer - 1] = (byte)((value >> 8) & 0xFF);
            stackPointer -= 2;
        }
    }
    
    ushort PopStack()
    {
        if (stackPointer < 0x08FF - 1)
        {
            stackPointer += 2;
            return (ushort)(sram[stackPointer - 1] | (sram[stackPointer] << 8));
        }
        return 0;
    }

    // Métodos públicos para control
    [ContextMenu("Iniciar Simulación")]
    public void StartSimulation()
    {
        if (!isRunning)
        {
            StartCoroutine(RunSimulation());
        }
    }

    [ContextMenu("Detener Simulación")]
    public void StopSimulation()
    {
        isRunning = false;
        inDelay = false;
        Debug.Log("Simulación detenida");
    }

    [ContextMenu("Reset Procesador")]
    public void ResetProcessor()
    {
        StopSimulation();
        InitializeProcessor();
    }

    [ContextMenu("Ejecutar Una Instrucción")]
    public void StepInstruction()
    {
        if (hexParser?.programMemory != null && !inDelay)
        {
            ExecuteNextInstruction();
        }
    }

    // Métodos de utilidad para acceder al estado
    public byte GetRegister(int index)
    {
        return (index >= 0 && index < 32) ? registers[index] : (byte)0;
    }

    public void SetRegister(int index, byte value)
    {
        if (index >= 0 && index < 32)
            registers[index] = value;
    }

    public bool GetStatusFlag(int flag)
    {
        return (statusRegister & (1 << flag)) != 0;
    }

    public void SetStatusFlag(int flag, bool value)
    {
        if (value)
            statusRegister |= (byte)(1 << flag);
        else
            statusRegister &= (byte)~(1 << flag);
    }

    void OnGUI()
    {
        if (!enableDebug) return;

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 40;
        labelStyle.normal.textColor = Color.white;
        
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 64;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.yellow;
        
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 32;
        
        Texture2D backgroundTexture = MakeBackgroundTexture(new Color(0, 0, 0, 0.8f));
        GUIStyle backgroundStyle = new GUIStyle();
        backgroundStyle.normal.background = backgroundTexture;
        
        Rect guiRect = new Rect(10, 10, 1200, 2400);
        GUI.Box(guiRect, "", backgroundStyle);
        
        GUILayout.BeginArea(new Rect(guiRect.x + 10, guiRect.y + 10, guiRect.width - 20, guiRect.height - 20));
        
        GUILayout.Label("=== AVR Arduino Simulator ===", titleStyle);
        GUILayout.Space(5);
        
        GUILayout.Label($"PC: 0x{programCounter:X4}", labelStyle);
        GUILayout.Label($"Instrucciones: {instructionCount}", labelStyle);
        GUILayout.Label($"Estado: {(isRunning ? "Ejecutando" : "Detenido")}", labelStyle);
        GUILayout.Label($"En Delay: {(inDelay ? $"Sí ({delayCounter:F0}ms)" : "No")}", labelStyle);
        
        GUILayout.Space(10);
        GUILayout.Label("Puerto B (Pin 13):", titleStyle);
        byte portB = ioRegisters[PORTB];
        bool ledState = (portB & 0x20) != 0;
        GUILayout.Label($"PORTB: 0x{portB:X2}", labelStyle);
        GUILayout.Label($"Pin 13 (LED): {(ledState ? "ON" : "OFF")}", labelStyle);
        
        GUILayout.Space(10);
        GUILayout.Label("Registros (R16-R31):", titleStyle);
        for (int i = 16; i < Math.Min(24, registers.Length); i++)
        {
            GUILayout.Label($"R{i}: 0x{registers[i]:X2}", labelStyle);
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button(isRunning ? "Detener" : "Iniciar", buttonStyle))
        {
            if (isRunning)
                StopSimulation();
            else
                StartSimulation();
        }
        
        if (GUILayout.Button("Step", buttonStyle))
        {
            StepInstruction();
        }
        
        if (GUILayout.Button("Reset", buttonStyle))
        {
            ResetProcessor();
        }
        
        GUILayout.EndArea();
    }
    
    private Texture2D MakeBackgroundTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}

// Clase helper para esperar teclas
public class WaitForKeyDown : CustomYieldInstruction
{
    private KeyCode key;
    
    public WaitForKeyDown(KeyCode keyCode)
    {
        key = keyCode;
    }
    
    public override bool keepWaiting
    {
        get { return !Input.GetKeyDown(key); }
    }
}