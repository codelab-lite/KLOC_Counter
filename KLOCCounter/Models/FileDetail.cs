#region File Header
// ---------------------------------------------------------------------------------------
// File Name     : FileDetail.cs
// Description   : Contains FileDetail properties
// application.
// Date          |    Author             |        Description
// ---------------------------------------------------------------------------------------
// 2019/07/13    |   Vinoth N            |          Created
// --------------------------------------------------------------------------------------- 
#endregion

#region Using
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

#region Namespace
namespace KLOCCounter.Models
{
    #region Class
    public class FileDetail
    {
        #region Properties
        public string FileName { get; set; }
        public FunctionDetail[] FunctionDetails { get; set; }
        public ObservableCollection<CounterModel> FunctionCounter { get; set; }
        public List<ErrorData> ErrorDetail { get; set; }
        #endregion
    }
    #endregion
}
#endregion
