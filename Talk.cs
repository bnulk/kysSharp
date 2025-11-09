using SDL;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

namespace kysSharp
{
    internal class Talk: Element
    {
        ///////////////////////////////////////////////////////////////////////
        // 私有成员变量
        ///////////////////////////////////////////////////////////////////////
        private string content_ = "";
        private int head_id_ = -1;
        private int head_style_ = 0;
        private int current_line_ = 0;
        private int width_ = 20;
        private int height_ = 5;
        private List<string> contents_ = new List<string>();

        ///////////////////////////////////////////////////////////////////////
        // 构造函数
        ///////////////////////////////////////////////////////////////////////
        public Talk() { }

        public Talk(string c, int h = -1) : this()
        {
            setContent(c);
            setHeadID(h);
        }

        ///////////////////////////////////////////////////////////////////////
        // 析构函数（C# 使用析构器或 Dispose 模式）
        ///////////////////////////////////////////////////////////////////////
        ~Talk() { }

        ///////////////////////////////////////////////////////////////////////
        // 公有接口函数（对应 C++ 的 setter）
        ///////////////////////////////////////////////////////////////////////
        public void setContent(string c)
        {
            content_ = c;
        }

        public void setHeadID(int h)
        {
            head_id_ = h;
        }

        public void setHeadStyle(int s)
        {
            head_style_ = s;
        }

        ///////////////////////////////////////////////////////////////////////
        // 绘制对话框内容（对应 C++: void Talk::draw()）
        ///////////////////////////////////////////////////////////////////////
        public override void draw()
        {
            // 若没有内容则不绘制
            if (string.IsNullOrEmpty(content_))
                return;

            // 绘制半透明背景框
            Engine.getInstance().fillColor(new SDL_Color() { r=0,g=0,b=0,a=128} , x_ + 225, y_ + 65, 530, 150);

            // 绘制头像（根据样式判断左右位置）
            if (head_id_ >= 0)
            {
                if (head_style_ == 0)
                {
                    TextureManager.getInstance().renderTexture("head", head_id_, x_ + 50, y_ + 50);
                }
                else
                {
                    TextureManager.getInstance().renderTexture("head", head_id_, x_ + 770, y_ + 50);
                }
            }

            // 计算要绘制的行数范围
            int end_line = current_line_ + height_;
            if (end_line > contents_.Count)
                end_line = contents_.Count;

            // 绘制每一行文字
            for (int i = current_line_; i < end_line; i++)
            {
                GameFont.getInstance().draw(
                    contents_[i],
                    24,
                    x_ + 250,
                    y_ + 75 + 25 * (i - current_line_),
                    new SDL_Color() { r = 255, g = 255, b = 255, a = 255 }
                );
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // 处理键盘事件（对应 C++: void Talk::dealEvent(BP_Event& e)）
        ///////////////////////////////////////////////////////////////////////
        public override void dealEvent(SDL_Event e)
        {
            if (e.type == (int)SDL_EventType.SDL_EVENT_KEY_UP)
            {
                if (current_line_ + height_ >= contents_.Count)
                {
                    setExit(true); // 对话结束
                }
                else
                {
                    current_line_ += height_; // 显示下一页
                }
                e.type = 0; // 重置事件类型
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // 进入场景时的初始化逻辑（对应 C++: void Talk::onEntrance()）
        ///////////////////////////////////////////////////////////////////////
        public override void onEntrance()
        {
            contents_.Clear();
            current_line_ = 0;

            // 将 content_ 按每行宽度 width_ 分割成多行文字
            for (int i = 0; i < content_.Length; i += width_)
            {
                int len = width_;
                if (i + len >= content_.Length)
                    len = content_.Length - i;

                contents_.Add(content_.Substring(i, len));
            }
        }











    }
}
