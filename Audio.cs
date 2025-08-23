using SDL;
using System;
using System.IO;
using System.Text;
using System.Text.Unicode;

namespace kysSharp
{
    internal unsafe class Audio : IDisposable
    {
        // 私有静态实例，确保 Audio 类的单例模式
        private static Audio? _audioInstance;

        private readonly List<IntPtr> music;                                    // 存储音乐的 MIX_Audio* 指针
        private readonly List<IntPtr> asound, esound;                           // 存储音效的 MIX_Audio* 指针
        private List<IntPtr> tracks;                                            // 存储播放轨道的 MIX_Track* 指针
        private IntPtr mixer;                                                   // 混音器的 MIX_Mixer* 指针
        private SDL_AudioDeviceID audioDeviceId;                                // 音频设备 ID


        // 单例模式获取 Audio 实例
        public static Audio getInstance()
        {
            if (_audioInstance == null)
            {
                _audioInstance = new Audio();
            }
            return _audioInstance;
        }

        // 私有构造函数，防止外部直接创建实例
        private Audio()
        {
            music = new List<IntPtr>();
            asound = new List<IntPtr>();
            esound = new List<IntPtr>();
            tracks = new List<IntPtr>();
        }

        // 初始化音频系统，加载音效和音乐
        public void Init()
        {
            // 初始化 SDL 的音频子系统
            if (SDL3.SDL_INIT_AUDIO < 0)
            {
                throw new Exception($"SDL 初始化失败！错误：{SDL3.SDL_GetError()}");
            }

            // 初始化 SDL3_mixer
            if (!SDL3_mixer.MIX_Init())
            {
                throw new Exception($"SDL3_mixer 初始化失败！错误：{SDL3.SDL_GetError()}");
            }
            
            // 定义音频规格
            SDL_AudioSpec spec = new SDL_AudioSpec
            {
                freq = 44100,                                                // 参数 44100 → 音频采样率，常用 CD 质量（44.1kHz）
                format = SDL_AudioFormat.SDL_AUDIO_S16LE,                    // 参数 SDL_AudioFormat.S16LSB → 16位有符号小端音频格式
                channels = 2                                                 // channels = 2 表示音频的声道数，也就是你播放的声音是单声道还是立体声。单声道为1，双声道为2。
            };




            // 打开音频设备
            //audioDeviceId = SDL3.SDL_OpenAudioDevice(0, &spec);              //0表示默认设备
            // 将 audioDeviceId = SDL3.SDL_OpenAudioDevice(null, &spec);
            // 修改为 audioDeviceId = SDL3.SDL_OpenAudioDevice(SDL3.SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK, &spec);
            //audioDeviceId = SDL3.SDL_OpenAudioDevice(SDL3.SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK, &spec);
            


            // 创建混音器设备
            mixer = (nint)SDL3_mixer.MIX_CreateMixerDevice(SDL3.SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK, &spec);
            if (mixer == IntPtr.Zero)
            {
                throw new Exception($"无法创建混音器设备！错误：{SDL3.SDL_GetError()}");
            }

            // 创建轨道（相当于 SDL2_mixer 的 8 个默认通道）
            for (int i = 0; i < 8; i++)
            {
                IntPtr track = (nint)SDL3_mixer.MIX_CreateTrack((MIX_Mixer*)mixer);
                if (track == IntPtr.Zero)
                {
                    Console.WriteLine($"无法创建轨道 {i}！错误：{SDL3.SDL_GetError()}");
                    continue;
                }
                tracks.Add(track);
            }


            byte[] pathBytes;
            // 循环模拟加载 24 个音乐资源
            for (int i = 1; i < 24; i++)
            {
                // 将字符串转换为 UTF - 8 编码的字节数组，并添加空终止符
                pathBytes = Encoding.UTF8.GetBytes("music\\" + i.ToString() + ".mid" + "\0");
                // 固定字节数组以获取指针
                fixed (byte* pathPtr = pathBytes)
                {
                    // 调用底层函数
                    nint m = (nint)SDL3_mixer.MIX_LoadAudio((MIX_Mixer*)mixer, pathPtr, false);
                    music.Add(m);
                }
            }
            for (int i = 0; i < 25; i++)
            {
                // 将字符串转换为 UTF - 8 编码的字节数组，并添加空终止符
                pathBytes = Encoding.UTF8.GetBytes("sound\\" + "atk" + i.ToString("D2") + ".wav" + "\0");
                // 固定字节数组以获取指针
                fixed (byte* pathPtr = pathBytes)
                {
                    // 调用底层函数
                    nint a = (nint)SDL3_mixer.MIX_LoadAudio((MIX_Mixer*)mixer, pathPtr, false);
                    asound.Add(a);
                }
            }
            for (int i = 0; i < 53; i++)
            {
                // 将字符串转换为 UTF - 8 编码的字节数组，并添加空终止符
                pathBytes = Encoding.UTF8.GetBytes("sound\\" + "e" + i.ToString("D2") + ".wav" + "\0");
                // 固定字节数组以获取指针
                fixed (byte* pathPtr = pathBytes)
                {
                    // 调用底层函数
                    nint e = (nint)SDL3_mixer.MIX_LoadAudio((MIX_Mixer*)mixer, pathPtr, false);
                    esound.Add(e);
                }
            }


        }

        // 播放背景音乐
        /*
        public bool PlayMusic(int num)
        {
            if (num < 0 || num >= music.Count)
                return false;
            if (music!=null)
            {
                return SDL3_mixer.MIX_PlayAudio((MIX_Mixer*)mixer, (MIX_Audio*)music[num]);
            }
            return false;
        }
        */
        // 按索引播放音乐，使用第一个轨道
        public void playMusic(int num)
        {
            if (num < 0 || num >= music.Count || tracks.Count == 0)
            {
                Console.WriteLine($"无效的音乐索引 {num} 或没有可用轨道！");
                return;
            }

            IntPtr track = tracks[0]; // 使用第一个轨道播放音乐
            if (!SDL3_mixer.MIX_SetTrackAudio((MIX_Track*)track, (MIX_Audio*)music[num]))
            {
                Console.WriteLine($"无法为轨道设置音频！错误：{SDL3.SDL_GetError()}");
                return;
            }

            SDL_PropertiesID sDL_PropertiesID = new SDL_PropertiesID();

            if (!SDL3_mixer.MIX_PlayTrack((MIX_Track*)track, sDL_PropertiesID))
            {
                Console.WriteLine($"无法播放轨道！错误：{SDL3.SDL_GetError()}");
            }
        }

        // 播放音效
        public void playASound(int num)
        {
            if (num < 0 || num >= asound.Count || tracks.Count == 0)
            {
                Console.WriteLine($"无效的音乐索引 {num} 或没有可用轨道！");
                return;
            }

            IntPtr track = tracks[0]; // 使用第一个轨道播放音乐
            if (!SDL3_mixer.MIX_SetTrackAudio((MIX_Track*)track, (MIX_Audio*)asound[num]))
            {
                Console.WriteLine($"无法为轨道设置音频！错误：{SDL3.SDL_GetError()}");
                return;
            }

            SDL_PropertiesID sDL_PropertiesID = new SDL_PropertiesID();

            if (!SDL3_mixer.MIX_PlayTrack((MIX_Track*)track, sDL_PropertiesID))
            {
                Console.WriteLine($"无法播放轨道！错误：{SDL3.SDL_GetError()}");
            }
        }

        // 播放音效
        public void playESound(int num)
        {
            if (num < 0 || num >= esound.Count || tracks.Count == 0)
            {
                Console.WriteLine($"无效的音乐索引 {num} 或没有可用轨道！");
                return;
            }

            IntPtr track = tracks[0]; // 使用第一个轨道播放音乐
            if (!SDL3_mixer.MIX_SetTrackAudio((MIX_Track*)track, (MIX_Audio*)esound[num]))
            {
                Console.WriteLine($"无法为轨道设置音频！错误：{SDL3.SDL_GetError()}");
                return;
            }

            SDL_PropertiesID sDL_PropertiesID = new SDL_PropertiesID();

            if (!SDL3_mixer.MIX_PlayTrack((MIX_Track*)track, sDL_PropertiesID))
            {
                Console.WriteLine($"无法播放轨道！错误：{SDL3.SDL_GetError()}");
            }
        }

        /// <summary>
        /// 检查音乐是否播放结束并重新播放
        /// </summary>
        /// <param name="num"></param>
        public void checkAndReplayMusic(int num)
        {
            if (tracks.Count == 0)
            {
                Console.WriteLine("没有可用的轨道进行检查！");
                return;
            }

            IntPtr track = tracks[0]; // 检查第一个轨道（用于音乐）
            if (!SDL3_mixer.MIX_TrackPlaying((MIX_Track*)track))
            {
                Console.WriteLine("音乐播放结束，重新播放...");
                playMusic(num);  // 你自己封装的播放方法
            }
        }

        public void stopMusic()
        {
            SDL3_mixer.MIX_StopAllTracks((MIX_Mixer*)mixer, 2000);
        }


        public void Dispose()
        {
            // 释放音乐
            foreach (var m in music)
            {
                SDL3_mixer.MIX_DestroyAudio((MIX_Audio*)m);
            }
            // 释放音效
            foreach (var m in asound)
            {
                SDL3_mixer.MIX_DestroyAudio((MIX_Audio*)m);
            }
            foreach (var m in esound)
            {
                SDL3_mixer.MIX_DestroyAudio((MIX_Audio*)m);
            }

            music.Clear();
            asound.Clear();
            esound.Clear();
            // 关闭 SDL_mixer 音频系统
            SDL3_mixer.MIX_DestroyMixer((MIX_Mixer*)mixer);

            foreach(var t in tracks)
            {
                SDL3_mixer.MIX_DestroyTrack((MIX_Track*)t);
            }
            
            SDL3_mixer.MIX_Quit();
            // 清理资源
            _audioInstance = null;
        }

















        }
}
