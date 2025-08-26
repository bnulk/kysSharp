using SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private Audio()
        {
            music = new List<IntPtr>();
            asound = new List<IntPtr>();
            esound = new List<IntPtr>();
            tracks = new List<IntPtr>();
            mixer= new IntPtr();

            ///////////////////////////////////////////////////////////////////////
            // 1. 初始化 SDL 音频子系统
            // SDL3.SDL_Init 是 SDL 的初始化函数，参数是要启用的子系统。
            // SDL_INIT_AUDIO 表示只初始化音频子系统。
            // 返回值是布尔类型，true 表示成功，false 表示失败。
            ///////////////////////////////////////////////////////////////////////
            if (SDL3.SDL_Init((SDL_InitFlags)SDL3.SDL_INIT_AUDIO) == false)
            {
                throw new Exception($"SDL 初始化失败！错误：{SDL3.SDL_GetError()}");
            }

            ///////////////////////////////////////////////////////////////////////
            // 2. 初始化 SDL3_mixer
            // SDL3_mixer 是 SDL3 的音频扩展库，主要用于简化音乐和音效的播放。
            // MIX_Init 返回 true 表示成功，false 表示失败。
            ///////////////////////////////////////////////////////////////////////
            if (!SDL3_mixer.MIX_Init())
            {
                Console.WriteLine($"SDL3_mixer 初始化失败：{SDL3.SDL_GetError()}");
                return;
            }

            ///////////////////////////////////////////////////////////////////////
            // 3. 配置音频规格 (AudioSpec)
            // SDL_AudioSpec 结构体定义了音频的参数：
            //   freq    → 采样率 (Hz)，44100 表示 CD 音质
            //   format  → 音频格式，这里是 16 位有符号小端整数 (常见标准)
            //   channels→ 声道数，2 表示立体声 (左声道 + 右声道)
            ///////////////////////////////////////////////////////////////////////
            SDL_AudioSpec spec = new SDL_AudioSpec
            {
                freq = 44100,
                format = SDL_AudioFormat.SDL_AUDIO_S16LE,
                channels = 2
            };

            ///////////////////////////////////////////////////////////////////////
            // 4. 打开默认音频设备
            // SDL_OpenAudioDevice 用来打开一个音频输出设备。
            // 第一个参数 SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK 表示使用默认输出设备。
            // 第二个参数 &spec 表示按照我们定义的规格来打开。
            // 返回值 dev 是一个设备 ID，用来标识这个音频设备。
            ///////////////////////////////////////////////////////////////////////
            SDL_AudioDeviceID dev = SDL3.SDL_OpenAudioDevice(SDL3.SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK, &spec);
            if (dev == 0)
            {
                Console.WriteLine($"打开音频设备失败: {SDL3.SDL_GetError()}");
                return;
            }

            ///////////////////////////////////////////////////////////////////////
            // 5. 创建 Mixer（混音器）
            // MIX_CreateMixerDevice 将一个 Mixer 绑定到指定的音频设备。
            // Mixer 的作用是管理音轨，支持同时播放多个声音并自动混合。
            ///////////////////////////////////////////////////////////////////////
            mixer = (IntPtr)SDL3_mixer.MIX_CreateMixerDevice(dev, &spec);
            if (mixer == IntPtr.Zero)
            {
                Console.WriteLine($"创建 Mixer 失败: {SDL3.SDL_GetError()}");
                return;
            }


            // 循环模拟加载 23 个音乐资源
            ///////////////////////////////////////////////////////////////////////
            // 6. 加载 WAV 文件
            // MIX_LoadAudio 用来加载音频文件到 Mixer。
            // 需要传入音频路径 (UTF-8 格式)，最后必须以 '\0' 结尾。
            // true/false 参数表示是否自动解码为原始 PCM 格式。
            ///////////////////////////////////////////////////////////////////////
            string path;
            int i;
            for (i=1;i<24;i++)
            {
                path = Path.Combine("..", "game", "music0", i.ToString() + ".MP3");
                if (!File.Exists(path))
                {
                    Console.WriteLine($"文件不存在：{path}");
                    return;
                }

                byte[] pathBytes = Encoding.UTF8.GetBytes(path + '\0');
                IntPtr audio;
                unsafe
                {
                    fixed (byte* ptr = pathBytes)
                    {
                        audio = (IntPtr)SDL3_mixer.MIX_LoadAudio((MIX_Mixer*)mixer, ptr, true);
                    }
                }

                if (audio == IntPtr.Zero)
                {
                    Console.WriteLine($"加载音频失败：{SDL3.SDL_GetError()}");
                    return;
                }
                music.Add(audio);
            }
            for (i = 0; i < 25; i++)
            {
                path = Path.Combine("..", "game", "sound", "atk"+i.ToString("00") + ".wav");
                if (!File.Exists(path))
                {
                    Console.WriteLine($"文件不存在：{path}");
                    return;
                }

                byte[] pathBytes = Encoding.UTF8.GetBytes(path + '\0');
                IntPtr audio;
                unsafe
                {
                    fixed (byte* ptr = pathBytes)
                    {
                        audio = (IntPtr)SDL3_mixer.MIX_LoadAudio((MIX_Mixer*)mixer, ptr, true);
                    }
                }

                if (audio == IntPtr.Zero)
                {
                    Console.WriteLine($"加载音频失败：{SDL3.SDL_GetError()}");
                    return;
                }
                asound.Add(audio);
            }
            for (i = 0; i < 53; i++)
            {
                path = Path.Combine("..", "game", "sound", "e" + i.ToString("00") + ".wav");
                if (!File.Exists(path))
                {
                    Console.WriteLine($"文件不存在：{path}");
                    return;
                }

                byte[] pathBytes = Encoding.UTF8.GetBytes(path + '\0');
                IntPtr audio;
                unsafe
                {
                    fixed (byte* ptr = pathBytes)
                    {
                        audio = (IntPtr)SDL3_mixer.MIX_LoadAudio((MIX_Mixer*)mixer, ptr, true);
                    }
                }

                if (audio == IntPtr.Zero)
                {
                    Console.WriteLine($"加载音频失败：{SDL3.SDL_GetError()}");
                    return;
                }
                esound.Add(audio);
            }
        }

        public void playMusic(int num)
        {
            if (!SDL3_mixer.MIX_PlayAudio((MIX_Mixer*)mixer, (MIX_Audio*)music[num]))
            {
                Console.WriteLine($"播放失败：{SDL3.SDL_GetError()}");
            }
            else
            {
                Console.WriteLine("播放中…");
            }
        }

        public void playAsound(int num)
        {
            if (!SDL3_mixer.MIX_PlayAudio((MIX_Mixer*)mixer, (MIX_Audio*)asound[num]))
            {
                Console.WriteLine($"播放失败：{SDL3.SDL_GetError()}");
            }
            else
            {
                Console.WriteLine("播放中…");
            }
        }

        public void playEsound(int num)
        {
            if (!SDL3_mixer.MIX_PlayAudio((MIX_Mixer*)mixer, (MIX_Audio*)esound[num]))
            {
                Console.WriteLine($"播放失败：{SDL3.SDL_GetError()}");
            }
            else
            {
                Console.WriteLine("播放中…");
            }
        }

        bool IsPlaying(MIX_Mixer* mixer, MIX_Audio* audio)
        {
            MIX_Audio* playing = SDL3_mixer.MIX_GetTrackPlaybackPosition(mixer);
            while (playing != null)
            {
                if (playing == audio) return true;
                playing = SDL3_mixer.MIX_NextAudio(mixer, playing);
            }
            return false;
        }


        public void Dispose()
        {
            SDL3_mixer.MIX_DestroyMixer((MIX_Mixer*)mixer);
            SDL3_mixer.MIX_Quit();
            SDL3.SDL_Quit();
        }
    }
}
