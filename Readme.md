# VolunteerScript

专用本校抢志愿者名额（表单大师）的程序

使用Mirai.Net、Mirai-CSharp、QRCodeDecoder-ImageSharp、Playwright

## 运行方法

先直接运行一次，按照要求安装Playwright后，会自动打开新建的info.json文件

### 运行前提

按照要求编写信息info.json文件：

```json
{
    // mirai代理地址，一般为本地回环
    "IpAddress": "localhost",
    // mirai代理端口，一般为8080
    "Port": 8080,
    // QQ机器人号码（加了志愿群的号，不使用QQ机器人也会用到此条目）
    "QqBot": 123,
    // mirai验证密钥
    "VerifyKey": "xxx",
    // 监听的群
    "GroupObserved": 123,
    // 你的名字
    "Name": "xxx",
    // 你的学号
    "Id": 123,
    // 你的QQ号（联系用）
    "Qq": 123,
    // 你的手机号
    "Tel": 123,
    // 你的年级
    "Grade": "xxx",
    // 你的专业
    "Major": "xxx",
    // 你的班级
    "Class": "xxx"
}
```

### 运行设置

Options记录代表运行设置，有以下几个字段（属性）

| 类型 | 名称 | 说明 |
| - | - | - |
| Mode | Mode | 运行模式 |
| string | ConfigPath | 设置路径 |
| int | PlacesToSubmit | 剩余多少名额时才去提交 |
| bool | AutoSubmit | 是否自动提交表单 |
| bool | ImagesEnabled | 网页是否显示图片（可以提高网页加载速度） |
| bool | ObservedGroupOnly | 机器人是否只监听指定群的图片消息 |
| string | QqFilesReceive | QQ机器人接收到的文件的位置 |
| string | TestFile | 测试时使用的图片文件（默认是桌面上名为1.jpeg的图片） |

### 运行说明

设置完后再次运行：

运行中控制台可以输入以下内容

| 内容 | 说明 |
| - | - |
| edit | 打开info.json并编辑 |
| load | 重新加载info.json文件 |
| exit | 退出程序 |
| （任意图片的本地路径） | 打开本地图片并自动填写（要求后缀名为webp、png、jpeg、jpg、bmp其中一种） |

### 运行模式

Mode目前有以下几种模式

| 内容 | 描述 | 说明 |
| - | - | - |
| MiraiNet | 机器人 | 自动监控机器人账号的图片消息和图片文件 |
| Local | 本地 | 自动监控Options.QqFilesReceive下的文件变化 |
| Test | 测试 | 直接使用Options.TestFile来测试Playwright自动化 |

注：

* 所有图片文件名都要求后缀名为webp、png、jpeg、jpg、bmp其中一种，且实际格式也要求被SixLabors.ImageSharp所支持

* 所有图片都会被当成二维码解析，并自动打开网页填写

* 机器人使用Mirai.Net框架

* 控制台会显示现在完成任务的记录
