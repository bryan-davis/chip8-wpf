using Chip8WPF.Chip8Core;
using System.Diagnostics;
using System.Windows.Input;

namespace Chip8WPF
{
    class Chip8Emulator
    {
        public delegate void RenderEventHandler(byte[] data);
        public event RenderEventHandler RenderHandler;

        private CPU cpu;
        private int opCodesPerFrame;
        private double microsecondsPerFrame;

        public bool Stop { get; set; }

        public Chip8Emulator()
        {
            cpu = new CPU();
            CalculateSpeedLimit();
        }
        
        public void LoadRom(string romPath)
        {
            cpu.LoadRom(romPath);
        }

        public void KeyUp(Key key)
        {
            cpu.Keyboard.KeyUp(key);
        }

        public void KeyDown(Key key)
        {
            cpu.Keyboard.KeyDown(key);
        }

        public void Run()
        {
            Stopwatch frameRateLimiter = new Stopwatch();
            frameRateLimiter.Start();
            double microsecondsPerTick = (1000.0 * 1000.0) / Stopwatch.Frequency;

            Stop = false;            
            while (!Stop)
            {
                double elapsedTime = frameRateLimiter.ElapsedTicks * microsecondsPerTick;
                if (elapsedTime >= microsecondsPerFrame)
                {
                    UpdateFrame();
                    Render();
                    frameRateLimiter.Restart();
                }
            }
        }

        private void UpdateFrame()
        {
            cpu.DecrementTimers();
            for (int i = 0; i < opCodesPerFrame; i++)
            {
                cpu.ExecuteNextOpCode();
            }
        }

        private void Render()
        {
            if (RenderHandler != null)
            {
                RenderHandler(cpu.ScreenData);
            }
        }

        private void CalculateSpeedLimit()
        {
            /* We need to get a higher precision for time slices. Unfortunately,
             * millisecond precision isn't good enough, as 16 ms slices equate to
             * ~62 updates per second, and 17 ms equate to ~58.
            */
            int framesPerSecond = Properties.Settings.Default.targetFrameRate;
            double millsecondsPerFrame = 1000.0 / framesPerSecond;
            microsecondsPerFrame = 1000.0 * millsecondsPerFrame;

            int opCodesPerSecond = Properties.Settings.Default.opCodesPerSecond;
            opCodesPerFrame = opCodesPerSecond / framesPerSecond;
        }
    }
}
