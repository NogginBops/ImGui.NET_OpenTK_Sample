using Dear_ImGui_Sample.Backends;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dear_ImGui_Sample
{
    class Program
    {
        static void Main()
        {
            Window wnd = new Window();
            wnd.Run();
            wnd.OnClosed();
        }
    }
}
