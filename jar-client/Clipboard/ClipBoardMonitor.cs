using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace JAR.Client
{
    class ClipBoardMonitor
    {
        /// <summary> 
        /// Next clipboard viewer window  
        /// </summary> 
        private IntPtr hWndNextViewer;

        /// <summary> 
        /// The <see cref="HwndSource"/> for this window. 
        /// </summary> 
        private HwndSource hWndSource;

        private IntPtr WinProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case Win32.WM_CHANGECBCHAIN:
                    if (wParam == hWndNextViewer)
                    {
                        // clipboard viewer chain changed, need to fix it. 
                        hWndNextViewer = lParam;
                    }
                    else if (hWndNextViewer != IntPtr.Zero)
                    {
                        // pass the message to the next viewer. 
                        Win32.SendMessage(hWndNextViewer, msg, wParam, lParam);
                    }
                    break;

                case Win32.WM_DRAWCLIPBOARD:
                    // clipboard content changed 
                    ClipboardChanged?.Invoke(this, EventArgs.Empty);
                    // pass the message to the next viewer. 
                    Win32.SendMessage(hWndNextViewer, msg, wParam, lParam);
                    break;
            }

            return IntPtr.Zero;
        }


        internal void Enable(MainWindow mainWindow)
        {
            WindowInteropHelper wih = new WindowInteropHelper(mainWindow);
            wih.EnsureHandle();
            hWndSource = HwndSource.FromHwnd(wih.Handle);

            hWndSource.AddHook(this.WinProc);   // start processing window messages 
            hWndNextViewer = Win32.SetClipboardViewer(hWndSource.Handle);   // set this window as a viewer 
        }

        internal void Disable()
        {
            // remove this window from the clipboard viewer chain 
            Win32.ChangeClipboardChain(hWndSource.Handle, hWndNextViewer);

            hWndNextViewer = IntPtr.Zero;
            hWndSource.RemoveHook(this.WinProc);
        }

        public event EventHandler ClipboardChanged;
    }


}
