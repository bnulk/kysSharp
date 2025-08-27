using System;

namespace kysSharp.Types
{
    /// <summary>
    /// 实际中的武学数据
    /// </summary>
    [Serializable]
    public class Magic : MagicSave
    {
        public int CalNeedMP(int level_index)
        {
            return NeedMP * ((level_index + 2) / 2);
        }

        //需要补充
        public int CalMaxLevelIndexByMP(int mp, int max_level)
        {
            max_level = GameUtil.limit(max_level, 0, Constant.MAX_MAGIC_LEVEL_INDEX);
            int level = GameUtil.limit(mp / (NeedMP * 2) * 2 - 1, 0, max_level);
            return level;
        }
    }
}
