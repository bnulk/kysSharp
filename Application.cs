using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kysSharp
{
    internal unsafe class Application
    {
        public Application() { }

        public int run()
        {
            var engine = Engine.getInstance();
            engine.setStartWindowSize(1024, 640);
            engine.init();                       //引擎初始化之后才能创建纹理

            engine.createAssistTexture(768, 480);

            var s = new TitleScene();            //开始界面
            s.run();

            return 0;
        }
    }
}
