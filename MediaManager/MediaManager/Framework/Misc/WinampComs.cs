//Poached some code from http://www.codeproject.com/Articles/12487/A-Winamp-Front-End-Library-with-C
namespace MediaManager.Framework.Misc
{
    using System;
    using System.Runtime.InteropServices;

    public static class WinampComs
    {
        const int WM_COMMAND = 0x111;
        private const string m_windowName = "Winamp v1.x";
        private const string strTtlEnd = " - Winamp";

        const int WA_NOTHING = 0;
        const int WA_PREVTRACK = 40044;
        const int WA_PLAY = 40045;
        const int WA_PAUSE = 40046;
        const int WA_STOP = 40047;
        const int WA_NEXTTRACK = 40048;
        const int WA_VOLUMEUP = 40058;
        const int WA_VOLUMEDOWN = 40059;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow([MarshalAs(UnmanagedType.LPTStr)] string lpClassName, [MarshalAs(UnmanagedType.LPTStr)] string lpWindowName);


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SendMessageA(IntPtr hwnd, int wMsg, int wParam, uint lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hwnd, string lpString, int cch);

        public static void Stop()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            SendMessageA(hwnd, WM_COMMAND, WA_STOP, WA_NOTHING);
        }

        public static void Play()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            SendMessageA(hwnd, WM_COMMAND, WA_PLAY, WA_NOTHING);
        }

        public static void Pause()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            SendMessageA(hwnd, WM_COMMAND, WA_PAUSE, WA_NOTHING);
        }

        public static void PrevTrack()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            SendMessageA(hwnd, WM_COMMAND, WA_PREVTRACK, WA_NOTHING);
        }

        public static void NextTrack()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            SendMessageA(hwnd, WM_COMMAND, WA_NEXTTRACK, WA_NOTHING);
        }

        public static void VolumeUp()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            SendMessageA(hwnd, WM_COMMAND, WA_VOLUMEUP, WA_NOTHING);
        }

        public static void VolumeDown()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            SendMessageA(hwnd, WM_COMMAND, WA_VOLUMEDOWN, WA_NOTHING);
        }

        public static void SetVolume(int Volume)
        { }

        public static string GetCurrentSongTitle()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);

            if (hwnd.Equals(IntPtr.Zero))
                return "N/A";

            string lpText = new string((char)0, 100);
            int intLength = GetWindowText(hwnd, lpText, lpText.Length);

            if ((intLength <= 0) || (intLength > lpText.Length))
                return "N/A";

            string strTitle = lpText.Substring(0, intLength);
            int intName = strTitle.IndexOf(strTtlEnd);
            int intLeft = strTitle.IndexOf("[");
            int intRight = strTitle.IndexOf("]");

            if ((intName >= 0) && (intLeft >= 0) && (intName < intLeft) && (intRight >= 0) && (intLeft + 1 < intRight))
                return strTitle.Substring(intLeft + 1, intRight - intLeft - 1);

            if ((strTitle.EndsWith(strTtlEnd)) && (strTitle.Length > strTtlEnd.Length))
                strTitle = strTitle.Substring(0, strTitle.Length - strTtlEnd.Length);

            int intDot = strTitle.IndexOf(".");
            if ((intDot > 0) && IsNumeric(strTitle.Substring(0, intDot)))
                strTitle = strTitle.Remove(0, intDot + 1);

            return strTitle.Trim();
        }

        private static bool IsNumeric(string Value)
        {
            try
            {
                double.Parse(Value);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
