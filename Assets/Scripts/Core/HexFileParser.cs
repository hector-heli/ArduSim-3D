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
        //for (ushort addr = 184; addr < 500; addr += 2)
        {
            if (programMemory.HasDataAt(addr) && programMemory.HasDataAt((ushort)(addr + 1)))
            {
                ushort instruction = programMemory.ReadInstruction(addr);
                string disassembly = DisassembleInstruction(addr); // CORREGIDO: pasar dirección
                Debug.Log($"0x{addr:X4}: 0x{instruction:X4} -> {disassembly}");
                // UNKNOWN: Si la instrucción no se reconoce, mostrarla
                // if (disassembly.StartsWith("UNKNOWN")) Debug.Log($"0x{addr:X4}: 0x{instruction:X4} -> {disassembly}");

                if (disassembly.StartsWith("JMP") || disassembly.StartsWith("CALL") || disassembly.StartsWith("STS") || disassembly.StartsWith("LDS")) addr += 2;
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

    // Desensamblador básico para debug
    public string DisassembleInstruction(ushort address)
    {
        ushort instruction = programMemory.ReadInstruction(address);

        // Instrucciones de 32 bits

        // STS k,Rr - Store Direct to Data Space - 1001 001d dddd 0000 kkkk kkkk kkkk kkkk
        if ((instruction & 0xFE0F) == 0x9200)
        {
            ushort? nextWord = programMemory.ReadNextWord(address);
            if (nextWord.HasValue)
            {
                byte rd = (byte)((instruction >> 4) & 0x1F);
                ushort fullAddress = nextWord.Value;
                return $"STS 0x{fullAddress:X4}, R{rd}";
            }
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
                fullAddress *= 2;
                return $"JMP 0x{fullAddress:X2}";
            }
            return "JMP (32-bit, incomplete)";
        }
        // LDS Load Direct from Data Space - 1001 000d dddd 0000 kkkk kkkk kkkk kkkk
        if ((instruction & 0xFE0F) == 0x9000)
        {
            ushort? nextWord = programMemory.ReadNextWord(address);
            if (nextWord.HasValue)
            {
                byte rd = (byte)((instruction >> 4) & 0x1F);
                ushort fullAddress = nextWord.Value;
                return $"LDS R{rd}, 0x{fullAddress:X4}";
            }
        }

        // Instrucciones de 16 bits
        
        // NOP
        if ((instruction & 0XFF00) == 0x0000)
            return "NOP";

        // SEI - Set Global Interrupt Flag - 1001 0100 0111 1000
        if (instruction == 0x9478)
            return "SEI";
        
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

        // ST X+, Rr - 1001 001r rrrr 1101

        if ((instruction & 0xFE0F) == 0x920D)
        {
            byte rr = (byte)((instruction >> 4) & 0x1F);
            return $"ST X+, R{rr}";
        }

        // CPI Rd, K (Compare Immediate) - 1111 KKKK dddd KKKK
        if ((instruction & 0xF000) == 0x3000)
        {
            byte rd = (byte)(16 + ((instruction >> 4) & 0x0F));
            byte k = (byte)(((instruction >> 4) & 0xF0) | (instruction & 0x0F));
            return $"CPI R{rd}, 0x{k:X2}";
        }

        // CPC Rd, Rr (Compare with Carry) - 0000 01rd dddd rrrr
        if ((instruction & 0xFC00) == 0x0400)
        {
            byte rd = (byte)((instruction >> 4) & 0x1F);
            byte rr = (byte)(((instruction >> 5) & 0x10) | (instruction & 0x0F));
            return $"CPC R{rd}, R{rr}";
        }

        // MOVW Rd, Rr - 0000 0001 dddd rrrr
        if ((instruction & 0xFF00) == 0x0100)   
        {
            byte rd = (byte)(2*(instruction >> 4 & 0x0F));
            byte rr = (byte)(2*(instruction & 0x0F));
            return $"MOVW R{rd}, R{rr}";
        }
        
        // SUBI Rd, K (Subtract Immediate) - 0101 KKKK dddd KKKK
        if ((instruction & 0xF000) == 0x5000)
        {
            byte rd = (byte)(16 + ((instruction >> 4) & 0x0F));
            byte k = (byte)(((instruction >> 4) & 0xF0) | (instruction & 0x0F));
            return $"SUBI R{rd}, 0x{k:X2}";
        }

        // ADIW Rd+1:Rd,K (Add Immediate to Word) - 1001 0110 KKdd KKKK
        if ((instruction & 0xFF00) == 0x9600)
        {
            byte rd = (byte)(24 + ((instruction >> 4) & 0x03)*2);
            byte k = (byte)(((instruction >> 2) & 0x30) | (instruction & 0x0F));
            return $"ADIW R{rd}, 0x{k:X2}";
        }

        // SBCI Rd, K (Subtract with Carry Immediate) - 0100 KKKK dddd KKKK
        if ((instruction & 0xF000) == 0x4000)
        {
            byte rd = (byte)(16 + ((instruction >> 4) & 0x0F));
            byte k = (byte)(((instruction >> 4) & 0xF0) | (instruction & 0x0F));
            return $"SBCI R{rd}, 0x{k:X2}";
        }

        // SBC Rd, Rr (Subtract with Carry) - 0000 10rd dddd rrrr 
        if ((instruction & 0xFC00) == 0x0800)
        {
            byte rd = (byte)((instruction >> 4) & 0x1F);
            byte rr = (byte)(((instruction >> 5) & 0x10) | (instruction & 0x0F));
            return $"SBC R{rd}, R{rr}";
        }

        // MULS Rd, Rr - Multiply Signed - 0000 0010 dddd rrrr
        if ((instruction & 0xFF00) == 0x0200)
        {
            byte rd = (byte)(16+((instruction >> 4) & 0x0F));
            byte rr = (byte)(16+(instruction & 0x0F));
            return $"MULS R{rd}, R{rr}";
        }

        // MULSU Rd, Rr - Multiply with Unsigned - 0000 0011 0ddd 0rrr
        if ((instruction & 0xFF88) == 0x0300)
        {
            byte rd = (byte)(16+((instruction >> 4) & 0x0F));
            byte rr = (byte)(16+(instruction & 0x0F));
            return $"MULSU R{rd}, R{rr}";
        }

        // POP Rd - Pop Register from Stack - 1001 000d dddd 1111
        if ((instruction & 0xFE0F) == 0x900F)
        {
            byte rd = (byte)((instruction >> 4) & 0x1F);
            return $"POP R{rd}";
        }

        // PUSH Rr - Push Register onto Stack - 1001 001d rrrr 1111
        if ((instruction & 0xFE0F) == 0x920F)
        {
            byte rr = (byte)((instruction >> 4) & 0x1F);
            return $"PUSH R{rr}";
        }

        // LPM Rd, Z - (Loads one byte pointed to by the Z-register into the destination register Rd) - 1001 000d dddd 0100
        if ((instruction & 0xFE0F) == 0x9004 || (instruction & 0xFE0F) == 0x9005)
        {
            byte rd = (byte)((instruction >> 4) & 0x1F);
            return (instruction & 0xFE0F) == 0x9004 ? $"LPM R{rd}, Z" : $"LPM R{rd}, Z+";
        }

        // AND Rd, Rr - Logical AND - 0010 00rd dddd rrrr
        if ((instruction & 0xFC00) == 0x2000)
        {
            byte rd = (byte)((instruction >> 4) & 0x1F);
            byte rr = (byte)(((instruction >> 5) & 0x10) | (instruction & 0x0F));
            return $"AND R{rd}, R{rr}";
        }

        // ANDI Rd, K - Logical AND Immediate - 0110 KKKK dddd KKKK
        if ((instruction & 0xF000) == 0x7000)
        {
            byte rd = (byte)(16 + ((instruction >> 4) & 0x0F));
            byte k = (byte)(((instruction >> 4) & 0xF0) | (instruction & 0x0F));
            return $"ANDI R{rd}, 0x{k:X2}";
        }   

        // OR Rd, Rr - Logical OR - 0010 10rd dddd rrrr
        if ((instruction & 0xFC00) == 0x2800)
        {
            byte rd = (byte)((instruction >> 4) & 0x1F);
            byte rr = (byte)(((instruction >> 5) & 0x10) | (instruction & 0x0F));
            return $"OR R{rd}, R{rr}";
        }

        // ORI Rd, K - Logical OR Immediate - 0110 KKKK dddd KKKK
        if ((instruction & 0xF000) == 0x6000)
        {
            byte rd = (byte)(16 + ((instruction >> 4) & 0x0F));
            byte k = (byte)(((instruction >> 4) & 0xF0) | (instruction & 0x0F));
            return $"ORI R{rd}, 0x{k:X2}";
        }

        // COM Rd - Complement Register - 1001 010d dddd 0000
        if ((instruction & 0xFE0F) == 0x9400)
        {
            byte rd = (byte)((instruction >> 4) & 0x1F);
            return $"COM R{rd}";
        }

        //INC Rd - Increment Register - 1001 010d dddd 0011
        if ((instruction & 0xFE0F) == 0x9403)
        {
            byte rd = (byte)((instruction >> 4) & 0x1F);
            return $"INC R{rd}";
        } 

        // ADC Rd, Rr – Add with Carry - 0001 11rd dddd rrrr
        if ((instruction & 0xFC00) == 0x1C00)
        {
            byte rd = (byte)((instruction >> 4) & 0x1F);
            byte rr = (byte)(((instruction >> 5) & 0x10) | (instruction & 0x0F));
            return $"ADC R{rd}, R{rr}";
        }

        // IN Rd, A - Load an I/O Location to Register - 1011 0AAd dddd AAAA
        if ((instruction & 0xF800) == 0xB000)
        {
            byte rd = (byte)((instruction >> 4) & 0x1F);
            byte a = (byte)((instruction>>5) & 0x30 |(instruction) & 0x0F);
            return $"IN R{rd}, 0x{a:X2}";
        }

        // LD rD, * - Load Indirect From Data Space to Register using X, Y or Z - 1001 000d dddd 1101
        if ((instruction & 0xFE0F) == 0x900C || (instruction & 0xFE0F) == 0x9008 || (instruction & 0xFE0F) == 0x8000)
        {
            // X: 1001 000d dddd 1100
            // Y: 1001 000d dddd 1000
            // Z: 1000 000d dddd 0000
            byte rd; 

            switch (instruction & 0xFE0F)
            {
                case 0x900C: rd = (byte)((instruction >> 4) & 0x1F); return $"LD R{rd},X";
                case 0x9008: rd = (byte)((instruction >> 4) & 0x1F); return $"LD R{rd},Y";
                case 0x8000: rd = (byte)((instruction >> 4) & 0x1F); return $"LD R{rd},Z";
                default: return "UNKNOWN LD";
            }
        }

        // ST *+ rR - Store Indirect to Data Space using X, Y or Z - 1001 001r rrrr 1100
        if ((instruction & 0xFE0F) == 0x920C || (instruction & 0xFE0F) == 0x8208 || (instruction & 0xFE0F) == 0x8200)
        {
            // X: 1001 001r rrrr 1100
            // Y: 1000 001r rrrr 1000
            // Z: 1000 001r rrrr 0000
            byte rr;

            switch (instruction & 0xFE0F)
            {
                case 0x920C: rr = (byte)((instruction >> 4) & 0x1F); return $"ST X, R{rr}";
                case 0x9208: rr = (byte)((instruction >> 4) & 0x1F); return $"ST Y, R{rr}";
                case 0x8200: rr = (byte)((instruction >> 4) & 0x1F); return $"ST Z, R{rr}";
                default: return "UNKNOWN ST";
            }
        }   

        // CLI - Clear Global Interrupt Flag - 1001 0100 1111 1000
        if (instruction == 0x94F8)
            return "CLI";

        // RET - 1001 0101 0000 1000
        if (instruction == 0x9508)
            return "RET";
            
        // RETI - 1001 0101 0000 1001
        if (instruction == 0x9518)
            return "RETI";

        // BREQ k - Branch if Equal - 1111 00kk kkkk k001
        if ((instruction & 0xFC07) == 0xF001)
        {
            sbyte k = (sbyte)((instruction >> 3) & 0x7F);
            if (k > 63) k = (sbyte)(k - 128);
            return $"BREQ {k * 2}";
        }
        
        // BRNE k - 1111 01kk kkkk k001
        if ((instruction & 0xFC07) == 0xF401)
        {
            sbyte k = (sbyte)((instruction >> 3) & 0x7F);
            if (k > 63) k = (sbyte)(k - 128);
            return $"BRNE {k*2}";
        }

        // BRCC k - Branch if Carry Cleared - 1111 01kk kkkk k000
        if ((instruction & 0xFC07) == 0xF400)
        {
            sbyte k = (sbyte)((instruction >> 3) & 0x7F);
            if (k > 63) k = (sbyte)(k - 128);
            return $"BRCC .{k*2}";
        }

        // BRCS k - Branch if Carry Set - 1111 00kk kkkk k000
        if ((instruction & 0xFC07) == 0xF000)
        {
            sbyte k = (sbyte)((instruction >> 3) & 0x7F);
            if (k > 63) k = (sbyte)(k - 128);
            return $"BRCS .{k*2}";
        }

        // CPSE Rd,Rr - Compare and Skip if Equal - 0000 00rd dddd rrrr
        if ((instruction & 0xFC00) == 0x1000)
        {
            byte rd = (byte)((instruction >> 4) & 0x1F);
            byte rr = (byte)(((instruction >> 5) & 0x10) | (instruction & 0x0F));
            return $"CPSE R{rd}, R{rr}";
        }

        // SBIS A, b - Skip if Bit in I/O Register Set - 1001 1011 AAAA Abbb
        if ((instruction & 0xFF00) == 0x9B00)
        {
            byte a = (byte)((instruction >> 3) & 0x1F);
            byte b = (byte)(instruction & 0x07);
            return $"SBIS 0x{a:X2}, {b}";
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

        // EOR Rd, Rr - 0010 01rd dddd rrrr
        if ((instruction & 0xFC00) == 0x2400)
        {
            byte rd = (byte)((instruction >> 4) & 0x1F);
            byte rr = (byte)(((instruction >> 5) & 0x10) | (instruction & 0x0F));
            return $"EOR R{rd}, R{rr}";
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