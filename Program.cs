using static System.Net.Mime.MediaTypeNames;

namespace kysSharp
{
    internal class Program
    {
        static unsafe int Main(string[] args)
        {
            var engine = Engine.getInstance();
            engine.setStartWindowSize(1024, 640);
            engine.init();                       //引擎初始化之后才能创建纹理

            engine.createAssistTexture(768, 480);

            var s = new TitleScene();            //开始界面
            s.run();
            s.dispose();

            return 0;
        }
    }
}
