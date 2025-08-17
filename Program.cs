using static System.Net.Mime.MediaTypeNames;

namespace kysSharp
{
    internal class Program
    {
        static int Main(string[] args)
        {
            Application app = new Application();
            return app.run();
        }
    }
}
