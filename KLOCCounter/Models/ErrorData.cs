#region File Header
// ---------------------------------------------------------------------------------------
// File Name     : ErrorData.cs
// Description   : Contains ErrorData properties
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
    public class ErrorData
    {
        #region Property
        public string FunctionName { get; set; }
        public string ErrorMessage { get; set; }
        public string[] Line { get; set; }
        #endregion 
    }
    #endregion
}
#endregion
