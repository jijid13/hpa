using System;
using System.Diagnostics;

namespace HomePersonalAssistant
{
    class Program
    {
        static void Main(string[] args)
        {
            Process customProc = Process.Start("node", "app.js");

            SpeechRecognition sr = new SpeechRecognition();
            sr.Load();
            sr.Start();
            Console.WriteLine("Press any key to close");
            Console.ReadKey();
            sr.Close();

            customProc.Kill();
        }

        static void CurrntDomain_Processexit(object sender, EventArgs e)
        {
            
        }
    }
}
