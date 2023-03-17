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
    public static async Task From(Stream stream, Config config, Options options)
    {
        try
        {
            var image = (await Image.LoadAsync(stream)).QrDecode()[0];
            var form = Encoding.Default.GetString(image);
            await Fill(form, config, options);
        }
        catch
        {
            return;
        }
    }

    public static async Task From(string path, Config config, Options options)
    {
        try
        {
            var image = (await Image.LoadAsync(path)).QrDecode()[0];
            var form = Encoding.Default.GetString(image);
            await Fill(form, config, options);
        }
        catch
        {
            return;
        }
    }

    public static async Task Fill(string url, Config config, Options options)
    {
        var browser = BrowserManager.Browser;

        Console.WriteLine($"Goto page {url}");
        var page = await browser.NewPageAsync();
        await page.GotoAsync(url, new()
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
        Console.WriteLine("Page Loaded");
        var elements = await page.Locator("//html/body/div[1]/form/ul/child::li").AllAsync();
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
                if (name is not null && name.Contains("时间"))
                {
                    Console.Write($"Radio\t {name}: ");
                    var a = await element.Locator("span").CountAsync();
                    foreach (var span in await element.Locator("span").AllAsync())
                    {
                        var ratio = span.Locator("input");
                        if (await ratio.CountAsync() is not 0)
                            if (await ratio.IsEnabledAsync())
                            {
                                var radioLabel = span.Locator("label").First;
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
                            //  await input.ClickAsync();
                            Console.Write("Submitted");
                        }
                        return;
                    }
                    Console.Write($"Text\t {name}: ");
                    var hint = await element.GetAttributeAsync("reqdtxt");
                    var value = name switch
                    {
                        "姓名" => config.Name,
                        "学号" => config.Id.ToString(),
                        "年级" => config.Grade,
                        "专业" => config.Major,
                        "班级" => config.Class,
                        "专业班级" => config.Major + config.Class,
                        "手机" => config.Tel.ToString(),
                        "电话" => config.Tel.ToString(),
                        "联系方式" => config.Tel.ToString(),
                        "QQ" => config.Qq.ToString(),
                        _ => 0 switch
                        {
                            0 when !string.IsNullOrEmpty(hint) => hint,
                            0 when name.Contains("专业班级") => config.Major + config.Class,
                            // 0 when name.Contains("输入图片中的文字") => "中",
                            _ => new DataTable().Compute(name.TrimEnd('='), "").ToString()
                        }
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
    }
}
