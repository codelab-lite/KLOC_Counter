#region File Header
// ---------------------------------------------------------------------------------------
// File Name     : LogModel.cs
// Description   : Contains Log related functions
// Date          |    Author             |        Description
// ---------------------------------------------------------------------------------------
// 2019/07/15    |   Vinoth N            |         Created
// --------------------------------------------------------------------------------------- 
#endregion

#region Usings
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

#region Namespace

namespace KLOCCounter.ViewModels
{
    #region Class
    public class LogModel
    {
        #region Static Method

        /// <summary>
        /// Log mothod
        /// </summary>
        /// <param name="text">Log details</param>
        /// <returns></returns>
        /// 2019/07/15, Vinoth N,  Initial Version
        public static void Log(string text)
        { 
            string excelPath = Environment.CurrentDirectory.ToString();
            if (excelPath.Contains(@"\bin\Debug"))
            {
                excelPath = excelPath.Remove((excelPath.Length - (@"\bin\Debug").Length));
            }
            bool exists = System.IO.Directory.Exists(excelPath + @"\\Logs");
            if (!exists)
            {
                System.IO.Directory.CreateDirectory(excelPath + @"\\Logs");
            }
            string path = excelPath + @"\\Logs\\KLOCLog.txt";
            using (StreamWriter writer = new StreamWriter(path, true))
                LogWritter(text, writer);
        }

        /// <summary>
        /// Write log details
        /// </summary>
        /// <param name="text">Log details</param>
        /// <returns></returns>
        /// 2019/07/15, Vinoth N,  Initial Version
        private static void LogWritter(string text, StreamWriter writer)
        {
            writer.WriteLine(string.Format("{0} {1}", text, DateTime.Now.ToString("dd/mm/yyyy hh:mm:ss")));
            writer.Close();
        }

        #endregion
    }
    #endregion
}
#endregion
