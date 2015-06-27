using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using denvlib.Services;
using denvlib.Injections;

namespace denvlib.Tasks
{
    public static class StringEncodingTask
    {
        public const TaskID ID = TaskID.StringEnc;
        private static Random rand = new Random();

        public static void Execute(ModuleDefMD module)
        {
            int key = rand.Next(97, 122);
           /* rand.GetBytes(byteKey);

            var dataType = new TypeDefUser(module.GlobalType.Namespace, "", module.CorLibTypes.GetTypeRef("System", "ValueType"));
            RenameTask.Rename(dataType);
            dataType.Layout = TypeAttributes.ExplicitLayout;
            dataType.Visibility = TypeAttributes.NestedPrivate;
            dataType.IsSealed = true;
            dataType.ClassLayout = new ClassLayoutUser(1, (uint)byteKey.Length);
            module.GlobalType.NestedTypes.Add(dataType);

            var dataField = new FieldDefUser("", new FieldSig(dataType.ToTypeSig()))
            {
                IsStatic = true,
                HasFieldRVA = true,
                InitialValue = byteKey,
                Access = FieldAttributes.CompilerControlled
            };
            module.GlobalType.Fields.Add(dataField);
            RenameTask.Rename(dataField);*/

            TypeDef stringInjType = NETUtils.ImportType(typeof(StringEncInj));
            MethodDef stringDecMeth = NETUtils.GetMethodByName(stringInjType, "StringDec");
            stringDecMeth.DeclaringType = null;
           stringDecMeth.Body.Instructions[13].OpCode = OpCodes.Ldc_I4;
            stringDecMeth.Body.Instructions[13].Operand = key;
            RenameTask.Rename(stringDecMeth, true);
            TypeDef cctor = module.GlobalType;
            cctor.Methods.Add(stringDecMeth);
            foreach (var method in module.GetTypes().SelectMany(type => type.Methods))
            {
                if (method != stringDecMeth && method.HasBody)
                {
                    List<Instruction> stringInstr = method.Body.Instructions.Where(instr => instr.OpCode == OpCodes.Ldstr).ToList();
                    for (int i = 0; i < stringInstr.Count; i++)
                    {
                        int index = method.Body.Instructions.IndexOf(stringInstr[i]);
                        stringInstr[i].Operand = Ecrypt((string)stringInstr[i].Operand,key);
                        method.Body.Instructions.Insert(index + 1, Instruction.Create(OpCodes.Call, stringDecMeth));
                    }
                }
            }
        }

        public static string Ecrypt(string @string, int key)
        {
            byte[] b = new byte[@string.Length];
            for (int i = 0; i < @string.Length; i++)
                b[i] = (byte)(@string[i] ^ key);
            return Encoding.UTF8.GetString(b);
        }
    }
}
