using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TecDocDataSplitter.DataProcessors;
using TecDocDataSplitter.Models;
using TecDocDataSplitter.Tools;
using System.IO.Compression;
using SharpCompress.Readers;
using SharpCompress.Common;
using SharpCompress.Archives;
using System.Collections;

namespace TecDocDataSplitter.Forms
{
    public partial class TecDocSplitterMainWindow : Form
    {
        private List<BaseTableDocumentStructure> _TablesConfigurations { get; set; }

        private Assembly _LoadedDataTypes { get; set; }

        private List<ITableParser> _TecDocTableParsers { get; set; } 

        private CancellationTokenSource _CancellationTokenSource { get; set; }

        public TecDocSplitterMainWindow()
        {
            InitializeComponent();

            button3.Enabled = false;
            _CancellationTokenSource = new CancellationTokenSource();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Json files(*.json)|*.json";
            DialogResult loadConfigDialogResult = openFileDialog1.ShowDialog();

            if (loadConfigDialogResult == DialogResult.OK)
            {
                string content = File.ReadAllText(openFileDialog1.FileName);

                if(!string.IsNullOrEmpty(content))
                {
                    this._TablesConfigurations = JsonConvert.DeserializeObject<List<BaseTableDocumentStructure>>(content);
                }

                if(this._TablesConfigurations.Count != 0)
                {
                    this.label1.Text = $"Configuration for tables loaded, count of tables in configuration: {this._TablesConfigurations.Count}";
                }
            }
            else
                return;

            openFileDialog1.Filter = null;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = $"Compiled assembly(*.dll)|*.dll";
            DialogResult loadAssemblyDialogResult = openFileDialog1.ShowDialog();

            if (loadAssemblyDialogResult == DialogResult.OK)
            {
                string assemblyPath = openFileDialog1.FileName;

                if (!string.IsNullOrEmpty(assemblyPath))
                {
                    this._LoadedDataTypes = Assembly.LoadFrom(assemblyPath);
                }

                if (this._LoadedDataTypes.DefinedTypes.Count() != 0)
                {
                    this.label2.Text = $"Assembly loaded. Count of data types defined in assembly {_LoadedDataTypes.DefinedTypes.Count()}";
                }
            }
            else
                return;

            openFileDialog1.Filter = null;

            if (this._LoadedDataTypes.DefinedTypes.Count() != 0 && this._TablesConfigurations.Count != 0)
            {
                button3.Enabled = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CheckFolderWithSourceArchives(this._CancellationTokenSource.Token);
            Thread.Sleep(500);
            RunUnExtractionProcess(this._CancellationTokenSource.Token);
        }

        private void RunUnExtractionProcess(CancellationToken cancellationToken)
        {
            if (directory == null || this._LoadedDataTypes == null 
                || this._TablesConfigurations == null || this._TablesConfigurations?.Count == 0)
                return;

            Task unExtOperationRunner = null;

            unExtOperationRunner = new Task(() => { UnExtractListOfArchiveProcess(); }, cancellationToken, TaskCreationOptions.LongRunning);

            unExtOperationRunner.Start();
        }

        private void UnExtractListOfArchiveProcess()
        {
            FileInfo[] files = null;

            if (this.directory != null)
            {
                files = directory.GetFiles();
            }

            if (!Directory.Exists(Environment.CurrentDirectory + "\\Temp"))
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\Temp");

            if (!Directory.Exists(Environment.CurrentDirectory + "\\Processed"))
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\Processed");

            int lastCounterValue = this.archivesToUnextrack;

            while (true)
            {
                reloadMark:
                if (lastCounterValue != archivesToUnextrack)
                    files = directory.GetFiles();

                foreach(FileInfo file in files)
                {
                    using (Stream stream = File.OpenRead(file.FullName))
                    {
                        try
                        {
                            using (var archive = ArchiveFactory.Open(stream))
                            {
                                foreach (var entry in archive.Entries)
                                {
                                    if (!entry.IsDirectory)
                                        entry.WriteToDirectory(Environment.CurrentDirectory + "\\Temp", new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    FileInfo[] unextructedFiles = new DirectoryInfo(Environment.CurrentDirectory + "\\Temp").GetFiles();

                    if (unextructedFiles.Length == 0)
                    {
                        ControlInvoker.SetControlText(this, label3, $"Something went wront while unextracting archive {file.Name}", true);
                        continue;
                    }

                    List<DataSeparator> fileParsers = new List<DataSeparator>(unextructedFiles.Length);

                    foreach(FileInfo f in unextructedFiles)
                    {
                        string tableNumber = f.Name.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                        BaseTableDocumentStructure tableConf = _TablesConfigurations.FirstOrDefault(t => t.TableNumber.Trim() == tableNumber.Trim());
                        var typeFromAssembly = _LoadedDataTypes.DefinedTypes.FirstOrDefault(t => t.Name == tableConf.TableName.Replace(" ", ""));

                        DataSeparator tableParser = new DataSeparator(typeFromAssembly, tableConf, new MySql.Data.MySqlClient.MySqlConnection(), f.FullName);

                        fileParsers.Add(tableParser);
                    }


                    if(fileParsers.Count != 0)
                    {
                        int threadCount = fileParsers.Count;
                        Task[] parsersExecutors = new Task[threadCount];

                        ArrayList = new ArrayList(threadCount);

                        for (int i =0; i < fileParsers.Count; i++)
                        {
                            DataSeparator current = fileParsers[i];

                            parsersExecutors[i] = new Task(() => { RunParser(current); }, TaskCreationOptions.LongRunning);

                            parsersExecutors[i].Start();
                        }

                        Task.WaitAll(parsersExecutors);
                    }

                    if (lastCounterValue != archivesToUnextrack)
                        goto reloadMark;


                }
            }
        }

        static void RunParser(DataSeparator separator)
        {
            List<object> toProcess = separator.ParseFile();
            ArrayList.Add(toProcess);
        }

        static ArrayList ArrayList = null;
        private void CheckFolderWithSourceArchives(CancellationToken cancellationToken)
        {
            DialogResult dialogResult = folderBrowserDialog1.ShowDialog();

            string folderPath = null;

            if (dialogResult == DialogResult.OK)
                folderPath = folderBrowserDialog1.SelectedPath;
            else
                return;

            if (string.IsNullOrEmpty(folderPath))
                return;

            Task folderChecker = null;

            folderChecker = new Task(() => { FolderChecker(folderPath); }, cancellationToken, TaskCreationOptions.LongRunning);

            folderChecker.Start();
        }

        DirectoryInfo directory = null;
        int archivesToUnextrack = 0;
        private void FolderChecker(string folderPath)
        {
            while (true)
            {
                directory = new DirectoryInfo(folderPath);

                if (directory == null)
                {
                    ControlInvoker.SetControlText(this, label3, "Cannot load info about selected directory");
                }

                FileInfo[] fileInfos = directory.GetFiles();

                if (fileInfos?.Length != 0)
                {
                    ControlInvoker.SetControlText(this, label3, $"Count of unextracted archives: {fileInfos.Length}");
                    archivesToUnextrack = fileInfos.Length;

                    Task.Delay(30000);
                }
                else
                {
                    ControlInvoker.SetControlText(this, label3, $"Folder is empty! Put archives to unextract or close the program!");
                    archivesToUnextrack = 0;

                    Task.Delay(30000);
                }
            }
        }

        private void TecDocSplitterMainWindow_Load(object sender, EventArgs e)
        {

        }
    }
}
