#region File Header
// ---------------------------------------------------------------------------------------
// File Name     : GetFileDetail.cs
// Description   : Contains GetFileDetail properties
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
    public class GetFileDetail
    {
        #region Properties
        public List<FileDetail> FileDetails { get; set; }
        public List<JavaFunctionDetail> JavaFunctionDetails { get; set; }
        #endregion
    }
    #endregion
}
#endregion