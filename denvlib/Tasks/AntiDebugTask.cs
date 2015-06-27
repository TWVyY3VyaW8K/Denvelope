using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using denvlib.Services;
using denvlib.Injections;

namespace denvlib.Tasks
{
    public static class AntiDebugTask
    {
        public const TaskID ID = TaskID.AntiDebug;

        public static void Execute(ModuleDefMD module)
        {
            TypeDef globalType = module.GlobalType;
            MethodDef cctor = globalType.FindStaticConstructor();
            TypeDef antiDebugType = NETUtils.ImportType(typeof(AntiDebugInj));
            MethodDef check = NETUtils.GetMethodByName(antiDebugType, "Check");
            MethodDef isDbgPresent = NETUtils.GetMethodByName(antiDebugType, "IsDebuggerPresent");
            MethodDef thread = NETUtils.GetMethodByName(antiDebugType, "t");
            antiDebugType.Methods.Remove(check);
            antiDebugType.Methods.Remove(isDbgPresent);
            antiDebugType.Methods.Remove(thread);
            //RenameTask.Rename(antiDebugType);
            RenameTask.Rename(check);
            RenameTask.Rename(thread);
            globalType.Methods.Add(check);
            globalType.Methods.Add(isDbgPresent);
            globalType.Methods.Add(thread);
            cctor.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, check));
        }
    }
}
