using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Shared
{
    public class ImageUploadSettings
    {
        public static readonly string[] AllowedExtensions =
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp",
            ".bmp", ".tiff", ".tif", ".heic", ".heif",
            ".svg", ".ico", ".avif"
        };
        public const int MaxFileSizeInMB = 5;
    }
}
