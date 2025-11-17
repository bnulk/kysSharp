using kysSharp.SystemUtils;
using kysSharp.Types;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace kysSharp
{
    public static class BattleConstant
    {
        public const int BATTLE_ROLE_COUNT = 4096;                    // 战场最大人数
        public const int BATTLEMAP_SAVE_LAYER_COUNT = 2;             // 数据文件存储地图数据层数
        public const int BATTLEMAP_LAYER_COUNT = 8;                  // 战场需要地图层数
        public const int BATTLEMAP_COORD_COUNT = 64;                 // 战场最大坐标
        public const int BATTLE_ENEMY_COUNT = 20;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public unsafe struct BattleInfo
    {
        public short ID;

        // 定长 ANSI 名称（对应 char Name[10]）
        public fixed sbyte Name[10];

        public short BattleFieldID;
        public short Exp;
        public short Music;

        public fixed short TeamMate[Constant.TEAMMATE_COUNT];
        public fixed short AutoTeamMate[Constant.TEAMMATE_COUNT];
        public fixed short TeamMateX[Constant.TEAMMATE_COUNT];
        public fixed short TeamMateY[Constant.TEAMMATE_COUNT];

        public fixed short Enemy[BattleConstant.BATTLE_ENEMY_COUNT];
        public fixed short EnemyX[BattleConstant.BATTLE_ENEMY_COUNT];
        public fixed short EnemyY[BattleConstant.BATTLE_ENEMY_COUNT];
    }

    //这个仅保存战场前两层
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BattleFieldData2
    {
        // 使用 fixed 数组代替托管类型
        public fixed short data[BattleConstant.BATTLEMAP_SAVE_LAYER_COUNT * BattleConstant.BATTLEMAP_COORD_COUNT * BattleConstant.BATTLEMAP_COORD_COUNT];

        // 提供索引器以便访问二维数据
        public short this[int layer, int index]
        {
            get
            {
                int offset = layer * BattleConstant.BATTLEMAP_COORD_COUNT * BattleConstant.BATTLEMAP_COORD_COUNT + index;
                fixed (short* ptr = data)
                {
                    return ptr[offset];
                }
            }
            set
            {
                int offset = layer * BattleConstant.BATTLEMAP_COORD_COUNT * BattleConstant.BATTLEMAP_COORD_COUNT + index;
                fixed (short* ptr = data)
                {
                    ptr[offset] = value;
                }
            }
        }
    }


    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// BattleMap 类：用于加载并保存战斗地图数据（对应 C++ BattleMap）
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    public class BattleMap
    {
        ////////////////////////////////////////////////////////////////
        // 成员变量
        ////////////////////////////////////////////////////////////////
        private List<BattleInfo> battleInfos = new();
        private List<BattleFieldData2> battleFieldData2 = new();

        private static readonly BattleMap instance = new BattleMap();

        ////////////////////////////////////////////////////////////////
        // 单例访问
        ////////////////////////////////////////////////////////////////
        public static BattleMap? battleMap_;
        public static BattleMap getInstance()
        {
            if (battleMap_ == null)
            {
                battleMap_ = new BattleMap();
                return battleMap_;
            }
            return battleMap_;
        }

        ////////////////////////////////////////////////////////////////
        // 构造函数
        ////////////////////////////////////////////////////////////////
        public unsafe BattleMap()
        {
            // 1️⃣ 读取 war.sta 文件
            battleInfos = GameFile.ReadFileToList<BattleInfo>(Path.Combine("game", "resource", "war.sta"));

            // 2️⃣ 读取 warfld.idx / warfld.grp 对应数据
            List<int> offset = new List<int>();
            List<int> length = new List<int>();
            byte[] battleMap = GameFile.GetIdxContent(
                Path.Combine("game", "resource", "warfld.idx"),
                Path.Combine("game", "resource", "warfld.grp"),
                ref offset,
                ref length
            );

            battleFieldData2 = new List<BattleFieldData2>(length.Count);
            for (int i = 0; i < length.Count; i++)
            {
                BattleFieldData2 field = new();
                int structSize = Marshal.SizeOf<BattleFieldData2>();
                if (offset[i] + structSize <= battleMap.Length)
                {
                    field = GameFile.BytesToStruct<BattleFieldData2>(battleMap, offset[i]);
                }
                battleFieldData2.Add(field);
            }

            // 3️⃣ 转换编码 CP950 → CP936
            foreach (var info in battleInfos)
            {
                PotConv.Cp950ToCp936(info.Name, 0, 10);
            }
        }

        ////////////////////////////////////////////////////////////////
        // 析构函数（可选）
        ////////////////////////////////////////////////////////////////
        ~BattleMap() { }

        ////////////////////////////////////////////////////////////////
        // 获取战斗信息
        ////////////////////////////////////////////////////////////////
        public BattleInfo? GetBattleInfo(int i)
        {
            if (i < 0 || i >= battleInfos.Count)
                return null;
            return battleInfos[i];
        }

        ////////////////////////////////////////////////////////////////
        // 拷贝地图层数据
        ////////////////////////////////////////////////////////////////
        public void CopyLayerData(int battleFieldId, int layer, MapSquare output)
        {
            if (battleFieldId < 0 || battleFieldId >= battleFieldData2.Count)
                return;

            var layerData = battleFieldData2[battleFieldId];
            int count = BattleConstant.BATTLEMAP_COORD_COUNT * BattleConstant.BATTLEMAP_COORD_COUNT;

            for (int i = 0; i < count; i++)
            {
                output.Data_[i] = layerData[layer, i];
            }
        }
    }





}
