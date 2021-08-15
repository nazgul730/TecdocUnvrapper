using Newtonsoft.Json;
using Spire.Pdf;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TecDocDataStructureAnalizer.Analizers;
using TecDocDataStructureAnalizer.Models;

namespace TecDocDataStructureAnalizer
{
    class Program
    {
        static void Main(string[] args)
        {
            PdfDocument document = new PdfDocument("TecDoc-Data-Format_Version_2.5_EN.pdf");
            List<string> s = new List<string>();

            foreach (PdfPageBase page in document.Pages)
            {
                string bp = page.ExtractText();
                s.Add(bp);
            }

            int c = 0;
            Regex reg = new Regex("((\\s)+© TecAlliance GmbH(\\s+)Seite ([0-9]{2,3}) von ([0-9]){3})");

            s = s.Select(t => t.Replace("Evaluation Warning : The document was created with Spire.PDF for .NET.", "").Replace("\r\n\r\n\r\n\r\n ", "")).ToList();

            s = s.Select(t => t.Trim(new char[] { ' ' }).Replace("TecDoc Data Format Data Format Version 2.5 (including version 2.4) \r\n               2.0.3, Published, 23.09.2020", "")).ToList();

            s = s.Skip(19).Take(1000).ToList();

            s = s.Select(t => t.Trim(new char[] { ' ', '\n', '\r' }).Replace("TecDoc Data Format Data Format Version 2.5 (including version 2.4) \r\n", "")).ToList();

            s = s.Select(t => t.Replace("                 2.0.3, Published, 23.09.2020                       Manufacturerers and Model Series \r\n", "")
            .Replace("                 2.0.3, Published, 23.09.2020                       Passenger Car Information \r\n", "")
            .Replace("                 2.0.3, Published, 23.09.2020                       Commercial Vehicle Information \r\n", "")
            .Replace("                 2.0.3, Published, 23.09.2020                       Engine Information \r\n", "")
            .Replace("                 2.0.3, Published, 23.09.2020                       Axle Information \r\n", "")
            .Replace("                 2.0.3, Published, 23.09.2020                       Search Structure \r\n", "")
            .Replace("                 2.0.3, Published, 23.09.2020                       Generic Articles \r\n", "")
            .Replace("                 2.0.3, Published, 23.09.2020                 Data Supplier Data", "")
            .Replace("                 2.0.3, Published, 23.09.2020                       General Supplier Data Tables", "")
            .Replace("                 2.0.3, Published, 23.09.2020                       Article Data Tables", "")
            .Replace("                 2.0.3, Published, 23.09.2020                                                  Each ArtNo must be contained in the data table (->DT200).", "")
            .Replace("                 2.0.3, Published, 23.09.2020                       Linkage Data Tables", "")
            .Replace("             2.0.3, Published, 23.09.2020                                     Linkages of the same article with the same vehicle and the same generic article must differ in \r\n                                     any relevant point.  \r\n\r\n                                     Valid differences are: \r\n\r\n                                     •     Article description (-> 200) \r\n\r\n                                     •     Additional article description (-> 200) \r\n\r\n                                     •     Article criteria set to immediate display (-> 210) \r\n\r\n                                     •     Article information set to immediate display (-> 206) \r\n\r\n                                     •     Linkage criteria set to immediate display (-> 400) \r\n\r\n                                     •     Linkage information set to immediate display (-> 401) \r\n                    If an article with the same generic article is linked more than once to the same linkage target, the \r\n                    LfdNr will determine the display sequence of these linkages.  \r\n\r\n                    The SeqNo within a combination (LnkTargetType, LnktargetNo, GenArtNo, ArtNo) must always  start \r\n                    with 1 and must be allocated without grap. Therefore, if an article is linked twice to a vehicle, the \r\n                    linkages need to have the LfdNr 1 and 2. This has to be observed, especially if linkages are deleted. \r\n                    Example: A brand has allocated the LfdNr 1, 2 and 3. In the following Data Release, the linkage with the \r\n                    LfdNr 1 is deleted. The remaining two linkages now have to be renumbered accordingly with 1 and 2, \r\n                    as the LfdNr 2 and 3 only are not permitted.", "")
            .Replace("                 2.0.3, Published, 23.09.2020                 Reference Data Tables", "")
            .Replace("                 2.0.3, Published, 23.09.2020                 General Reference Data", "")
            ).ToList();

            s = s.Select(t => reg.Replace(t, "")).ToList();
            s = s.Where(t => !string.IsNullOrEmpty(t)).ToList();

            Regex detectTable = new Regex("^[0-9]{3}");
            int previousTableIndex = 0;
            for (int i = 0; i < s.Count; i++)
            {
                if (detectTable.IsMatch(s[i]))
                {
                    previousTableIndex = i;
                }
                else
                {
                    s[previousTableIndex] = s[previousTableIndex] + s[i];
                    s[i] = "";
                }
            }

            s = s.Where(t => !string.IsNullOrEmpty(t)).ToList();

            TecDocStructureConfigBuilder structureFileAnilizer = new TecDocStructureConfigBuilder(s);
            List<BaseTableDocumentStructure> tablesStruckture = structureFileAnilizer.StartToBuildStrucktureConfig();
            c = 2;
            File.WriteAllText("CurrentTecDocDataStructure.json", JsonConvert.SerializeObject(tablesStruckture, Formatting.Indented));

            DataTypeCreator dtc = new DataTypeCreator(tablesStruckture);
            string generatedCode = dtc.BuildDataTypeCodes();
            c = 3;

            CompilerParameters cp = new CompilerParameters();
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");

            cp.GenerateExecutable = false;
            cp.GenerateInMemory = false;
            cp.TreatWarningsAsErrors = false;
            cp.OutputAssembly = "TecDocDataTypes.dll";
            cp.ReferencedAssemblies.AddRange(new string[] { "System.dll" });

            CompilerResults cr = provider.CompileAssemblyFromSource(cp,
                    generatedCode);
            c = 4;
        }
    }
}
