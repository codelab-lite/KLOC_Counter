#region File Header
// ---------------------------------------------------------------------------------------
// File Name     : CounterModel.cs
// Description   : Base model for Store KLOC results
// application.
// Date          |    Author             |        Description
// ---------------------------------------------------------------------------------------
// 2019/07/13    |   Vinoth N            |          Created
// --------------------------------------------------------------------------------------- 
#endregion

#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

#region Namespace
namespace KLOCCounter.Models
{
    #region Class
    public class CounterModel
    {
        #region Properties
        public string FileName { get; set; }
        public string FunctionName { get; set; }
        public string Description { get; set; }
        public string ErrorMessage { get; set; }
        public string ModType { get; set; }
        public int AllCount { get; set; }
        public int NewCount { get; set; }
        public int AddCount { get; set; } 
        public int ModCount { get; set; }
        public int DelCount { get; set; }
        public int Total { get; set; } 
        public bool? Error { get; set; }
        public bool IsGUI { get; set; }

        #endregion
    }
    #endregion
}
#endregion