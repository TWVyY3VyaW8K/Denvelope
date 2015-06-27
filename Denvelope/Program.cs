using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using denvlib;

namespace Denvelope
{
    class Program
    {
        static void Main(string[] args)
        {
            string file = args[0];
            List<TaskID> taskList = new List<TaskID>();
            Preset preset = Preset.none;
            if (args.Length > 1)
            {
                for (int i = 1; i < args.Length; i++)
                {
                    string arg = args[i];
                    if (arg == "-min")
                        preset = Preset.min;
                    else if (arg == "-mid")
                        preset = Preset.mid;
                    else if (arg == "-max")
                        preset = Preset.max;
                    else
                    {
                        if (arg == "-invalidMd")
                            taskList.Add(TaskID.InvalidMD);
                        if (arg == "-ren")
                            taskList.Add(TaskID.Ren);
                        if (arg == "-antiDbg")
                            taskList.Add(TaskID.AntiDebug);
                        if (arg == "-stringEnc")
                            taskList.Add(TaskID.StringEnc);
                        if (arg == "-ctrlFlow")
                            taskList.Add(TaskID.CtrlFlow);
                    }
                }
            }
            else
                preset = Preset.max;
            Context ctx;
            if (preset == Preset.none)
                ctx = new Context(file, taskList);
            else
                ctx = new Context(file, preset);
            Processor.LogInfo l = LogInfo;
            Processor.LogWarn w = LogWarn;
            Processor.LogErr e = LogErr;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("                       Denvelope obfuscator 2014 - 2015" + Environment.NewLine);
            Console.ResetColor();
            Console.WriteLine("File: " + file + Environment.NewLine);
            Processor.Process(ctx, l, w, e);
            Console.WriteLine("Finished.");
            Console.Read();
        }

        public static void LogInfo(string s)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("[Info] "+s);
            Console.ResetColor();
        }

        public static void LogWarn(string s)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[Warning] " + s);
            Console.ResetColor();
        }

        public static void LogErr(string s)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[Error] " + s);
            Console.ResetColor();
        }
    }
}
