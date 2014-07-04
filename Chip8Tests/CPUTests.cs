using Chip8WPF.Chip8Core;
using NUnit.Framework;
using System;
using System.Windows.Input;

namespace Chip8Tests
{
    [TestFixture]
    public class CPUTests
    {
        private CPU cpu;

        [SetUp]
        public void Setup()
        {
            cpu = new CPU();
        }

        [TearDown]
        public void TearDown()
        {
            cpu = null;
        }

        [Test]
        public void OpCode00E0_RandomScreenData_SetsScreenDataToZero()
        {
            byte[] screenData = new byte[cpu.ScreenData.Length];
            Random random = new Random();
            for (int i = 0; i < screenData.Length; i++)
            {
                screenData[i] = (byte)random.Next(255);
            }

            cpu.ScreenData = screenData;
            ushort opCode = 0x00E0;
            cpu.ExecuteNextOpCode(opCode);

            for (int i = 0; i < screenData.Length; i++)
            {                
                Assert.That(cpu.ScreenData[i], Is.EqualTo(0));
            }
        }

        [Test]
        public void OpCode00EE_ProgramCounterSetToStackPopValue()
        {
            ushort value = 123;
            cpu.Stack.Push(value);
            ushort opCode = 0x00EE;
            cpu.ExecuteNextOpCode(opCode);
            
            Assert.That(cpu.ProgramCounter, Is.EqualTo(value));
        }

        [Test]
        public void OpCode1NNN_ProgramCounterSetToNNN()
        {
            ushort opCode = 0x1234;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.ProgramCounter, Is.EqualTo(0x234));
        }

        [Test]
        public void OpCode2NNN_StackPushesProgramCounter_ProgramCounterSetToNNN()
        {
            ushort programCounter = 123;
            cpu.ProgramCounter = 123;
            ushort opCode = 0x2234;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.ProgramCounter, Is.EqualTo(0x234));
            Assert.That(cpu.Stack.Peek(), Is.EqualTo(programCounter));
        }

        [Test]
        public void OpCode3XNN_RegisterXEqualsNN_ProgramCounterIncrementedByTwo()
        {
            ushort opCode = 0x3144;
            cpu.Registers[0x1] = 0x44;
            ushort programCounter = cpu.ProgramCounter;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.ProgramCounter, Is.EqualTo(programCounter + 2));
        }

        [Test]
        public void OpCode3XNN_RegisterXDoesNotEqualNN_ProgramCounterNotIncremented()
        {
            ushort opCode = 0x3144;
            cpu.Registers[0x1] = 0x88;
            ushort programCounter = cpu.ProgramCounter;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.ProgramCounter, Is.EqualTo(programCounter));
        }        

        [Test]
        public void OpCode4XNN_RegisterXDoesNotEqualNN_ProgramCounterIncrementedByTwo()
        {
            ushort opCode = 0x4144;
            cpu.Registers[0x1] = 0x88;
            ushort programCounter = cpu.ProgramCounter;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.ProgramCounter, Is.EqualTo(programCounter + 2));
        }

        [Test]
        public void OpCode4XNN_RegisterXEqualsNN_ProgramCounterNotIncremented()
        {
            ushort opCode = 0x4144;
            cpu.Registers[0x1] = 0x44;
            ushort programCounter = cpu.ProgramCounter;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.ProgramCounter, Is.EqualTo(programCounter));
        }

        [Test]
        public void OpCode5XY0_RegisterXValueEqualsRegisterYValue_ProgramCounterIncrementedByTwo()
        {
            ushort opCode = 0x5230;
            cpu.Registers[0x2] = 10;
            cpu.Registers[0x3] = 10;
            ushort programCounter = cpu.ProgramCounter;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.ProgramCounter, Is.EqualTo(programCounter + 2));
        }

        [Test]
        public void OpCode5XY0_RegisterXValueDoesNotEqualRegisterYValue_ProgramCounterNotIncremented()
        {
            ushort opCode = 0x5230;
            cpu.Registers[0x2] = 10;
            cpu.Registers[0x3] = 15;
            ushort programCounter = cpu.ProgramCounter;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.ProgramCounter, Is.EqualTo(programCounter));
        }

        [Test]
        public void OpCode6XNN_RegisterXValueIsSetToNN()
        {
            ushort opCode = 0x6C45;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.Registers[0xC], Is.EqualTo(0x45));
        }

        [Test]
        public void OpCode7XNN_NNIsAddedToRegisterXValue()
        {
            ushort opCode = 0x7B15;
            byte registerXValue = cpu.Registers[0xB];
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.Registers[0xB], Is.EqualTo(registerXValue + 0x15));
        }

        [Test]
        public void OpCode8XY0_SetsRegisterXToRegisterYValue()
        {
            ushort opCode = 0x8AB0;
            byte registerYValue = 25;
            cpu.Registers[0xB] = registerYValue;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.Registers[0xA], Is.EqualTo(registerYValue));
        }

        [Test]
        public void OpCode8XY1_RegisterXValueBitwiseOrWithRegisterYValue()
        {
            ushort opCode = 0x8AB1;            
            cpu.Registers[0xA] = 10;
            cpu.Registers[0xB] = 20;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.Registers[0xA], Is.EqualTo(10 | 20));
        }

        [Test]
        public void OpCode8XY2_RegisterXValueBitwiseAndWithRegisterYValue()
        {
            ushort opCode = 0x8AB2;
            cpu.Registers[0xA] = 10;
            cpu.Registers[0xB] = 20;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.Registers[0xA], Is.EqualTo(10 & 20));
        }

        [Test]
        public void OpCode8XY3_RegisterXValueBitwiseXorWithRegisterYValue()
        {
            ushort opCode = 0x8AB3;
            cpu.Registers[0xA] = 10;
            cpu.Registers[0xB] = 20;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.Registers[0xA], Is.EqualTo(10 ^ 20));
        }

        [Test]
        public void OpCode8XY4_WithOverflow_RegisterFSetToOne()
        {
            ushort opCode = 0x8AB4;
            cpu.Registers[0xA] = 100;
            cpu.Registers[0xB] = 200;
            cpu.ExecuteNextOpCode(opCode);

            // 1 byte overflow for 200 + 100 = 300 - 256 (1 << 8)
            Assert.That(cpu.Registers[0xA], Is.EqualTo(200 + 100 - 256));
            // Overflow should set the 0xF register to 1
            Assert.That(cpu.Registers[0xF], Is.EqualTo(1));
        }

        [Test]
        public void OpCode8XY4_NoOverflow_RegisterFSetToZero()
        {
            ushort opCode = 0x8AB4;
            cpu.Registers[0xA] = 10;
            cpu.Registers[0xB] = 20;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.Registers[0xA], Is.EqualTo(10 + 20));
            // No overflow means the 0xF register should be 0
            Assert.That(cpu.Registers[0xF], Is.EqualTo(0));
        }

        [Test]
        public void OpCode8XY5_WithBorrow_RegisterFSetToZero()
        {
            ushort opCode = 0x8AB5;
            cpu.Registers[0xA] = 10;
            cpu.Registers[0xB] = 20;
            cpu.ExecuteNextOpCode(opCode);

            // Underflow would be 256 (1 << 8) + 10 - 20
            Assert.That(cpu.Registers[0xA], Is.EqualTo(10 - 20 + 256));
            // A borrow means the 0xF register should be 0
            Assert.That(cpu.Registers[0xF], Is.EqualTo(0));
        }

        [Test]
        public void OpCode8XY5_NoBorrow_RegisterFSetToOne()
        {
            ushort opCode = 0x8AB5;
            cpu.Registers[0xA] = 20;
            cpu.Registers[0xB] = 10;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.Registers[0xA], Is.EqualTo(20 - 10));
            // No borrow means the 0xF register should be 1
            Assert.That(cpu.Registers[0xF], Is.EqualTo(1));
        }

        [Test]
        public void OpCode8XY6_LowBitNotSet_RegisterFSetToZero()
        {
            ushort opCode = 0x8AB6;
            // 22 is 0001 0110 in binary, least significant bit is 0
            byte registerXValue = 22;
            cpu.Registers[0xA] = registerXValue;            
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.Registers[0xA], Is.EqualTo(registerXValue >> 1));
            Assert.That(cpu.Registers[0xF], Is.EqualTo(0));
        }

        [Test]
        public void OpCode8XY6_LowBitSet_RegisterFSetToOne()
        {
            ushort opCode = 0x8AB6;
            // 21 is 0001 0101 in binary, least significant bit is 1
            byte registerXValue = 21;
            cpu.Registers[0xA] = registerXValue;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.Registers[0xA], Is.EqualTo(registerXValue >> 1));
            Assert.That(cpu.Registers[0xF], Is.EqualTo(1));
        }

        [Test]
        public void OpCode8XY7_WithBorrow_RegisterFSetToZero()
        {
            ushort opCode = 0x8AB7;
            cpu.Registers[0xA] = 20;
            cpu.Registers[0xB] = 10;
            cpu.ExecuteNextOpCode(opCode);

            // Underflow would be 256 (1 << 8) + 10 - 20
            Assert.That(cpu.Registers[0xA], Is.EqualTo(10 - 20 + 256));
            // A borrow means the 0xF register should be 0
            Assert.That(cpu.Registers[0xF], Is.EqualTo(0));
        }

        [Test]
        public void OpCode8XY7_NoBorrow_RegisterFSetToOne()
        {
            ushort opCode = 0x8AB7;
            cpu.Registers[0xA] = 10;
            cpu.Registers[0xB] = 20;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.Registers[0xA], Is.EqualTo(20 - 10));
            // A borrow means the 0xF register should be 1
            Assert.That(cpu.Registers[0xF], Is.EqualTo(1));
        }

        [Test]
        public void OpCode8XYE_HighBitSet_RegisterFSetToOne()
        {
            ushort opCode = 0x8ABE;
            // 129 is 100 0001 in binary, most significant bit is 1
            byte registerXValue = 129;
            cpu.Registers[0xA] = registerXValue;
            cpu.ExecuteNextOpCode(opCode);

            // Need to explicitly calculate the expected value as a byte,
            // otherwise the compiler will treat it as an int
            byte expectedValue = (byte)(registerXValue << 1);
            Assert.That(cpu.Registers[0xA], Is.EqualTo(expectedValue));
            Assert.That(cpu.Registers[0xF], Is.EqualTo(1));
        }

        [Test]
        public void OpCode8XYE_HighBitNotSet_RegisterFSetToZero()
        {
            ushort opCode = 0x8ABE;
            // 65 is 010 0001 in binary, most significant bit is 1
            byte registerXValue = 65;
            cpu.Registers[0xA] = registerXValue;
            cpu.ExecuteNextOpCode(opCode);
            
            Assert.That(cpu.Registers[0xA], Is.EqualTo(registerXValue << 1));
            Assert.That(cpu.Registers[0xF], Is.EqualTo(0));
        }

        [Test]
        public void OpCode9XY0_RegisterXValueEqualsRegisterYValue_InstructionNotSkipped()
        {
            ushort opCode = 0x9AB0;
            cpu.Registers[0xA] = 10;
            cpu.Registers[0xB] = 10;
            ushort programCounter = cpu.ProgramCounter;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.ProgramCounter, Is.EqualTo(programCounter));
        }

        [Test]
        public void OpCode9XY0_RegisterXValueDoesNotEqualRegisterYValue_InstructionSkipped()
        {
            ushort opCode = 0x9AB0;
            cpu.Registers[0xA] = 10;
            cpu.Registers[0xB] = 20;
            ushort programCounter = cpu.ProgramCounter;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.ProgramCounter, Is.EqualTo(programCounter + 2));
        }

        [Test]
        public void OpCodeANNN_InstructionAddressSetToNNN()
        {
            ushort opCode = 0xA123;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.InstructionAddress, Is.EqualTo(0x123));
        }

        [Test]
        public void OpCodeBNNN_ProgramCounterSetToNNNPlusRegister0()
        {
            ushort opCode = 0xB123;
            cpu.Registers[0x0] = 100;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.ProgramCounter, Is.EqualTo(0x123 + 100));
        }

        [Test]
        public void OpCodeEX9E_KeyStoredInRegisterXIsPressed_NextInstructionIsSkipped()
        {
            ushort opCode = 0xE29E;
            cpu.Registers[0x2] = 3;
            // Key D3 is mapped to keyboard[3]
            cpu.Keyboard.KeyDown(Key.D3);
            ushort programCounter = cpu.ProgramCounter;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.ProgramCounter, Is.EqualTo(programCounter + 2));
        }

        [Test]
        public void OpCodeEX9E_KeyStoredInRegisterXIsNotPressed_NextInstructionNotSkipped()
        {
            ushort opCode = 0xE29E;
            cpu.Registers[0x2] = 2;
            // Key D3 is mapped to keyboard[3]
            cpu.Keyboard.KeyDown(Key.D3);
            ushort programCounter = cpu.ProgramCounter;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.ProgramCounter, Is.EqualTo(programCounter));
        }

        [Test]
        public void OpCodeEXA1_KeyStoredInRegisterXIsNotPressed_NextInstructionIsSkipped()
        {
            ushort opCode = 0xE2A1;
            cpu.Registers[0x2] = 2;
            // Key D3 is mapped to keyboard[3]
            cpu.Keyboard.KeyDown(Key.D3);
            ushort programCounter = cpu.ProgramCounter;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.ProgramCounter, Is.EqualTo(programCounter + 2));
        }

        [Test]
        public void OpCodeEXA1_KeyStoredInRegisterXIsPressed_NextInstructionNotSkipped()
        {
            ushort opCode = 0xE2A1;
            cpu.Registers[0x2] = 3;
            // Key D3 is mapped to keyboard[3]
            cpu.Keyboard.KeyDown(Key.D3);
            ushort programCounter = cpu.ProgramCounter;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.ProgramCounter, Is.EqualTo(programCounter));
        }

        [Test]
        public void OpCodeFX07_RegisterXIsSetToTheDelayTimerValue()
        {
            ushort opCode = 0xF307;
            cpu.DelayTimer = 50;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.Registers[0x3], Is.EqualTo(50));
        }

        [Test]
        public void OpCodeFX0A_KeyPressed_ValueStoredInRegisterX()
        {
            ushort opCode = 0xF50A;
            // Key D3 is mapped to keyboard[3]
            cpu.Keyboard.KeyDown(Key.D3);
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.Registers[0x5], Is.EqualTo(3));
        }

        [Test]
        public void OpCodeFX15_DelayTimerSetToRegisterX()
        {
            ushort opCode = 0xF715;
            cpu.Registers[0x7] = 25;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.DelayTimer, Is.EqualTo(25));
        }

        [Test]
        public void OpCodeFX18_SoundTimerSetToRegisterX()
        {
            ushort opCode = 0xF718;
            cpu.Registers[0x7] = 25;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.SoundTimer, Is.EqualTo(25));
        }

        [Test]
        public void OpCodeFX1E_RegisterXAddedToInstructionAddress()
        {
            ushort opCode = 0xF71E;
            cpu.Registers[0x7] = 25;
            cpu.InstructionAddress = 100;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.InstructionAddress, Is.EqualTo(125));
        }

        [Test]
        public void OpCodeFX29_InstructionAddressSetToTheAddressInMemoryThatCharacterIsStored()
        {
            ushort opCode = 0xF129;
            // Characters 0 - F
            for (byte character = 0; character < cpu.Registers.Length; character++)
            {
                cpu.Registers[0x1] = character;
                cpu.ExecuteNextOpCode(opCode);

                // Fonts are 4x5 in size, hence the multiplier of 5
                Assert.That(cpu.InstructionAddress, Is.EqualTo(character * 5));
            }            
        }

        [Test]
        public void OpCodeFX33_StoreBinaryCodedRepresentationOfRegisterX()
        {
            ushort opCode = 0xF733;
            cpu.Registers[0x7] = 234;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.Memory[cpu.InstructionAddress], Is.EqualTo(2));
            Assert.That(cpu.Memory[cpu.InstructionAddress + 1], Is.EqualTo(3));
            Assert.That(cpu.Memory[cpu.InstructionAddress + 2], Is.EqualTo(4));
        }

        [Test]
        // Store the values of registers V0 to VX inclusive in memory starting at address I
        // I is set to I + X + 1 after operation
        public void OpCodeFX55_Registers0ThroughFStoredInMemory()
        {
            ushort opCode = 0xFF55;
            int registerCount = cpu.Registers.Length;

            for (byte i = 0; i < registerCount; i++)
            {
                cpu.Registers[i] = (byte)(i + 1);
            }

            ushort currentAddress = cpu.InstructionAddress;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.InstructionAddress, Is.EqualTo(currentAddress + registerCount));

            for (int i = 0; i < registerCount; i++)
            {
                Assert.That(cpu.Memory[cpu.InstructionAddress - registerCount + i], Is.EqualTo(i + 1));
            }
        }

        [Test]
        public void OpCodeFX65_RegistersFilledWithValuesFromMemory()
        {
            ushort opCode = 0xFF65;
            int registerCount = cpu.Registers.Length;

            for (byte i = 0; i < registerCount; i++)
            {
                cpu.Memory[cpu.InstructionAddress + i] = (byte)(i + 1);
            }

            ushort currentAddress = cpu.InstructionAddress;
            cpu.ExecuteNextOpCode(opCode);

            Assert.That(cpu.InstructionAddress, Is.EqualTo(currentAddress + registerCount));

            for (int i = 0; i < registerCount; i++)
            {
                Assert.That(cpu.Registers[i], Is.EqualTo(i + 1));
            }
        }
    }
}
