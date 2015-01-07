using Chip8WPF.Chip8Core;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace Chip8WPF.Chip8System
{
    class Chip8Emulator : INotifyPropertyChanged
    {
        public delegate void RenderEventHandler(byte[] data);
        public event RenderEventHandler RenderHandler;

        private CPU cpu;
        private int opCodesPerFrame;
        private double microsecondsPerFrame;
        private string saveStateDirectory;
        private string currentGame;

        public string CurrentGame
        {
            get { return currentGame; }
            private set
            {
                currentGame = value;
                RaisePropertyChanged("CurrentGame");
            }
        }

        public bool Stop { get; set; }

        public Chip8Emulator()
        {
            cpu = new CPU();
            saveStateDirectory = Path.Combine(Directory.GetCurrentDirectory(), "save");
            CalculateSpeedLimit();
        }
        
        public void LoadRom(string filename)
        {
            cpu.LoadRom(filename);
            CurrentGame = Path.GetFileNameWithoutExtension(filename);
        }

        public void KeyUp(Key key)
        {
            cpu.Keyboard.KeyUp(key);
        }

        public void KeyDown(Key key)
        {
            cpu.Keyboard.KeyDown(key);
        }

        public void SaveState()
        {
            string saveStateFile = StateUtil.GenerateSaveStateName(saveStateDirectory, currentGame);
            StateUtil.SaveState(saveStateFile, cpu);
        }

        public void LoadState(string saveStateFile)
        {
            cpu = StateUtil.LoadState(saveStateFile);
            CurrentGame = Path.GetFileNameWithoutExtension(saveStateFile);
        }

        public void Run()
        {
            Stopwatch frameRateLimiter = new Stopwatch();
            frameRateLimiter.Start();
            double microsecondsPerTick = (1000.0 * 1000.0) / Stopwatch.Frequency;

            Stop = false;
            while (!Stop)
            {
                frameRateLimiter.Restart();
                UpdateFrame();
                Render();

                double elapsedTime;
                do
                {
                    elapsedTime = frameRateLimiter.ElapsedTicks * microsecondsPerTick;
                } while (elapsedTime < microsecondsPerFrame);
            }
        }        

        private void UpdateFrame()
        {
            cpu.DecrementTimers();
            for (int i = 0; i < opCodesPerFrame; i++)
            {
                ushort opCode = cpu.ReadNextOpCode();
                cpu.ExecuteNextOpCode(opCode);
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

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }        
    }
}
