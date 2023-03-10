using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;
using VolunteerScript.Mirai.Net;
using VolunteerScript.Services;
using VolunteerScript.Utilities;

namespace VolunteerScript;

public static partial class Program
{
    public static Config Config = null!;

    private enum Mode
    {
        MiraiNet,
        Local,
        Test
    }

    private static readonly Mode _bot = Mode.MiraiNet;

    public static async Task Main()
    {
        try
        {
            static void Block()
            {
                while (true)
                    if (Console.ReadLine() is "exit")
                        return;
            }

            HttpClientExtensions.Initialize();

            Config = GetConfig();

            _ = await BrowserManager.GetBrowser();

            switch (_bot)
            {
                case Mode.MiraiNet:
                {
                    using var bot = new MiraiBot
                    {
                        Address = $"{Config.IpAddress}:{Config.Port}",
                        QQ = Config.QqBot.ToString(),
                        VerifyKey = Config.VerifyKey
                    };

                    await bot.LaunchAsync();

                    _ = bot.MessageReceived.OfType<GroupMessageReceiver>()
                        .Subscribe(GroupMessage.OnGroupMessage);

                    Block();
                    break;
                }
                case Mode.Test:
                {
                    await FillForm.From(@"C:\Users\poker\Desktop\1.jpeg");
                    break;
                }
                case Mode.Local:
                {
                    var fileSystemWatcher = new FileSystemWatcher($@"C:\Softwares\Tencent\{Config.QqBot}\FileRecv")
                    {
                        EnableRaisingEvents = true
                    };
                    fileSystemWatcher.Created += async (o, e) =>
                    {
                        if (MyRegex().IsMatch(e.FullPath))
                            while (true)
                                try
                                {
                                    await FillForm.From(e.FullPath);
                                    break;
                                }
                                catch (IOException)
                                {
                                    _ = Task.Delay(100);
                                }
                    };
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Block();
        }
        finally
        {
            await BrowserManager.Quit();
        }
    }

    private const string ConfigPath = "conf.json";

    private static Config GetConfig()
    {
        if (File.Exists(ConfigPath) &&
            JsonSerializer.Deserialize<Config>(File.ReadAllText(ConfigPath))
                is { } config)
            return config;

        throw new("需要设置");
    }

    [GeneratedRegex(@"[\\\w]+\.(webp|png|jpeg|jpg|bmp)$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
