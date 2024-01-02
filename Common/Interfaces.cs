using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace CS2010.Common
{
	public interface IMaintenance
	{
		int Insert();
		int Update();
		int Delete();
		void CheckInsert();
		void CheckUpdate();
		void CheckDelete();

		string DisplayField();
		string KeyField();

		DataTable GetList();
	}

}
