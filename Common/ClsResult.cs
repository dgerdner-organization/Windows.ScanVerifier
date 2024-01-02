using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS2010.Common
{
    /// <summary>
    /// Can be used a base class for return results from a method call or event firing
    /// </summary>
    public class ClsResult
    {

        private int _Min = 0;
        /// <summary>
        /// For viewable status (Status Bar) ... Low end range of Process count 
        /// </summary>
        public int Min 
        { 
            get
            {
                return _Min;
            }
            set
            {
                _Min = value;
            }
        }


        private int _Max = 0;
        /// <summary>
        /// For viewable status (Status Bar) ... High end range of Process count
        /// </summary>
        public int Max
        {
            get
            {
                return _Max;
            }
            set 
            {
                _Max = value;
            }
        }

        private int _Counter = 0;
        /// <summary>
        /// Counts the number of operations of a looping structure
        /// </summary>
        public int Counter
        {
            get
            {
                return _Counter;
            }
            set
            {
                _Counter = value;
            }
        }

        private int _CounterSuccess = 0;
        /// <summary>
        /// Counts the number of succcessful operations 
        /// </summary>
        public int CounterSuccess 
        {
            get
            {
                return _CounterSuccess;
            }
            set
            {
                _CounterSuccess = value;
            }
        }

        private int _CounterFailed = 0;
        /// <summary>
        /// Counts the number of failed operations 
        /// </summary>
        public int CounterFailed
        {
            get
            {
                return _CounterFailed;
            }
            set
            {
                _CounterFailed = value;
            }
        }


        private List<string> _ErrMsg = new List<string>();
        /// <summary>
        /// Holds the List of error messages
        /// </summary>
        public List<string> ErrMsg
        {
            get
            {
                return _ErrMsg;
            }
        }

        /// <summary>
        /// Adds an error message 
        /// </summary>
        /// <param name="msg">Error Message</param>
        public void ErrMsgAdd(string msg)
        {
            if (_ErrMsg.IsNull()) ErrMsgClear();
            _ErrMsg.Add(msg);
        }

        public void ErrMsgAdd(string msgFormat, params object[] msgParams)
        {
            _ErrMsg.Add( string.Format( msgFormat, msgParams));
        }

        /// <summary>
        /// Clears the error message list
        /// </summary>
        public void ErrMsgClear()
        {
            _ErrMsg = new List<string>();
        }

        /// <summary>
        /// Assembles an single error message string from the list of errors encountered.
        /// </summary>
        public string ErrMsgText 
        {
            get
            {
                if (_ErrMsg.IsNull()) return string.Empty;

                StringBuilder temp = new StringBuilder();

                foreach (string s in _ErrMsg)
                    temp.AppendLine(s);

                return temp.ToString();
            }
        }

        /// <summary>
        /// Examines the ErrMsg List property and determines if there are errors.
        /// </summary>
        public bool AreThereErrors
        {
            get
            {
                if (_ErrMsg.IsNull()) return false;
                return (_ErrMsg.Count > 0); 
            }
        }

        private bool _Complete = false;
        /// <summary>
        /// In the case were this class is used as an Event Arg (Event), 
        /// this value should be set to true when processing is complete.
        /// Best used on a process summary screen.
        /// </summary>
        public bool Complete 
        {
            get
            {
                return _Complete;
            }
            set
            {
                _Complete = value;
            }
        }

    }
}
