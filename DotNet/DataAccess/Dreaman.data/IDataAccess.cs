using System;
using System.Data;

namespace Dreaman.DataAccess
{
	/// <summary>
	/// IDataAccess�ӿڣ�ͨ�õ����ṩ���޹ص����ݷ��ʽӿڣ������׼SQL����ʹ��ʱϣ����ʵ��
	/// ��������ݿ��޹ص����ݷ��ʲ㡣
	/// </summary>
	public interface IDataAccess
	{
		/// <summary>
		/// ִ���������ȴ�ʱ��
		/// </summary>
		int CommandTimeout
		{
			get;set;
		}
		/// <summary>
		/// ���һ�β������쳣��Ϣ
		/// </summary>
		Exception Error
		{
			get;
		}
		/// <summary>
		/// ��ǰ�����ݿ��ṩ��
		/// </summary>
		string Driver
		{
			get;
		}
		/// <summary>
		/// ����һ����������
		/// </summary>
		/// <param name="name"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		IDbDataParameter CreateParameter(string name,object val);
		/// <summary>
		/// �õ�SQL���������������Ϣ
		/// </summary>
		/// <param name="sqlstr"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		DataTable GetSchemaTable(string sqlstr,params IDbDataParameter[] args);
		/// <summary>
		/// �õ�SQL���������������Ϣ
		/// </summary>
		/// <param name="sqlstr"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		DataTable GetSchemaStruct(string sqlstr,params IDbDataParameter[] args);
		/// <summary>
		/// ִ��SQL��䷵��һ��IDataReader���ڲ����ݿ�������IDataReader.Closeʱ�رա�
		/// </summary>
		/// <param name="sqlstr"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		IDataReader ExecuteDataReader(string sqlstr,params IDbDataParameter[] args);
		/// <summary>
		/// ִ��SQL��䷵��һ��IDataReader���ڲ����ݿ�������IDataReader.Closeʱ�رա�
		/// </summary>
		/// <param name="sqlstr"></param>
		/// <param name="behavior"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		IDataReader ExecuteDataReader(string sqlstr,CommandBehavior behavior,params IDbDataParameter[] args);
		/// <summary>
		/// ִ��SQL��䷵��һ��DataSet
		/// </summary>
		/// <param name="sqlstr"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		DataSet ExecuteDataSet(string sqlstr,params IDbDataParameter[] args);
		/// <summary>
		/// ִ��SQL��䷵��һ������ֵ
		/// </summary>
		/// <param name="sqlstr"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		object ExecuteScalar(string sqlstr,params IDbDataParameter[] args);
		/// <summary>
		/// ִ�в����ؽ������SQL��䣬ͨ����DDL���SELECT DML
		/// </summary>
		/// <param name="sqlstr"></param>
		/// <param name="args"></param>
		void ExecuteNonQuery(string sqlstr,params IDbDataParameter[] args);
		/// <summary>
		/// ��ͬһ��������ִ�ж����ǲ�ѯSQL
		/// </summary>
		/// <param name="sqlstrs"></param>
		void ExecuteNonQuery2(params string[] sqlstrs);
		/// <summary>
		/// ��ͬһ��������ִ�ж�����ѯSQL������һ��DataSet
		/// </summary>
		/// <param name="sqlstrs"></param>
		/// <returns></returns>
		DataSet ExecuteDataSet2(params string[] sqlstrs);
		/// <summary>
		/// ��ʼһ���������񣬷��ع����µ�DataSet
		/// </summary>
		/// <param name="sqlstr"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		DataSet BeginUpdate(string sqlstr,params IDbDataParameter[] args);
		/// <summary>
		/// ��ʼһ���������񣬷��ع����µ�DataSet
		/// </summary>
		/// <param name="sqlstrs"></param>
		/// <returns></returns>
		DataSet BeginUpdate2(params string[] sqlstrs);
		/// <summary>
		/// ��������������BeginUpdate/BeginUpdate2�ɶ�ʹ�ã�����������ǰ�濪ʼ��������ʱ���ص�DataSet��
		/// ��Ȼ�����е������Ѿ������¹���
		/// </summary>
		/// <param name="ds"></param>
		void EndUpdate(DataSet ds);
	}
	/// <summary>
	/// ���ݷ������ͣ���Ӧ.NET�еļ��������ṩ�ߣ������������ݷ��ʹ���������Ӧ�����ݷ��ʶ���
	/// </summary>
	public enum DataAccessType : int
	{
		SQL,OleDb,Oracle,Odbc
	}
}
