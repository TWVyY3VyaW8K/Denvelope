using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using dnlib.DotNet.MD;
using denvlib.Services;
using denvlib.Injections;

namespace denvlib.Tasks
{
    public static class StringEncodingTaskTest
    {
        public const TaskID ID = TaskID.StringEnc;
        private static byte[] encodingBytes;
        private static RandomGen random = new RandomGen();
        private static MethodDef cctor;
        private static FieldDef GlobalDataField;
        private static Dictionary<FieldDef, Tuple<byte[], int>> staticFields;

        public static void Execute(ModuleDefMD module)
        {
            cctor = module.GlobalType.FindStaticConstructor();
            Dictionary<FieldDef, Tuple<byte[], int>> fields = new Dictionary<FieldDef,Tuple<byte[], int>>();
            List<byte> data = new List<byte>();
            int count = 0;
            foreach (var method in  module.GetTypes().SelectMany(type => type.Methods))
            {
                List<Instruction> stringInstr = method.Body.Instructions.Where(instr => instr.OpCode == OpCodes.Ldstr).ToList();
                for (int i = 0; i < stringInstr.Count; i++)
                {
                    byte[] stringByte = Encoding.UTF8.GetBytes(stringInstr[i].Operand as string);
                    data.AddRange(stringByte);
                    FieldDef field = CreateField(module);
                    fields.Add(field, Tuple.Create(stringByte, count));
                    method.DeclaringType.Fields.Add(field);
                    stringInstr[i].OpCode = OpCodes.Ldsfld;
                    stringInstr[i].Operand = field;
                    count++;
                }
            }
            staticFields = fields;
            data = Encrypt(data.ToArray()).ToList();
            var dataType = new TypeDefUser("", "", module.CorLibTypes.GetTypeRef("System", "ValueType"));
            RenameTask.Rename(dataType);
            dataType.Layout = TypeAttributes.ExplicitLayout;
            dataType.Visibility = TypeAttributes.NestedPrivate;
            dataType.IsSealed = true;
            dataType.ClassLayout = new ClassLayoutUser(1, (uint)data.Count);
            module.GlobalType.NestedTypes.Add(dataType);

            var dataField = new FieldDefUser("", new FieldSig(dataType.ToTypeSig()))
            {
                IsStatic = true,
                HasFieldRVA = true,
                InitialValue = data.ToArray(),
                Access = FieldAttributes.CompilerControlled
            };
            module.GlobalType.Fields.Add(dataField);
            GlobalDataField = dataField;
            RenameTask.Rename(dataField);
            NETUtils.listener.OnWriterEvent += OnWriterEvent;
        }

        private static FieldDef CreateField(ModuleDefMD module)
        {
            FieldAttributes attrb = FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.HasFieldRVA | FieldAttributes.CompilerControlled;
            FieldDef field = new FieldDefUser("", new FieldSig(module.CorLibTypes.String), attrb);
            RenameTask.Rename(field);
            return field;
        }

        private static byte[] Encrypt(byte[] data)
        {
            encodingBytes = new byte[16];
            random.GetBytes(encodingBytes);
            for (int i = 0; i < data.Length; i++)
                data[i] ^= encodingBytes[i & 15];
            return data;
        }

        private static void OnWriterEvent(object sender, NETUtils.ModuleWriterListener.ModuleWriterListenerEventArgs e)
        {
            if (e.WriterEvent == ModuleWriterEvent.MDBeginCreateTables)
            {
                ModuleWriterBase writer = (ModuleWriterBase)sender;
                uint sigBlob = writer.MetaData.BlobHeap.Add(encodingBytes);
                uint sigRid = writer.MetaData.TablesHeap.StandAloneSigTable.Add(new RawStandAloneSigRow(sigBlob));
                uint sigToken = 0x11000000 | sigRid;
                Inject(sigToken);
            }
        }

        private static void Inject(uint sigToken)
        {
            ModuleDef mod = cctor.Module;
            TypeDef stringInjType = NETUtils.ImportType(typeof(StringEncInj));
            MethodDef stringInjMethod = NETUtils.GetMethodByName(stringInjType, "StringInj");
            MethodDef InsertInstr = NETUtils.GetMethodByName(stringInjType, "InsertFields");
            stringInjMethod.DeclaringType = null;
            cctor.DeclaringType.Methods.Add(stringInjMethod);
            RenameTask.Rename(stringInjMethod);
            cctor.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, stringInjMethod));
            var instr = stringInjMethod.Body.Instructions;
            instr[7].OpCode = OpCodes.Ldc_I4;
            instr[7].Operand = Convert.ToInt32(sigToken);
            instr[10].Operand = GlobalDataField;
            instr[20].Operand = GlobalDataField;
            instr[36].Operand = GlobalDataField;
            instr[44].Operand = GlobalDataField;
            MethodDef insertMeth = new MethodDefUser("", MethodSig.CreateStatic(mod.CorLibTypes.Void),
                MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig);
            RenameTask.Rename(insertMeth);
            insertMeth.Body = new CilBody();
            cctor.Body.Instructions.Insert(1, Instruction.Create(OpCodes.Call, insertMeth));
            cctor.DeclaringType.Methods.Add(insertMeth);
            List<Instruction> instertListInstr = InsertInstr.Body.Instructions.ToList();
            instertListInstr.RemoveAt(instertListInstr.Count - 1);
            int i = 0;
            foreach (var item in staticFields)
            {
                Instruction[] instrList = new Instruction[instertListInstr.Count];
                instertListInstr.CopyTo(instrList);
                int stringlenght = item.Value.Item1.Length;
                instrList[2].Operand = GlobalDataField;
                instrList[3].OpCode = OpCodes.Ldc_I4;
                instrList[3].Operand = i;
                instrList[4].OpCode = OpCodes.Ldc_I4;
                instrList[4].Operand = i + stringlenght;
                instrList[6].Operand = item.Key;
                i += stringlenght;
                foreach (var instrr in instrList)
                    insertMeth.Body.Instructions.Add(instrr.Clone());
            }
            insertMeth.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }
    }
}
