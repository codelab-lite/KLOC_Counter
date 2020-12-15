#region File Header
// ---------------------------------------------------------------------------------------

// All rights are reserved. Reproduction or transmission in whole or in part, 
// in any form or by any means, electronic, mechanical or otherwise, 
// is prohibited without the prior written consent of the copyright owner.
// File Name     : MainWindow.cs
// Description   : Contains view related functions and events
// Date          |    Author             |        Description
// ---------------------------------------------------------------------------------------
// 2019/07/13    |   Vinoth N            |          Created
// --------------------------------------------------------------------------------------- 
#endregion


#region Usings
using KLOCCounter.Models;
using KLOCCounter.ViewModels;
using Microsoft.Windows.Controls.Primitives;
using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Excel = Microsoft.Office.Interop.Excel;
#endregion

#region Namespace
namespace KLOCCounter
{
    #region Class
    public partial class MainWindow : Window
    {
        #region Variable

        int errorCount = 0;     
        public GetFileDetail results = new GetFileDetail();
        public List<ErrorData> ErrorDetails = new List<ErrorData>();
        private ObservableCollection<string> errorCollection = new ObservableCollection<string>();
        private ObservableCollection<string> detailCollection = new ObservableCollection<string>();
        private ObservableCollection<CounterModel> p_Counter = new ObservableCollection<CounterModel>();
        private ObservableCollection<CounterModel> lineCollection = new ObservableCollection<CounterModel>();
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        CancellationToken token = new CancellationToken();
        #endregion

        #region Property

        public ObservableCollection<CounterModel> LineCollection
        {
            get { return lineCollection; }
            set { lineCollection = value; }
        } 
        public ObservableCollection<string> DetailCollection
        {
            get { return detailCollection; }
            set { detailCollection = value; }
        }
        public ObservableCollection<string> ErrorCollection
        { 
            get { return errorCollection; }
            set { errorCollection = value; }
        }
        public ObservableCollection<CounterModel> Counter
        {
            get { return p_Counter; }
            set { p_Counter = value; }
        }

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            counterGrid.ItemsSource = LineCollection;
            l_FunctionDetail.ItemsSource = DetailCollection;
            Loadgif.Visibility = Visibility.Hidden;
            b_Download.IsEnabled = false;
            tbk_counter.Text = "0";
        }

        #endregion

        #region Event     
        /// <summary>
        /// Select folder path
        /// </summary>
        /// <param name="sender">Button object details</param>
        /// <param name="e">Event Arguments</param>
        /// <returns></returns>
        /// 2019/07/13, Vinoth N,  Initial Version
        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folder = new System.Windows.Forms.FolderBrowserDialog();
            if (folder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                t_Location.Text = folder.SelectedPath;
            }
        }

        /// <summary>
        /// Grid click event
        /// </summary>
        /// <param name="sender">Grid row object details</param>
        /// <param name="e">Event Arguments</param>
        /// <returns>Select function name on List box</returns>
        /// 2019/07/13, Vinoth N,  Initial Version
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                int selected_index = counterGrid.SelectedIndex;
                if (counterGrid.SelectedItem is CounterModel item)
                {
                    if (item.Error == true)
                    {
                        int errorIndex = ErrorCollection.ToList().FindIndex(a => a.Contains("<<<" + item.FunctionName + ">>>"));
                        //l_PODetails.SelectedIndex = errorIndex;
                        //l_PODetails.ScrollIntoView(l_PODetails.SelectedItem);
                    }
                    else
                    {
                        //l_PODetails.UnselectAll();
                    }
                    int detailIndex = DetailCollection.ToList().FindIndex(a => a.Contains("<<<" + item.FunctionName + ">>>"));
                    l_FunctionDetail.SelectedIndex = detailIndex;
                    l_FunctionDetail.ScrollIntoView(l_FunctionDetail.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                LogModel.Log(ex.Message);
            }
            
        }

        /// <summary>
        /// Run the verification
        /// </summary>
        /// <param name="sender">Button object details</param>
        /// <param name="e">Event Arguments</param>
        /// <returns>Bind LOC count to view</returns>
        /// 2019/07/13, Vinoth N,  Initial Version
        private async void Counter_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            DisableControls();
            try
            {
                results = new GetFileDetail();
                string location = t_Location.Text;
                string tag = t_POTag.Text;
                int i = 1;
                var count = 0;
                if (StartingFlag())
                {
                    if (Directory.Exists(location))
                    {
                        Loadgif.Visibility = Visibility.Visible;
                        b_Download.IsEnabled = true;
                        counterStatus.Content = "Processing..";
                        counterStatus.Foreground = Brushes.Green;
                        await Task.Run(() => results = ViewModel.StartCounter(location, tag));                      
                        if (results.JavaFunctionDetails != null)
                        {
                            foreach (var counter in results.JavaFunctionDetails)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    LineCollection.Add(new CounterModel
                                    {
                                        Total = results.JavaFunctionDetails.IndexOf(counter) + 1,
                                        FileName = counter.FileName,
                                        FunctionName = counter.FunctionName,
                                        Description = counter.Description,
                                        AllCount = counter.AllCount,
                                        AddCount = counter.AddCount,
                                        ModCount = counter.ModCount,
                                        NewCount = counter.NewCount,
                                        DelCount = counter.DelCount,
                                        Error = counter.Error,
                                        IsGUI = counter.IsGUI
                                    });
                                });
                                Dispatcher.Invoke(() => DetailCollection.Add("<<<" + counter.FunctionName + ">>>"));
                                if (counter.FullFunctionLine != null)
                                {
                                    foreach (var line in counter.FullFunctionLine)
                                    {
                                        Dispatcher.Invoke(() => DetailCollection.Add(line));
                                    }
                                }
                                count += counter.AllCount;
                            }
                        }
                        var counterValue = $"{count}";
                        tbk_counter.Text = counterValue;
                        errorCount = LineCollection.Where(p => p.Error == true).Count();
                        FinishVerification(true);
                    }
                    else
                    {
                        b_Download.IsEnabled = false;
                        MessageBox.Show("Location Not Found !");
                    }              
                }
            }
            catch (Exception ex)
            {
                LogModel.Log(ex.Message);
                LogModel.Log(ex.StackTrace);
            }
        }

        /// <summary>
        /// Download LOC Report
        /// </summary>
        /// <param name="sender">Button object details</param>
        /// <param name="e">Event Arguments</param>
        /// <returns>Download report as excel document</returns>
        /// 2019/07/13, Vinoth N,  Initial Version
        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            if (errorCount > 0)
            {
                MessageBox.Show("Please fix the Defects then try.");
            }
            else
            {
                token = tokenSource.Token;
                DisableControls();
                loading.Visibility = Visibility.Visible;
                counterStatus.Content = "Processing...";
                counterStatus.Foreground = Brushes.Green;
                await Task.Run(() => { DeliveryListGeneration(); }, token).ContinueWith(delegate { FinishVerification(false); });
             }
            
        }

        /// <summary>
        /// Clear Grid and Listbox
        /// </summary>
        /// <param name="sender">Button object details</param>
        /// <param name="e">Event Arguments</param>
        /// <returns></returns>
        /// 2019/07/13, Vinoth N,  Initial Version
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void Open_File(object sender, RoutedEventArgs e)
        {
            if (counterGrid.SelectedItem is CounterModel item)
            {
                var bug = counterGrid.SelectedItem as CounterModel;
                if (!String.IsNullOrEmpty(bug.FileName))
                {
                    string location = string.Concat(t_Location.Text, "\\", bug.FileName);
                    try
                    {
                        var sb = new StringBuilder();
                        sb.AppendFormat("\"{0}\" -n{1}", location, 0);
                        Process.Start("notepad++.exe", sb.ToString());
                    }
                    catch (Exception)
                    {
                        Process.Start("notepad.exe", location);
                    }
                }
            }
        }
        #endregion

        #region Private Method
        /// <summary>
        /// Convert report as excel document
        /// </summary>
        /// <returns></returns>
        /// 2019/07/13, Vinoth N,  Initial Version
        private void DeliveryListGeneration()
        {
            string sheetName = "";
            string resonForChange = "";
            string msg = string.Empty;
            string location = string.Empty;
            Excel.Application excelapp = new Excel.Application();
            Excel.Workbook workbook = null;
            this.Dispatcher.Invoke(() =>
            {
                sheetName = t_POTag.Text ?? "non";
                resonForChange = t_Reason.Text ?? "";
                location = t_Location.Text;
            });
            if (sheetName.Length < 30)
            {
                if (sheetName.Contains("[") || sheetName.Contains("]"))
                {
                    char[] chars = new char[] {'[', ']', '/', '?', '*'};
                    sheetName = chars.Aggregate(sheetName, (c1, c2) => c1.Replace(c2, ' '));
                    sheetName = sheetName.Replace(" ", "");
                }                
                try
                {
                    LogModel.Log("Report Generation Started");
                    string excelPath = System.Environment.CurrentDirectory.ToString();
                    if (excelPath.Contains(@"\bin\Debug"))
                    {
                        excelPath = excelPath.Remove((excelPath.Length - (@"\bin\Debug").Length));
                    }
                    
                    excelapp.Visible = false;
                    workbook = excelapp.Workbooks.Open(excelPath + @"\\App_Data\\KLOC_Template.xls");
                    PrepareKLOCReport(workbook, sheetName, resonForChange);
                    this.Dispatcher.Invoke(() =>
                    {
                        using (System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog() { Filter = "XLS|*.xls|XLSX|*.xlsx", ValidateNames = true, FileName ="PRS Report", InitialDirectory = location })
                        {
                            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                if (Path.GetExtension(sfd.FileName) == ".xlsx")
                                    workbook.SaveAs(sfd.FileName, Excel.XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing, false, false, Excel.XlSaveAsAccessMode.xlShared, Excel.XlSaveConflictResolution.xlUserResolution, Type.Missing, Type.Missing);
                                else
                                    workbook.SaveAs(sfd.FileName);
                                msg = "Download Completed";
                                counterStatus.Content = msg;
                                LogModel.Log(msg);
                            }
                            else
                            {
                                msg = "Download Cancelled";
                                StopGeneration(msg);
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    StopGeneration(ex.Message);
                    LogModel.Log(ex.Message);
                    LogModel.Log(ex.StackTrace);
                    MessageBox.Show("Excel Template Not Found !");
                }
                finally
                {
                    CloseWorkSheet(excelapp, workbook);
                    LogModel.Log("Report Generation Ended");
                }
            }
            else
            {
                msg = "Excel sheet name length is exceeded: " + sheetName + ". Please change the project name then try...";
                StopGeneration(msg);
                MessageBox.Show(msg);
            }
        }

        /// <summary>
        /// Generate KLOC report document
        /// </summary>
        /// <param name="workbook">Workbook name</param>
        /// <param name="sheetName">Sheet Name</param>
        /// <param name="reason">Reason for change</param>
        /// <returns></returns>
        /// 2019/07/13, Vinoth N,  Initial Version
        private void PrepareKLOCReport(Excel.Workbook workbook, string sheetName, string reason)
        {
            List<string> files = new List<string>();
            try
            {
                Excel.Worksheet worksheet = null;
                var ChangeCollection = LineCollection.Where(p => (p.AllCount > 0) ).ToList();
                foreach(var collecion in ChangeCollection)
                {
                    collecion.ModType = "N";
                }
                worksheet = workbook.Sheets[2];
                Excel.Range usedrange = worksheet.UsedRange;
                usedrange.Rows.AutoFit();
                var sheet = (Excel.Worksheet)workbook.Worksheets.Item[2];
                try
                {
                    sheet.Name = sheetName;
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Sheet name has unsupported format: "+ sheetName + ". Please change the project name");
                    LogModel.Log(ex.Message);
                    LogModel.Log(ex.StackTrace);
                }
                //Get the rows and fill the data
                if (worksheet.Rows != null)
                {
                    int colNo = worksheet.UsedRange.Columns.Count;
                    int rowNo = worksheet.UsedRange.Rows.Count;
                    object[,] array = worksheet.UsedRange.Rows.Value;
                    int rowCount = LineCollection.Count();
                    if (rowCount > 0)
                    {
                        int rowInx = 6;
                        foreach (CounterModel lines in ChangeCollection)
                        {
                            //append new row
                            RunProgressbar(ChangeCollection.IndexOf(lines) + 1, ChangeCollection.Count - 1, lines);
                            Excel.Range line = (Excel.Range)worksheet.Rows[rowInx];
                            worksheet.UsedRange.Cells[rowInx, 3] = lines.FileName.ToString();
                            if(lines.FunctionName != null)
                            {
                                worksheet.UsedRange.Cells[rowInx, 4] = ParseFunctionName(lines.FunctionName.ToString());
                            }
                            worksheet.UsedRange.Cells[rowInx, 7] = reason != null ? reason.ToString() : "";
                            worksheet.UsedRange.Cells[rowInx, 6] = "-";
                            worksheet.UsedRange.Cells[rowInx, 5] = lines.Description != null ? lines.Description.ToString() : "";
                            worksheet.UsedRange.Cells[rowInx, 8] = "-";
                            worksheet.UsedRange.Cells[rowInx, 9] = "-";
                            worksheet.UsedRange.Cells[rowInx, 12] = "-";
                            worksheet.UsedRange.Cells[rowInx, 13] = "-";
                            worksheet.UsedRange.Cells[rowInx, 12] = "N";
                            if (lines.IsGUI == true)
                                worksheet.UsedRange.Cells[rowInx, 13] = "G";
                            else
                                worksheet.UsedRange.Cells[rowInx, 13] = "L";
                            worksheet.UsedRange.Cells[rowInx, 14] = lines.AllCount;
                            worksheet.UsedRange.Cells[rowInx, 15] = lines.AddCount;
                            worksheet.UsedRange.Cells[rowInx, 16] = lines.ModCount;
                            worksheet.UsedRange.Cells[rowInx, 17] = lines.DelCount;
                            rowInx++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogModel.Log(ex.Message);
                LogModel.Log(ex.StackTrace);
                throw new FileNotFoundException("Couldn't find your folder ! ");
            }

        }

        /// <summary>
        /// End the working thread
        /// </summary>
        /// <returns></returns>
        /// 2019/07/13, Vinoth N,  Initial Version
        private void FinishVerification(bool flag)
        {
            Dispatcher.Invoke(() =>
            {
                Loadgif.Visibility = Visibility.Hidden;
                loading.Visibility = Visibility.Hidden;
                l_FunctionDetail.Background = Brushes.AliceBlue;
                dashboardProgress.Value = 0;
                if (errorCount > 0)
                {
                    counterStatus.Content = errorCount + " defect encountered";
                    counterStatus.Foreground = Brushes.Red;
                }
                else if (flag == true)
                {
                    counterStatus.Content = "Completed";
                }
                EnableControls();
            });
        }

        /// <summary>
        /// Set progress bar value
        /// </summary>
        /// <returns></returns>
        /// 2019/07/13, Vinoth N,  Initial Version
        private void RunProgressbar(double currentProcess, double overAllProcess, CounterModel lines)
        {
            double progressValue = (double)(currentProcess / overAllProcess) * 100.0 ;
            Dispatcher.Invoke(() =>
            {            
                dashboardProgress.Value = progressValue;
                counterStatus.Content = "Extracting: " + lines.FileName + "/" + lines.FunctionName ;
            });
        }

        /// <summary>
        /// Set progress bar value
        /// </summary>
        /// <returns></returns>
        /// 2019/07/13, Vinoth N,  Initial Version
        private void CloseWorkSheet(Excel.Application excelApp, Excel.Workbook workBook)
        {            
            if (workBook != null)
            {
                workBook.Close(false, Missing.Value, Missing.Value);
                workBook = null;
                excelApp.Quit();
            }
        }

        /// <summary>
        /// Clear Grid and Listbox
        /// </summary>
        /// <returns></returns>
        /// 2019/07/13, Vinoth N,  Initial Version
        private void Clear()
        {
            LineCollection.Clear();
            DetailCollection.Clear();
            ErrorCollection.Clear();
            counterStatus.Content = "";
            b_Download.IsEnabled = false;
            tbk_counter.Text = "0";
        }

        /// <summary>
        /// Enable Controls
        /// </summary>
        /// <returns></returns>
        /// 2019/07/13, Vinoth N,  Initial Version
        private void EnableControls()
        {
            t_Location.IsEnabled = true;
            t_POTag.IsEnabled = true;
            t_ProjectName.IsEnabled = true;
            b_Clear.IsEnabled = true;
            b_Counter.IsEnabled = true;
            b_Download.IsEnabled = true;
        }

        /// <summary>
        /// End the Report Generation
        /// </summary>
        /// <returns></returns>
        /// 2019/07/13, Vinoth N,  Initial Version
        private void StopGeneration(string Data)
        {
            this.Dispatcher.Invoke(() =>
            {
                loading.Visibility = Visibility.Hidden;
                counterStatus.Content = "Generation Canceled";
                counterStatus.Foreground = Brushes.Red;
                dashboardProgress.Value = 0;
                LogModel.Log(Data);
                EnableControls();
            });         
        }
        /// <summary>
        /// Disable Controls
        /// </summary>
        /// <returns></returns>
        /// 2019/07/13, Vinoth N,  Initial Version
        private void DisableControls() 
        {
            t_Location.IsEnabled = false;
            t_POTag.IsEnabled = false;
            t_ProjectName.IsEnabled = false;
            b_Clear.IsEnabled = false;
            b_Counter.IsEnabled = false;
            b_Download.IsEnabled = false;
        }

        /// <summary>
        /// Enable Verification 
        /// </summary>
        /// <returns></returns>
        /// 2019/07/13, Vinoth N,  Initial Version
        private bool StartingFlag()
        {
            bool flag = true;
            if (String.IsNullOrEmpty(t_Location.Text))
            {
                 flag = false;
                 MessageBox.Show("Please select Project location");
            }
            else if (String.IsNullOrEmpty(t_POTag.Text))
            {
                 flag = false;
                 MessageBox.Show("Please Enter PO Tag");
            }
            else if (String.IsNullOrEmpty(t_ProjectName.Text))
            {
                 flag = false;
                 MessageBox.Show("Please Enter ProjectName");
            }
            EnableControls();
            return flag;
        }

        /// <summary>
        /// Remove unwanted char from function name 
        /// </summary>
        /// <returns></returns>
        /// 2019/11/29, Vinoth N,  Initial Version
        private string ParseFunctionName(string functionName)
        { 
            string s = functionName;
            int start = s.IndexOf("(");
            if(start > 0)
            {
                int end = s.IndexOf(")", start);
                if(end > 0)
                {
                    if(end - start > 1)
                    {
                        string result = s.Substring(start + 1, end - start - 1);
                        s = s.Replace(result, String.Empty);
                        s = s.Replace("(", String.Empty).Replace(")", String.Empty).Trim();
                    }
                    else
                    {
                        s = s.Replace("()", String.Empty);
                    }
                }
            }
          
            return s;
        }
        #endregion

        private void b_Cancel_Click(object sender, RoutedEventArgs e)
        {
            //tokenSource.Cancel();
            //l_FunctionDetail.SetSelected(0, true);
        }
    }
    #endregion
}
#endregion