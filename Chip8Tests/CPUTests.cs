using Chip8WPF.Chip8Core;
using NUnit.Framework;
using System;

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
            ushort opCode = 0x6F45;
            cpu.ExecuteNextOpCode(opCode);
            Assert.That(cpu.Registers[0xF], Is.EqualTo(0x45));
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
    }
}
