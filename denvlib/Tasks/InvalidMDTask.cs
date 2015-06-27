using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;
using denvlib.Services;

namespace denvlib.Tasks
{
    public static class InvalidMDTask
    {
        public const TaskID ID = TaskID.InvalidMD;
        public static RandomGen random = new RandomGen();

        public static void Execute(ModuleDefMD module)
        {
            NETUtils.listener.OnWriterEvent += OnWriterEvent;
        }

        private static void OnWriterEvent(object sender, NETUtils.ModuleWriterListener.ModuleWriterListenerEventArgs e)
        {
            var writer = (ModuleWriterBase)sender;
            if (e.WriterEvent == ModuleWriterEvent.MDEndCreateTables)
            {
                int r = random.Next(8, 16);
                for (int i = 0; i < r; i++)
                    writer.MetaData.TablesHeap.ENCLogTable.Add(new RawENCLogRow((uint)random.Next(),(uint) random.Next()));
                r = random.Next(8, 16);
                for (int i = 0; i < r; i++)
                    writer.MetaData.TablesHeap.ENCMapTable.Add(new RawENCMapRow((uint)random.Next()));
            }
        }
    }
}
