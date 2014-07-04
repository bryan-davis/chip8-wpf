using Chip8WPF.Chip8Core;
using NUnit.Framework;
using System.Collections.Generic;
using System.Windows.Input;

namespace Chip8Tests
{
    [TestFixture]
    public class KeyboardTests
    {
        private Keyboard keyboard;
        private static readonly IDictionary<Key, int> keyMap = new Dictionary<Key, int>
        {
            { Key.D1, 0x1 },
            { Key.D2, 0x2 },
            { Key.D3, 0x3 },
            { Key.D4, 0xC },
            { Key.Q, 0x4 },
            { Key.W, 0x5 },
            { Key.E, 0x6 },
            { Key.R, 0xD },
            { Key.A, 0x7 },
            { Key.S, 0x8 },
            { Key.D, 0x9 },
            { Key.F, 0xE },
            { Key.Z, 0xA },
            { Key.X, 0x0 },
            { Key.C, 0xB },
            { Key.V, 0xF }
        };

        [SetUp]
        public void Setup()
        {
            keyboard = new Keyboard();
        }

        [TearDown]
        public void TearDown()
        {
            keyboard = null;
        }

        [Test]
        public void GetAnyKey_InitialConstruction_NegativeOne()
        {
            int keyPressed = keyboard.GetAnyKey();

            Assert.That(keyPressed, Is.EqualTo(-1));
        }

        [Test]
        public void GetAnyKeyKeyDown_UnsupportedKeyState_NegativeOne()
        {
            keyboard.KeyDown(Key.P);
            int keyPressed = keyboard.GetAnyKey();

            Assert.That(keyPressed, Is.EqualTo(-1));
        }

        [Test]
        public void GetAnyKeyKeyUp_UnsupportedKeyState_NegativeOne()
        {
            keyboard.KeyUp(Key.P);
            int keyPressed = keyboard.GetAnyKey();

            Assert.That(keyPressed, Is.EqualTo(-1));
        }

        [Test]
        public void KeyDownKeyUp_AllSupportedKeys_True()
        {
            foreach (var pair in keyMap)
            {
                keyboard.KeyDown(pair.Key);
                bool keyPressed = keyboard.IsKeyPressed(pair.Value);

                Assert.That(keyPressed, Is.True);

                keyboard.KeyUp(pair.Key);
                keyPressed = keyboard.IsKeyPressed(pair.Value);

                Assert.That(keyPressed, Is.False);
            }
        }        

        [Test]
        public void IsKeyPressed_InputLessThanZero_False()
        {
            bool keyPressed = keyboard.IsKeyPressed(-1);

            Assert.That(keyPressed, Is.False);
        }

        [Test]
        public void IsKeyPressed_InputGreaterThanKeyCount_False()
        {
            bool keyPressed = keyboard.IsKeyPressed(18);

            Assert.That(keyPressed, Is.False);
        }
    }
}
