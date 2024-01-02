using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;

namespace CS2010.Common
{
    public abstract class sql_base
    {

        #region Event Handler

        public delegate void SearchEventHandler(object sender, SearchEventArgs e);
        public event SearchEventHandler SearchStatus;

        #endregion

        #region Variables

        private Thread _thread;
        private ThreadStart _threadstart;
        private string _threadsql;

        private DateTime? _starttime = null;
        private DateTime? _endtime = null;

        private DataTable _data;

        private Boolean _running = false;
        private SearchStatusCd _status_cd = SearchStatusCd.Idle;

        private Boolean _Async = true;

        #endregion

        #region Init

        public sql_base()
        {
            Clear();
        }

        #endregion

        #region Connection

        private ClsConnection _internal_connection;

        private ClsConnection _connection
        {
            get
            {

                if (_internal_connection == null)
                {
                    ClsConnection _conn_temp = ClsConMgr.Manager[connection_key];
                    _internal_connection = new ClsConnection( _conn_temp.DbConnectionString,_conn_temp.DbProvider);
                }
                return _internal_connection;
            }
            set
            {
                _internal_connection = null;
            }

        }

        #endregion

        #region Abstract Properties

        protected abstract string connection_key
        {
            get;
        }

        protected abstract string base_query
        {
            get;
        }

        #endregion

        #region Properites

        public DataTable Data
        {
            get { return _data; }
        }

        public int RowsAffected
        {
            get
            {
                if (_data == null) return 0;
                return _data.Rows.Count;
            }
        }

        public Boolean Async
        {
            get
            {
                return _Async;
            }
            set
            {
                _Async = value;
            }
        }

        private TimeSpan _elapsed_time;

        public TimeSpan ElapsedTime
        {
            get
            {
                if (_elapsed_time == null)
                    return new TimeSpan();

                return _elapsed_time;
            }
        }

        public string Message_ElapsedTime
        {
            get
            {
                if (_elapsed_time.Minutes > 0)
                    return string.Format("Query Time: {0} minutes and {1}.{2} seconds", ElapsedTime.Minutes, ElapsedTime.Seconds, ElapsedTime.Milliseconds);

                return string.Format("Query Time: {0}.{1} seconds", ElapsedTime.Seconds, ElapsedTime.Milliseconds);
            }
        }

        public string Message_RowsAffected
        {
            get
            {
                return string.Format("Rows Returned: {0:#,###}", RowsAffected);
            }
        }

        public string Message_RowsAffectedElapsedTime
        {
            get
            {
                return string.Format("{0} ... {1}", Message_RowsAffected, Message_ElapsedTime);
            }
        }

        public SearchStatusCd StatusCd
        {
            get
            {
                return _status_cd;
            }
        }

        #endregion

        #region Protected

        protected void RunWhere(string where)
        {
            if (_Async)
                _RunWhereAsync(where);
            else
                _RunWhere(where);

        }

        protected void RunWhere()
        {
            RunWhere(string.Empty);
        }

        #endregion

        #region Private

        private void _RunWhere(string where)
        {
            string sql = base_query;
            sql = sql.Replace("[WHERE]", where);

            _starttime = DateTime.Now;
            _data = _connection.GetDataTable(sql);
            _endtime = DateTime.Now;
            SetElapsedTime();
            ResetTimer();
        }

        private void _RunWhereAsync(string where)
        {
            if (_running) return;

            _status_cd = SearchStatusCd.Running;
            _running = true;

            _threadsql = base_query;
            _threadsql = _threadsql.Replace("[WHERE]", where);

            _starttime = DateTime.Now;
            _threadstart = new ThreadStart(ThreadRun);
            _thread = new Thread(_threadstart);
            _thread.Start();
        }

        private DateTime _StartOfDay(DateTime dt)
        {
            dt = dt.AddHours(-1 * dt.Hour);
            dt = dt.AddMinutes(-1 * dt.Minute);
            dt = dt.AddSeconds(-1 * dt.Second);
            return dt;
        }

        private DateTime _EndOfDay(DateTime dt)
        {
            dt = dt.AddHours(23 - dt.Hour);
            dt = dt.AddMinutes(59 - dt.Minute);
            dt = dt.AddSeconds(59 - dt.Second);
            return dt;
        }

        private void ThreadRun()
        {
            _data = _connection.GetDataTable(_threadsql);

            _connection = null;
            _endtime = DateTime.Now;
            _running = false;
            SetElapsedTime();
            _status_cd = SearchStatusCd.Complete;

            SearchStatus(this, new SearchEventArgs(_data,_status_cd, RowsAffected, ElapsedTime));

            ResetTimer();
        }

        private void SetElapsedTime()
        {
            _elapsed_time = _endtime.Value.Subtract(_starttime.Value);
        }

        private void ResetTimer()
        {
            _starttime = null;
            _endtime = null;
        }

        #endregion

        #region Public Methods

        public DateTime? StartOfDay(DateTime? dt)
        {
            if (dt == null) return null;
            return _StartOfDay((DateTime)dt);
        }

        public DateTime? EndOfDay(DateTime? dt)
        {
            if (dt == null) return null;
            return _EndOfDay((DateTime)dt);
        }

        public void Abort()
        {
            if (_thread != null) _thread.Abort();

            _thread = null;
            _threadstart = null;
            _connection = null;
            _endtime = DateTime.Now;
            _running = false;
            //SetElapsedTime();
            _status_cd = SearchStatusCd.Aborted;

            SearchStatus(this, new SearchEventArgs(null,_status_cd, 0, ElapsedTime));

            ResetTimer();
        }

        public void Clear()
        {
            _data = null;
            _starttime = null;
            _endtime = null;
            _running = false;
            _status_cd = SearchStatusCd.Idle;
        }

        public void Run()
        {
            RunWhere();
        }

        #endregion

    }

    public enum SearchStatusCd
    {
        Idle,		// DOING NOTHING
        Running,	// QUERY RUNNING
        Aborted,	// ABORTED USER
        Complete	// COMPLETED (NORMAL)
    }

    public class SearchEventArgs : EventArgs
    {
        private DataTable _data;

        public DataTable Data
        {
            get { return _data; }
        }

        private SearchStatusCd _status;

        public SearchStatusCd Status
        {
            get { return _status; }
        }

        private int _RowsAffected;

        public int RowsAffected
        {
            get { return _RowsAffected; }
        }

        private TimeSpan _ElapsedTime;

        public TimeSpan ElapsedTime
        {
            get { return _ElapsedTime; }
        }

        public string Message_ElapsedTime
        {
            get
            {
                if (_ElapsedTime.Minutes > 0)
                    return string.Format("Query Time: {0} minutes and {1}.{2} seconds", ElapsedTime.Minutes, ElapsedTime.Seconds, ElapsedTime.Milliseconds);

                return string.Format("Query Time: {0}.{1} seconds", ElapsedTime.Seconds, ElapsedTime.Milliseconds);
            }
        }

        public string Message_RowsAffected
        {
            get
            {
                return string.Format("Rows Returned: {0:#,###}", RowsAffected);
            }
        }

        public string Message_RowsAffectedElapsedTime
        {
            get
            {
                return string.Format("{0} ... {1}", Message_RowsAffected, Message_ElapsedTime);
            }
        }

        public SearchEventArgs(DataTable dt, SearchStatusCd s, int rowsaffected, TimeSpan elapsedtime)
        {
            _data = dt;
            _status = s;
            _RowsAffected = rowsaffected;
            _ElapsedTime = elapsedtime;
        }
    }	

}
