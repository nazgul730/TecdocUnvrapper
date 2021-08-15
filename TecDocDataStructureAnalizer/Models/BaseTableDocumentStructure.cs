using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TecDocDataStructureAnalizer.Models
{
    public class BaseTableDocumentStructure
    {
        public string TableNumber { get; set; }
        public string TableName { get; set; }
        public int RowLength { get; set; }
        public List<string> PrimaryKeyFieldNames { get; set; }
        public List<TableFieldsDescriber> DescribedFields { get; set; }
    }
}
