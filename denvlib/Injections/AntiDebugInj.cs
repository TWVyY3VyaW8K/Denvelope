using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace denvlib.Injections
{
    public class AntiDebugInj
    {
        [DllImport("kernel32")]
        public static extern bool IsDebuggerPresent();

        public static void Check()
        {
            Thread th = new Thread(t);
            th.Start();
           /* Thread t = new Thread(() =>
             {
                 //Thread.CurrentThread.IsBackground = true;
                 while (true)
                 {
                     if (Debugger.IsAttached || IsDebuggerPresent())
                         Environment.FailFast("");
                     Thread.Sleep(1000);
                 }
             });
            t.Start();*/
        }

        public static void t()
        {
            Thread.CurrentThread.IsBackground = true;
            while (true)
            {
                if (Debugger.IsAttached || IsDebuggerPresent())
                    Environment.FailFast("");
                Thread.Sleep(1000);
            }
        }
    }
}