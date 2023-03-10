using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions.Http.Managers;
using VolunteerScript.Services;
using VolunteerScript.Utilities;

namespace VolunteerScript.Mirai.Net;

public static class GroupMessage
{
    public static void OnGroupMessage(GroupMessageReceiver receiver)
    {
        // if (receiver.GroupId != _config.GroupObserved.ToString())
        //     return;
        var url = "";
        foreach (var message in receiver.MessageChain)
            switch (message)
            {
                case FileMessage file:
                    var fileDownloadInfo = FileManager.GetFileAsync(receiver.GroupId, file.FileId, true).GetAwaiter().GetResult().DownloadInfo;
                    url = fileDownloadInfo.Url;
                    break;
                case ImageMessage img:
                    url = img.Url;
                    break;
            }

        if (url is "")
            return;
        FillForm.From(url.DownloadStreamAsync().GetAwaiter().GetResult());
    }
}
