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
        
        // Método para leer datos en little-endian (para datos)
        public ushort ReadWord(ushort address)
        {
            byte low = ReadByte(address);
            byte high = ReadByte((ushort)(address + 1));
            return (ushort)(low | (high << 8));
        }
        
        // NUEVO: Método específico para leer instrucciones AVR
        public ushort ReadInstruction(ushort address)
        {
            // AVR almacena las instrucciones en formato específico
            // Leer dos bytes consecutivos
            byte firstByte = ReadByte(address);
            byte secondByte = ReadByte((ushort)(address + 1));
            
            // Formar la instrucción de 16 bits
            // En AVR, las instrucciones se almacenan como little-endian en memoria
            ushort instruction = (ushort)(firstByte | (secondByte << 8));
            
            return instruction;
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
    }

    [Header("Configuración")]
    public string hexFilePath = "sketch.hex";
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
            path,
            Path.Combine(Application.streamingAssetsPath, path),
            Path.Combine(Application.dataPath, "StreamingAssets", path),
            Path.Combine(Application.persistentDataPath, path)
        };

        foreach (string testPath in possiblePaths)
        {
            Debug.Log($"Intentando cargar archivo HEX desde: {testPath}");
            if (File.Exists(testPath))
            {
                return ParseHexFile(testPath);
            }
        }

        Debug.LogError($"Archivo HEX no encontrado: {path}");
        return false;
    }

    private bool ParseHexFile(string filePath)
    {
        try
        {
            string[] lines = File.ReadAllLines(filePath);
            hexRecords.Clear();
            programMemory.memory.Clear();

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
                            // Mostrar las primeras instrucciones para debug
                            DebugFirstInstructions();
                        }
                        return true;
                        
                    case 0x02: // Extended Segment Address Record
                        baseAddress = (ushort)(BitConverter.ToUInt16(record.data, 0) << 4);
                        break;
                        
                    case 0x04: // Extended Linear Address Record
                        baseAddress = (ushort)(BitConverter.ToUInt16(record.data, 0) << 16);
                        break;
                        
                    case 0x05: // Start Linear Address Record
                        programMemory.entryPoint = BitConverter.ToUInt16(record.data, 0);
                        break;
                }
            }

            if (debugOutput)
            {
                Debug.Log($"Archivo HEX cargado exitosamente. Total bytes: {totalBytes}");
                DebugFirstInstructions();
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
        
        if (debugOutput && record.dataLength > 0)
        {
            Debug.Log($"Cargados {record.dataLength} bytes en dirección 0x{address:X4}");
        }
    }

    // NUEVO: Método para debug de las primeras instrucciones
    private void DebugFirstInstructions()
    {
        Debug.Log("=== PRIMERAS INSTRUCCIONES ===");
        for (ushort addr = 0; addr < 16; addr += 2)
        {
            if (programMemory.HasDataAt(addr) && programMemory.HasDataAt((ushort)(addr + 1)))
            {
                ushort instruction = programMemory.ReadInstruction(addr);
                Debug.Log($"PC: 0x{addr:X4} -> Instrucción: 0x{instruction:X4}");
            }
        }
    }

    // Métodos públicos para acceder a la memoria del programa
    public byte GetProgramByte(ushort address)
    {
        return programMemory.ReadByte(address);
    }

    public ushort GetProgramWord(ushort address)
    {
        return programMemory.ReadWord(address);
    }
    
    // NUEVO: Método específico para obtener instrucciones
    public ushort GetInstruction(ushort address)
    {
        return programMemory.ReadInstruction(address);
    }

    public void DumpMemory(ushort startAddress = 0, int length = 256)
    {
        Debug.Log($"=== DUMP DE MEMORIA (0x{startAddress:X4} - 0x{(startAddress + length):X4}) ===");
        
        for (int i = 0; i < length; i += 16)
        {
            string line = $"0x{(startAddress + i):X4}: ";
            
            for (int j = 0; j < 16 && (i + j) < length; j++)
            {
                byte value = programMemory.ReadByte((ushort)(startAddress + i + j));
                line += $"{value:X2} ";
            }
            
            Debug.Log(line);
        }
    }
    
    // NUEVO: Dump específico para instrucciones
    public void DumpInstructions(ushort startAddress = 0, int count = 16)
    {
        Debug.Log($"=== DUMP DE INSTRUCCIONES (desde 0x{startAddress:X4}) ===");
        
        for (int i = 0; i < count; i++)
        {
            ushort addr = (ushort)(startAddress + (i * 2));
            if (programMemory.HasDataAt(addr))
            {
                ushort instruction = programMemory.ReadInstruction(addr);
                Debug.Log($"PC: 0x{addr:X4} -> 0x{instruction:X4}");
            }
        }
    }

    // Método para cargar archivo desde el editor
    [ContextMenu("Cargar Archivo HEX")]
    public void LoadHexFromMenu()
    {
        LoadHexFile();
    }

    [ContextMenu("Dump Memoria")]
    public void DumpMemoryFromMenu()
    {
        if (programMemory != null)
        {
            DumpMemory(0, 512);
        }
    }
    
    [ContextMenu("Dump Instrucciones")]
    public void DumpInstructionsFromMenu()
    {
        if (programMemory != null)
        {
            DumpInstructions(0, 32);
        }
    }

    // Información estadística
    public int GetLoadedBytesCount()
    {
        return programMemory?.memory.Count ?? 0;
    }

    public ushort GetHighestAddress()
    {
        ushort highest = 0;
        if (programMemory?.memory != null)
        {
            foreach (var address in programMemory.memory.Keys)
            {
                if (address > highest) highest = address;
            }
        }
        return highest;
    }
}