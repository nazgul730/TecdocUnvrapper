using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TecDocDataSplitter.Models
{
    public class TableFieldsDescriber
    {
        public string FieldName { get; set; }
        public string OldFieldName { get; set; }
        public int StartIndex { get; set; }
        public int DataLength { get; set; }
        public string FieldDataType { get; set; }
        public string Description { get; set; }
    }
}
