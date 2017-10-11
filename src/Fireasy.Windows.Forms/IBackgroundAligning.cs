using System.Drawing;

namespace Fireasy.Windows.Forms
{
    /// <summary>
    /// 表示控件上的背景图像可以指定对齐方式。
    /// </summary>
    public interface IBackgroundAligning
    {
        /// <summary>
        /// 获取或设置背景图像的对齐方式。
        /// </summary>
        ContentAlignment BackgroundImageAligment { get; set; }
    }
}
