using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using VolunteerScript.Utilities;

namespace VolunteerScript.Services;

public static class FillForm
{
    public static async Task From(Stream stream)
    {
        try
        {
            var image = (await Image.LoadAsync(stream)).QrDecode()[0];
            var form = Encoding.Default.GetString(image);
            await Fill(form, Program.Config);
        }
        catch
        {
            return;
        }
    }

    public static async Task From(string path)
    {
        try
        {
            var image = (await Image.LoadAsync(path)).QrDecode()[0];
            var form = Encoding.Default.GetString(image);
            await Fill(form, Program.Config);
        }
        catch
        {
            return;
        }
    }

    public static async Task Fill(string url, Config config)
    {
        var browser = BrowserManager.Browser;

        var page = await browser.NewPageAsync();
        await page.GotoAsync(url);
        var elements = await page.Locator("//html/body/div[1]/form/ul/child::*").AllAsync();

        foreach (var element in elements)
            try
            {
                var label = element.Locator("//label");
                if (await label.CountAsync() is 0)
                    return;
                var name = (await label.InnerTextAsync()).TrimStart('*');
                if (name is "")
                    continue;
                if (name.Contains("时间"))
                    foreach (var span in await element.Locator("span").AllAsync())
                    {
                        var input = span.Locator("input");
                        if (!await input.IsEnabledAsync())
                            continue;
                        var ratio = span.Locator("label");
                        await ratio.ClickAsync();
                        break;
                    }
                else
                {
                    var input = element.Locator("input");
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
                }
            }
            catch
            {
                continue;
            }
    }
}
