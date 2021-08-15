using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TecDocDataSplitter.Forms;

namespace TecDocDataSplitter
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Thread application = new Thread(() =>
            {
                TecDocSplitterMainWindow t = new TecDocSplitterMainWindow();

                t.ShowDialog();
            });
            application.SetApartmentState(ApartmentState.STA);
            application.Start();            
            application.Join();
        }
    }
}
