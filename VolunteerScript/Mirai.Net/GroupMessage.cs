using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions.Http.Managers;
using VolunteerScript.Services;
using VolunteerScript.Utilities;

namespace VolunteerScript.Mirai.Net;

public static class GroupMessage
{
    public static async void OnGroupMessage(GroupMessageReceiver receiver, Config config, Options options)
    {
        // if (receiver.GroupId != _config.GroupObserved.ToString())
        //     return;
        var url = "";
        foreach (var message in receiver.MessageChain)
            switch (message)
            {
                case FileMessage file:
                    var fileDownloadInfo = (await FileManager.GetFileAsync(receiver.GroupId, file.FileId, true)).DownloadInfo;
                    url = fileDownloadInfo.Url;
                    break;
                case ImageMessage img:
                    url = img.Url;
                    break;
            }

        if (url is "")
            return;
        await FillForm.From(await url.DownloadStreamAsync(), config, options);
    }
}
