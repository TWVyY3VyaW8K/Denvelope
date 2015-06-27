using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using denvlib.Tasks;
using denvlib.Services;

namespace denvlib
{
    public static class Processor
    {
        public delegate void LogInfo(string s);
        public delegate void LogWarn(string s);
        public delegate void LogErr(string s);

        public static string Process(Context ctx, LogInfo info, LogWarn war, LogErr err)
        {
            try
            {
                string startobf = "Starting ";
                ModuleDefMD module = ModuleDefMD.Load(ctx.FileName);
                module.GlobalType.FindOrCreateStaticConstructor();
                foreach (TaskID task in ctx.TasksList)
                {
                    switch (task)
                    {
                        case AntiDebugTask.ID:
                            {
                                info(startobf + "Anti-Debug Task...");
                                AntiDebugTask.Execute(module);
                            } break;
                        case StringEncodingTask.ID:
                            {
                                info(startobf + "String encoding Task...");
                                StringEncodingTask.Execute(module);
                            } break;
                        case ControlFlowTask.ID:
                            {
                                info(startobf + "Control Flow Task...");
                                ControlFlowTask.Execute(module);
                            } break;
                       case InvalidMDTask.ID:
                            {
                                info(startobf + "Invalid Metadata Task...");
                                InvalidMDTask.Execute(module);
                            } break;
                        case RenameTask.ID:
                            RenameTask.IsObfuscationActive = true; break;
                    }
                }
                info(startobf + "Rename Task..." + Environment.NewLine);
                    RenameTask.Execute(module);
                module.Write(ctx.NewPath, new ModuleWriterOptions() { Listener = NETUtils.listener });
                return ctx.NewPath;
            }
            catch (Exception ex)
            {
                err(ex.Message);
                return "";
            }
        }
    }
}
