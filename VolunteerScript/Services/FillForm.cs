using System;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Playwright;
using SixLabors.ImageSharp;
using VolunteerScript.Utilities;

namespace VolunteerScript.Services;

public static class FillForm
{
    public static async Task From(Stream stream, Info info, Options options)
    {
        try
        {
            var image = (await Image.LoadAsync(stream)).QrDecode()[0];
            var form = Encoding.Default.GetString(image);
            await GotoFill(form, info, options);
        }
        catch
        {
            return;
        }
    }

    public static async Task From(string path, Info info, Options options)
    {
        try
        {
            var image = (await Image.LoadAsync(path)).QrDecode()[0];
            var form = Encoding.Default.GetString(image);
            await GotoFill(form, info, options);
        }
        catch
        {
            return;
        }
    }

    private static async Task GotoFill(string url, Info info, Options options)
    {
        var browser = BrowserManager.Browser;

        Console.WriteLine($"Goto page {url}");

        var newContext = await browser.NewContextAsync(BrowserManager.Pw.Devices["iPhone 12"]);
        var page = await newContext.NewPageAsync();

        page.Load += async (_, _) =>
        {
            if (!await Fill(page, info, options))
            {
                Console.WriteLine("Wait for more submissions, Refreshing");
                _ = await page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });
            }
        };

        _ = await page.GotoAsync(url, new() { WaitUntil = WaitUntilState.NetworkIdle });

        // while (!await Fill(page, info, options))
        // {
        //     Console.WriteLine("Refreshing");
        //     _ = await page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });
        // }
    }


    private static async Task<bool> Fill(IPage  page, Info info, Options options)
    {
        Console.WriteLine();
        Console.WriteLine("Page Loaded");

        // var elements = await page.Locator("//html/body/div[1]/form/ul/child::li").AllAsync();
        var elements = await page.Locator("//html/body/div[1]/div[2]/form/ul/child::li").AllAsync();
        Console.WriteLine($"Get {elements.Count} elements");

        foreach (var element in elements)
            try
            {
                var label = element.Locator("label").First;
                var name = (string?)null;
                if (await label.CountAsync() is not 0)
                {
                    name = (await label.InnerTextAsync()).TrimStart('*');
                    if (name is "")
                        continue;
                }
                if (name is not null && (name.Contains("时间") || name.Contains("单选")))
                {
                    Console.Write($"Radio\t {name}: ");
                    var a = await element.Locator("span").CountAsync();
                    foreach (var span in await element.Locator("span").AllAsync())
                    {
                        var ratio = span.Locator("input");
                        var labels = span.Locator("label");
                        if (await ratio.CountAsync() is 1 && await labels.CountAsync() is 2)
                            if (await ratio.IsEnabledAsync() || options.Mode is Mode.Test)
                            {
                                var radioLabel = labels.First;
                                var restLabel = labels.Nth(1);
                                var restCountString = (await restLabel.InnerTextAsync())[4..^1];
                                var restCount = int.Parse(restCountString);
                                if (restCount >= options.PlacesToSubmit || options.Mode is Mode.Test)
                                    return false;
                                await radioLabel.ClickAsync();
                                Console.Write("Checked ");
                                break;
                            }
                            else
                                Console.Write("Skipped ");
                    }
                }
                else
                {
                    var input = element.Locator("input");
                    if (name is null)
                    {
                        Console.Write("Button\t :");
                        if (options.AutoSubmit && await input.GetAttributeAsync("type") is "submit")
                        {
                            await input.ClickAsync();
                            Console.Write("Submitted");
                        }
                        return true;
                    }
                    Console.Write($"Text\t {name}: ");
                    var hint = await element.GetAttributeAsync("reqdtxt");
                    var value = name switch
                    {
                        "姓名" => info.Name,
                        "学号" => info.Id.ToString(),
                        "年级" => info.Grade,
                        "专业班级" => info.Major + info.Class,
                        "专业" => info.Major,
                        "班级" => info.Class,
                        "手机" or "电话" or "联系方式" => info.Tel.ToString(),
                        "QQ" => info.Qq.ToString(),
                        _ when !string.IsNullOrEmpty(hint) => hint,
                        _ when name.Contains("专业班级") => info.Major + info.Class,
                        _ => new DataTable().Compute(name.TrimEnd('=', '?', '？'), "").ToString()
                    };

                    await input.FillAsync(value!);
                    Console.Write(value);
                }
            }
            catch
            {
                continue;
            }
            finally
            {
                Console.WriteLine();
            }

        return true;
    }
}
