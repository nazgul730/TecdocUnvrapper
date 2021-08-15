namespace TecDocDataStructureAnalizer.Models
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
