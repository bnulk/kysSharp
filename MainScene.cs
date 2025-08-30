using SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace kysSharp
{
    internal class MainScene : Scene
    {
        public static MainScene mainScene_= new MainScene();
        public static MainScene getInstance()
        {
            return mainScene_;
        }

        public MainScene()
        {
            full_window_ = true;

            if (_readed == false)
            {
                short[] tmpShort;
                int length = Earth.Length;
                GameFile.readFile("resource/earth.002", out tmpShort, length);
                OneDim2TwoDim(ref Earth, tmpShort);
                GameFile.readFile("resource/surface.002", out tmpShort, length);
                OneDim2TwoDim(ref Surface, tmpShort);
                GameFile.readFile("resource/building.002", out tmpShort, length);
                OneDim2TwoDim(ref Building, tmpShort);
                GameFile.readFile("resource/buildx.002", out tmpShort, length);
                OneDim2TwoDim(ref BuildY, tmpShort);
                GameFile.readFile("resource/buildy.002", out tmpShort, length);
                OneDim2TwoDim(ref BuildX, tmpShort);
            }
            _readed = true;

            man_x_ = 240;
            man_y_ = 240;

            Console.WriteLine("toward=" + towards_.ToString());

            //一百朵云
            for (int i = 0; i < 100; i++)
            {
                var c = new Cloud();
                c.InitCloud();
                cloudVector.Add(c);
            }

            GetEntrance();
        }



        public const int maxX = 480;
        public const int maxY = 480;
        public short[,] mapArray= new short[maxX, maxY]; //地图数组

        public short[,] Earth = new short[480, 480];                                         //short两字节
        public short[,] Surface = new short[480, 480];
        public short[,] Building = new short[480, 480];
        public short[,] BuildX = new short[480, 480];
        public short[,] BuildY = new short[480, 480];
        public short[,] Entrance = new short[480, 480];

        int MAN_PIC_0 = 2501;         //初始主角图偏移量
        int MAN_PIC_COUNT = 7;        //单向主角图张数
        int REST_PIC_0 = 2529;        //主角休息图偏移量
        int REST_PIC_COUNT = 6;       //单向休息图张数
        int SHIP_PIC_0 = 3715;        //初始主角图偏移量
        int SHIP_PIC_COUNT = 4;       //单向主角图张数
        int BEGIN_REST_TIME = 200;    //开始休息的时间
        int REST_INTERVAL = 15;       //休息图切换间隔

        Stack<Point> wayQue = new Stack<Point>();                                               //栈(路径栈)


        public int cloudX, cloudY;
        public int manPicture;
        public int restTime = 0;                   //停止操作的时间
        public int cloud_restTime = 0;             //云消失的时间
        public const int cloudSize = 240;              //云朵宽度
        public const int tag_mainLayer = 1;     //主层编号 
        public const int tag_wordLayer = 1;     //文字层编号


        public bool isEsc = false;					//是否已打开系统菜单

        Cloud.CloudTowards cloudTowards = Cloud.CloudTowards.Left;
        List<Cloud> cloudVector= new List<Cloud>();

        public bool _readed = false;

        ~MainScene()
        {
            Engine.getInstance().stopMusic(); // 停止音乐
        }

        private struct DrawInfo
        {
            public int i;
            public Point p;
        }

        public override void draw()
        {
            int k = 0;

            var t0 = Engine.getInstance().getTicks();

            Dictionary<int, DrawInfo> map = new Dictionary<int, DrawInfo>();
            DrawInfo tmpDrawInfo = new DrawInfo();

            TextureManager.getInstance().renderTexture("mmap", 0, 0, 0);


            for (int sum = -view_sum_region_; sum <= -view_sum_region_ + 15; sum++)
            {
                for (int i = -view_width_region_; i <= view_width_region_; i++)
                {
                    int i1 = man_x_ + i + (sum / 2);
                    int i2 = man_y_ - i + (sum - sum / 2);
                    var p = getPositionOnRender(i1, i2, man_x_, man_y_);
                    if (isOutLine(i1,i2)==false)
                    {
                        //共分3层，地面，表面，建筑，主角包括在建筑中
                        if (Earth[i1, i2] > 0)
                        {
                            TextureManager.getInstance().renderTexture("mmap", Earth[i1, i2] / 2, p.x, p.y);
                        }
                        if (Surface[i1, i2] > 0)
                        {
                            TextureManager.getInstance().renderTexture("mmap", Surface[i1, i2] / 2, p.x, p.y);
                        }
                        
                        if (Building[i1, i2] > 0)
                        {
                            var t = Building[i1, i2] / 2;
                            //根据图片的宽度计算图的中点, 为避免出现小数, 实际是中点坐标的2倍
                            //次要排序依据是y坐标
                            //直接设置z轴

                            var w = TextureManager.getInstance().map_["mmap"][t].w;
                            var h = TextureManager.getInstance().map_["mmap"][t].h;
                            var dy = TextureManager.getInstance().map_["mmap"][t].dy;

                            int c = ((i1 + i2) - (w + 35) / 36 - (dy - h + 1) / 9) * 1024 + i1;                                 //显示的顺序,楞搞的一个公式

                            tmpDrawInfo.i = t;
                            tmpDrawInfo.p = p;


                            if (map.ContainsKey(2 * c + 1) == false)
                            {
                                map.Add(2 * c + 1, tmpDrawInfo);
                            }
                            else
                            {
                                map[2 * c + 1] = tmpDrawInfo;
                            }

                        }                        
                        
                        if (i1 == man_x_ && i2 == man_x_)
                        {
                            manPicture = MAN_PIC_0 + (int)towards_ * MAN_PIC_COUNT + step_;          //每个方向的第一张是静止图
                            if (restTime >= BEGIN_REST_TIME)
                            {
                                manPicture = MAN_PIC_0 + (int)towards_ * MAN_PIC_COUNT + (restTime - BEGIN_REST_TIME) / REST_INTERVAL % REST_PIC_COUNT;
                            }
                            int c = 1024 * (i1 + i2) + i1 + 2000000;


                            tmpDrawInfo.i = manPicture;
                            tmpDrawInfo.p = p;
                            if (map.ContainsKey(2 * c) == false)
                            {
                                map.Add(2 * c, tmpDrawInfo);
                            }
                            else
                            {
                                map[2 * c] = tmpDrawInfo;
                            }
                        }
                        
                    }
                    k++;
                }
            }
            //按键值排序，由小到大。
            map = map.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);


            foreach (var item in map)
            {
                TextureManager.getInstance().renderTexture("mmap", item.Value.i, item.Value.p.x, item.Value.p.y);
            }

            var t1 = Engine.getInstance().getTicks();


            //云的贴图
            foreach (var c in cloudVector)
            {
                c.draw();
            }

            Engine.getInstance().playMusic(1); //播放背景音乐

        }
       
        public void CloudMove()
        {
            foreach (var c in cloudVector)
            {
                c.ChangePosition();
                c.SetPositionOnScreen(man_x_, man_x_, render_center_x_, render_center_y_);
            }
        }

        /// <summary>
        /// 一维数组填入二维数组
        /// </summary>
        /// <param name="twoDim">二维数组</param>
        /// <param name="oneDim">一维数组</param>
        private void OneDim2TwoDim(ref short[,] twoDim, short[] oneDim)
        {
            int dim = twoDim.GetLength(0);
            int i, j;

            for (i = 0; i < dim; i++)
            {
                for (j = 0; j < dim; j++)
                {
                    twoDim[i, j] = oneDim[i * dim + j];
                }
            }
        }

        
        /// <summary>
        /// 获取入口
        /// </summary>
        public void GetEntrance()
        {
            for (int x = 0; x < maxX; x++)
            {
                for (int y = 0; y < maxY; y++)
                {
                    Entrance[x, y] = -1;
                }
            }
        }

        /// <summary>
        /// 按键值升序排列Dictionary
        /// </summary>
        /// <param name="dic">字典</param>
        /// <returns>键值升序排列的Dictionary</returns>
        private IOrderedEnumerable<KeyValuePair<int, DrawInfo>> DictionarySort(Dictionary<int, DrawInfo> dic)
        {
            return dic.OrderBy(i => i.Key);
        }


        /// <summary>
        /// //计时器，负责画图以及一些其他问题
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">键信息</param>
        public override void dealEvent(SDL_Event e)
        {
            int x = man_x_, y = man_y_;

            if (e.type == (uint)SDL_EventType.SDL_EVENT_KEY_DOWN)
            {
                switch (e.key.key)
                {
                    case SDL_Keycode.SDLK_LEFT:
                        {
                            y--;
                            CheckIsEntrance(x, y);
                            walk(x, y, Towards.LeftUp);
                            StopFindWay();
                            break;
                        }
                    case SDL_Keycode.SDLK_RIGHT:
                        {
                            y++;
                            CheckIsEntrance(x, y);
                            walk(x, y, Towards.RightDown);
                            StopFindWay();
                            break;
                        }
                    case SDL_Keycode.SDLK_UP:
                        {
                            x--;
                            CheckIsEntrance(x, y);
                            walk(x, y, Towards.RightUp);
                            StopFindWay();
                            break;
                        }
                    case SDL_Keycode.SDLK_DOWN:
                        {
                            x++;
                            CheckIsEntrance(x, y);
                            walk(x, y, Towards.LeftDown);
                            StopFindWay();
                            break;
                        }
                    case SDL_Keycode.SDLK_ESCAPE:
                        {
                            StopFindWay();
                            break;
                        }
                    default:
                        {
                            restTime++;
                            break;
                        }
                }
            }

            CloudMove();
        }


        public void walk(int x, int y, Towards t)
        {
            if (canWalk(x, y))
            {
                man_x_ = x;
                man_y_ = y;
            }
            if (towards_ != t)
            {
                towards_ = t;
                //step = 0;
            }
            else
            {
                step_++;
            }
            step_ = step_ % MAN_PIC_COUNT;
            restTime = 0;
        }

        /// <summary>
        /// 检查是否在入口
        /// </summary>
        /// <param name="x">坐标x</param>
        /// <param name="y">坐标y</param>
        /// <returns></returns>
        public bool CheckIsEntrance(int x, int y)
        {
            return false;
        }

        /// <summary>
        /// 停止寻路
        /// </summary>
        public void StopFindWay()
        {
            if (wayQue != null)
            {
                while (wayQue.Count != 0)
                {
                    wayQue.Pop();
                }
            }
        }

        /// <summary>
        /// 检查是否能走
        /// </summary>
        /// <param name="x">坐标x</param>
        /// <param name="y">坐标y</param>
        /// <returns>是否能走</returns>
        public override bool canWalk(int x, int y)
        {
            if (IsBuilding(x, y) || IsOutLine(x, y) || IsWater(x, y))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 检查是否为建筑
        /// </summary>
        /// <param name="x">坐标x</param>
        /// <param name="y">坐标y</param>
        /// <returns>是否为建筑</returns>
        public bool IsBuilding(int x, int y)
        {
            short num = Building[BuildX[x, y], BuildY[x, y]];
            if(num>0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 检查是否越界
        /// </summary>
        /// <param name="x">坐标x</param>
        /// <param name="y">坐标y</param>
        /// <returns>是否越界</returns>
        public bool IsOutLine(int x, int y)
        {
            if (x < 0 || x > maxX || y < 0 || y > maxY)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 检查是否越出屏幕
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool IsOutScreen(int x, int y)
        {
            if (Math.Abs(man_x_ - x) >= 2 * view_width_region_ || Math.Abs(man_y_ - y) >= view_sum_region_)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 检查是否为水
        /// </summary>
        /// <param name="x">坐标x</param>
        /// <param name="y">坐标y</param>
        /// <returns>是否为水</returns>
        public bool IsWater(int x, int y)
        {
            if (Earth[x, y] == 838 || Earth[x, y] >= 612 && Earth[x, y] <= 670)
            {
                return true;
            }
            else if (Earth[x, y] >= 358 && Earth[x, y] <= 362
                || Earth[x, y] >= 506 && Earth[x, y] <= 670
                || Earth[x, y] >= 1016 && Earth[x, y] <= 1022)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

















    }
}
