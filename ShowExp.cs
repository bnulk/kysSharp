using kysSharp.Types;
using SDL;

namespace kysSharp
{
    public class ShowExp: Element
    {
        public List<Role> roles_ { get; private set; } = new List<Role>();

        public ShowExp()
        {
            x_ = 100;
            y_ = 100;
        }

        public override void onPressedOK()
        {
            ExitWithResult(0);
        }

        public override void onPressedCancel()
        {
            ExitWithResult(-1);
        }

        public override void draw()
        {
            // 填充半透明黑色背景
            Engine.getInstance().fillColor(new SDL_Color() { r=0, g=0, b=0, a=192 }, 0, 0, -1, -1);

            for (int i = 0; i < roles_.Count; i++)
            {
                Role r = roles_[i];

                int x = x_ + (i % 3) * 300;
                int y = y_ + (i / 3) * 200;

                // 绘制头像
                TextureManager.getInstance().renderTexture("head", r.HeadID, x, y);

                // 格式化文字
                string text = $"{GameUtil.EraseModredundantChar(r.strName)}獲得經驗{r.ExpGot}";

                // 绘制经验文本
                GameFont.getInstance().draw(text, 20, x, y + 170, new SDL_Color() { r = 255, g = 255, b = 255, a = 255 });
            }
        }

        public void setRoles(List<Role> roles)
        {
            roles_ = roles;
        }
    }
}
