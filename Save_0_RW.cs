using kysSharp.SystemUtils;
using kysSharp.Types;
using System;
using System.Collections.Generic;

namespace kysSharp
{
    partial class Save
    {
        private void ReadDataToProtagonistInformation(byte[] bytes, int offset, int length, ref ProtagonistInformation protagonistInformation)
        {
            int i;
            int[] s = new int[length / 4];
            s = ByteUtils.ByteToInt32(bytes, offset, length);

            protagonistInformation.InShip = s[0];
            protagonistInformation.InSubMap = s[1];
            protagonistInformation.MainMapX = s[2];
            protagonistInformation.MainMapY = s[3];
            protagonistInformation.SubMapX = s[4];
            protagonistInformation.SubMapY = s[5];
            protagonistInformation.FaceTowards = s[6];
            protagonistInformation.ShipX = s[7];
            protagonistInformation.ShipY = s[8];
            protagonistInformation.ShipX1 = s[9];
            protagonistInformation.ShipY1 = s[10];
            protagonistInformation.Encode = s[11];

            for (i = 0; i < 6; i++)
            {
                protagonistInformation.Team[i] = s[12 + i];
            }

            for (i = 0; i < 200; i++)
            {
                protagonistInformation.Items[i].item_id = s[18 + i * 2];
                protagonistInformation.Items[i].count = s[18 + i * 2 + 1];
            }
        }

        private void ReadDataToRoles(byte[] bytes, int offset, int length, int length_one, ref List<Role> roles_mem_)
        {
            int i, j;
            int offsetRole;
            byte[] tmpBytes;
            sbyte[] tmpSbytes;
            short[] tmpChar = new short[4];
            Role role = new Role();
            roles_mem_ = new List<Role>();



            int[] s = new int[length / 4];
            s = ByteUtils.ByteToInt32(bytes, offset, length);

            int count = length / length_one;
            for (i = 0; i < count; i++)
            {
                role = new Role();
                offsetRole = i * length_one / 4;

                role.ID = s[offsetRole + 0];
                role.HeadID = s[offsetRole + 1];
                role.IncLife = s[offsetRole + 2];
                role.UnUse = s[offsetRole + 3];
                for (j = 0; j < 5; j++)
                {
                    tmpBytes = BitConverter.GetBytes(s[offsetRole + 4 + j]);
                    tmpSbytes = ByteUtils.ByteToSbyte(tmpBytes);
                    role.Name[4 * j] = tmpSbytes[0];
                    role.Name[4 * j + 1] = tmpSbytes[1];
                    role.Name[4 * j + 2] = tmpSbytes[2];
                    role.Name[4 * j + 3] = tmpSbytes[3];
                }
                for (j = 0; j < 5; j++)
                {
                    tmpBytes = BitConverter.GetBytes(s[offsetRole + 9 + j]);
                    tmpSbytes = ByteUtils.ByteToSbyte(tmpBytes);
                    role.Nick[4 * j] = tmpSbytes[0];
                    role.Nick[4 * j + 1] = tmpSbytes[1];
                    role.Nick[4 * j + 2] = tmpSbytes[2];
                    role.Nick[4 * j + 3] = tmpSbytes[3];
                }
                role.Sexual = s[offsetRole + 14];
                role.Level = s[offsetRole + 15];
                role.Exp = s[offsetRole + 16];
                role.HP = s[offsetRole + 17];
                role.MaxHP = s[offsetRole + 18];
                role.Hurt = s[offsetRole + 19];
                role.Poison = s[offsetRole + 20];
                role.PhysicalPower = s[offsetRole + 21];
                role.ExpForMakeItem = s[offsetRole + 22];
                role.Equip0 = s[offsetRole + 23];
                role.Equip1 = s[offsetRole + 24];
                for (j = 0; j < 15; j++)
                {
                    role.Frame[j] = s[offsetRole + 25 + j];
                }
                role.MPType = s[offsetRole + 40];
                role.MP = s[offsetRole + 41];
                role.MaxMP = s[offsetRole + 42];
                role.Attack = s[offsetRole + 43];
                role.Speed = s[offsetRole + 44];
                role.Defence = s[offsetRole + 45];
                role.Medcine = s[offsetRole + 46];
                role.UsePoison = s[offsetRole + 47];
                role.Detoxification = s[offsetRole + 48];
                role.AntiPoison = s[offsetRole + 49];
                role.Fist = s[offsetRole + 50];
                role.Sword = s[offsetRole + 51];
                role.Knife = s[offsetRole + 52];
                role.Unusual = s[offsetRole + 53];
                role.HiddenWeapon = s[offsetRole + 54];
                role.Knowledge = s[offsetRole + 55];
                role.Morality = s[offsetRole + 56];
                role.AttackWithPoison = s[offsetRole + 57];
                role.AttackTwice = s[offsetRole + 58];
                role.Fame = s[offsetRole + 59];
                role.IQ = s[offsetRole + 60];
                role.PracticeItem = s[offsetRole + 61];
                role.ExpForItem = s[offsetRole + 62];
                for (j = 0; j < 10; j++)
                {
                    role.MagicID[j] = s[offsetRole + 63 + j];
                }
                for (j = 0; j < 10; j++)
                {
                    role.MagicLevel[j] = s[offsetRole + 73 + j];
                }
                for (j = 0; j < 4; j++)
                {
                    role.TakingItem[j] = s[offsetRole + 83 + j];
                }
                for (j = 0; j < 4; j++)
                {
                    role.TakingItemCount[j] = s[offsetRole + 87 + j];
                }
                roles_mem_.Add(role);
            }
        }

        private void ReadDataToItems(byte[] bytes, int offset, int length, int length_one, ref List<Item> items_mem_)
        {
            int i, j;
            int offsetItem;
            byte[] tmpBytes;
            sbyte[] tmpSbytes;
            short[] tmpChar = new short[4];
            Item item = new Item();
            items_mem_ = new List<Item>();



            int[] s = new int[length / 4];
            s = ByteUtils.ByteToInt32(bytes, offset, length);

            int count = length / length_one;
            for (i = 0; i < count; i++)
            {
                item = new Item();
                offsetItem = i * length_one / 4;

                item.ID = s[offsetItem + 0];
                for (j = 0; j < 10; j++)
                {
                    tmpBytes = BitConverter.GetBytes(s[offsetItem + 1 + j]);
                    tmpSbytes = ByteUtils.ByteToSbyte(tmpBytes);
                    item.Name[4 * j] = tmpSbytes[0];
                    item.Name[4 * j + 1] = tmpSbytes[1];
                    item.Name[4 * j + 2] = tmpSbytes[2];
                    item.Name[4 * j + 3] = tmpSbytes[3];
                }
                for (j = 0; j < 10; j++)
                {
                    item.Name1[j] = s[offsetItem + 11 + j];
                }
                for (j = 0; j < 15; j++)
                {
                    tmpBytes = BitConverter.GetBytes(s[offsetItem + 21 + j]);
                    tmpSbytes = ByteUtils.ByteToSbyte(tmpBytes);
                    item.Introduction[4 * j] = tmpSbytes[0];
                    item.Introduction[4 * j + 1] = tmpSbytes[1];
                    item.Introduction[4 * j + 2] = tmpSbytes[2];
                    item.Introduction[4 * j + 3] = tmpSbytes[3];
                }
                item.MagicID = s[offsetItem + 36];
                item.HiddenWeaponEffectID = s[offsetItem + 37];
                item.User = s[offsetItem + 38];
                item.EquipType = s[offsetItem + 39];
                item.ShowIntroduction = s[offsetItem + 40];
                item.ItemType = s[offsetItem + 41];
                item.UnKnown5 = s[offsetItem + 42];
                item.UnKnown6 = s[offsetItem + 43];
                item.UnKnown7 = s[offsetItem + 44];
                item.AddHP = s[offsetItem + 45];
                item.AddMaxHP = s[offsetItem + 46];
                item.AddPoison = s[offsetItem + 47];
                item.AddPhysicalPower = s[offsetItem + 48];
                item.ChangeMPType = s[offsetItem + 49];
                item.AddMP = s[offsetItem + 50];
                item.AddMaxMP = s[offsetItem + 51];
                item.AddAttack = s[offsetItem + 52];
                item.AddSpeed = s[offsetItem + 53];
                item.AddDefence = s[offsetItem + 54];
                item.AddMedcine = s[offsetItem + 55];
                item.AddUsePoison = s[offsetItem + 56];
                item.AddDetoxification = s[offsetItem + 57];
                item.AddAntiPoison = s[offsetItem + 58];
                item.AddFist = s[offsetItem + 59];
                item.AddSword = s[offsetItem + 60];
                item.AddKnife = s[offsetItem + 61];
                item.AddUnusual = s[offsetItem + 62];
                item.AddHiddenWeapon = s[offsetItem + 63];
                item.AddKnowledge = s[offsetItem + 64];
                item.AddMorality = s[offsetItem + 65];
                item.AddAttackTwice = s[offsetItem + 66];
                item.AddAttackWithPoison = s[offsetItem + 67];
                item.OnlySuitableRole = s[offsetItem + 68];
                item.NeedMPType = s[offsetItem + 69];
                item.NeedMP = s[offsetItem + 70];
                item.NeedAttack = s[offsetItem + 71];
                item.NeedSpeed = s[offsetItem + 72];
                item.NeedUsePoison = s[offsetItem + 73];
                item.NeedMedcine = s[offsetItem + 74];
                item.NeedDetoxification = s[offsetItem + 75];
                item.NeedFist = s[offsetItem + 76];
                item.NeedSword = s[offsetItem + 77];
                item.NeedKnife = s[offsetItem + 78];
                item.NeedUnusual = s[offsetItem + 79];
                item.NeedHiddenWeapon = s[offsetItem + 80];
                item.NeedIQ = s[offsetItem + 81];
                item.NeedExp = s[offsetItem + 82];
                item.NeedExpForMakeItem = s[offsetItem + 83];
                item.NeedMaterial = s[offsetItem + 84];
                for (j = 0; j < 5; j++)
                {
                    item.MakeItem[j] = s[offsetItem + 85 + j];
                }
                for (j = 0; j < 5; j++)
                {
                    item.MakeItemCount[j] = s[offsetItem + 90 + j];
                }

                items_mem_.Add(item);
            }
        }

        private void ReadDataToSubmapInfos(byte[] bytes, int offset, int length, int length_one, ref List<SubMapInfo> submap_infos_mem_)
        {
            int i, j;
            int offsetSubmapInfo;
            byte[] tmpBytes;
            sbyte[] tmpSbytes;
            short[] tmpChar = new short[4];
            SubMapInfo submapInfo = new SubMapInfo();
            submap_infos_mem_ = new List<SubMapInfo>();

            int[] s = new int[length / 4];
            s = ByteUtils.ByteToInt32(bytes, offset, length);

            int count = length / length_one;
            for (i = 0; i < count; i++)
            {
                submapInfo = new SubMapInfo();
                offsetSubmapInfo = i * length_one / 4;

                submapInfo.ID = s[offsetSubmapInfo + 0];
                for (j = 0; j < 5; j++)
                {
                    tmpBytes = BitConverter.GetBytes(s[offsetSubmapInfo + 1 + j]);
                    tmpSbytes = ByteUtils.ByteToSbyte(tmpBytes);
                    submapInfo.Name[4 * j] = tmpSbytes[0];
                    submapInfo.Name[4 * j + 1] = tmpSbytes[1];
                    submapInfo.Name[4 * j + 2] = tmpSbytes[2];
                    submapInfo.Name[4 * j + 3] = tmpSbytes[3];
                }
                submapInfo.ExitMusic = s[offsetSubmapInfo + 6];
                submapInfo.EntranceMusic = s[offsetSubmapInfo + 7];
                submapInfo.JumpSubMap = s[offsetSubmapInfo + 8];
                submapInfo.EntranceCondition = s[offsetSubmapInfo + 9];
                submapInfo.MainEntranceX1 = s[offsetSubmapInfo + 10];
                submapInfo.MainEntranceY1 = s[offsetSubmapInfo + 11];
                submapInfo.MainEntranceX2 = s[offsetSubmapInfo + 12];
                submapInfo.MainEntranceY2 = s[offsetSubmapInfo + 13];
                submapInfo.EntranceX = s[offsetSubmapInfo + 14];
                submapInfo.EntranceY = s[offsetSubmapInfo + 15];
                for (j = 0; j < 3; j++)
                {
                    submapInfo.ExitX[j] = s[offsetSubmapInfo + 16 + j];
                }
                for (j = 0; j < 3; j++)
                {
                    submapInfo.ExitY[j] = s[offsetSubmapInfo + 19 + j];
                }
                submapInfo.JumpX = s[offsetSubmapInfo + 22];
                submapInfo.JumpY = s[offsetSubmapInfo + 23];
                submapInfo.JumpReturnX = s[offsetSubmapInfo + 24];
                submapInfo.JumpReturnY = s[offsetSubmapInfo + 25];

                submap_infos_mem_.Add(submapInfo);
            }
        }

        private void ReadDataToMagics(byte[] bytes, int offset, int length, int length_one, ref List<Magic> magics_mem_)
        {
            int i, j;
            int offsetMagic;
            byte[] tmpBytes;
            sbyte[] tmpSbytes;
            short[] tmpChar = new short[4];
            Magic magic = new Magic();
            magics_mem_ = new List<Magic>();

            int[] s = new int[length / 4];
            s = ByteUtils.ByteToInt32(bytes, offset, length);

            int count = length / length_one;
            for (i = 0; i < count; i++)
            {
                magic = new Magic();
                offsetMagic = i * length_one / 4;

                magic.ID = s[offsetMagic + 0];
                for (j = 0; j < 5; j++)
                {
                    tmpBytes = BitConverter.GetBytes(s[offsetMagic + 1 + j]);
                    tmpSbytes = ByteUtils.ByteToSbyte(tmpBytes);
                    magic.Name[4 * j] = tmpSbytes[0];
                    magic.Name[4 * j + 1] = tmpSbytes[1];
                    magic.Name[4 * j + 2] = tmpSbytes[2];
                    magic.Name[4 * j + 3] = tmpSbytes[3];
                }
                for (j = 0; j < 5; j++)
                {
                    magic.Unknown[j] = s[offsetMagic + 6 + j];
                }
                magic.SoundID = s[offsetMagic + 11];
                magic.MagicType = s[offsetMagic + 12];
                magic.EffectID = s[offsetMagic + 13];
                magic.HurtType = s[offsetMagic + 14];
                magic.AttackAreaType = s[offsetMagic + 15];
                magic.NeedMP = s[offsetMagic + 16];
                magic.WithPoison = s[offsetMagic + 17];
                for (j = 0; j < 10; j++)
                {
                    magic.Attack[j] = s[offsetMagic + 18 + j];
                }
                for (j = 0; j < 10; j++)
                {
                    magic.SelectDistance[j] = s[offsetMagic + 28 + j];
                }
                for (j = 0; j < 10; j++)
                {
                    magic.AttackDistance[j] = s[offsetMagic + 38 + j];
                }
                for (j = 0; j < 10; j++)
                {
                    magic.AddMP[j] = s[offsetMagic + 48 + j];
                }
                for (j = 0; j < 10; j++)
                {
                    magic.HurtMP[j] = s[offsetMagic + 58 + j];
                }

                magics_mem_.Add(magic);
            }
        }

        private void ReadDataToShops(byte[] bytes, int offset, int length, int length_one, ref List<Shop> shops_mem_)
        {
            int i, j;
            int offsetShop;
            Shop shop = new Shop();
            shops_mem_ = new List<Shop>();

            int[] s = new int[length / 4];
            s = ByteUtils.ByteToInt32(bytes, offset, length);

            int count = length / length_one;
            for (i = 0; i < count; i++)
            {
                shop = new Shop();
                offsetShop = i * length_one / 4;

                for (j = 0; j < 5; j++)
                {
                    shop.ItemID[j] = s[offsetShop + j];
                }
                for (j = 0; j < 5; j++)
                {
                    shop.Total[j] = s[offsetShop + 5 + j];
                }
                for (j = 0; j < 5; j++)
                {
                    shop.Price[j] = s[offsetShop + 10 + j];
                }

                shops_mem_.Add(shop);
            }
        }

        private void ReadDataToSubmapLayerData(short[] shorts, ref List<SubMapInfo> submapInfos)
        {
            int numberOfEveryLayerInfo = Constant.SUBMAP_COORD_COUNT * Constant.SUBMAP_COORD_COUNT;                        //单层信息数目为64*64
            int numberOfEverySubmapInfo = Constant.SUBMAP_LAYER_COUNT * numberOfEveryLayerInfo;                           //单个子场景数目信息6*64*64

            for (int i = 0; i < submapInfos.Count; i++)                         //84
            {
                int thisNumberOfEverySubmapInfo = i * numberOfEverySubmapInfo;
                for (int j = 0; j < Constant.SUBMAP_LAYER_COUNT; j++)           //6
                {
                    int thisNumberOfEveryLayerInfo = j * numberOfEveryLayerInfo;
                    for (int k = 0; k < numberOfEveryLayerInfo; k++)                        //单层信息数目为64*64
                    {
                        submapInfos[i].layer_data_[j, k] = shorts[thisNumberOfEverySubmapInfo + thisNumberOfEveryLayerInfo + k];
                    }
                }
            }
        }

        private void ReadDataToSubmapEvent(short[] shorts, ref List<SubMapInfo> submapInfos)
        {
            int numberOfEveryEventInfo = 11;
            int numberOfEverySubmapEvent = 11 * Constant.SUBMAP_EVENT_COUNT;

            for (int i = 0; i < submapInfos.Count; i++)                          //84
            {
                int thisNumberOfEverySubmapEvent = i * numberOfEverySubmapEvent;
                for (int j = 0; j < Constant.SUBMAP_EVENT_COUNT; j++)            //200
                {
                    int thisNumberOfEveryEventInfo = j * numberOfEveryEventInfo;
                    submapInfos[i].events_[j] = new SubmapEvent();
                    submapInfos[i].events_[j].CannotWalk = shorts[thisNumberOfEverySubmapEvent + thisNumberOfEveryEventInfo + 0];
                    submapInfos[i].events_[j].Index = shorts[thisNumberOfEverySubmapEvent + thisNumberOfEveryEventInfo + 1];
                    submapInfos[i].events_[j].Event1 = shorts[thisNumberOfEverySubmapEvent + thisNumberOfEveryEventInfo + 2];
                    submapInfos[i].events_[j].Event2 = shorts[thisNumberOfEverySubmapEvent + thisNumberOfEveryEventInfo + 3];
                    submapInfos[i].events_[j].Event3 = shorts[thisNumberOfEverySubmapEvent + thisNumberOfEveryEventInfo + 4];
                    submapInfos[i].events_[j].CurrentPic = shorts[thisNumberOfEverySubmapEvent + thisNumberOfEveryEventInfo + 5];
                    submapInfos[i].events_[j].EndPic = shorts[thisNumberOfEverySubmapEvent + thisNumberOfEveryEventInfo + 6];
                    submapInfos[i].events_[j].BeginPic = shorts[thisNumberOfEverySubmapEvent + thisNumberOfEveryEventInfo + 7];
                    submapInfos[i].events_[j].PicDelay = shorts[thisNumberOfEverySubmapEvent + thisNumberOfEveryEventInfo + 8];
                    submapInfos[i].events_[j].GetX(shorts[thisNumberOfEverySubmapEvent + thisNumberOfEveryEventInfo + 9]);
                    submapInfos[i].events_[j].GetY(shorts[thisNumberOfEverySubmapEvent + thisNumberOfEveryEventInfo + 10]);
                }
            }
        }















    }
}
