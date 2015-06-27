using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using denvlib;

namespace denvlib
{
    public class Context
    {
        string fileName;
        Preset preset;
        List<TaskID> tasksList;

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        public string NewPath
        {
            get
            {
                string basePath = Path.GetDirectoryName(FileName);
                string newDirectoryName = Directory.CreateDirectory(Path.Combine(basePath, "Protected\\")).FullName;
                return Path.Combine(newDirectoryName + Path.GetFileName(FileName));
            }
        }

        public Preset Preset
        {
            get { return preset; }
            set 
            {
                preset = value;
                List<TaskID> tasks = new List<TaskID>();
                switch (preset)
                {
                    case denvlib.Preset.min:
                        {
                            tasks.Add(TaskID.Ren);
                            tasks.Add(TaskID.AntiDebug);
                        }
                        break;
                    case denvlib.Preset.mid:
                        {
                            tasks.Add(TaskID.Ren);
                            tasks.Add(TaskID.AntiDebug);
                            tasks.Add(TaskID.StringEnc);
                        }
                        break;
                    case denvlib.Preset.max:
                        {
                            tasks.Add(TaskID.Ren);
                            tasks.Add(TaskID.AntiDebug);
                            tasks.Add(TaskID.StringEnc);
                            tasks.Add(TaskID.CtrlFlow);
                            tasks.Add(TaskID.InvalidMD);
                        }
                        break;
                    default: break;
                }
                TasksList = tasks;
            }
        }

        public List<TaskID> TasksList { 
            get
            {
                return tasksList;
            }
            set
            {
                tasksList = value.Distinct().ToList();
                tasksList.Sort();
            }
        }

        public Context(string file)
        {
            FileName = file;
            Preset = denvlib.Preset.none;
        }

        public Context(string file, Preset preset)
        {
            FileName = file;
            Preset = preset;
        }

        public Context(string file, List<TaskID> taskList)
        {
            FileName = file;
            Preset = denvlib.Preset.none;
            TasksList = taskList;
        }
    }
}