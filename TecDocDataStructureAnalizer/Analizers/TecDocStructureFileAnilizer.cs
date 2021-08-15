using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TecDocDataStructureAnalizer.Models;

namespace TecDocDataStructureAnalizer.Analizers
{
    public class TecDocStructureConfigBuilder
    {
        private List<string> _DataToAnalize { get; set; }

        private object _Locker = new object();

        public TecDocStructureConfigBuilder(IEnumerable<string> dataToAnalize)
        {
            this._DataToAnalize = dataToAnalize.ToList();
        }

        public List<BaseTableDocumentStructure> StartToBuildStrucktureConfig()
        {
            int threadCount = Environment.ProcessorCount < 10 ? Environment.ProcessorCount * 4 : Environment.ProcessorCount * 2;

            List<BaseTableDocumentStructure> resultContainer = new List<BaseTableDocumentStructure>();
            for(int i = 0; i < this._DataToAnalize.Count; i++)
            //Parallel.For(0, this._DataToAnalize.Count, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, i =>
             {
                 string currentPage = this._DataToAnalize[i];
                 try
                 {
                    anotherTableOnPage:
                     string[] splittedPage = currentPage.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                     int cnt = 1;

                     BaseTableDocumentStructure currentPageStructure = new BaseTableDocumentStructure();

                     for (int k = 0; k < splittedPage.GetLength(0); k++)
                     {
                         string row = splittedPage[k].Trim(new char[] { ' ', '\t' });
                         splittedPage[k] = row;
                     }

                     Regex tableNumber = new Regex("^[0-9]{3} ", RegexOptions.Compiled);

                    Match tableNameMatch = tableNumber.Match(splittedPage[0]);

                    string newTable = "";
                    int takeCountOfTable = 0;
                    for (int k = 0; k < splittedPage.GetLength(0); k++)
                     {
                        
                         if(k !=0 && tableNumber.IsMatch(splittedPage[k]))
                         {
                            if(!splittedPage[k].StartsWith("999"))
                            {
                                takeCountOfTable = k;
                                newTable += string.Join("\n",splittedPage.Skip(k));
                            }
                         }
                     }

                    Regex multipleSpaces = new Regex("[ ]{2,}", RegexOptions.Compiled);
                    for (int k = 0; k < splittedPage.GetLength(0); k++)
                    {
                        string row = multipleSpaces.Replace(splittedPage[k], "♥");
                        splittedPage[k] = row;
                    }

                    if (tableNameMatch.Groups[0].Value.ToString() == "400 ")
                    {
                        cnt = 4;

                        int primaryKeyRowSplitCounter = splittedPage[5].Split("♥".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Count();

                        if (primaryKeyRowSplitCounter > 2)
                        {
                            cnt = 5;
                            cnt = 5;
                            string copyOfPrimaryKeyRow = splittedPage[5].Replace(splittedPage[5].Split("♥".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0], "").Replace(splittedPage[5].Split("♥".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1], "");
                            List<string> tempSplittedPage = splittedPage.ToList();
                            cnt = 6;

                            tempSplittedPage.Insert(6, copyOfPrimaryKeyRow);
                            splittedPage = tempSplittedPage.ToArray();
                        }
                    }

                    splittedPage = takeCountOfTable == 0 ? splittedPage : splittedPage.Take(takeCountOfTable).ToArray();
                     splittedPage = splittedPage.Take(1).Union(splittedPage.Skip(3).Take(1)).Union(splittedPage.Skip(5).Take(1)).Union(splittedPage.Skip(7).Take(50)).ToArray();


                    currentPageStructure.TableNumber = tableNameMatch.Groups[0].Value.ToString().Trim();
                     currentPageStructure.TableName = CultureInfo
                                .CurrentCulture
                                    .TextInfo.ToTitleCase(splittedPage[0].Replace(tableNameMatch.Groups[0].Value.ToString(), "").Trim()
                                            .Replace("(", "")
                                            .Replace(")", "")
                                            .Replace("-", "")
                                            .Replace("/", "")
                                            .Replace("–", "")).Replace(" ", "").Split("…".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                     currentPageStructure.RowLength = int.Parse(splittedPage[1].Split("♥".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1]);

                    //int primaryKeyRowSplitCounter = splittedPage[2].Split("♥".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Count();

                    //if(primaryKeyRowSplitCounter > 2)
                    //{
                    //    cnt = 5;
                    //    cnt = 5;
                    //    string copyOfPrimaryKeyRow = splittedPage[2].Replace(splittedPage[2].Split("♥".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0], "").Replace(splittedPage[2].Split("♥".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1], "");
                    //    List<string> tempSplittedPage = splittedPage.ToList();
                    //    cnt = 6;

                    //    tempSplittedPage.Insert(3, copyOfPrimaryKeyRow);
                    //    splittedPage = tempSplittedPage.ToArray();
                    //}

                     currentPageStructure.PrimaryKeyFieldNames = splittedPage[2].Split("♥".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList();

                     List<TableFieldsDescriber> describers = new List<TableFieldsDescriber>();

                    if(tableNameMatch.Groups[0].Value.ToString() == "001 ")
                    {
                        describers = GetTableFields(splittedPage.Skip(2).ToList());
                    }
                    else
                     describers = GetTableFields(splittedPage.Skip(3).ToList());

                     cnt = 4;

                     currentPageStructure.DescribedFields = describers;

                     lock (this._Locker)
                     {
                         resultContainer.Add(currentPageStructure);

                        if(!string.IsNullOrEmpty(newTable))
                        {
                            currentPage = newTable;
                            goto anotherTableOnPage;
                        }
                     }
                 }
                 catch(Exception ex)
                 {
                     bool throughBeginmark = false;

                 beginMark:
                     
                     string[] splittedPage = currentPage.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                     int cnt = 1;

                     BaseTableDocumentStructure currentPageStructure = new BaseTableDocumentStructure();

                     for (int k = 0; k < splittedPage.GetLength(0); k++)
                     {
                         string row = splittedPage[k].Trim(new char[] { ' ', '\t' });
                         if (k < 6 && k != 0 && !splittedPage[k].Contains(":"))
                             splittedPage[k] = "";
                         else
                             splittedPage[k] = row;
                     }

                     splittedPage = splittedPage.Where(t => !string.IsNullOrEmpty(t)).ToArray();

                     Regex multipleSpaces = new Regex("[ ]{2,}", RegexOptions.Compiled);
                     for (int k = 0; k < splittedPage.GetLength(0); k++)
                     {
                         string row = multipleSpaces.Replace(splittedPage[k], "♥");
                         splittedPage[k] = row;
                     }
                     if(!throughBeginmark)
                        splittedPage = splittedPage.Take(1).Union(splittedPage.Skip(3).Take(1)).Union(splittedPage.Skip(5).Take(1)).Union(splittedPage.Skip(7).Take(50)).ToArray();
                     else
                     {
                         splittedPage = splittedPage.Take(1).Union(splittedPage.Skip(2).Take(1)).Union(splittedPage.Skip(4).Take(1)).Union(splittedPage.Skip(7).Take(50)).ToArray();
                     }
                     
                     Regex tableNumber = new Regex("^[0-9]{3}", RegexOptions.Compiled);
                     Match tableNameMatch = tableNumber.Match(splittedPage[0]);
                     currentPageStructure.TableNumber = tableNameMatch.Groups[0].Value.ToString().Trim();
                     currentPageStructure.TableName = splittedPage[0].Replace(tableNameMatch.Groups[0].Value.ToString(), "").Trim();
                     try
                     {
                         currentPageStructure.RowLength = int.Parse(splittedPage[1].Split("♥".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1]);
                     }
                     catch(Exception ex2)
                     {
                        try
                        {
                            Regex length = new Regex("(Length:(♥| )+[0-9]+){1}");
                            Match lenMatch = length.Match(splittedPage[1]);

                            if(!lenMatch.Success)
                            {
                                splittedPage = currentPage.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                splittedPage = splittedPage.Take(1).Union(splittedPage.Skip(2).Take(1)).Union(splittedPage.Skip(4).Take(1)).Union(splittedPage.Skip(6).Take(50)).ToArray();


                                lenMatch = length.Match(splittedPage[1]);

                                currentPageStructure.RowLength = int.Parse(new Regex("(Length:(♥| )+){1}").Replace(lenMatch.Groups[0].Value.ToString(), ""));

                                for (int k = 0; k < splittedPage.GetLength(0); k++)
                                {
                                    string row = multipleSpaces.Replace(splittedPage[k], "♥");
                                    splittedPage[k] = row;
                                }
                            }
                            else
                            {

                            }
                        }
                        catch(Exception ex3)
                        {
                            throughBeginmark = true;
                            goto beginMark;
                        }                         
                     }
                     currentPageStructure.PrimaryKeyFieldNames = splittedPage[2].Split("♥".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList();

                     List<TableFieldsDescriber> describers = new List<TableFieldsDescriber>();

                     describers = GetTableFields(splittedPage.Skip(3).ToList());

                     cnt = 4;

                     currentPageStructure.DescribedFields = describers;

                     lock (this._Locker)
                     {
                         resultContainer.Add(currentPageStructure);
                     }
                 }
             }//);

            return resultContainer;
        }

        private List<TableFieldsDescriber> GetTableFields(List<string> unsplittedRows)
        {
            List<TableFieldsDescriber> result = new List<TableFieldsDescriber>();

            Regex alnum = new Regex("[^A-z0-9]+");

            for (int i = 0; i < unsplittedRows.Count; i++)
            {
                string[] toBuildFieldData = unsplittedRows[i].Split("♥".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                
                if ((toBuildFieldData.GetLength(0) == 6 || toBuildFieldData.GetLength(0) > 6) && toBuildFieldData[0] != "Name")
                {
                    try
                    {
                        TableFieldsDescriber newItem = new TableFieldsDescriber
                        {
                            FieldName = alnum.Replace(toBuildFieldData[0].Replace("(0 = No, 1 = Yes)", "").Replace("-", ""), ""),
                            OldFieldName = toBuildFieldData[1],
                            StartIndex = int.Parse(toBuildFieldData[2]),
                            DataLength = int.Parse(toBuildFieldData[3]),
                            FieldDataType = GetType(toBuildFieldData[4]),
                            Description = toBuildFieldData[5]

                            
                        };

                        result.Add(newItem);
                    }
                    catch(Exception ex)
                    {
                        try
                        {
                            TableFieldsDescriber newItem = new TableFieldsDescriber
                            {
                                FieldName = alnum.Replace(toBuildFieldData[0].Replace("(0 = No, 1 = Yes)", "").Replace("-", ""), ""),
                                OldFieldName = null,
                                StartIndex = int.Parse(toBuildFieldData[1]),
                                DataLength = int.Parse(toBuildFieldData[2]),
                                FieldDataType = GetType(toBuildFieldData[3]),
                                Description = toBuildFieldData[4]
                            };

                            result.Add(newItem);
                        }
                        catch(Exception)
                        {

                        }
                    }
                }
                else if((toBuildFieldData.GetLength(0) == 5 || toBuildFieldData.GetLength(0) > 5) && toBuildFieldData[0] != "Name")
                {
                    try
                    {
                        TableFieldsDescriber newItem = new TableFieldsDescriber
                        {
                            FieldName = alnum.Replace(toBuildFieldData[0].Replace("(0 = No, 1 = Yes)", "").Replace("-", ""), ""),
                            OldFieldName = null,
                            StartIndex = int.Parse(toBuildFieldData[1]),
                            DataLength = int.Parse(toBuildFieldData[2]),
                            FieldDataType = GetType(toBuildFieldData[3]),
                            Description = toBuildFieldData[4]
                        };

                        result.Add(newItem);
                    }
                    catch
                    {
                        try
                        {
                            TableFieldsDescriber newItem = new TableFieldsDescriber
                            {
                                FieldName = alnum.Replace(toBuildFieldData[0].Replace("(0 = No, 1 = Yes)", "").Replace("-", ""), ""),
                                OldFieldName = toBuildFieldData[1],
                                StartIndex = int.Parse(toBuildFieldData[2]),
                                DataLength = int.Parse(toBuildFieldData[3]),
                                FieldDataType = GetType(toBuildFieldData[4]),
                                Description = toBuildFieldData[4]
                            };
                            result.Add(newItem);
                        }
                        catch (Exception)
                        {
                            TableFieldsDescriber newItem = new TableFieldsDescriber
                            {
                                FieldName = alnum.Replace(toBuildFieldData[0].Replace("(0 = No, 1 = Yes)", "").Replace("-", ""), ""),
                                OldFieldName = toBuildFieldData[1],
                                StartIndex = result[result.Count - 1].StartIndex + result[result.Count - 1].DataLength,
                                DataLength = int.Parse(toBuildFieldData[2]),
                                FieldDataType = GetType(toBuildFieldData[3]),
                                Description = toBuildFieldData[4]
                            };

                            result.Add(newItem);
                        }
                    }
                }
            }

            return result;
        }

        private string GetType(string tdTypeDefinition)
        {
            TypeCode dataType = TypeCode.String;
            switch (tdTypeDefinition)
            {
                case "N":
                    dataType = TypeCode.Int64;
                    break;
                case "(N)":
                    dataType = TypeCode.Int64;
                    break;
                case "U":
                    dataType = TypeCode.String;
                    break;
                case "(U)":
                    dataType = TypeCode.String;
                    break;
                case "C":
                    dataType = TypeCode.String;
                    break;
                case "(C)":
                    dataType = TypeCode.String;
                    break;
                default:
                    dataType = TypeCode.String;
                    break;
            }

            return dataType.ToString();
        }
    }
}
