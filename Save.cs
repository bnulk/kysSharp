using kysSharp.Types;
using System;
using System.Collections.Generic;

namespace kysSharp
{

    partial class Save
    {
        public const int sizeOfProtagonistInformation = 418;                      //418个整型变量，418*4=1672字节。
        public const int sizeOfRoleSave = 364;
        public const int sizeOfItemSave = 380;
        public const int sizeOfSubmapInfoSave = 104;
        public const int sizeOfMagicSave = 272;
        public const int sizeOfShopSave = 60;
        public const int sdata_length_ = 49152;    //sdata_length_ = sizeof(MAP_INT) * Constant.SUBMAP_LAYER_COUNT * Constant.SUBMAP_COORD_COUNT * Constant.SUBMAP_COORD_COUNT;
        public const int ddata_length_ = 4400;     //ddata_length_ = sizeof(SubMapEvent) * Constant.SUBMAP_EVENT_COUNT;



        public ProtagonistInformation protagonistInformation = new ProtagonistInformation();

        //缓冲区，无他用
        private int[] buffer_ = new int[100];


        public static Save save_ = new Save();
        public static Save GetInstance()
        {
            return save_;
        }

        //注意在读取之后，offset比length尾部会多一个元素，该值即总长度
        private List<int> offset_ = new List<int>();
        private List<int> length_ = new List<int>();


        private List<Role> roles_ = new List<Role>();
        private List<Magic> magics_ = new List<Magic>();
        private List<Item> items_ = new List<Item>();
        private List<SubmapInfo> submap_infos_ = new List<SubmapInfo>();
        private List<Shop> shops_ = new List<Shop>();

        private Dictionary<string, Role> roles_by_name_ = new Dictionary<string, Role>();
        private Dictionary<string, Item> items_by_name_ = new Dictionary<string, Item>();
        private Dictionary<string, Magic> magics_by_name_ = new Dictionary<string, Magic>();
        private Dictionary<string, SubmapInfo> submap_infos_by_name_ = new Dictionary<string, SubmapInfo>();




        public void SetSavePointer(ref List<Save> v, int size)
        {

        }

        public void ToPtrVector(ref List<Save> v, ref List<Save> v_ptr)
        {

        }


        public Role GetRole(int i) { if (i < 0 || i >= roles_.Count) { return null; } return roles_[i]; }
        public Magic GetMagic(int i) { if (i <= 0 || i >= magics_.Count) { return null; } return magics_[i]; }  //0号武功无效
        public Item GetItem(int i) { if (i < 0 || i >= items_.Count) { return null; } return items_[i]; }
        public SubmapInfo GetSubMapInfo(int i) { if (i < 0 || i >= submap_infos_.Count) { return null; } return submap_infos_[i]; }
        public Shop GetShop(int i) { if (i < 0 || i >= shops_.Count) { return null; } return shops_[i]; }

        public int GetTeamMateID(int i) { return protagonistInformation.Team[i]; }
        public int GetMoneyCountInBag() { return GetItemCountInBag(Constant.MONEY_ITEM_ID); }

        public Role GetRoleByName(string name) { return roles_by_name_[name]; }
        public Magic GetMagicByName(string name) { return magics_by_name_[name]; }
        public Item GetItemByName(string name) { return items_by_name_[name]; }
        public SubmapInfo GetSubMapRecordByName(string name) { return submap_infos_by_name_[name]; }

        public List<Role> GetRoles() { return save_.roles_; }
        public List<Magic> GetMagics() { return save_.magics_; }
        public List<Item> GetItems() { return save_.items_; }
        public List<SubmapInfo> GetSubMapInfos() { return save_.submap_infos_; }
        public List<Shop> GetShops() { return save_.shops_; }

        public string GetFilename(int i, char c)
        {
            string filename = "";
            if (i > 0)
            {
                filename = Path.Combine("..","game","save" , c.ToString() + i.ToString() + ".grp");
                if (c == 'r') { filename += "32"; }
            }
            else
            {
                if (c == 'r')
                {
                    filename = Path.Combine("..", "game", "save", "ranger.grp32");
                }
                else if (c == 's')
                {
                    filename = Path.Combine("..", "game", "save", "allsin.grp");
                }
                else if (c == 'd')
                {
                    filename = Path.Combine("..", "game", "save", "alldef.grp");
                }
            }
            return filename;
        }

        public bool CheckSaveFileExist(int num)
        {
            return System.IO.File.Exists(GetFilename(num, 'r'))
                && System.IO.File.Exists(GetFilename(num, 's'))
                && System.IO.File.Exists(GetFilename(num, 'd'));
        }


        public bool Load(int num)
        {
            if (!CheckSaveFileExist(num)) { return false; }
            string filenamer = GetFilename(num, 'r');
            string filenames = GetFilename(num, 's');
            string filenamed = GetFilename(num, 'd');
            string filename_idx = Path.Combine("..", "game", "save", "ranger.idx32");

            var rgrp = GameFile.GetIdxContent(filename_idx, filenamer, ref offset_, ref length_);

            ReadDataToProtagonistInformation(rgrp, offset_[0], length_[0], ref protagonistInformation);
            ReadDataToRoles(rgrp, offset_[1], length_[1], sizeOfRoleSave, ref roles_);
            ReadDataToItems(rgrp, offset_[2], length_[2], sizeOfItemSave, ref items_);
            ReadDataToSubmapInfos(rgrp, offset_[3], length_[3], sizeOfSubmapInfoSave, ref submap_infos_);
            ReadDataToMagics(rgrp, offset_[4], length_[4], sizeOfMagicSave, ref magics_);
            ReadDataToShops(rgrp, offset_[5], length_[5], sizeOfShopSave, ref shops_);


            int submap_count = submap_infos_.Count;

            short[] sdata = new short[submap_count * sdata_length_ / 2];
            short[] ddata = new short[submap_count * ddata_length_ / 2];

            GameFile.readFile(filenames, out sdata, submap_count * sdata_length_ / 2);
            GameFile.readFile(filenamed, out ddata, submap_count * ddata_length_ / 2);

            ReadDataToSubmapLayerData(sdata, ref submap_infos_);
            ReadDataToSubmapEvent(ddata, ref submap_infos_);

            foreach (var i in roles_)
            {
                PotConv.FromCP950ToString(i.Name, ref i.strName);
                PotConv.FromCP950ToString(i.Nick, ref i.strNick);
            }
            foreach (var i in items_)
            {
                PotConv.FromCP950ToString(i.Name, ref i.strName);
                PotConv.FromCP950ToString(i.Introduction, ref i.strIntroduction);
            }
            foreach (var i in magics_)
            {
                PotConv.FromCP950ToString(i.Name, ref i.strName);
            }
            foreach (var i in submap_infos_)
            {
                PotConv.FromCP950ToString(i.Name, ref i.strName);
            }

            MakeMaps();

            return true;
        }



        public bool ThisSave(int num)
        {
            string filenamer = GetFilename(num, 'r');
            string filenames = GetFilename(num, 's');
            string filenamed = GetFilename(num, 'd');

            return true;
        }

        public Role? GetTeamMate(int i)
        {
            if (i < 0 || i >= Constant.TEAMMATE_COUNT)
            {
                return null;
            }
            int r = protagonistInformation.Team[i];
            if (r < 0 || r >= roles_.Count)
            {
                return null;
            }
            return roles_[r];
        }

        public Item? GetItemByBagIndex(int i)
        {
            if (i < 0 || i >= Constant.ITEM_IN_BAG_COUNT)
            {
                return null;
            }
            int r = protagonistInformation.Items[i].item_id;
            if (r < 0 || r >= items_.Count)
            {
                return null;
            }
            return items_[r];
        }

        public int GetItemCountByBagIndex(int i)
        {
            return protagonistInformation.Items[i].count;
        }

        public int GetItemCountInBag(Item item)
        {
            return GetItemCountByBagIndex(item.ID);           //原程序不同？？
        }

        public int GetItemCountInBag(int item_id)
        {
            for (int i = 0; i < Constant.ITEM_IN_BAG_COUNT; i++)
            {
                var id = protagonistInformation.Items[i].item_id;
                if (id < 0) { break; }
                if (id == item_id)
                {
                    return protagonistInformation.Items[i].count;
                }
            }
            return 0;
        }

        public void MakeMaps()
        {
            // 清空所有字典
            roles_by_name_.Clear();
            magics_by_name_.Clear();
            items_by_name_.Clear();
            submap_infos_by_name_.Clear();

            foreach (var role in roles_)
            {
                if (role.Name != null && role.Name.Length > 0)
                {
                    // 将 sbyte[] 转换成 string，这里用 UTF8 作为示例
                    string name = System.Text.Encoding.UTF8.GetString((byte[])(Array)role.Name);

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        roles_by_name_[name] = role;
                    }
                }
            }

            foreach (var magic in magics_)
            {
                if (magic.Name != null && magic.Name.Length > 0)
                {
                    // 将 sbyte[] 转换成 string，这里用 UTF8 作为示例
                    string name = System.Text.Encoding.UTF8.GetString((byte[])(Array)magic.Name);

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        magics_by_name_[name] = magic;
                    }
                }
            }


            foreach (var item in items_)
            {
                if (item.Name != null && item.Name.Length > 0)
                {
                    // 将 sbyte[] 转换成 string，这里用 UTF8 作为示例
                    string name = System.Text.Encoding.UTF8.GetString((byte[])(Array)item.Name);

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        items_by_name_[name] = item;
                    }
                }
            }
            

            foreach (var submapInfo in submap_infos_)
            {
                if (submapInfo.Name != null && submapInfo.Name.Length > 0)
                {
                    // 将 sbyte[] 转换成 string，这里用 UTF8 作为示例
                    string name = System.Text.Encoding.UTF8.GetString((byte[])(Array)submapInfo.Name);

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        submap_infos_by_name_[name] = submapInfo;
                    }
                }
            }
            
        }

        public Magic GetRoleLearnedMagic(ref Role r, int i)
        {
            if (i < 0 || i >= Constant.ROLE_MAGIC_COUNT) { return null; }
            return GetMagic(r.MagicID[i]);
        }

        public int GetRoleLearnedMagicLevelIndex(ref Role r, ref Magic m)
        {
            for (int i = 0; i < Constant.ROLE_MAGIC_COUNT; i++)
            {
                if (r.MagicID[i] == m.ID)
                {
                    return r.GetRoleMagicLevelIndex(i);
                }
            }
            return -1;
        }


    }
}
