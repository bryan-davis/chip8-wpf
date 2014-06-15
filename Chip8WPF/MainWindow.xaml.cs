using Microsoft.Win32;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Chip8WPF
{
    public partial class MainWindow : Window
    {
        private const int renderWidth = 64;
        private const int renderHeight = 32;
        private readonly Int32Rect renderRectangle;
        
        private Chip8Emulator emulator;
        private Thread emulatorThread;
        private WriteableBitmap renderFrame;        

        public MainWindow()
        {
            InitializeComponent();

            emulator = new Chip8Emulator();
            emulator.RenderHandler += emulator_Render;
            
            renderFrame = new WriteableBitmap(renderWidth, renderHeight, 
                96, 96, PixelFormats.Gray8, null);
            renderedImage.Source = renderFrame;
            // To be used for writing a new frame to renderFrame
            renderRectangle = new Int32Rect(0, 0, renderWidth, renderHeight);
        }                

        private void StopEmulation()
        {
            if (emulatorThread != null && emulatorThread.IsAlive)
            {
                emulator.Stop = true;
                emulatorThread.Join(1000);
            }
        }

        private void StartEmulation()
        {
            emulatorThread = new Thread(new ThreadStart(emulator.Run));
            emulatorThread.Start();
        }

        private void Render(byte[] screenData)
        {
            renderFrame.WritePixels(renderRectangle, screenData, renderWidth, 0);
        }

        private void emulator_Render(byte[] screenData)
        {
            // The emulator thread cannot access the render frame, hence the
            // call to Dispatcher.BeginInvoke(), instead of rendering directly.
            App.Current.Dispatcher.BeginInvoke((Action<byte[]>)Render, screenData);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            emulator.KeyDown(e.Key);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            emulator.KeyUp(e.Key);
        }

        private void FileExit_Click(object sender, RoutedEventArgs e)
        {            
            Close();
        }

        private void LoadRom_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            bool? fileChosen = dialog.ShowDialog();

            if (fileChosen == true)
            {
                StopEmulation();

                try
                {
                    emulator.LoadRom(dialog.FileName);
                }
                catch (SystemException ex)
                {
                    MessageBox.Show(ex.Message, "Failed to Open File",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                                
                StartEmulation();
            }
        }        

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // We stop emulation to prevent the emulator thread from making
            // render event calls on this window after it has been destroyed,
            // which would result in a NullReferenceException.
            StopEmulation();
        }
    }
}
