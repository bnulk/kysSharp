using SDL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static kysSharp.GameRandom;
using static System.Net.Mime.MediaTypeNames;

namespace kysSharp
{
    internal unsafe class TitleScene : Scene
    {
        public int count_ = 0;
        public int head_id_ = 0;

        public int head_x_, head_y_;

        public UISave? menu_load_;
        public Menu menu_;

        int cur = 0; // 走马灯当前图索引
        double showX = 1024; // 走马灯位置

        public TitleScene()
        {
            full_window_ = true;
            Engine.getInstance().tic();      //画走马灯图片用

            menu_ = new Menu();
            Texture? texture = TextureManager.getInstance().loadTexture("title", 17);
            if(texture!=null)
            {
                menu_.setTexture(texture);
            }
            
            menu_.setPosition(800, 300);
            var b = new Button("title", 3, 23, 23);
            menu_.addChild(b, 20, 0);
            b = new Button("title", 4, 24, 24);
            menu_.addChild(b, 20, 50);
            b = new Button("title", 5, 25, 25);
            menu_.addChild(b, 20, 100);
            //menu_load_ = new UISave();
        }

        public override void draw()
        {
            int count = count_ / 20;
            TextureManager.getInstance().renderTexture("title", 0, 0, 0);
            TextureManager.getInstance().renderTexture("title", 1, 260, 400);

            int alpha = 255 - Math.Abs(255 - count_ % 510);
            count_++;
            RandomClassical.srand();
            if (alpha == 0)
            {
                head_id_ = RandomClassical.rand(115);
                head_x_ = RandomClassical.rand(1024 - 150);
                head_y_ = RandomClassical.rand(640 - 150);
            }

            SDL_Color sDL_Color = new SDL_Color() { r = 255, g = 255, b = 255, a = 198 };
            TextureManager.getInstance().renderTexture("head", head_id_, head_x_, head_y_, sDL_Color, (byte)alpha);

            //走马灯
            uint now = Engine.getInstance().getTicks();
            int elapsed = (int)now - Engine.getInstance().time_;            

            if((double)elapsed*0.1>3000)
            {
                Engine.getInstance().time_ = (int)now - 15000;
            }

            for (int i = 0; i < 10; i++)
            {
                double positionX = 1024 - (double)elapsed * 0.1 + (double)i * 150;
                if (positionX < -476)
                {
                    showX = positionX + 1500;
                }
                else
                {
                    showX = positionX;
                }
                byte showAlpha = getAlphaBasedOnShowX(showX);
                TextureManager.getInstance().renderTexture("head", i, (int)showX, 510, default, showAlpha);
            }
        }


        public override void dealEvent(SDL_Event e)
        {
            int r = menu_.run();

            if (r == 0)
            {
                Save.getInstance().load(0);
                //Script::getInstance()->runScript("../game/script/0.lua");
                var random_role = new RandomRole();
                random_role.SetRole(Save.getInstance().GetRole(0));
                if (random_role.runAtPosition(300, 0) == 0)
                {
                    MainScene.getInstance().setManPosition(
                        Save.getInstance().protagonistInformation.MainMapX, Save.getInstance().protagonistInformation.MainMapY);
                    //MainScene.getInstance().forceEnterSubScene(70, 19, 20);
                    MainScene.getInstance().setTowards(Towards.RightDown);
                    MainScene.getInstance().run();
                }

            }
            if (r == 1)
            {
                /*
                if (menu_load_.run() >= 0)
                {
                    //Save::getInstance()->getRole(0)->MagicLevel[0] = 900;    //测试用
                    //Script::getInstance()->runScript("../game/script/0.lua");
                    MainScene::getIntance()->run();
                }
                */
            }
            if (r == 2)
            {
                setExit(true);
            }
        }

        public override void onEntrance()
        {
            Audio.getInstance().playMusic(1);
        }

        private void getSdlTextureSize(SDL_Texture* texture, ref int w, ref int h)
        {
            float* pw = stackalloc float[1];
            float* ph = stackalloc float[1];

            SDL3.SDL_GetTextureSize(texture, pw, ph);

            w = (int)(*pw);
            h = (int)(*ph);
        }

        private byte getAlphaBasedOnShowX(double showX)
        {
            if(showX<0)
                showX = 0;
            byte alpha = (byte)(255-Math.Abs(512 - showX) / 512 * 200);
            return alpha;
        }
















    }
}
