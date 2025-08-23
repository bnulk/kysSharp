using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace kysSharp
{
    internal class UISave: MenuText
    {
        private enum MyIds
        {
            AUTO_SAVE_ID = 11
        }
        private int mode_ = 0;  //0为读档，1为存档


        public UISave()
        {
            // 创建一个字符串列表（相当于 C++ 的 std::vector<std::string>）
            List<string> strings = new List<string>();

            // 遍历 0~10 的存档
            for (int i = 0; i <= 10; i++)
            {
                // 生成字符串，相当于 C++ 的 convert::formatString()
                //string filename = Save.getFilename(i, 'r');
                //string fileTime = FileUtil.getFileTime(filename);
                string fileTime ="fileTime";

                string str = string.Format("進度{0:00}  {1}", i, fileTime);

                // 加入到列表
                strings.Add(str);
            }

            // 生成自动存档信息
            //string autoFile = Save.getFilename(AUTO_SAVE_ID, 'r');
            //string autoTime = FileUtil.getFileTime(autoFile);

            //string autoStr = string.Format("自動檔  {0}", autoTime);
            //strings.Add(autoStr);

            // 设置 UI 上的显示字符串
            setStrings(strings);

            // 屏蔽进度0 (假设 childs_ 是 UI 控件列表)
            if (childs_.Count > 0)
            {
                childs_[0].setVisible(false);
            }

            // 布局 UI
            arrange(0, 0, 0, 28);
        }
        public void setMode(int m) { mode_ = m; }
        public override void onEntrance()
        {
            //存档时屏蔽自动档
            if (mode_ == 1)
            {
                childs_[childs_.Count-1].setVisible(false);
            }
        }
























    }
}
