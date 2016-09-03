using System;
using System.Data;

namespace Dreaman.DataAccess
{
	/// <summary>
	/// DataAccessFactory ��ժҪ˵����
	/// </summary>	
	
	public sealed class DataAccessFactory
	{
		/// <summary>
		/// ����һ�����ݷ��ʶ���ʵ��������ָ�����ṩ���������Ӧ�����Ӵ���
		/// </summary>
		/// <param name="_type"></param>
		/// <param name="connstr"></param>
		/// <returns></returns>
		public static IDataAccess Produce(DataAccessType _type,string connstr)
		{
			switch(_type)
			{
				case DataAccessType.SQL:
					return new SQLDataAccess(connstr);
				case DataAccessType.Oracle:
					return new OracleDataAccess(connstr);
				case DataAccessType.OleDb:
					return new OleDbDataAccess(connstr);
				case DataAccessType.Odbc:
					return new OdbcDataAccess(connstr);
				default:
					return null;
			}			
		}
		/// <summary>
		/// ����洢���̵��õ�SQL��䣺exec ������ �����б�
		/// </summary>
		/// <param name="name"></param>
		/// <param name="ps"></param>
		/// <returns></returns>
		public static string BuildProcSQL(string name,params string[] ps)
		{
			string sqlstr="exec "+name+" ";
			string prestr="";
			foreach(string s in ps)
			{
				sqlstr+=prestr+"'"+s.Replace("'","''")+"'";
				prestr=",";
			}
			return sqlstr;
		}
	}
}
