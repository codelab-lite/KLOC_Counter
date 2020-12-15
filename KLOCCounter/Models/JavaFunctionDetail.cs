#region File Header
// ---------------------------------------------------------------------------------------
// File Name     : PyFunctionDetail.cs
// Description   : Contains PyFunctionDetail properties
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
    public class JavaFunctionDetail : CounterModel
    {
        #region Properties
        public string ErrorMessage { get; set; }
        public List<string> HeaderLine { get; set; }
        public List<string> BodyLine { get; set; }
        public List<string> ErrorLine { get; set; }
        public List<string> FullFunctionLine { get; set; }
        #endregion
    }
    #endregion
}
#endregion