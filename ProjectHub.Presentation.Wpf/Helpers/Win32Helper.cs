using System.Runtime.InteropServices;

namespace ProjectHub.Presentation.Wpf.Helpers;

/// <summary>
/// Windows API 帮助类
/// 封装常用的 Win32 API 调用
/// </summary>
public static class Win32Helper
{
    /// <summary>
    /// WM_COPYDATA 消息常量
    /// </summary>
    public const int WM_COPYDATA = 0x004A;

    /// <summary>
    /// COPYDATASTRUCT 结构体，用于进程间通信
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct COPYDATASTRUCT
    {
        public IntPtr dwData;
        public int cbData;
        public IntPtr lpData;
    }

    /// <summary>
    /// 查找窗口句柄
    /// </summary>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    /// <summary>
    /// 发送窗口消息
    /// </summary>
    [DllImport("user32.dll")]
    public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);
}
