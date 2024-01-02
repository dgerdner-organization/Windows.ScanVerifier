using System;
using System.Collections.Generic;
using System.Text;

namespace CS2010.Common
{
    /// <summary>
    /// Base Class for the Busines Objects
    /// </summary>
    public class ClsBaseClass
    {
        #region Error/Warning Members

        #region Properties

        public List<string> _Errors = new List<string>();
        public List<string> _Warnings = new List<string>();
        public Exception Ex;  
        public Boolean HasErrors { get { return _Errors.Count > 0; } }
        public Boolean HasWarnings { get { return _Warnings.Count > 0; } } 

        #endregion

        #region Public Methods

        /// <summary>
        /// Clears the _Errors List
        /// </summary>
        public void ResetErrors()
        {
            _Errors.Clear();
        }

        /// <summary>
        /// Clears the _Warnings List
        /// </summary>
        public void ResetWarnings()
        {
            _Warnings.Clear();
        }

        /// <summary>
        /// Clears the _Errors List, _Warnings List and sets Ex to null
        /// </summary>
        public void ResetAll()
        {
            ResetErrors();
            ResetWarnings();
            Ex = null;
        }

        /// <summary>
        /// Adds Error Message to _Errors list
        /// </summary>
        /// <param name="fmt">String Format</param>
        /// <param name="args">String Arguments</param>
        public void AddError(string fmt, params object[] args)
        {
            _Errors.Add(string.Format(fmt, args));
        }

        /// <summary>
        /// Adds Error Message to _Errors list
        /// </summary>
        /// <param name="pError">String Error</param>
        public void AddError(string pError)
        {
            _Errors.Add(pError);
        }

        /// <summary>
        /// Adds Warning Message to _Warnings list
        /// </summary>
        /// <param name="fmt">String Format</param>
        /// <param name="args">String Arguments</param>
        public void AddWarning(string fmt, params object[] args)
        {
            _Warnings.Add(string.Format(fmt, args));
        }


        /// <summary>
        /// Adds Warning Message to _Warnings list
        /// </summary>
        /// <param name="pError">String Warning</param>
        public void AddWarning(string pWarning)
        {
            _Warnings.Add(pWarning);
        }

        /// <summary>
        /// Appends the Errors and Warnings from passed parameter 
        /// </summary>
        /// <param name="objBC">Base Class</param>
        /// <returns>Returns whether we have Errors or not</returns>
        public Boolean AppendErrorWarning(ClsBaseClass objBC)
        {

            foreach (string e in objBC._Errors) _Errors.Add(e);
            
            foreach (string w in objBC._Warnings) _Warnings.Add(w);

            return (HasErrors);

        }        

        #endregion

        #endregion

    }
}
