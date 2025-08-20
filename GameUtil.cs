using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kysSharp
{
    //此类中是一些游戏中的公式，例如使用物品的效果，伤害公式等
    //通常来说应该全部是静态函数
    internal class GameUtil
    {
        static GameUtil game_util_;
        List<int> level_up_list_= new List<int>();

        public static int limit(int current, int min_value, int max_value)
        {
            if (current < min_value) { current = min_value; }
            if (current > max_value) { current = max_value; }
            return current;
        }
    }
}
