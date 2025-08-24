using SDL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kysSharp
{
    //游戏执行和绘制的基础类，凡需要显示画面或者处理事件的，均继承自此
    internal class Element: IDisposable
    {
        private static List<Element> root_ = new List<Element>();   //所有需要绘制的内容都存储在这个静态向量中
        private static int prev_present_ticks_;
        private const int max_delay_ = 10;

        protected List<Element> childs_= new List<Element>();
        protected bool visible_ = true;
        protected int result_ = -1;
        protected bool full_window_ = false;              //true时表示当前画面为起始层，此时低于本层的将不予显示，节省资源
        protected bool exit_ = false;                 //子类的过程中设此值为true，即表示下一个循环将退出
        protected bool running_ = false;
        protected int stay_frame_ = -1;
        protected int current_frame_ = 0;

        protected int x_ = 0;
        protected int y_ = 0;
        protected int w_ = 0;
        protected int h_ = 0;
        protected int pass_child_ = -1;
        protected int press_child_ = -1;
        protected int tag_;

        public State state_ = State.Normal;

        public Element()
        {
            prev_present_ticks_ = 0;
        }

        public void Dispose()
        {
            childs_.Clear();
        }

        //通常来说，部分与操作无关的逻辑放入draw和dealEvent都问题不大，但是建议draw中仅有绘图相关的操作

        public virtual void backRun() { }                                  //一直运行，可以放入总计数器
        public virtual void draw() { }                                     //如何画本节点
        public virtual void dealEvent(SDL_Event e) { }                     //每个循环中处理事件，在子节点需要执行动画时可以不被进行
        public virtual void dealEvent2(SDL_Event e) { }                    //每个循环中处理事件，任何时候都会被执行，可用于制动
        public virtual void onEntrance() { }                               //进入本节点的事件，例如亮屏等
        public virtual void onExit() { }                                   //离开本节点的事件，例如黑屏等

        public virtual void onPressedOK() { }                             //按下回车或鼠标左键的事件，子类视情况继承或者留空
        public virtual void onPressedCancel() { }                         //按下esc或鼠标右键的事件，子类视情况继承或者留空

        public static void drawAll()
        {
            int t0 = (int)Engine.getInstance().getTicks();
            //从最后一个独占屏幕的场景开始画
            int begin_base = 0;
            for (int i = 0; i < root_.Count; i++)    //记录最后一个全屏的层
            {
                root_[i].backRun();
                if (root_[i].full_window_==true)
                {
                    begin_base = i;
                }
            }
            for (int i = begin_base; i < root_.Count; i++)  //从最后一个全屏层开始画
            {
                var b = root_[i];
                if (b.visible_ && !b.exit_)
                {
                    b.drawSelfAndChilds();
                }
            }
        }

        public static void addOnRootTop(Element element) { root_.Add(element); }

        /// <summary>
        /// 从绘制的根节点移除
        /// </summary>
        /// <param name="element">要移除的节点</param>
        /// <returns></returns>
        public static Element? removeFromRoot(Element element)
        {
            if (element == null)
            {
                if (root_.Count > 0)
                {
                    element = root_[^1];              // 取最后一个
                    root_.RemoveAt(root_.Count - 1);   // 删除最后一个
                }
            }
            else
            {
                for (int i = 0; i < root_.Count; i++)
                {
                    if (root_[i] == element)
                    {
                        root_.RemoveAt(i);   // 删除第 i 个元素
                        break;
                    }
                }
            }
            return element;
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="element">子节点</param>
        public void addChild(Element element)
        {
            element.setTag(childs_.Count);
            childs_.Add(element);
        }

        /// <summary>
        /// 添加节点并同时设置子节点的位置
        /// </summary>
        /// <param name="element">子节点</param>
        /// <param name="x">子节点位置x</param>
        /// <param name="y">子节点位置y</param>
        public void addChild(Element element, int x, int y)
        {
            addChild(element);
            element.setPosition(x_ + x, y_ + y);
        }

        public int getChildCount() { return childs_.Count; }

        public Element getChild(int i) { return childs_[i]; }

        /// <summary>
        /// 移除某个节点
        /// </summary>
        /// <param name="element">节点</param>
        public void removeChild(Element element)
        {
            for (int i = 0; i < childs_.Count; i++)
            {
                if (childs_[i] == element)
                {
                    childs_.RemoveAt(i);
                    break;
                }
            }
        }

        //清除子节点
        public void clearChilds()
        {
            childs_.Clear();
        }

        public int getTag() { return tag_; }
        public void setTag(int t) { tag_ = t; }

        public void setPosition(int x, int y)
        {
            foreach (var c in childs_)
            {
                c.setPosition(c.x_ + x - x_, c.y_ + y - y_);
            }
            x_ = x; y_ = y;
        }
        public void setSize(int w, int h) { w_ = w; h_ = h; }
        public void getPosition(int x, int y) { x = x_; y = y_; }
        public void getSize(int w, int h) { w = w_; h = h_; }

        public bool inSide(int x, int y)
        {
            return x > x_ && x < x_ + w_ && y > y_ && y < y_ + h_;
        }

        public int getResult() { return result_; }
        public void setResult(int r) { result_ = r; }
        public bool getVisible() { return visible_; }
        public void setVisible(bool v) { visible_ = v; }

        public State getState() { return state_; }
        public void setState(State s) { state_ = s; }

        public static void clearEvent(SDL_Event e) { e.type = (uint)SDL_EventType.SDL_EVENT_FIRST; }
        static Element getCurrentTopDraw() { return root_.Last(); }
        public void setAllChildState(State s)
        {
            foreach (var c in childs_)
            {
                c.state_ = s;
            }
        }

        public void setAllChildVisible(bool v)
        {
            foreach (var c in childs_)
            {
                c.visible_ = v;
            }
        }

        public int findNextVisibleChild(int i0, int direct)
        {
            if (direct == 0 || childs_.Count == 0) { return i0; }
            direct = direct > 0 ? 1 : -1;

            int i1 = i0;
            for (int i = 1; i < childs_.Count; i++)
            {
                i1 += direct;
                i1 = (i1 + childs_.Count) % childs_.Count;
                if (childs_[i1].visible_)
                {
                    return i1;
                }
            }
            return i0;
        }

        public int findFristVisibleChild()
        {
            for (int i = 0; i < childs_.Count; i++)
            {
                if (childs_[i].visible_)
                {
                    return i;
                }
            }
            return -1;
        }

        public void setExit(bool e) { exit_ = e; }
        public bool isRunning() { return running_; }

        public void exitWithResult(int r) { setExit(true); result_ = r; }

        public int getPassChild() { return pass_child_; }
        public void forcePassChild()
        {
            for (int i = 0; i < childs_.Count; i++)
            {
                childs_[i].setState(State.Normal);
                if (i == pass_child_)
                {
                    childs_[i].setState(State.Pass);
                }
            }
        }
        public int getPressChild() { return press_child_; }
        public void pressToResult() { result_ = press_child_; }

        public void setStayFrame(int s) { stay_frame_ = s; }
        public void checkFrame()
        {
            current_frame_++;
            if (stay_frame_ > 0 && current_frame_ >= stay_frame_)
            {
                exit_ = true;
            }
        }

        public bool isPressOK(SDL_Event e)
        {
            if (e.type == (uint)SDL_EventType.SDL_EVENT_KEY_DOWN)
            {
                if(e.key.key == SDL_Keycode.SDLK_RETURN || e.key.key == SDL_Keycode.SDLK_KP_ENTER || e.key.key== SDL_Keycode.SDLK_SPACE)
                return true; 
            }
            if (e.type == (uint)SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN)
            {
                if(e.button.button==SDL3.SDL_BUTTON_LEFT)
                {
                    return true;
                }                
            }
            return false;
        }
        public bool isPressCancel(SDL_Event e)
        {
            if (e.type == (uint)SDL_EventType.SDL_EVENT_KEY_DOWN)
            {
                if (e.key.key == SDL_Keycode.SDLK_ESCAPE)
                {
                    return true;
                }
            }
            if (e.type == (uint)SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN)
            {
                if (e.button.button == SDL3.SDL_BUTTON_RIGHT)
                {
                    return true;
                }
            }
            return false;
        }

        //画出自身和子节点
        private void drawSelfAndChilds()
        {
            if (visible_)
            {
                draw();
                foreach (var c in childs_)
                {
                    if (c.visible_) { c.drawSelfAndChilds(); }
                }
            }
        }


        /// <summary>
        /// 处理自身的事件响应
        /// 只处理当前的节点和当前节点的子节点，检测鼠标是否在范围内
        /// 注意全屏类的节点要一直接受事件
        /// </summary>
        private void checkStateAndEvent(SDL_Event e)
        {
            if (visible_ || full_window_ != false)
            {
                //注意这里是反向
                for (int i = childs_.Count - 1; i >= 0; i--)
                {
                    childs_[i].checkStateAndEvent(e);
                }

                checkSelfState(e);
                checkChildState();                                              //获取按键和经过的子控件标号
                //可以在dealEvent中改变原有状态，强制设置某些情况
                dealEvent(e);
                //为简化代码，将按下回车和ESC的操作写在此处
                if (isPressOK(e)) { onPressedOK(); }
                if (isPressCancel(e)) { onPressedCancel(); }
            }
            else
            {
                state_ = State.Normal;
            }
        }

        //检测事件并将绘制的图显示出来
        private void checkEventAndPresent(bool check_event)
        {
            SDL_Event e= new SDL_Event();
            var engine = Engine.getInstance();
            //while (engine->pollEvent(e) > 0);  //实际是只要最后一个事件
            engine.pollEvent(ref e);
            if (check_event)
            {
                checkStateAndEvent(e);
            }
            
            dealEvent2(e);
            switch (e.type)
            {
                case (uint)SDL_EventType.SDL_EVENT_QUIT:
                    //UISystem::askExit();
                    break;
                default:
                    break;
            }
            clearEvent(e);
            int t1 = (int)engine.getTicks();
            int t = max_delay_ - (t1 - prev_present_ticks_);
            if (t > max_delay_) { t = max_delay_; }
            if (t <= 0) { t = 1; }
            engine.delay(t);
            engine.renderPresent();
            prev_present_ticks_ = t1;
        }

        private void checkChildState()
        {
            press_child_ = -1;
            //pass_child_ = -1;  注意pass是不改的，维持上一次的状态
            //获取子节点的状态
            for (int i = 0; i < getChildCount(); i++)
            {
                if (getChild(i).getState() == State.Press)
                {
                    press_child_ = i;
                }
                if (getChild(i).getState() == State.Pass)
                {
                    pass_child_ = i;
                }
            }
            if (press_child_ >= 0) { pass_child_ = press_child_; }
        }

        private void checkSelfState(SDL_Event e)
        {
            //检测鼠标经过，按下等状态
            if (e.type == (uint)SDL_EventType.SDL_EVENT_MOUSE_MOTION)
            {
                if (inSide((int)e.motion.x, (int)e.motion.y))
                {
                    state_ = State.Pass;
                }
                else
                {
                    state_ = State.Normal;
                }
            }
            if ((e.type == (uint)SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN || e.type == (uint)SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP)
                && e.button.button == (byte)SDL3.SDL_BUTTON_LEFT)
            {
                if (inSide((int)e.button.x, (int)e.button.y))
                {
                    state_ = State.Press;
                }
                else
                {
                    state_ = State.Normal;
                }
            }
            if ((e.type == (uint)SDL_EventType.SDL_EVENT_KEY_DOWN || e.type == (uint)SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP)
                && (e.key.key == SDL_Keycode.SDLK_RETURN || e.key.key == SDL_Keycode.SDLK_RETURN2 || e.key.key == SDL_Keycode.SDLK_SPACE))
            {
                //按下键盘的空格或者回车时，将pass的按键改为press
                if (state_ == State.Pass)
                {
                    state_ = State.Press;
                }
            }
        }

        /// <summary>
        /// 运行本节点，参数为是否在root中运行，为真则参与绘制，为假则不会被画出
        /// </summary>
        /// <param name="in_root">是否在root中运行。为真则参与绘制，为假则不会被画出。</param>
        /// <returns></returns>
        public int run(bool in_root= true)
        {
            exit_ = false;
            visible_ = true;
            if (in_root) { addOnRootTop(this); }         //按照参数in_root，如果true，就把这个Element加入到root_列表中。基于root_画Element。
            onEntrance();                                //刚运行这个Element，需要运行的东西
            running_ = true;
            while (!exit_)
            {
                if (root_.Count==0) { break; }
                checkEventAndPresent(true);
                drawAll();
                checkFrame();
            }
            running_ = false;
            onExit();
            if (in_root) { removeFromRoot(this); }
            return result_;
        }

        public int runAtPosition(int x = 0, int y = 0, bool in_root = true) { setPosition(x, y); return run(in_root); }

        /// <summary>
        /// 设置从begin开始的全部节点状态为退出
        /// </summary>
        /// <param name="begin">begin开始的节点标号</param>
        public void exitAll(int begin)
        {
            for (int i = begin; i < root_.Count; i++)
            {
                root_[i].exit_ = true;
                foreach (var c in root_[i].childs_)
                {
                    c.exit_ = true;
                }
            }
        }

        /// <summary>
        /// 重复绘制并刷新画面 times 次
        /// 每次循环可以调用一个用户自定义的函数（可选）
        /// 如果 exit_ 触发，则提前退出循环
        /// </summary>
        /// <param name="times"></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /////////////////////////////////////////////////////////////////////////
        // 专门用来在某些情况下做动画的显示和延时
        // 中间可以插入一个回调函数来补充额外逻辑
        /////////////////////////////////////////////////////////////////////////
        public int drawAndPresent(int times, Action<object?>? func=null, object? data = null)
        {
            if (times < 1) return 0;
            if (times > 100) times = 100;
            for (int i = 0; i < times; i++)
            {
                drawAll();   // 绘制所有内容

                // 调用用户回调
                func?.Invoke(data);

                checkEventAndPresent(false);

                if (exit_) break;
            }
            return times;
        }

        /// <summary>
        /// 泛型方法：从 root_ 中获取最后一个能转换为 T 的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T? getPointerFromRoot<T>() where T : class
        {
            for (int i = root_.Count - 1; i >= 0; i--)
            {
                // 尝试转换成 T
                if (root_[i] is T ptr)
                {
                    return ptr; // 找到后返回
                }
            }
            return null; // 没有找到
        }






    }
}
