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
    private const string InfoName = "info.json";

    public static Options Options { get; } = new(Mode.Local, $@".\{InfoName}", 4, false, true, false, @"C:\Software\Tencent\{QQBot}\FileRecv", Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\1.jpeg");

    public static Info Info { get; private set; } = null!;

    private static async Task Open()
    {
        Console.WriteLine($"opening {InfoName}.");
        using var process = new Process
        {
            StartInfo = new(Options.ConfigPath)
            {
                UseShellExecute = true
            }
        };
        _ = process.Start();
        await process.WaitForExitAsync();
    }

    private static async Task NewOpen()
    {
        Console.WriteLine($"creating {InfoName}.");
        var fs = File.Create(Options.ConfigPath);
        await JsonSerializer.SerializeAsync(fs, new Info(), new JsonSerializerOptions { WriteIndented = true });
        await fs.DisposeAsync();
        await Open();
    }

    private static async Task<Info?> Load()
    {
        Console.WriteLine($"loading {InfoName}.");
        if (File.Exists(Options.ConfigPath))
        {
            await using var fs = File.OpenRead(Options.ConfigPath);
            if (await JsonSerializer.DeserializeAsync<Info>(fs) is { } info)
                return info;
            else
            {
                Console.WriteLine($"{InfoName} is corrupted.");
                await Open();
            }
        }
        else
        {
            Console.WriteLine($"{InfoName} does not exist.");
            await NewOpen();
        }
        Console.WriteLine($"Call \"load\" again to read the new {InfoName}.");
        return null;
    }

    private static async Task<bool> Block()
    {
        while (true)
        {
            switch (Console.ReadLine())
            {
                case "edit":
                {
                    await Open();
                    break;
                }
                case "load":
                {
                    if (await Load() is { } info)
                        Info = info;
                    return false;
                }
                case "exit":
                    return true;
                case { } path when PicRegex().IsMatch(path):
                    _ = FillForm.From(path, Info, Options);
                    break;
            }
        }
    }

    public static async Task Main()
    {
        try
        {
            while ((Info = (await Load())!) is null)
            {
            }

            HttpClientExtensions.Initialize();

            _ = await BrowserManager.GetBrowser(Options);

            while (true)
            {
                switch (Options.Mode)
                {
                    case Mode.MiraiNet:
                    {
                        using var bot = new MiraiBot
                        {
                            Address = $"{Info.IpAddress}:{Info.Port}",
                            QQ = Info.QqBot.ToString(),
                            VerifyKey = Info.VerifyKey
                        };

                        await bot.LaunchAsync();

                        _ = bot.MessageReceived.OfType<GroupMessageReceiver>()
                            .Subscribe(r => GroupMessage.OnGroupMessage(r, Info, Options));

                        Console.WriteLine($"{Info.QqBot} listening.");
                        if (await Block())
                            return;
                        break;
                    }
                    case Mode.Local:
                    {
                        var path = string.Format(Options.QqFilesReceive, Info.QqBot);
                        var fileSystemWatcher = new FileSystemWatcher(path)
                        {
                            EnableRaisingEvents = true
                        };
                        fileSystemWatcher.Created += async (_, e) =>
                        {
                            if (PicRegex().IsMatch(e.FullPath))
                                await FillForm.From(e.FullPath, Info, Options);
                        };
                        Console.WriteLine($"{path} watching.");
                        if (await Block())
                            return;
                        break;
                    }
                    case Mode.Test:
                    {
                        var path = string.Format(Options.TestFile);
                        if (File.Exists(path))
                        {
                            Console.WriteLine($"{path} testing.");
                            await FillForm.From(path, Info, Options);
                        }
                        else
                            Console.WriteLine($"{path} does not exist.");
                        if (await Block())
                            return;
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        finally
        {
            await BrowserManager.Quit();
        }
    }

    [GeneratedRegex(@"[\\\w]+\.(webp|png|jpeg|jpg|bmp)$", RegexOptions.Compiled)]
    private static partial Regex PicRegex();
}
