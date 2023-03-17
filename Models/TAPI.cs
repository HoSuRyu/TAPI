using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.ComponentModel;
using System;

namespace TAPI_Interviewer.Models
{
    partial class TAPIDataContext
    {
    }

    


    public partial class SP_SHOUSE2021H_GETLISTResult
    {
        private List<SP_SHOUSE2021P_GETLISTResult> _listSP_SHOUSE2021P_GETLISTResult = new List<SP_SHOUSE2021P_GETLISTResult>();

        public List<SP_SHOUSE2021P_GETLISTResult> ListSP_SHOUSE2021P_GETLISTResult
        {
            get
            {
                return this._listSP_SHOUSE2021P_GETLISTResult;
            }
            set
            {
                if ((this._listSP_SHOUSE2021P_GETLISTResult != value))
                {
                    this._listSP_SHOUSE2021P_GETLISTResult = value;
                }
            }
        }
    }
    public partial class SP_SHOUSE2021NH_GETLISTResult
    {
        private List<SP_SHOUSE2021P_GETLISTResult> _listSP_SHOUSE2021P_GETLISTResult = new List<SP_SHOUSE2021P_GETLISTResult>();

        public List<SP_SHOUSE2021P_GETLISTResult> ListSP_SHOUSE2021P_GETLISTResult
        {
            get
            {
                return this._listSP_SHOUSE2021P_GETLISTResult;
            }
            set
            {
                if ((this._listSP_SHOUSE2021P_GETLISTResult != value))
                {
                    this._listSP_SHOUSE2021P_GETLISTResult = value;
                }
            }
        }


    }

   
}