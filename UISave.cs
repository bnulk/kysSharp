///////////////////////////////////////////////////////////////////////////////////////////////////////
// UISave.cs
// 功能：存档/读档界面
// 说明：C# 等价转换自 C++ 版本 UISave.cpp
///////////////////////////////////////////////////////////////////////////////////////////////////////

using kysSharp;
using System;
using System.Collections.Generic;

public class UISave : MenuText // 假设 Menu 继承自 TextBox（保持原结构）
{
    private int mode_ = 0; // 0 = 读取模式，1 = 存档模式
    private const int AUTO_SAVE_ID = 11;

    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    // 构造函数：初始化存档选项列表
    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    public UISave()
    {
        List<string> strings = new List<string>();

        for (int i = 0; i <= 10; i++)
        {
            string filename = Save.GetFilename(i, 'r');
            string timeStr = GameFile.GetFileTime(filename);
            string line = ConvertLibs.FormatString("進度{0:00}  {1}", i, timeStr);
            strings.Add(line);
        }

        string autoFilename = Save.GetFilename(AUTO_SAVE_ID, 'r');
        string autoTime = GameFile.GetFileTime(autoFilename);
        strings.Add(ConvertLibs.FormatString("自動檔  {0}", autoTime));

        setStrings(strings);

        // 屏蔽第0个存档
        childs_[0].setVisible(false);

        arrange(0, 0, 0, 28);
    }

    public void setMode(int m) { mode_ = m; }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    // 析构函数（C# 不需要手动释放）
    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    ~UISave() { }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    // 进入界面时触发
    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    public override void onEntrance()
    {
        // 存档时屏蔽自动档
        if (mode_ == 1)
        {
            childs_[childs_.Count - 1].setVisible(false);
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    // 按下确认键时触发
    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    public override void onPressedOK()
    {
        pressToResult();

        if (result_ >= 0)
        {
            // 读取模式
            if (mode_ == 0 && Save.CheckSaveFileExist(result_))
            {
                Load(result_);
                setExit(true);
            }

            // 存档模式
            if (mode_ == 1)
            {
                SaveGame(result_);
                setExit(true);
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    // 读取存档
    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Load(int r)
    {
        var subScene = getPointerFromRoot<SubScene>();
        var save = Save.getInstance();
        var mainScene = MainScene.getInstance();

        save.load(r);

        if (save == null)
            return;

        mainScene.setManPosition(save.protagonistInformation.MainMapX, save.protagonistInformation.MainMapY);


        if (save.protagonistInformation.InSubMap >= 0)
        {
            if (subScene != null)
            {
                subScene.forceJumpSubScene(save.protagonistInformation.InSubMap, save.protagonistInformation.SubMapX, save.protagonistInformation.SubMapY);
            }
            else
            {
                mainScene.ForceEnterSubScene(save.protagonistInformation.InSubMap, save.protagonistInformation.SubMapX, save.protagonistInformation.SubMapY);
            }
        }
        else
        {
            if (subScene != null)
            {
                subScene.forceExit();
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    // 存储存档
    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    private void SaveGame(int r)
    {
        var subScene = getPointerFromRoot<SubScene>();
        var save = Save.getInstance();
        var mainScene = MainScene.getInstance();

        mainScene.getManPosition(ref save.protagonistInformation.MainMapX, ref save.protagonistInformation.MainMapY);

        if (subScene != null)
        {
            subScene.getManPosition(ref save.protagonistInformation.SubMapX, ref save.protagonistInformation.SubMapY);
            save.protagonistInformation.InSubMap = subScene.getMapInfo().ID;
        }
        else
        {
            save.protagonistInformation.InSubMap = -1;
        }

        save.SaveGame(r);
    }




}
