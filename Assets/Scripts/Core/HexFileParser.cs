using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HexFileParser : MonoBehaviour
{
    [System.Serializable]
    public class HexRecord
    {
        public byte dataLength;
        public ushort address;
        public byte recordType;
        public byte[] data;
        public byte checksum;
    }

    public class ProgramMemory
    {
        public Dictionary<ushort, byte> memory = new Dictionary<ushort, byte>();
        public ushort entryPoint = 0;
        
        public void WriteByte(ushort address, byte value)
        {
            memory[address] = value;
        }
        
        public byte ReadByte(ushort address)
        {
            return memory.ContainsKey(address) ? memory[address] : (byte)0xFF;
        }
        
        // Método para leer instrucciones AVR (16 bits, little-endian)
        public ushort ReadWord(ushort address)
        {
            byte low = ReadByte(address);
            byte high = ReadByte((ushort)(address + 1));
            return (ushort)(low | (high << 8));
        }
        
        // Método específico para leer instrucciones AVR - CORREGIDO
        public ushort ReadInstruction(ushort address)
        {
            if (address % 2 != 0) 
                throw new ArgumentException("Address must be even");
            
            // Usar ReadWord que ya maneja el diccionario correctamente
            return ReadWord(address);
        }
        
        // Método para verificar si una dirección contiene datos válidos
        public bool HasDataAt(ushort address)
        {
            return memory.ContainsKey(address);
        }
        
        // Método para obtener rango de memoria válida
        public (ushort min, ushort max) GetMemoryRange()
        {
            if (memory.Count == 0) return (0, 0);
            
            ushort min = ushort.MaxValue;
            ushort max = ushort.MinValue;
            
            foreach (var address in memory.Keys)
            {
                if (address < min) min = address;
                if (address > max) max = address;
            }
            
            return (min, max);
        }
        
        // NUEVO: Método para leer la siguiente palabra (para instrucciones de 32 bits)
        public ushort? ReadNextWord(ushort address)
        {
            ushort nextAddr = (ushort)(address + 2);
            if (HasDataAt(nextAddr) && HasDataAt((ushort)(nextAddr + 1)))
            {
                return ReadWord(nextAddr);
            }
            return null;
        }
    }

    [Header("Configuración")]
    public string hexFilePath = "Blink.hex";
    public bool loadOnStart = true;
    public bool debugOutput = true;

    [Header("Estado del Programa")]
    public ProgramMemory programMemory;
    public List<HexRecord> hexRecords;

    void Start()
    {
        programMemory = new ProgramMemory();
        hexRecords = new List<HexRecord>();
        
        if (loadOnStart)
        {
            LoadHexFile();
        }
    }

    public bool LoadHexFile(string filePath = null)
    {
        string path = filePath ?? hexFilePath;
        
        // Intentar cargar desde diferentes ubicaciones
        string[] possiblePaths = {
            Path.Combine(Application.dataPath, "StreamingAssets", path),
            Path.Combine(Application.streamingAssetsPath, path),
            Path.Combine(Application.persistentDataPath, path),
            Path.Combine(Application.dataPath, path), // Directorio Assets
            path
        };

        foreach (string testPath in possiblePaths)
        {
            if (debugOutput)
                Debug.Log($"Intentando cargar archivo HEX desde: {testPath}");
                
            if (File.Exists(testPath))
            {
                Debug.Log($"Archivo encontrado: {testPath}");
                return ParseHexFile(testPath);
            }
        }

        Debug.LogError($"Archivo HEX no encontrado: {path}");
        Debug.LogError("Asegúrate de que el archivo Blink.hex esté en la carpeta StreamingAssets");
        return false;
    }

    private bool ParseHexFile(string filePath)
    {
        try
        {
            string[] lines = File.ReadAllLines(filePath);
            hexRecords.Clear();
            programMemory.memory.Clear();
            programMemory.entryPoint = 0; // AGREGADO: Reset entry point

            if (debugOutput)
                Debug.Log($"Parseando archivo HEX: {filePath} ({lines.Length} líneas)");

            ushort baseAddress = 0;
            int totalBytes = 0;

            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line) || !line.StartsWith(":"))
                    continue;

                HexRecord record = ParseHexRecord(line);
                if (record == null) continue;

                hexRecords.Add(record);

                switch (record.recordType)
                {
                    case 0x00: // Data Record
                        LoadDataRecord(record, baseAddress);
                        totalBytes += record.dataLength;
                        break;
                        
                    case 0x01: // End of File Record
                        if (debugOutput)
                        {
                            Debug.Log($"Fin de archivo HEX. Total bytes cargados: {totalBytes}");
                            DebugFirstInstructions();
                        }
                        return true;
                        
                    case 0x02: // Extended Segment Address Record
                        baseAddress = (ushort)(BitConverter.ToUInt16(new byte[] { record.data[1], record.data[0] }, 0) << 4);
                        if (debugOutput)
                            Debug.Log($"Extended Segment Address: 0x{baseAddress:X4}");
                        break;
                        
                    case 0x04: // Extended Linear Address Record
                        baseAddress = (ushort)(BitConverter.ToUInt16(new byte[] { record.data[1], record.data[0] }, 0) << 16);
                        if (debugOutput)
                            Debug.Log($"Extended Linear Address: 0x{baseAddress:X4}");
                        break;
                        
                    case 0x05: // Start Linear Address Record
                        if (record.dataLength >= 4)
                        {
                            // Para AVR, el entry point es típicamente en los primeros 2 bytes
                            programMemory.entryPoint = BitConverter.ToUInt16(new byte[] { record.data[0], record.data[1] }, 0);
                        }
                        if (debugOutput)
                            Debug.Log($"Entry Point: 0x{programMemory.entryPoint:X4}");
                        break;
                }
            }

            if (debugOutput)
            {
                Debug.Log($"Archivo HEX cargado exitosamente. Total bytes: {totalBytes}");
                DebugFirstInstructions();
                DebugMemoryRange();
            }
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al parsear archivo HEX: {e.Message}");
            return false;
        }
    }

    private HexRecord ParseHexRecord(string line)
    {
        try
        {
            if (line.Length < 11) return null;

            HexRecord record = new HexRecord();
            
            // Parsear campos del registro Intel HEX
            record.dataLength = Convert.ToByte(line.Substring(1, 2), 16);
            record.address = Convert.ToUInt16(line.Substring(3, 4), 16);
            record.recordType = Convert.ToByte(line.Substring(7, 2), 16);
            
            // Extraer datos
            record.data = new byte[record.dataLength];
            for (int i = 0; i < record.dataLength; i++)
            {
                record.data[i] = Convert.ToByte(line.Substring(9 + i * 2, 2), 16);
            }
            
            // Checksum
            record.checksum = Convert.ToByte(line.Substring(9 + record.dataLength * 2, 2), 16);
            
            // Verificar checksum
            if (!VerifyChecksum(record, line))
            {
                Debug.LogWarning($"Checksum incorrecto en línea: {line}");
                return null;
            }
            
            return record;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al parsear línea HEX: {line} - {e.Message}");
            return null;
        }
    }

    private bool VerifyChecksum(HexRecord record, string line)
    {
        byte calculatedChecksum = 0;
        
        // Sumar todos los bytes excepto el checksum
        for (int i = 1; i < line.Length - 2; i += 2)
        {
            calculatedChecksum += Convert.ToByte(line.Substring(i, 2), 16);
        }
        
        // El checksum es el complemento a dos
        calculatedChecksum = (byte)(256 - calculatedChecksum);
        
        return calculatedChecksum == record.checksum;
    }

    private void LoadDataRecord(HexRecord record, ushort baseAddress)
    {
        ushort address = (ushort)(baseAddress + record.address);
        
        for (int i = 0; i < record.dataLength; i++)
        {
            programMemory.WriteByte((ushort)(address + i), record.data[i]);
        }
        
        // if (debugOutput && record.dataLength > 0)
        // {
        //     Debug.Log($"Cargados {record.dataLength} bytes en dirección 0x{address:X4}");
        // }
    }

    // Método mejorado para debug de las primeras instrucciones
    private void DebugFirstInstructions()
    {
        Debug.Log("=== PRIMERAS INSTRUCCIONES DEL PROGRAMA ===");
        
        // Mostrar las primeras 10 instrucciones
        for (ushort addr = 0; addr < programMemory.GetMemoryRange().max; addr += 2)
        {
            if (programMemory.HasDataAt(addr) && programMemory.HasDataAt((ushort)(addr + 1)))
            {
                ushort instruction = programMemory.ReadInstruction(addr);
                string disassembly = DisassembleInstruction(addr); // CORREGIDO: pasar dirección
                Debug.Log($"0x{addr:X4}: 0x{instruction:X4} -> {disassembly}");
                if (disassembly.StartsWith("JMP") || disassembly.StartsWith("CALL")) addr += 2;
            }
        }
    }

    // Método para debug del rango de memoria
    private void DebugMemoryRange()
    {
        var (min, max) = programMemory.GetMemoryRange();
        Debug.Log($"Rango de memoria cargada: 0x{min:X4} - 0x{max:X4}");
        Debug.Log($"Total de bytes en memoria: {programMemory.memory.Count}");
    }

    // Desensamblador básico para debug - CORREGIDO
    public string DisassembleInstruction(ushort address)
    {
        ushort instruction = programMemory.ReadInstruction(address);
        
        // NOP
        if (instruction == 0x0000)
            return "NOP";
        
        // LDI Rd, K (Load Immediate) - 1110 KKKK dddd KKKK
        if ((instruction & 0xF000) == 0xE000)
        {
            byte rd = (byte)(16 + ((instruction >> 4) & 0x0F));
            byte k = (byte)(((instruction >> 4) & 0xF0) | (instruction & 0x0F));
            return $"LDI R{rd}, 0x{k:X2}";
        }
        
        // OUT A, Rr - 1011 1AAr rrrr AAAA
        if ((instruction & 0xF800) == 0xB800)
        {
            byte rr = (byte)((instruction >> 4) & 0x1F);
            byte a = (byte)(((instruction >> 5) & 0x30) | (instruction & 0x0F));
            return $"OUT 0x{a:X2}, R{rr}";
        }
        
        // SBI A, b - 1001 1010 AAAA Abbb
        if ((instruction & 0xFF00) == 0x9A00)
        {
            byte a = (byte)((instruction >> 3) & 0x1F);
            byte b = (byte)(instruction & 0x07);
            return $"SBI 0x{a:X2}, {b}";
        }
        
        // CBI A, b - 1001 1000 AAAA Abbb
        if ((instruction & 0xFF00) == 0x9800)
        {
            byte a = (byte)((instruction >> 3) & 0x1F);
            byte b = (byte)(instruction & 0x07);
            return $"CBI 0x{a:X2}, {b}";
        }
        
        
        // RET - 1001 0101 0000 1000
        if (instruction == 0x9508)
            return "RET";
        
        // BRNE k - 1111 01kk kkkk k001
        if ((instruction & 0xFC07) == 0xF401)
        {
            sbyte k = (sbyte)((instruction >> 3) & 0x7F);
            if (k > 63) k = (sbyte)(k - 128);
            return $"BRNE {k}";
        }
        
        // ADD Rd, Rr - 0000 11rd dddd rrrr
        if ((instruction & 0xFC00) == 0x0C00)
        {
            byte rd = (byte)((instruction >> 4) & 0x1F);
            byte rr = (byte)(((instruction >> 5) & 0x10) | (instruction & 0x0F));
            return $"ADD R{rd}, R{rr}";
        }
        
        // SUB Rd, Rr - 0001 10rd dddd rrrr
        if ((instruction & 0xFC00) == 0x1800)
        {
            byte rd = (byte)((instruction >> 4) & 0x1F);
            byte rr = (byte)(((instruction >> 5) & 0x10) | (instruction & 0x0F));
            return $"SUB R{rd}, R{rr}";
        }
        
        // MOV Rd, Rr - 0010 11rd dddd rrrr
        if ((instruction & 0xFC00) == 0x2C00)
        {
            byte rd = (byte)((instruction >> 4) & 0x1F);
            byte rr = (byte)(((instruction >> 5) & 0x10) | (instruction & 0x0F));
            return $"MOV R{rd}, R{rr}";
        }
        
        // CP Rd, Rr - 0001 01rd dddd rrrr
        if ((instruction & 0xFC00) == 0x1400)
        {
            byte rd = (byte)((instruction >> 4) & 0x1F);
            byte rr = (byte)(((instruction >> 5) & 0x10) | (instruction & 0x0F));
            return $"CP R{rd}, R{rr}";
        }
        
        // DEC Rd - 1001 010d dddd 1010
        if ((instruction & 0xFE0F) == 0x940A)
        {
            byte rd = (byte)((instruction >> 4) & 0x1F);
            return $"DEC R{rd}";
        }

        // RJMP k - 1100 kkkk kkkk kkkk
        if ((instruction & 0xF000) == 0xC000)
        {
            short k = (short)(instruction & 0x0FFF);
            if (k > 0x7FF) k = (short)(k - 0x1000);
            return $"RJMP {k}";
        }
        
        // CALL k - 1001 010k kkkk 111k (primera palabra)
        if ((instruction & 0xFE0E) == 0x940E)
        {
            ushort? nextWord = programMemory.ReadNextWord(address);
            if (nextWord.HasValue)
            {
                uint fullAddress = (uint)((instruction & 0x01F1) << 16 | nextWord.Value);
                return $"CALL 0x{fullAddress:X6}";
            }
            return "CALL (32-bit, incomplete)";
        }

        // JMP k (32-bit) - 1001 010k kkkk 110k  kkkk kkkk kkkk kkkk
        if ((instruction & 0xFE0E) == 0x940C)
        {
            ushort? nextWord = programMemory.ReadNextWord(address);
            if (nextWord.HasValue)
            {
                uint fullAddress = (uint)(((instruction & 0x01F0) << 13) | ((instruction & 0x0001) << 16) | nextWord.Value);
                Debug.Log($"{fullAddress*2}");
                fullAddress *= 2;
                return $"JMP 0x{fullAddress:X2}";
            }
            return "JMP (32-bit, incomplete)";
        }
        
        return $"UNKNOWN (0x{instruction:X4})";
    }

    // Métodos públicos para acceso externo
    [ContextMenu("Cargar Archivo HEX")]
    public void LoadHexFileFromMenu()
    {
        LoadHexFile();
    }

    [ContextMenu("Debug Primeras Instrucciones")]
    public void DebugFirstInstructionsFromMenu()
    {
        if (programMemory != null)
            DebugFirstInstructions();
    }

    [ContextMenu("Debug Rango de Memoria")]
    public void DebugMemoryRangeFromMenu()
    {
        if (programMemory != null)
            DebugMemoryRange();
    }

    // Métodos para acceso desde otros scripts
    public bool IsLoaded()
    {
        return programMemory != null && programMemory.memory.Count > 0;
    }

    public int GetLoadedInstructionCount()
    {
        if (programMemory == null) return 0;
        return programMemory.memory.Count / 2; // Cada instrucción AVR son 2 bytes
    }

    public void ClearMemory()
    {
        if (programMemory != null)
        {
            programMemory.memory.Clear();
            programMemory.entryPoint = 0;
        }
        
        if (hexRecords != null)
            hexRecords.Clear();
            
        Debug.Log("Memoria del programa limpiada");
    }
}