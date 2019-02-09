﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ISA.DAL;
using Microsoft.Reporting.WinForms;
using ISA.Common;
using System.Data.SqlClient;
using System.Data.OleDb;
using Excel = Microsoft.Office.Interop.Excel;



namespace ISA.Showroom.Finance.GL
{
    public partial class frmRpt06Neraca : ISA.Controls.BaseForm
    {
        DateTime fromDate = new DateTime();
        DateTime toDate = new DateTime();
        string kodeGudang = GlobalVar.Gudang;

        public frmRpt06Neraca()
        {
            InitializeComponent();
        }

        private void initializeUnitCombo()
        {
            Dictionary<string, string> lst = new Dictionary<string, string>();
            lst.Add("all", "ALL");
            lst.Add("honda", "HONDA");
            lst.Add("avalis", "AVALIS");
            lst.Add("ahass", "AHASS");
            cboCabang.DataSource = new BindingSource(lst, null);
            cboCabang.DisplayMember = "Key";
            cboCabang.DisplayMember = "Value";
            cboCabang.SelectedIndex = 0;
        }


        private void cmdClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmRpt06LabaRugi_Load(object sender, EventArgs e)
        {
            SetControl();
        }

        private void cmdOk_Click(object sender, EventArgs e)
        {
            if (GlobalVar.PerusahaanID == "HLD") kodeGudang = lookupGudang1.GudangID;
            try
            {
                KeyValuePair<string, string> unit = (KeyValuePair<string, string>)cboCabang.SelectedValue;
                this.Cursor = Cursors.WaitCursor;
                fromDate = new DateTime(monthYearBox1.Year, monthYearBox1.Month, 1);
                toDate = fromDate.AddMonths(1).AddDays(-1);
                DataTable dt = new DataTable();
                using (Database db = new Database(GlobalVar.DBHoldingName))
                {
                    db.Commands.Add(db.CreateCommand("[rsp_GL_06Neraca_dv]"));
                    db.Commands[0].Parameters.Add(new Parameter("@fromDate", SqlDbType.DateTime, fromDate));
                    db.Commands[0].Parameters.Add(new Parameter("@toDate", SqlDbType.DateTime, toDate));
                    db.Commands[0].Parameters.Add(new Parameter("@kodeGudang", SqlDbType.VarChar, kodeGudang));
                    db.Commands[0].Parameters.Add(new Parameter("@tabel", SqlDbType.VarChar, unit.Key));
                    dt = db.Commands[0].ExecuteDataTable();

                }

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show(Messages.Confirm.NoDataAvailable);
                    return;
                }


                ShowReport(dt);

            
            }
            catch (System.Exception ex)
            {
                Error.LogError(ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }


        private void SetControl()
        {
            monthYearBox1.Month = GlobalVar.GetServerDate.Month;
            monthYearBox1.Year = GlobalVar.GetServerDate.Year;
            lookupGudang1.GudangID = "";
            if (GlobalVar.PerusahaanID != "HLD") lookupGudang1.Enabled = false;
            initializeUnitCombo();
            cboCabang.SelectedIndex = 0;
        }

        private void ShowReport(DataTable dt)
        {
            //construct parameter
            //string periode;
            KeyValuePair<string, string> unit = (KeyValuePair<string, string>)cboCabang.SelectedValue;
            string uparams = unit.Key == "all" ? "" : "UNIT: "+ unit.Value;

            DateTime currPeriode = toDate;
            DateTime prevPeriode = fromDate.AddDays(-1);
            try
            {
                this.Cursor = Cursors.WaitCursor;
                List<ReportParameter> rptParams = new List<ReportParameter>();
                rptParams.Add(new ReportParameter("Periode", currPeriode.ToString("dd-MMM-yyyy") + " dan " + prevPeriode.ToString("dd-MMM-yyyy")));
                rptParams.Add(new ReportParameter("UserID", SecurityManager.UserInitial));
                rptParams.Add(new ReportParameter("KodeGudang", kodeGudang));
                rptParams.Add(new ReportParameter("Unit", uparams.ToUpper()));

                //call report viewer
                frmReportViewer ifrmReport = new frmReportViewer("GL.Laporan.rpt06Neraca.rdlc", rptParams, dt, "dsGL_Data");
                ifrmReport.Text = "Buku Besar";
                ifrmReport.Show();
            }
            catch (System.Exception ex)
            {
                Error.LogError(ex);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        

        
    }
}
