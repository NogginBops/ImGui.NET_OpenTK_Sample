using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace Dear_ImGui_Sample
{
    class Program
    {
        static void Main()
        {
            Window wnd = new Window();

            wnd.MakeCurrent();
            wnd.Load();

            Stopwatch watch = new Stopwatch();
            
            while (wnd.Exists)
            {
                watch.Restart();

                wnd.ProcessEvents();

                wnd.MakeCurrent();
                wnd.Update(watch.ElapsedTicks / (double)Stopwatch.Frequency);

                if (wnd.IsExiting)
                {
                    wnd.Dispose();
                }
            }
        }
    }
}
