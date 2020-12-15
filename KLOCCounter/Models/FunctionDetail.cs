#region File Header
// ---------------------------------------------------------------------------------------
// File Name     : FunctionDetail.cs
// Description   : Contains FunctionDetail properties
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
    public class FunctionDetail
    {
        #region Properties
        public string FunctioName { get; set; }
        public string[] LineDetails { get; set; }
        #endregion
    }
    #endregion
}
#endregion