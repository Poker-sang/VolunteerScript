using System;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace VolunteerScript.Utilities;

public static class BrowserManager
{
    public static IBrowser Browser { get; set; } = null!;
    public static IPlaywright Pw { get; set; } = null!;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="timeout">seconds</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static async Task<IBrowser> GetBrowser(Options options, int timeout = 8000)
    {
        Pw = await Playwright.CreateAsync();

        return Browser = await Pw.Chromium.LaunchAsync(new()
        {
            Headless = false,
            Channel = "msedge",
            Args = new[] { $"--blink-settings=imagesEnabled={options.ImagesEnabled.ToString().ToLower()}" },
            Timeout = timeout
        });
        // "--disable-blink-features=AutomationControlled" 
    }

    public static async Task Quit()
    {
        if (Browser == null!)
            return;
        await Browser.CloseAsync();
        await Browser.DisposeAsync();
        Browser = null!;
        Pw.Dispose();
        GC.Collect();
    }
}
