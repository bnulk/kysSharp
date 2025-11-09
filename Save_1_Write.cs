using kysSharp.SystemUtils;
using kysSharp.Types;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace kysSharp
{
    partial class Save
    {
        int i, j;

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // 主保存函数（核心逻辑）
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        public bool SaveGame(int num)
        {
            string filenamer = GetFilename(num, 'r');
            string filenames = GetFilename(num, 's');
            string filenamed = GetFilename(num, 'd');

            // 主数据区
            byte[] wgrp = new byte[offset_[^1]];

            // 写入grp各个部分
            WriteProtagonistInformationToData(ref wgrp, offset_[0], length_[0], protagonistInformation);
            WriteRolesToData(ref wgrp, offset_[1], length_[1], sizeOfRoleSave, roles_);
            WriteItemsToData(wgrp, offset_[2], length_[2], sizeOfItemSave, items_);
            WriteSubmapInfosToData(wgrp, offset_[3], length_[3], sizeOfSubmapInfoSave, submap_infos_);
            WriteMagicsToData(wgrp, offset_[4], length_[4], sizeOfMagicSave, magics_);
            WriteShopsToData(wgrp, offset_[5], length_[5], sizeOfShopSave, shops_);

            int submap_count = submap_infos_.Count;

            // 分配short数组
            short[] sdata = new short[submap_count * sdata_length_ / 2];
            short[] ddata = new short[submap_count * ddata_length_ / 2];

            // 把数据写入short数组
            WriteSubmapLayerDataToData(sdata, submap_infos_);
            WriteSubmapEventToData(ddata, submap_infos_);

            // 最终写入全部grp
            try
            {
                // 写入文件
                GameFile.WriteFile(filenames, sdata, submap_count * sdata_length_ / 2);
                GameFile.WriteFile(filenamed, ddata, submap_count * ddata_length_ / 2);
                bool ok = GameFile.WriteFile(filenamer, wgrp, offset_[^1]);
                return ok;
            }
            catch
            {
                return false;
            }


        }

        ///////////////////////////////////////////////////////////////////////
        // 函数名称：WriteProtagonistInformationToData
        // 功能说明：将 ProtagonistInformation 中的数据写入 byte[] 数组
        // 参数说明：
        //   bytes —— 目标字节数组
        //   offset —— 写入起始位置
        //   length —— 可用空间长度（字节）
        //   protagonistInformation —— 主角信息结构体
        ///////////////////////////////////////////////////////////////////////
        private void WriteProtagonistInformationToData(ref byte[] bytes, int offset, int length, in ProtagonistInformation protagonistInformation)
        {
            ///////////////////////////////////////////////////////////////////////
            // 1️⃣ 创建临时 int 数组（与 ReadDataToProtagonistInformation 对应）
            ///////////////////////////////////////////////////////////////////////
            int[] s = new int[length / 4];

            s[0] = protagonistInformation.InShip;
            s[1] = protagonistInformation.InSubMap;
            s[2] = protagonistInformation.MainMapX;
            s[3] = protagonistInformation.MainMapY;
            s[4] = protagonistInformation.SubMapX;
            s[5] = protagonistInformation.SubMapY;
            s[6] = protagonistInformation.FaceTowards;
            s[7] = protagonistInformation.ShipX;
            s[8] = protagonistInformation.ShipY;
            s[9] = protagonistInformation.ShipX1;
            s[10] = protagonistInformation.ShipY1;
            s[11] = protagonistInformation.Encode;

            ///////////////////////////////////////////////////////////////////////
            // 2️⃣ 队伍成员（Team[6]）
            ///////////////////////////////////////////////////////////////////////
            for (int i = 0; i < 6; i++)
            {
                s[12 + i] = protagonistInformation.Team[i];
            }

            ///////////////////////////////////////////////////////////////////////
            // 3️⃣ 物品数据（Items[200]）
            // 每个物品包含 item_id 与 count，占 2 个 int
            ///////////////////////////////////////////////////////////////////////
            for (int i = 0; i < 200; i++)
            {
                s[18 + i * 2] = protagonistInformation.Items[i].item_id;
                s[18 + i * 2 + 1] = protagonistInformation.Items[i].count;
            }

            ///////////////////////////////////////////////////////////////////////
            // 4️⃣ 将 int 数组转换为字节数组并写入 bytes[offset]
            ///////////////////////////////////////////////////////////////////////
            byte[] tempBytes = ByteUtils.Int32ToByte(s); // 与 ByteToInt32 相反的函数

            if (tempBytes.Length > length)
                throw new ArgumentException("Write buffer is too small for protagonist data.");

            Array.Copy(tempBytes, 0, bytes, offset, tempBytes.Length);
        }

        ///////////////////////////////////////////////////////////////////////
        // 函数名称：WriteRolesToData
        // 功能说明：将 List<Role> 中的数据按顺序写入 byte[] 数组
        // 参数说明：
        //   bytes —— 目标字节数组
        //   offset —— 写入起始偏移
        //   length —— 可用字节长度
        //   length_one —— 每个 Role 结构占用的字节数
        //   roles_mem_ —— 要写入的角色列表
        ///////////////////////////////////////////////////////////////////////
        private void WriteRolesToData(ref byte[] bytes, int offset, int length, int length_one, in List<Role> roles_mem_)
        {
            int i, j;
            int offsetRole;
            byte[] tmpBytes;
            sbyte[] tmpSbytes;

            ///////////////////////////////////////////////////////////////////////
            // 1️⃣ 创建整型数组（总长度 = length / 4）
            ///////////////////////////////////////////////////////////////////////
            int[] s = new int[length / 4];

            ///////////////////////////////////////////////////////////////////////
            // 2️⃣ 遍历每个角色，写入 s 数组
            ///////////////////////////////////////////////////////////////////////
            for (i = 0; i < roles_mem_.Count; i++)
            {
                var role = roles_mem_[i];
                offsetRole = i * length_one / 4;

                s[offsetRole + 0] = role.ID;
                s[offsetRole + 1] = role.HeadID;
                s[offsetRole + 2] = role.IncLife;
                s[offsetRole + 3] = role.UnUse;

                // 写入 Name（每个 int 对应 4 个 sbyte）
                for (j = 0; j < 5; j++)
                {
                    tmpSbytes = new sbyte[4]
                    {
                role.Name[4 * j],
                role.Name[4 * j + 1],
                role.Name[4 * j + 2],
                role.Name[4 * j + 3]
                    };
                    tmpBytes = ByteUtils.SbyteToByte(tmpSbytes);
                    s[offsetRole + 4 + j] = BitConverter.ToInt32(tmpBytes, 0);
                }

                // 写入 Nick（同上）
                for (j = 0; j < 5; j++)
                {
                    tmpSbytes = new sbyte[4]
                    {
                role.Nick[4 * j],
                role.Nick[4 * j + 1],
                role.Nick[4 * j + 2],
                role.Nick[4 * j + 3]
                    };
                    tmpBytes = ByteUtils.SbyteToByte(tmpSbytes);
                    s[offsetRole + 9 + j] = BitConverter.ToInt32(tmpBytes, 0);
                }

                s[offsetRole + 14] = role.Sexual;
                s[offsetRole + 15] = role.Level;
                s[offsetRole + 16] = role.Exp;
                s[offsetRole + 17] = role.HP;
                s[offsetRole + 18] = role.MaxHP;
                s[offsetRole + 19] = role.Hurt;
                s[offsetRole + 20] = role.Poison;
                s[offsetRole + 21] = role.PhysicalPower;
                s[offsetRole + 22] = role.ExpForMakeItem;
                s[offsetRole + 23] = role.Equip0;
                s[offsetRole + 24] = role.Equip1;

                for (j = 0; j < 15; j++)
                {
                    s[offsetRole + 25 + j] = role.Frame[j];
                }

                s[offsetRole + 40] = role.MPType;
                s[offsetRole + 41] = role.MP;
                s[offsetRole + 42] = role.MaxMP;
                s[offsetRole + 43] = role.Attack;
                s[offsetRole + 44] = role.Speed;
                s[offsetRole + 45] = role.Defence;
                s[offsetRole + 46] = role.Medcine;
                s[offsetRole + 47] = role.UsePoison;
                s[offsetRole + 48] = role.Detoxification;
                s[offsetRole + 49] = role.AntiPoison;
                s[offsetRole + 50] = role.Fist;
                s[offsetRole + 51] = role.Sword;
                s[offsetRole + 52] = role.Knife;
                s[offsetRole + 53] = role.Unusual;
                s[offsetRole + 54] = role.HiddenWeapon;
                s[offsetRole + 55] = role.Knowledge;
                s[offsetRole + 56] = role.Morality;
                s[offsetRole + 57] = role.AttackWithPoison;
                s[offsetRole + 58] = role.AttackTwice;
                s[offsetRole + 59] = role.Fame;
                s[offsetRole + 60] = role.IQ;
                s[offsetRole + 61] = role.PracticeItem;
                s[offsetRole + 62] = role.ExpForItem;

                for (j = 0; j < 10; j++)
                {
                    s[offsetRole + 63 + j] = role.MagicID[j];
                }
                for (j = 0; j < 10; j++)
                {
                    s[offsetRole + 73 + j] = role.MagicLevel[j];
                }
                for (j = 0; j < 4; j++)
                {
                    s[offsetRole + 83 + j] = role.TakingItem[j];
                }
                for (j = 0; j < 4; j++)
                {
                    s[offsetRole + 87 + j] = role.TakingItemCount[j];
                }
            }

            ///////////////////////////////////////////////////////////////////////
            // 3️⃣ 将 int[] → byte[] 并复制到目标 bytes[offset]
            ///////////////////////////////////////////////////////////////////////
            byte[] tmpAllBytes = ByteUtils.Int32ToByte(s);
            if (tmpAllBytes.Length > length)
                throw new ArgumentException("Write buffer is too small for role data.");

            Array.Copy(tmpAllBytes, 0, bytes, offset, tmpAllBytes.Length);
        }

        /////////////////////////////////////////////////////////////////////////
        // 函数：WriteItemsToData
        // 作用：将内存中的 List<Item> 写回到 byte[] 中，用于存档或导出。
        // 参数：
        //   bytes        —— 目标 byte 数组（要写入的数据区域）
        //   offset       —— 写入起始偏移
        //   length       —— 可写入的总长度（通常 = items_mem_.Count * length_one）
        //   length_one   —— 每个 Item 的长度（单位：字节）
        //   items_mem_   —— 要写入的物品列表
        /////////////////////////////////////////////////////////////////////////
        private void WriteItemsToData(byte[] bytes, int offset, int length, int length_one, in List<Item> items_mem_)
        {
            int i, j;
            int offsetItem;
            byte[] tmpBytes;
            sbyte[] tmpSbytes;

            // 把 int[] 映射成底层 32 位整数序列（与读函数相反）
            int[] s = new int[length / 4];
            int count = Math.Min(items_mem_.Count, length / length_one);

            for (i = 0; i < count; i++)
            {
                var item = items_mem_[i];
                offsetItem = i * length_one / 4;

                // ID
                s[offsetItem + 0] = item.ID;

                // Name (40字节 -> 10个int)
                for (j = 0; j < 10; j++)
                {
                    tmpSbytes = new sbyte[4];
                    tmpSbytes[0] = item.Name[4 * j];
                    tmpSbytes[1] = item.Name[4 * j + 1];
                    tmpSbytes[2] = item.Name[4 * j + 2];
                    tmpSbytes[3] = item.Name[4 * j + 3];
                    tmpBytes = ByteUtils.SbyteToByte(tmpSbytes);
                    s[offsetItem + 1 + j] = BitConverter.ToInt32(tmpBytes, 0);
                }

                // Name1
                for (j = 0; j < 10; j++)
                {
                    s[offsetItem + 11 + j] = item.Name1[j];
                }

                // Introduction (60字节 -> 15个int)
                for (j = 0; j < 15; j++)
                {
                    tmpSbytes = new sbyte[4];
                    tmpSbytes[0] = item.Introduction[4 * j];
                    tmpSbytes[1] = item.Introduction[4 * j + 1];
                    tmpSbytes[2] = item.Introduction[4 * j + 2];
                    tmpSbytes[3] = item.Introduction[4 * j + 3];
                    tmpBytes = ByteUtils.SbyteToByte(tmpSbytes);
                    s[offsetItem + 21 + j] = BitConverter.ToInt32(tmpBytes, 0);
                }

                // 普通字段
                s[offsetItem + 36] = item.MagicID;
                s[offsetItem + 37] = item.HiddenWeaponEffectID;
                s[offsetItem + 38] = item.User;
                s[offsetItem + 39] = item.EquipType;
                s[offsetItem + 40] = item.ShowIntroduction;
                s[offsetItem + 41] = item.ItemType;
                s[offsetItem + 42] = item.UnKnown5;
                s[offsetItem + 43] = item.UnKnown6;
                s[offsetItem + 44] = item.UnKnown7;
                s[offsetItem + 45] = item.AddHP;
                s[offsetItem + 46] = item.AddMaxHP;
                s[offsetItem + 47] = item.AddPoison;
                s[offsetItem + 48] = item.AddPhysicalPower;
                s[offsetItem + 49] = item.ChangeMPType;
                s[offsetItem + 50] = item.AddMP;
                s[offsetItem + 51] = item.AddMaxMP;
                s[offsetItem + 52] = item.AddAttack;
                s[offsetItem + 53] = item.AddSpeed;
                s[offsetItem + 54] = item.AddDefence;
                s[offsetItem + 55] = item.AddMedcine;
                s[offsetItem + 56] = item.AddUsePoison;
                s[offsetItem + 57] = item.AddDetoxification;
                s[offsetItem + 58] = item.AddAntiPoison;
                s[offsetItem + 59] = item.AddFist;
                s[offsetItem + 60] = item.AddSword;
                s[offsetItem + 61] = item.AddKnife;
                s[offsetItem + 62] = item.AddUnusual;
                s[offsetItem + 63] = item.AddHiddenWeapon;
                s[offsetItem + 64] = item.AddKnowledge;
                s[offsetItem + 65] = item.AddMorality;
                s[offsetItem + 66] = item.AddAttackTwice;
                s[offsetItem + 67] = item.AddAttackWithPoison;
                s[offsetItem + 68] = item.OnlySuitableRole;
                s[offsetItem + 69] = item.NeedMPType;
                s[offsetItem + 70] = item.NeedMP;
                s[offsetItem + 71] = item.NeedAttack;
                s[offsetItem + 72] = item.NeedSpeed;
                s[offsetItem + 73] = item.NeedUsePoison;
                s[offsetItem + 74] = item.NeedMedcine;
                s[offsetItem + 75] = item.NeedDetoxification;
                s[offsetItem + 76] = item.NeedFist;
                s[offsetItem + 77] = item.NeedSword;
                s[offsetItem + 78] = item.NeedKnife;
                s[offsetItem + 79] = item.NeedUnusual;
                s[offsetItem + 80] = item.NeedHiddenWeapon;
                s[offsetItem + 81] = item.NeedIQ;
                s[offsetItem + 82] = item.NeedExp;
                s[offsetItem + 83] = item.NeedExpForMakeItem;
                s[offsetItem + 84] = item.NeedMaterial;

                // MakeItem[5]
                for (j = 0; j < 5; j++)
                {
                    s[offsetItem + 85 + j] = item.MakeItem[j];
                }

                // MakeItemCount[5]
                for (j = 0; j < 5; j++)
                {
                    s[offsetItem + 90 + j] = item.MakeItemCount[j];
                }
            }

            // 写入 byte 数组
            byte[] newBytes = ByteUtils.Int32ToByte(s);
            Buffer.BlockCopy(newBytes, 0, bytes, offset, length);
        }

        /////////////////////////////////////////////////////////////////////////
        // 函数：WriteSubmapInfosToData
        // 作用：将内存中的 List<SubMapInfo> 写回到 byte[] 数组
        // 参数：
        //   bytes             —— 目标 byte 数组
        //   offset            —— 起始偏移
        //   length            —— 可写总长度
        //   length_one        —— 每个 SubMapInfo 的字节长度
        //   submap_infos_mem_ —— 要写入的子地图信息列表
        /////////////////////////////////////////////////////////////////////////
        private void WriteSubmapInfosToData(byte[] bytes, int offset, int length, int length_one, in List<SubMapInfo> submap_infos_mem_)
        {
            int i, j;
            int offsetSubmapInfo;
            byte[] tmpBytes;
            sbyte[] tmpSbytes;

            int[] s = new int[length / 4];
            int count = Math.Min(submap_infos_mem_.Count, length / length_one);

            for (i = 0; i < count; i++)
            {
                var submapInfo = submap_infos_mem_[i];
                offsetSubmapInfo = i * length_one / 4;

                s[offsetSubmapInfo + 0] = submapInfo.ID;

                // 写 Name（5个int，每个4字节，共20字节）
                for (j = 0; j < 5; j++)
                {
                    tmpSbytes = new sbyte[4]
                    {
                submapInfo.Name[4 * j],
                submapInfo.Name[4 * j + 1],
                submapInfo.Name[4 * j + 2],
                submapInfo.Name[4 * j + 3]
                    };
                    tmpBytes = ByteUtils.SbyteToByte(tmpSbytes);
                    s[offsetSubmapInfo + 1 + j] = BitConverter.ToInt32(tmpBytes, 0);
                }

                s[offsetSubmapInfo + 6] = submapInfo.ExitMusic;
                s[offsetSubmapInfo + 7] = submapInfo.EntranceMusic;
                s[offsetSubmapInfo + 8] = submapInfo.JumpSubMap;
                s[offsetSubmapInfo + 9] = submapInfo.EntranceCondition;
                s[offsetSubmapInfo + 10] = submapInfo.MainEntranceX1;
                s[offsetSubmapInfo + 11] = submapInfo.MainEntranceY1;
                s[offsetSubmapInfo + 12] = submapInfo.MainEntranceX2;
                s[offsetSubmapInfo + 13] = submapInfo.MainEntranceY2;
                s[offsetSubmapInfo + 14] = submapInfo.EntranceX;
                s[offsetSubmapInfo + 15] = submapInfo.EntranceY;

                for (j = 0; j < 3; j++) s[offsetSubmapInfo + 16 + j] = submapInfo.ExitX[j];
                for (j = 0; j < 3; j++) s[offsetSubmapInfo + 19 + j] = submapInfo.ExitY[j];

                s[offsetSubmapInfo + 22] = submapInfo.JumpX;
                s[offsetSubmapInfo + 23] = submapInfo.JumpY;
                s[offsetSubmapInfo + 24] = submapInfo.JumpReturnX;
                s[offsetSubmapInfo + 25] = submapInfo.JumpReturnY;
            }

            byte[] newBytes = ByteUtils.Int32ToByte(s);
            Buffer.BlockCopy(newBytes, 0, bytes, offset, length);
        }

        /////////////////////////////////////////////////////////////////////////
        // 函数：WriteMagicsToData
        // 作用：将内存中的 List<Magic> 写回到 byte[] 数组
        /////////////////////////////////////////////////////////////////////////
        private void WriteMagicsToData(byte[] bytes, int offset, int length, int length_one, in List<Magic> magics_mem_)
        {
            int i, j;
            int offsetMagic;
            byte[] tmpBytes;
            sbyte[] tmpSbytes;

            int[] s = new int[length / 4];
            int count = Math.Min(magics_mem_.Count, length / length_one);

            for (i = 0; i < count; i++)
            {
                var magic = magics_mem_[i];
                offsetMagic = i * length_one / 4;

                s[offsetMagic + 0] = magic.ID;

                // 写 Name（5个int）
                for (j = 0; j < 5; j++)
                {
                    tmpSbytes = new sbyte[4]
                    {
                magic.Name[4 * j],
                magic.Name[4 * j + 1],
                magic.Name[4 * j + 2],
                magic.Name[4 * j + 3]
                    };
                    tmpBytes = ByteUtils.SbyteToByte(tmpSbytes);
                    s[offsetMagic + 1 + j] = BitConverter.ToInt32(tmpBytes, 0);
                }

                for (j = 0; j < 5; j++) s[offsetMagic + 6 + j] = magic.Unknown[j];

                s[offsetMagic + 11] = magic.SoundID;
                s[offsetMagic + 12] = magic.MagicType;
                s[offsetMagic + 13] = magic.EffectID;
                s[offsetMagic + 14] = magic.HurtType;
                s[offsetMagic + 15] = magic.AttackAreaType;
                s[offsetMagic + 16] = magic.NeedMP;
                s[offsetMagic + 17] = magic.WithPoison;

                for (j = 0; j < 10; j++) s[offsetMagic + 18 + j] = magic.Attack[j];
                for (j = 0; j < 10; j++) s[offsetMagic + 28 + j] = magic.SelectDistance[j];
                for (j = 0; j < 10; j++) s[offsetMagic + 38 + j] = magic.AttackDistance[j];
                for (j = 0; j < 10; j++) s[offsetMagic + 48 + j] = magic.AddMP[j];
                for (j = 0; j < 10; j++) s[offsetMagic + 58 + j] = magic.HurtMP[j];
            }

            byte[] newBytes = ByteUtils.Int32ToByte(s);
            Buffer.BlockCopy(newBytes, 0, bytes, offset, length);
        }

        /////////////////////////////////////////////////////////////////////////
        // 函数：WriteShopsToData
        // 作用：将内存中的 List<Shop> 写回到 byte[] 数组
        /////////////////////////////////////////////////////////////////////////
        private void WriteShopsToData(byte[] bytes, int offset, int length, int length_one, in List<Shop> shops_mem_)
        {
            int i, j;
            int offsetShop;
            int[] s = new int[length / 4];
            int count = Math.Min(shops_mem_.Count, length / length_one);

            for (i = 0; i < count; i++)
            {
                var shop = shops_mem_[i];
                offsetShop = i * length_one / 4;

                for (j = 0; j < 5; j++) s[offsetShop + j] = shop.ItemID[j];
                for (j = 0; j < 5; j++) s[offsetShop + 5 + j] = shop.Total[j];
                for (j = 0; j < 5; j++) s[offsetShop + 10 + j] = shop.Price[j];
            }

            byte[] newBytes = ByteUtils.Int32ToByte(s);
            Buffer.BlockCopy(newBytes, 0, bytes, offset, length);
        }

        /////////////////////////////////////////////////////////////////////////
        // 写出子场景图层数据
        /////////////////////////////////////////////////////////////////////////
        private void WriteSubmapLayerDataToData(short[] shorts, in List<SubMapInfo> submapInfos)
        {
            int numberOfEveryLayerInfo = Constant.SUBMAP_COORD_COUNT * Constant.SUBMAP_COORD_COUNT;    // 单层信息数：64*64
            int numberOfEverySubmapInfo = Constant.SUBMAP_LAYER_COUNT * numberOfEveryLayerInfo;         // 单个子场景信息数：6*64*64

            for (int i = 0; i < submapInfos.Count; i++)  // 共84个子场景
            {
                int thisNumberOfEverySubmapInfo = i * numberOfEverySubmapInfo;
                for (int j = 0; j < Constant.SUBMAP_LAYER_COUNT; j++)  // 每个场景6层
                {
                    int thisNumberOfEveryLayerInfo = j * numberOfEveryLayerInfo;
                    for (int k = 0; k < numberOfEveryLayerInfo; k++)  // 每层64*64格
                    {
                        shorts[thisNumberOfEverySubmapInfo + thisNumberOfEveryLayerInfo + k] = submapInfos[i].layer_data_[j, k];
                    }
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////
        // 写出子场景事件数据
        /////////////////////////////////////////////////////////////////////////
        private void WriteSubmapEventToData(short[] shorts, in List<SubMapInfo> submapInfos)
        {
            int numberOfEveryEventInfo = 11;
            int numberOfEverySubmapEvent = numberOfEveryEventInfo * Constant.SUBMAP_EVENT_COUNT;

            for (int i = 0; i < submapInfos.Count; i++)   // 84个子场景
            {
                int thisNumberOfEverySubmapEvent = i * numberOfEverySubmapEvent;
                for (int j = 0; j < Constant.SUBMAP_EVENT_COUNT; j++)  // 每个场景200个事件
                {
                    int thisNumberOfEveryEventInfo = j * numberOfEveryEventInfo;

                    shorts[thisNumberOfEverySubmapEvent + thisNumberOfEveryEventInfo + 0] = submapInfos[i].events_[j].CannotWalk;
                    shorts[thisNumberOfEverySubmapEvent + thisNumberOfEveryEventInfo + 1] = submapInfos[i].events_[j].Index;
                    shorts[thisNumberOfEverySubmapEvent + thisNumberOfEveryEventInfo + 2] = submapInfos[i].events_[j].Event1;
                    shorts[thisNumberOfEverySubmapEvent + thisNumberOfEveryEventInfo + 3] = submapInfos[i].events_[j].Event2;
                    shorts[thisNumberOfEverySubmapEvent + thisNumberOfEveryEventInfo + 4] = submapInfos[i].events_[j].Event3;
                    shorts[thisNumberOfEverySubmapEvent + thisNumberOfEveryEventInfo + 5] = submapInfos[i].events_[j].CurrentPic;
                    shorts[thisNumberOfEverySubmapEvent + thisNumberOfEveryEventInfo + 6] = submapInfos[i].events_[j].EndPic;
                    shorts[thisNumberOfEverySubmapEvent + thisNumberOfEveryEventInfo + 7] = submapInfos[i].events_[j].BeginPic;
                    shorts[thisNumberOfEverySubmapEvent + thisNumberOfEveryEventInfo + 8] = submapInfos[i].events_[j].PicDelay;
                    shorts[thisNumberOfEverySubmapEvent + thisNumberOfEveryEventInfo + 9] = submapInfos[i].events_[j].PosX;  // 注意：反向对应 GetX()
                    shorts[thisNumberOfEverySubmapEvent + thisNumberOfEveryEventInfo + 10] = submapInfos[i].events_[j].PosY; // 注意：反向对应 GetY()
                }
            }
        }


























    }
}
