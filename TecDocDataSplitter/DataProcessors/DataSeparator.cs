using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TecDocDataSplitter.Models;

namespace TecDocDataSplitter.DataProcessors
{
    public interface ITableParser
    {

    }

    public class DataSeparator : ITableParser
    {
        private BaseTableDocumentStructure CurrentConfig { get; set; }
        private IDbConnection CurrentConnetction { get; set; }
        public Type Type { get; set; }
        private string Path { get; set; }
        public DataSeparator(Type dataType, BaseTableDocumentStructure config, IDbConnection dbConnection, string filePath)
        {
            Type = dataType;
            CurrentConfig = config;
            CurrentConnetction = dbConnection;
            Path = filePath;
        }

        public List<object> ParseFile()
        {
            string[] fileContent = File.ReadAllLines(Path);
            //List<string> processedVals = new List<string>();

            List<object> listOfTableConfigObj = new List<object>();
            for(int i = 0; i < fileContent.Length; i++)
            {
                string currentString = fileContent[i];
                //string processedString = "(";

                var activatedObj = Activator.CreateInstance(Type);
                for(int k =0; k < this.CurrentConfig.DescribedFields.Count; k++)
                {
                    PropertyInfo p = Type.GetProperty(this.CurrentConfig.DescribedFields[k].FieldName);

                    if(p == null)
                    {
                        p = Type.GetProperty(this.CurrentConfig.DescribedFields[k].FieldName+"Field");

                        if (p == null)
                            p = Type.GetProperty(this.CurrentConfig.DescribedFields[k].FieldName + "1");
                    }

                    TypeCode code = (TypeCode)Enum.Parse(typeof(TypeCode), p.PropertyType.Name);
                    string processedVal = currentString.Substring(this.CurrentConfig.DescribedFields[k].StartIndex, this.CurrentConfig.DescribedFields[k].DataLength).Trim();
                    //processedString += (this.CurrentConfig.DescribedFields[k].FieldDataType.ToUpper() == "STRING" 
                    //    ? $"\"{currentString.Substring(this.CurrentConfig.DescribedFields[k].StartIndex, this.CurrentConfig.DescribedFields[k].DataLength).Trim()}\"," 
                    //    :$"{currentString.Substring(this.CurrentConfig.DescribedFields[k].StartIndex, this.CurrentConfig.DescribedFields[k].DataLength)},");

                    if (string.IsNullOrEmpty(processedVal))
                    {
                        if (code == TypeCode.Int16 || code == TypeCode.Int32
                            || code == TypeCode.Int64 || code == TypeCode.UInt16
                            || code == TypeCode.UInt32 || code == TypeCode.UInt64
                            || code == TypeCode.Byte || code == TypeCode.SByte)
                        {
                            processedVal = "0";
                        }
                    }

                    p.SetValue(activatedObj, Convert.ChangeType(processedVal, code));
                }
                //processedString = processedString.Trim(new char[] { ',' });
                //processedString += ")";
                fileContent[i] = null;
                //processedVals.Add(processedString);
                listOfTableConfigObj.Add(activatedObj);
            }
            fileContent = null;

            GC.Collect(2, GCCollectionMode.Forced);

            return listOfTableConfigObj;
        }
    }
}
