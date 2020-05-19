using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;

namespace Dear_ImGui_Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            GraphicsMode mode = new GraphicsMode(new ColorFormat(24), 16, 8, 4, new ColorFormat(32), 2, false);
            Window wnd = new Window(mode);
            wnd.Run();
        }
    }
}
