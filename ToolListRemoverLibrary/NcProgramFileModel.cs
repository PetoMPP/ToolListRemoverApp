using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolListRemoverLibrary
{
    public class NcProgramFileModel
    {
        public string FileId { get; set; }
        public string MachineId { get; set; }
        public string Extension { get; set; }
        public string StateId { get; set; }
        public int Version { get; set; }
        public string Path { get; set; }
    }
}
