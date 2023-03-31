using System;
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
    public static Options Options { get; } = new(Mode.Test, @".\conf.json", true, true, false);

    public static Config Config { get; } =
        File.Exists(Options.ConfigPath) &&
        JsonSerializer.Deserialize<Config>(File.ReadAllText(Options.ConfigPath)) is { } config
            ? config!
            : throw new("需要设置");

    public static async Task Main()
    {
        try
        {
            static void Block()
            {
                while (true)
                {
                    switch (Console.ReadLine())
                    {
                        case "exit":
                            break;
                        case { } path when MyRegex().IsMatch(path):
                            _ = FillForm.From(path, Config, Options);
                            break;
                    }
                }
            }

            HttpClientExtensions.Initialize();

            _ = await BrowserManager.GetBrowser(Options);

            switch (Options.Mode)
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
                        .Subscribe(r => GroupMessage.OnGroupMessage(r, Config, Options));

                    Console.WriteLine($"{Config.QqBot} listening.");
                    Block();
                    break;
                }
                case Mode.Local:
                {
                    var path = $@"C:\Software\Tencent\{Config.QqBot}\FileRecv";
                    var fileSystemWatcher = new FileSystemWatcher(path)
                    {
                        EnableRaisingEvents = true
                    };
                    fileSystemWatcher.Created += async (_, e) =>
                    {
                        if (MyRegex().IsMatch(e.FullPath))
                            await FillForm.From(e.FullPath, Config, Options);
                    };
                    Console.WriteLine($"{path} watching.");
                    Block();
                    break;
                }
                case Mode.Test:
                {
                    var path = $@"{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}\1.jpeg";
                    Console.WriteLine($"{path} testing.");
                    await FillForm.From(path, Config, Options);
                    Block();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        finally
        {
            await BrowserManager.Quit();
        }
    }

    [GeneratedRegex(@"[\\\w]+\.(webp|png|jpeg|jpg|bmp)$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
