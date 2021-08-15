using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TecDocDataSplitter.Tools
{
    public static class ControlInvoker
    {
        delegate void SetTextCallback(Form f, Control ctrl, string text, bool concatMode = false);
        delegate void SetVisibleCallback(Form f, Control ctrl, bool visible);

        public static void SetControlText(Form form, Control ctrl, string text, bool concatMode = false)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (ctrl.InvokeRequired)
            {
                if (concatMode)
                {
                    form.Invoke(new MethodInvoker(() =>
                    {
                        ctrl.Text += text;
                    }));
                }
                else
                {
                    SetTextCallback d = new SetTextCallback(SetControlText);
                    form.Invoke(d, new object[] { form, ctrl, text, concatMode });
                }
            }
            else
            {
                ctrl.Text = text;
            }
        }

        public static void SetControlTextForeColor(Form form, Control ctrl, Color color)
        {
            if (ctrl.InvokeRequired)
            {
                form.Invoke(new MethodInvoker(() =>
                {
                    ctrl.ForeColor = color;
                }));
            }
            else
            {
                ctrl.ForeColor = color;
            }
        }

        public static void SetControlTextBackColor(Form form, Control ctrl, Color color)
        {
            if (ctrl.InvokeRequired)
            {
                form.Invoke(new MethodInvoker(() =>
                {
                    ctrl.BackColor = color;
                }));
            }
            else
            {
                ctrl.BackColor = color;
            }
        }

        public static void SetControlVisibleState(Form form, Control ctrl, bool visible)
        {
            if (ctrl.InvokeRequired)
            {
                SetVisibleCallback d = new SetVisibleCallback(SetControlVisibleState);
                form.Invoke(d, new object[] { form, ctrl, visible });
            }
            else
            {
                ctrl.Visible = visible;
            }
        }

        public static void SetControlEnableState(Form form, Control ctrl, bool enabled)
        {
            if (ctrl.InvokeRequired)
            {
                SetVisibleCallback d = new SetVisibleCallback(SetControlEnableState);
                form.Invoke(d, new object[] { form, ctrl, enabled });
            }
            else
            {
                ctrl.Enabled = enabled;
            }
        }
        //public static void SetTimerEnableState(Form form, Timer ctrl, bool enabled)
        //{
        //    if (ctrl..InvokeRequired)
        //    {
        //        SetVisibleCallback d = new SetVisibleCallback(SetControlEnableState);
        //        form.Invoke(d, new object[] { form, ctrl, enabled });
        //    }
        //    else
        //    {
        //        ctrl.Enabled = enabled;
        //    }
        //}

        delegate void ClearDataGridView(Form form, DataGridView ctrl);

        public static void DataGridViewClearRows(Form form, DataGridView ctrl)
        {
            if (ctrl.InvokeRequired)
            {
                ClearDataGridView del = new ClearDataGridView(DataGridViewClearRows);

                ctrl.Invoke(del, new object[] { form, ctrl });

                //ctrl.Invoke(new MethodInvoker(() =>
                //{
                //    //ctrl = new DataGridView();

                //    //ctrl.DataSource = null;

                //    ctrl.Rows.Clear();
                //    ctrl.Columns.Clear();

                //    //ctrl.Invoke((MethodInvoker)delegate () { ctrl.Refresh(); });
                //    ctrl.Refresh();
                //}));
            }
            else
            {
                //ctrl = new DataGridView();

                //ctrl.DataSource = null;

                //ctrl.Rows.Clear();
                //ctrl.Columns.Clear();

                while (ctrl.Rows.Count > 0)
                {
                    ctrl.Rows.RemoveAt(0);
                }

                ctrl.Refresh();
            }
        }

        public static void SetDataGridViewSource(Form form, DataGridView ctrl, object data)
        {
            if (ctrl.InvokeRequired)
            {
                form.Invoke(new MethodInvoker(() =>
                {
                    ctrl.DataSource = null;
                    ctrl.DataSource = data;
                }));
            }
            else
            {
                ctrl.DataSource = null;
                ctrl.DataSource = data;
            }
        }

        //public delegate void DataGridViewFillRows(List<object> data);
        public static void SetDataGridViewFillRows(Form form, DataGridView ctrl, Action<DataGridView, List<object>> fillAction, List<object> data)
        {
            if (form.InvokeRequired)
            {
                ctrl.Invoke(new MethodInvoker(() =>
                {
                    fillAction.Invoke(ctrl, data);
                }));
            }
            else
            {
                fillAction.Invoke(ctrl, data);
            }
        }

        public static void AddDataGridViewColumns(Form form, DataGridView ctrl, DataGridViewColumn data)
        {
            if (ctrl.InvokeRequired)
            {
                ctrl.Invoke(new MethodInvoker(() =>
                {
                    ctrl.Columns.Add(data);
                }));
            }
            else
            {
                ctrl.Columns.Add(data);
            }
        }

        public static void SetDataGridViewReadOnly(Form form, DataGridView ctrl, bool readOnly)
        {
            if (ctrl.InvokeRequired)
            {
                ctrl.Invoke(new MethodInvoker(() =>
                {
                    for (int j = 0; j < ctrl.ColumnCount; j++)
                    {
                        if (!(ctrl.Columns[j] is DataGridViewComboBoxColumn))
                            ctrl.Columns[j].ReadOnly = readOnly;
                    }
                }));
            }
            else
            {
                for (int j = 0; j < ctrl.ColumnCount; j++)
                {
                    if (!(ctrl.Columns[j] is DataGridViewComboBoxColumn))
                        ctrl.Columns[j].ReadOnly = readOnly;
                }
            }
        }
    }
}
