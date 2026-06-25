using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using WinFormsApp1;

namespace AiHighlightsWinFormsUi.MediaPipeline
{
    public class MediaPlayer
    {
        public static Task PlayAsync(string videoFilePath) =>
            Task.Run(() => PlayVideoFromShell(videoFilePath));

        public static void PlayVideoFromShell(string videoFilePath)
        {
            Program.LogToForm("Playing video from shell: " + videoFilePath);

            var startInfo = new ProcessStartInfo
            {
                FileName = videoFilePath,                 // Specify the path to your video file
                UseShellExecute = true,                   // Use the shell to start the process with default handler
            };

            Process startedProcess = null;
            try
            {
                // Use the static Start which may return null when UseShellExecute=true and the shell
                // opens the document (no process to track).
                startedProcess = Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Program.LogToForm("Failed to start process: " + ex.Message);
            }

            if (startedProcess != null)
            {
                try
                {
                    startedProcess.WaitForExit(); // Wait for the process to finish
                }
                catch (Exception ex)
                {
                    Program.LogToForm("Error waiting for process exit: " + ex.Message);
                }
            }
            else
            {
                Program.LogToForm("No process associated; cannot wait for exit (likely opened by shell).");
            }

            Program.LogToForm("Finished");
        }
    }
}


/*
 
// https://stackoverflow.com/questions/31465630/ffplay-successfully-moved-inside-my-winform-how-to-set-it-borderless

namespace xFFplay
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);


        //Process ffplay = null;

        public Form1()
        {
            InitializeComponent();
            Application.EnableVisualStyles();
            this.DoubleBuffered = true;
        }

        public Process ffplay = new Process();
        private void xxxFFplay()
        {
            // start ffplay 

            //public Process ffplay = new Process();
            ffplay.StartInfo.FileName = "ffplay.exe";
            ffplay.StartInfo.Arguments = "Revenge.mp4";
            ffplay.StartInfo.CreateNoWindow = true;
            ffplay.StartInfo.RedirectStandardOutput = true;
            ffplay.StartInfo.UseShellExecute = false;

            ffplay.EnableRaisingEvents = true;
            ffplay.OutputDataReceived += (o, e) => Debug.WriteLine(e.Data ?? "NULL", "ffplay");
            ffplay.ErrorDataReceived += (o, e) => Debug.WriteLine(e.Data ?? "NULL", "ffplay");
            ffplay.Exited += (o, e) => Debug.WriteLine("Exited", "ffplay");
            ffplay.Start();

            Thread.Sleep(500); // you need to wait/check the process started, then...

            // child, new parent
            // make 'this' the parent of ffmpeg (presuming you are in scope of a Form or Control)
            SetParent(ffplay.MainWindowHandle, this.Handle);

            // window, x, y, width, height, repaint
            // move the ffplayer window to the top-left corner and set the size to 320x280
            MoveWindow(ffplay.MainWindowHandle, 0, 0, 320, 280, true);
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            xxxFFplay();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try { ffplay.Kill(); }
            catch { }
        }
    }
}

 */