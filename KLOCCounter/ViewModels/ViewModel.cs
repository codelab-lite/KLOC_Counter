#region File Header
// ---------------------------------------------------------------------------------------
// File Name     : ViewModel.cs
// Description   : Contains model related functions
// Date          |    Author             |        Description
// ---------------------------------------------------------------------------------------
// 2019/07/13    |   Vinoth N            |          Created
// --------------------------------------------------------------------------------------- 
#endregion

#region Using
using KLOCCounter.Models;
using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;
#endregion

#region NameSpace
namespace KLOCCounter.ViewModels
{
    #region Class
    public class ViewModel
    {

        #region Properties
        private ObservableCollection<CounterModel> p_Counter;
        private ObservableCollection<CounterModel> Counter
            {
                get { return p_Counter; }
                set {p_Counter = value; }
            }

        private static ObservableCollection<CounterModel> lineCollection;
        private static ObservableCollection<CounterModel> LineCollection
        {
            get { return lineCollection; }
            set { lineCollection = value; }
        }

        public static string missingStartTag = "Start Tag is missing on this function";
        public static string missingEndTag = "End Tag is missing on this function";
        public static string invalidHeader = "Caution : Name of the function and function header not matched !";
        public static string invalidFormat = "Function header not in expected format";

        #endregion


        #region Constructor

        public ViewModel()
        {
        }

        #endregion

        #region Public Method

        private static bool IsExcluded(List<string> exludedDirList, string target)
        {
            return exludedDirList.Any(d => new DirectoryInfo(target).Name.Equals(d));
        }

        /// <summary>
        /// Get KLOC Details
        /// </summary>
        /// <param name="Location">Project source loaction details</param>
        /// <param name="poTag">Tag Name</param>
        /// <returns>Return LOC count</returns>
        /// 2019/07/13, Vinoth N,  Initial Version
        public static GetFileDetail StartCounter(string location, string poTag)
        {
            var output = new GetFileDetail();
            try
            {
                List<FileDetail> collectionCSharp = new List<FileDetail>();
                List<JavaFunctionDetail> collectionJava = new List<JavaFunctionDetail>(); 
                List<string> _excludedDirectories = new List<string>() { "without_tag"};
                var filteredDirs = Directory.GetDirectories(location).Where(d => !IsExcluded(_excludedDirectories, d));
                foreach(var dir in filteredDirs)
                {
                    var items = Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Where(name => name.EndsWith(".java") || name.EndsWith(".js"));
                    foreach (var item in items)
                    {
                        var result = GetLOCDetails(item, location);
                        if (result.Count > 0)
                        {
                            foreach (var res in result)
                            {
                                collectionJava.Add(new JavaFunctionDetail
                                {
                                    FileName = res.FileName.Replace(location + @"\", String.Empty),
                                    Description = res.Description,
                                    FunctionName = res.FunctionName,
                                    BodyLine = res.BodyLine,
                                    HeaderLine = res.HeaderLine,
                                    AllCount = res.AllCount,
                                    NewCount = res.NewCount,
                                    ModCount = res.ModCount,
                                    AddCount = res.AddCount,
                                    DelCount = res.DelCount,
                                    Error = res.Error,
                                    FullFunctionLine = res.FullFunctionLine,
                                    ErrorLine = res.ErrorLine,
                                    IsGUI = res.IsGUI
                                });
                            }
                        }
                        RemoveComments(item, location);
                    }
                }
                output.JavaFunctionDetails = collectionJava.ToList(); 
            }
            catch (Exception ex)
            {
                LogModel.Log(ex.Message);
                LogModel.Log(ex.StackTrace);
            }
            return output;
        }

        #endregion

        #region Private Method
        private static List<JavaFunctionDetail> GetLOCDetails(string fileName, string filePath)
        {
            List<JavaFunctionDetail> output = new List<JavaFunctionDetail>();
            FileInfo fi = new FileInfo(fileName);
            switch (fi.Extension)
            {
                case ".java":
                    output = GetFunctions(fileName, filePath, false);
                    break;
                case ".js":
                    output = GetFunctions(fileName, filePath, true);
                    break;
            }
            return output;
        }
        
        private static void RemoveComments(string file, string path)
        {
            try {
                FileInfo fi = new FileInfo(file);
                string[] specifiers = new[] { "//", "Func_Name", ":" };
                string sourceFile = System.IO.Path.GetFileName(path);

                string newDir = fi.DirectoryName.Replace(@"\" + sourceFile, @"\" + sourceFile + @"\without_tag");
                if (!Directory.Exists(newDir))
                {
                    Directory.CreateDirectory(newDir);
                }
                string newFile = newDir + @"\" + fi.Name;
                if (File.Exists(newFile)) File.Delete(newFile);

                using (var sw = new StreamWriter(newFile))
                using (var fs = File.OpenRead(file))
                using (var sr = new StreamReader(fs, Encoding.UTF8))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (specifiers.All(s => line.Contains(s)))
                        {
                            // Do nothing
                        }
                        else
                            sw.WriteLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                LogModel.Log(ex.Message);
                LogModel.Log(ex.StackTrace);
            }
        }
        private static List<JavaFunctionDetail> GetFunctions(string fileName, string filePath, bool isJS)
        {
            List<JavaFunctionDetail> output = new List<JavaFunctionDetail>();
            string[] allBodyLines = File.ReadAllLines(fileName, Encoding.UTF8);
            for (int i = 0; i < allBodyLines.Length; i++)
            {
                if (IsFunction(allBodyLines[i], isJS))
                {
                    output.Add(GetFunctionDetail(i, allBodyLines, fileName, isJS));
                }
            }
            return output;
        }
        private static bool IsFunction(string line, bool isJs = false)
        {
            bool status = false;
            if (isJs)
            {
                string[] specifiers = new[] { "handleDelete", "handleEdit", "render" };
                string[] brackets = new[] { "(", ")" };
                if (specifiers.Any(s => line.Contains(s)))
                {
                    if (brackets.All(s => line.Contains(s)))
                    {
                        if (!line.Contains('}'))
                        {
                            status = true;
                        }
                    }
                }
                else if (specifiers.Any(s => line.Contains("function ")))
                {
                    if (brackets.All(s => line.Contains(s)))
                    {
                        status = true;
                    }
                }
            }
            else
            {
                string[] specifiers = new[] { "private ", "protected ", "public " };
                string[] brackets = new[] { "(", ")" };
                if (specifiers.Any(s => line.Contains(s)))
                {
                    if (brackets.All(s => line.Contains(s)))
                    {
                        if (!line.Contains(';'))
                        {
                            status = true;
                        }
                    }
                }
            }
            return status;
        }
        private static bool CheckComment(string line, ref bool commentFlag)
        {
            string replaceWith = "";
            string removedBreaks = line.Replace("\t", replaceWith).Replace("\n", replaceWith).Replace("\r", replaceWith).Trim();
            var skip = false;
            if (!String.IsNullOrWhiteSpace(removedBreaks))
            {
                if (removedBreaks.StartsWith("/*") && removedBreaks.Contains("*/"))
                {
                    skip = true;
                }
                else if (removedBreaks.StartsWith("//"))
                {
                    skip = true;
                }
                if (skip == false)
                {
                    if (removedBreaks.StartsWith("/*"))
                    {
                        commentFlag = true;
                        skip = true;
                    }
                    else if (commentFlag == true && removedBreaks.Contains("*/"))
                    {
                        commentFlag = false;
                        skip = true;
                    }
                    if(commentFlag)
                        skip = commentFlag;
                }
            }
            else
            {
                skip = true;
            }

            return skip;
        }
        private static string ExtractFunctionName(string strSource)
        {
            string strStart = "(";
            string strEnd = " ";
            int Start;
            string output = "";
            try
            {
                if (strSource.Contains(strStart) && strSource.Contains(strEnd))
                {
                    Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                    char[] characters = strSource.ToCharArray();
                    for(int i = Start; i >= 0; i--)
                    {
                        if (characters[i] == ' ')
                        {
                            i++;
                            Start--;
                            output = strSource.Substring(i, Start - i);
                            break;
                        }
                    }
                }
                else
                {
                    string part = strSource.Substring(0, strSource.IndexOf('('));
                    output = part.Replace("\t", "").Replace("\n", "").Replace("\r", "").Trim();
                }
            }
            catch (Exception ex)
            {
                output = "";
                LogModel.Log(ex.Message);
                LogModel.Log(ex.StackTrace);
            }
            return output;
        }
        private static string ExtractDescription(int index, string[] body, bool isJS)
        {
            string output = string.Empty;
            try
            {
                for(int i = index - 1; i > 0; i--)
                {
                    var txt = body[i];
                    if (IsFunction(txt, isJS))
                        return "";
                    if (txt.Contains("//"))
                    {                      
                        string[] txtArray = txt.Split(':');
                        if(txtArray.Length > 1)
                        {
                            output = txtArray[1];
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogModel.Log(ex.Message);
                LogModel.Log(ex.StackTrace);
            }
            return output.Trim();
        }
        private static Tuple<List<string>, int> ExtractFunctionBody(int index, string[] body)
        {
            int Start = 0;
            int End = 0;
            List<string> functionBody = new List<string>();
            List<string> lines = new List<string>();
            bool commentFlag = false;
            for (int j = index; j < body.Length; j++)
            {
                var txt = body[j];
                if (!CheckComment(txt, ref commentFlag))
                {
                    lines.Add(txt);
                    if (txt.Contains("{"))
                        Start++;
                    if (txt.Contains("}"))
                        End++;
                }
                functionBody.Add(txt);             
                if (Start > 0 && Start == End)
                    break;
            }
            return Tuple.Create(functionBody, lines.Count());
        }
        private static JavaFunctionDetail GetFunctionDetail(int index, string[] body, string fileName, bool isJS)
        {
            JavaFunctionDetail code = new JavaFunctionDetail();
            for (int i = index; i < body.Length; i++)
            {
                code.IsGUI = isJS;
                code.FileName = fileName;
                code.Description = ExtractDescription(i, body, isJS);
                code.FunctionName = ExtractFunctionName(body[i]);
                var collection = ExtractFunctionBody(i, body);
                code.FullFunctionLine = collection.Item1;
                code.AllCount = collection.Item2;
                code.BodyLine = new List<string>();
                break;
            }
            return code;
        }

        #endregion

    }
    #endregion
}
#endregion
