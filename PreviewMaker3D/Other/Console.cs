using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PreviewMaker3D
{
    public class MyConsole
    {
        MainForm mainForm;

        public MyConsole(MainForm mainForm_)
        {
            mainForm = mainForm_;
            AllocConsole();
            _ = ConsoleProcessing();
        }

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        private async Task ConsoleProcessing()
        {
            while (true)
            {
                try
                {
                    string[] input = new string[0];

                    await Task.Run(() => { input = Console.ReadLine().Split(' '); });

                    switch (input[0])
                    {
                        case "Add":
                            Add(input);
                            break;
                        default:
                            throw new Exception($"Unknown command: {input[0]}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
            }
        }

        void Add(string[] input)
        {
            if (input[1] == "Cube")
                mainForm.CreateCube();
        }
    }
}
