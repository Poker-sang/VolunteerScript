using Microsoft.Extensions.Logging.Abstractions;
using QRCodeDecoderLibrary;
using SixLabors.ImageSharp;

namespace VolunteerScript.Utilities;

public static class QrCodeExtensions
{
    public static QRDecoder Decoder { get; } = new(new NullLogger<QRDecoder>());

    public static byte[][] QrDecode(this Image image) => Decoder.ImageDecoder(image);
}
