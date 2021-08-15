using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TecDocDataStructureAnalizer.Models;
using System.Globalization;

namespace TecDocDataStructureAnalizer.Analizers
{
    public class DataTypeCreator
    {
        private List<BaseTableDocumentStructure> _TablesConf { get; set; }

        public DataTypeCreator(List<BaseTableDocumentStructure> baseTables)
        {
            this._TablesConf = baseTables;
        }

        public string BuildDataTypeCodes()
        {
            string code = "using System;\n\nnamespace TecDocDataTypes\n{\n";

            for(int i = 0; i < this._TablesConf.Count; i++)
            {
                string part = this.AnalizeTableConfigItem(this._TablesConf[i]);

                code += part + "\n";
            }

            return code+"}";
        }

        private string AnalizeTableConfigItem(BaseTableDocumentStructure configItem)
        {
            string className = configItem.TableName.Replace(" ", "");
            string clssCode = $"public class " +
                $"{className}\n" +
                 "{\n";

            int fieldCounter = 1;

            for(int i = 0; i < configItem.DescribedFields.Count; i++)
            {
                TableFieldsDescriber currentProperty = configItem.DescribedFields[i];

                string dataType = "";

                switch(currentProperty.FieldDataType)
                {
                    case "Int64":
                        dataType = "long";
                        break;
                    case "Int32":
                        dataType = "int";
                        break;
                    case "String":
                        dataType = "string";
                        break;
                    default:
                        dataType = "string";
                        break;
                }

                string property = $"\tpublic {dataType} {(currentProperty.FieldName == className ? currentProperty.FieldName+"Field": currentProperty.FieldName)} " +"{ get; set; }\n";


                addMark:
                if (clssCode.Contains(property))
                {
                    property = $"\tpublic {dataType} {(currentProperty.FieldName == className ? currentProperty.FieldName + "Field" : currentProperty.FieldName)}{fieldCounter} " + "{ get; set; }\n";
                    fieldCounter++;
                    goto addMark;
                }
                
                clssCode += property;
            }
            clssCode += "}\n";
            int c = 2;
            return clssCode;
        }
    }
}
