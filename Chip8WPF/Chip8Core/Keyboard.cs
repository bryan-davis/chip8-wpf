using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Chip8WPF.Chip8Core
{
    [Serializable]
    public class Keyboard
    {
        /*
           Keyboard       Chip8 Keypad   
           +-+-+-+-+      +-+-+-+-+
           |1|2|3|4|      |1|2|3|C|
           +-+-+-+-+      +-+-+-+-+
           |Q|W|E|R|      |4|5|6|D|
           +-+-+-+-+  =>  +-+-+-+-+
           |A|S|D|F|      |7|8|9|E|
           +-+-+-+-+      +-+-+-+-+
           |Z|X|C|V|      |A|0|B|F|
           +-+-+-+-+      +-+-+-+-+  
        */
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
        
        private bool[] keyState;
        private const int keyCount = 16;

        public Keyboard()
        {
            // Input is done with a "hex" keyboard that has 16 keys which range from 0 to F.
            keyState = new bool[keyCount];
        }

        public int GetAnyKey()
        {
            for (int i = 0; i < keyState.Length; i++)
            {
                if (keyState[i])
                {
                    return i;
                }
            }

            return -1;
        }

        public void KeyDown(Key key)
        {
            int index;
            if (keyMap.TryGetValue(key, out index))
            {
                keyState[index] = true;
            }
        }

        public void KeyUp(Key key)
        {
            int index;
            if (keyMap.TryGetValue(key, out index))
            {
                keyState[index] = false;
            }
        }

        public bool IsKeyPressed(int key)
        {
            return (key >= 0 && key < keyCount) ? keyState[key] : false;
        }
    }
}
