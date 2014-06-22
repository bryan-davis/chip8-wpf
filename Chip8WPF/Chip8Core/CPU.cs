using System;
using System.Collections.Generic;
using System.IO;
using System.Media;

namespace Chip8WPF.Chip8Core
{
    [Serializable]
    class CPU
    {
        private const int screenWidth = 64;
        private const int screenHeight = 32;
        private const int baseAddress = 0x200;

        private Memory memory;
        private byte[] registers;
        private ushort instructionAddress;
        private ushort programCounter;
        private byte delayTimer;
        private byte soundTimer;
        private Stack<ushort> stack;
        private SoundPlayer sound;

        public Keyboard Keyboard { get; private set; }
        public byte[] ScreenData { get; private set; }        
        
        public CPU()
        {            
            ResetCPU();
        }

        public void ResetCPU()
        {
            // The initial values for these are based off the wikipedia page for Chip8
            // http://en.wikipedia.org/wiki/Chip8#Virtual_machine_description
            memory = new Memory();
            registers = new byte[16];
            instructionAddress = 0;
            programCounter = baseAddress;
            stack = new Stack<ushort>();
            delayTimer = 0;
            soundTimer = 0;
            Keyboard = new Keyboard();
            ScreenData = new byte[screenHeight * screenWidth];
            sound = new SoundPlayer(@"assets\beep.wav");            
        }

        public void LoadRom(string romPath)
        {
            ResetCPU();
            using (Stream fileStream = File.OpenRead(romPath))
            {
                fileStream.Read(memory.Bytes, baseAddress, (int)fileStream.Length);
            }
        }

        public void DecrementTimers()
        {
            // Delay and sound timers constantly decrease until they reach 0
            if (delayTimer > 0)
            {
                delayTimer--;
            }

            if (soundTimer > 0)
            {
                // Only play when the sound timer is 1 to prevent stuttering
                if (soundTimer == 1)
                {
                    sound.Play();
                }                
                soundTimer--;
            }            
        }

        public void ExecuteNextOpCode(ushort opCode)
        {
            switch (opCode & 0xF000)
            {
                case 0x0000: ExecuteOpCode0000(opCode);
                    break;
                case 0x1000: OpCode1NNN(opCode);
                    break;
                case 0x2000: OpCode2NNN(opCode);
                    break;
                case 0x3000: OpCode3XNN(opCode);
                    break;
                case 0x4000: OpCode4XNN(opCode);
                    break;
                case 0x5000: OpCode5XY0(opCode);
                    break;
                case 0x6000: OpCode6XNN(opCode);
                    break;
                case 0x7000: OpCode7XNN(opCode);
                    break;
                case 0x8000: ExecuteOpCode8000(opCode);
                    break;
                case 0x9000: OpCode9XY0(opCode);
                    break;
                case 0xA000: OpCodeANNN(opCode);
                    break;
                case 0xB000: OpCodeBNNN(opCode);
                    break;
                case 0xC000: OpCodeCXNN(opCode);
                    break;
                case 0xD000: OpCodeDXYN(opCode);
                    break;
                case 0xE000: ExecuteOpCodeE000(opCode);
                    break;
                case 0xF000: ExecuteOpCodeF000(opCode);
                    break;
                default:
                    break;
            }
        }

        public ushort ReadNextOpCode()
        {            
            ushort opCode = memory[programCounter];
            opCode <<= 8;
            opCode |= memory[programCounter + 1];
            programCounter += 2;
            return opCode;
        }

        // Converts a 2d screen coordinate to the 1d index
        // that the screen data is actually stored in
        private int Convert2dTo1d(int x, int y)
        {            
            return (y * screenWidth) + x;
        }

        private void ClearScreen()
        {
            for (int i = 0; i < ScreenData.Length; i++)
            {
                ScreenData[i] = 0;
            }
        }

        #region OpCode Decoders

        private void ExecuteOpCode0000(ushort opCode)
        {
            switch (opCode)
            {
                case 0x00E0: OpCode00E0();
                    break;
                case 0x00EE: OpCode00EE();
                    break;
                default:
                    break;
            }
        }

        private void ExecuteOpCode8000(ushort opCode)
        {
            switch (opCode & 0x000F)
            {
                case 0x0: OpCode8XY0(opCode);
                    break;
                case 0x1: OpCode8XY1(opCode);
                    break;
                case 0x2: OpCode8XY2(opCode);
                    break;
                case 0x3: OpCode8XY3(opCode);
                    break;
                case 0x4: OpCode8XY4(opCode);
                    break;
                case 0x5: OpCode8XY5(opCode);
                    break;
                case 0x6: OpCode8XY6(opCode);
                    break;
                case 0x7: OpCode8XY7(opCode);
                    break;
                case 0xE: OpCode8XYE(opCode);
                    break;
                default:
                    break;
            }
        }

        private void ExecuteOpCodeE000(ushort opCode)
        {
            switch (opCode & 0x000F)
            {
                case 0xE: OpCodeEX9E(opCode);
                    break;
                case 0x1: OpCodeEXA1(opCode);
                    break;                
                default:
                    break;
            }
        }

        private void ExecuteOpCodeF000(ushort opCode)
        {
            switch (opCode & 0x00FF)
            {
                case 0x07: OpCodeFX07(opCode);
                    break;
                case 0x0A: OpCodeFX0A(opCode);
                    break;
                case 0x15: OpCodeFX15(opCode);
                    break;
                case 0x18: OpCodeFX18(opCode);
                    break;
                case 0x1E: OpCodeFX1E(opCode);
                    break;
                case 0x29: OpCodeFX29(opCode);
                    break;
                case 0x33: OpCodeFX33(opCode);
                    break;
                case 0x55: OpCodeFX55(opCode);
                    break;
                case 0x65: OpCodeFX65(opCode);
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region OpCodes
        // Information for the Opcode functionality came from the following:
        // http://en.wikipedia.org/wiki/Chip8#Opcode_table
        // http://mattmik.com/chip8.html

        // Calls RCA 1802 program at address NNN.
        private void OpCode0NNN(ushort opCode)
        {
            // Do nothing for this opcode; the 1802 CPU is not being emulated
            return; 
        }

        // Clear the screen
        private void OpCode00E0()
        {
            ClearScreen();
        }

        // Return from a subroutine
        private void OpCode00EE()
        {
            programCounter = stack.Pop();            
        }

        // Jump to address at NNN
        private void OpCode1NNN(ushort opCode)
        {
            programCounter = (ushort)(opCode & 0x0FFF);
        }

        // Call subroutine at NNN
        private void OpCode2NNN(ushort opCode)
        {
            stack.Push(programCounter);
            programCounter = (ushort)(opCode & 0x0FFF);
        }

        // Skip the next instruction if VX equals NN
        private void OpCode3XNN(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            byte value = (byte)(opCode & 0x00FF);
            if (registers[registerX] == value)
            {
                programCounter += 2;
            }
        }

        // Skip the next instruction if VX doesn't equal NN
        private void OpCode4XNN(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            int value = opCode & 0x00FF;
            if (registers[registerX] != value)
            {
                programCounter += 2;
            }
        }

        // Skips the next instruction if VX equals VY
        private void OpCode5XY0(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            int registerY = (opCode & 0x00F0) >> 4;
            if (registers[registerX] == registers[registerY])
            {
                programCounter += 2;
            }
        }

        // Sets VX to NN
        private void OpCode6XNN(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            byte value = (byte)(opCode & 0x00FF);
            registers[registerX] = value;
        }

        // Adds NN to VX
        private void OpCode7XNN(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            byte value = (byte)(opCode & 0x00FF);
            registers[registerX] += value;
        }

        // Sets VX to the value of VY
        private void OpCode8XY0(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            int registerY = (opCode & 0x00F0) >> 4;
            registers[registerX] = registers[registerY];
        }

        // Sets VX to VX bitwise OR VY
        private void OpCode8XY1(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            int registerY = (opCode & 0x00F0) >> 4;
            registers[registerX] |= registers[registerY];
        }

        // Sets VX to VX bitwise AND VY
        private void OpCode8XY2(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            int registerY = (opCode & 0x00F0) >> 4;
            registers[registerX] &= registers[registerY];
        }

        // Sets VX to VX bitwise XOR VY
        private void OpCode8XY3(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            int registerY = (opCode & 0x00F0) >> 4;
            registers[registerX] ^= registers[registerY];
        }

        // Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there isn't
        private void OpCode8XY4(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            int registerY = (opCode & 0x00F0) >> 4;            
            
            registers[0xF] = 0;
            int result = registers[registerX] + registers[registerY];

            if (result > 255)
            {
                registers[0xF] = 1;
            }

            registers[registerX] += registers[registerY];
        }

        // VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't
        private void OpCode8XY5(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            int registerY = (opCode & 0x00F0) >> 4;

            registers[0xF] = 1;
            if (registers[registerX] < registers[registerY])
            {
                registers[0xF] = 0;
            }

            registers[registerX] -= registers[registerY];
        }

        // Shifts VX right by one. VF is set to the value of the least significant bit of VX before the shift
        private void OpCode8XY6(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            int lsb = registers[registerX] & 0x1;
            registers[0xF] = (byte)lsb;
            registers[registerX] >>= 1;            
        }

        // Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't
        private void OpCode8XY7(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            int registerY = (opCode & 0x00F0) >> 4;

            registers[0xF] = 1;
            if (registers[registerY] < registers[registerX])
            {
                registers[0xF] = 0;
            }

            registers[registerX] = (byte)(registers[registerY] - registers[registerX]);
        }

        // Shifts VX left by one. VF is set to the value of the most significant bit of VX before the shift
        private void OpCode8XYE(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            int msb = registers[registerX] >> 7;
            registers[0xF] = (byte)msb;
            registers[registerX] <<= 1;  
        }

        // Skips the next instruction if VX doesn't equal VY
        private void OpCode9XY0(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            int registerY = (opCode & 0x00F0) >> 4;
            if (registers[registerX] != registers[registerY])
            {
                programCounter += 2;
            }
        }

        // Sets I to the address NNN
        private void OpCodeANNN(ushort opCode)
        {
            instructionAddress = (ushort)(opCode & 0x0FFF);
        }

        // Jumps to the address NNN plus V0
        private void OpCodeBNNN(ushort opCode)
        {
            int address = opCode & 0x0FFF;
            programCounter = (ushort)(address + registers[0x0]);
        }

        // Sets VX to a random number and NN
        private void OpCodeCXNN(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            int value = (opCode & 0x00FF);
            Random random = new Random();
            registers[registerX] = (byte)(random.Next() & value);
        }

        /*
        Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels. 
        Each row of 8 pixels is read as bit-coded (with the most significant bit of each byte displayed 
        on the left) starting from memory location I; I value doesn't change after the execution of this 
        instruction. As described above, VF is set to 1 if any screen pixels are flipped from set to 
        unset when the sprite is drawn, and to 0 if that doesn't happen. 
        
        All sprites are drawn to the screen using an exclusive-or (XOR) mode; when a request to draw a 
        sprite is processed, the given sprite's data is XOR'd with the current graphics data of the screen.
        */
        private void OpCodeDXYN(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            int registerY = (opCode & 0x00F0) >> 4;            
            
            int startingPointX = registers[registerX];
            int startingPointY = registers[registerY];
            registers[0xF] = 0;            
            
            // N pixels high
            int height = opCode & 0x000F;
            for (int line = 0; line < height; line++, startingPointY++)
            {
                byte spriteData = memory[instructionAddress + line];
                int screenIndex = Convert2dTo1d(startingPointX, startingPointY);

                // 8 pixels/bits wide; we're starting from the most significant bit
                // We don't attempt to draw if the sprite goes out of screen bounds
                for (int column = 7; column >= 0 && screenIndex < ScreenData.Length; column--, screenIndex++)
                {
                    int mask = 1 << column;
                    byte color = 0; // black

                    // The bit is set; color = white
                    if ((spriteData & mask) == mask)                     
                        color = 255;

                    // VF is set to 1 if any screen pixels are toggled from set to unset
                    if (ScreenData[screenIndex] == 255 && color == 255)                       
                        registers[0xF] = 1;

                    ScreenData[screenIndex] ^= color;
                }
            }
        }

        // Skips the next instruction if the key stored in VX is pressed
        private void OpCodeEX9E(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            int key = registers[registerX];
            if (Keyboard.IsKeyPressed(key))
            {
                programCounter += 2;
            }
        }

        // Skips the next instruction if the key stored in VX isn't pressed
        private void OpCodeEXA1(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            int key = registers[registerX];
            if (!Keyboard.IsKeyPressed(key))
            {
                programCounter += 2;
            }
        }

        // Sets VX to the value of the delay timer
        private void OpCodeFX07(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            registers[registerX] = delayTimer;
        }

        // A key press is awaited, and then stored in VX
        private void OpCodeFX0A(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            int key = Keyboard.GetAnyKey();            
            if (key == -1)
            {
                // This is done to prevent the thread from potentially hanging
                // with a while(true) loop. By setting the program counter
                // back, it will just repeat the same instruction.
                programCounter -= 2;
            }
            else
            {
                registers[registerX] = (byte)key;
            }            
        }

        // Sets the delay timer to VX
        private void OpCodeFX15(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            delayTimer = registers[registerX];
        }

        // Sets the sound timer to VX
        private void OpCodeFX18(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            soundTimer = registers[registerX];
        }

        // Adds VX to I
        private void OpCodeFX1E(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            instructionAddress += registers[registerX];
        }

        // Sets I to the location of the sprite for the character in VX. Characters 0-F 
        // (in hexadecimal) are represented by a 4x5 font
        private void OpCodeFX29(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            instructionAddress = (ushort)(registers[registerX] * 5);
        }

        /*
        Stores the Binary-coded decimal representation of VX, with the most significant 
        of three digits at the address in I, the middle digit at I + 1, and the least 
        significant digit at I + 2. (In other words, take the decimal representation 
        of VX, place the hundreds digit in memory at location in I, the tens digit at 
        location I+1, and the ones digit at location I+2.)
        */
        private void OpCodeFX33(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            byte decimalValue = registers[registerX];

            byte hundreds = (byte)(decimalValue / 100);
            byte tens = (byte)((decimalValue / 10) % 10);
            byte ones = (byte)(decimalValue % 10);

            memory[instructionAddress] = hundreds;
            memory[instructionAddress + 1] = tens;
            memory[instructionAddress + 2] = ones;
        }

        // Stores V0 to VX in memory starting at address I
        private void OpCodeFX55(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            for (int i = 0; i <= registerX; i++)
            {
                memory[instructionAddress + i] = registers[i];                
            }

            instructionAddress = (ushort)(instructionAddress + registerX + 1);
        }

        // Fills V0 to VX with values from memory starting at address I
        private void OpCodeFX65(ushort opCode)
        {
            int registerX = (opCode & 0x0F00) >> 8;
            for (int i = 0; i <= registerX; i++)
            {
                registers[i] = memory[instructionAddress + i];
            }

            instructionAddress = (ushort)(instructionAddress + registerX + 1);
        }

        #endregion
    }
}
