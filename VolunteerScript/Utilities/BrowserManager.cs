using System;
using System.Collections.Generic;
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
    /// <param name="timeout">minutes</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static async Task<IBrowser> GetBrowser(int timeout = 8000)
    {
        Pw = await Playwright.CreateAsync();
        return Browser = await Pw.Chromium.LaunchAsync(new()
        {
            Headless = false,
            Channel = "msedge",
            Args = new List<string> { "--disable-blink-features=AutomationControlled" },
            Timeout = timeout
        });
        // "blink-settings=imagesEnabled=false",

        // throw new EdgeDriverBusyException("Only one EdgeDriver instance can exist at a time.");
        // throw new("EdgeDriver is busy! You should not make two requests at the same time.");
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
