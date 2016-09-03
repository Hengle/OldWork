using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Data.OleDb;
using System.Text;
using System.Xml;
using System.IO;
using Dreaman.DataAccess;

namespace Dreaman.InfoService
{
	/// <summary>
	/// һ��ģ����һ��Ψһ��ʶ,һ������,һ����̨����ģ����һ��UI���ɡ�
	/// ���ǵ�չʾ�뽻�����ֶ��Ƿǳ����Ի��ķ��棬���ǲ�������MVCģʽ����������������ǰ�V��C�ϲ���UI��
	/// ������һ��ģ����ǵ���UI����������ģ��ID��UI�����ģ���������ѯģ�Ͳ�ʹ������������UI����ʹ��ģ
	/// �鶨�����ģ�ͣ����������Ķ�����Ҫ�ǿ��ǵ��ṩ����ģ�ͻ��߹���UI�Ŀ����ԣ�
	/// </summary>
	public struct Module
	{
		public int ModuleID;
		public string Description;
		public string Model;
		public string UI;
		public override string ToString()
		{
			return "ModuleID:"+ModuleID+" Description:"+Description+" Model:"+Model+" UI:"+UI;
		}

	}
	/// <summary>
	/// ģ���������ͨ����һ���־û��洢��ģ�鼯�ϣ���Ϊģ����֯�Ǳ��ֲ�Ķ������Ƿǳ����Ի��ģ�ģ�������
	/// �����ṩģ����֯�������Ϣ��
	/// ģ��ĸ����־�Ӧ��ȫ�־û�����ǰ����ģ����UI��������һ���ַ�������������
	/// ���������ַ�������Ϊ��ģ����UI�ı�ʶ��
	/// </summary>
	public class ModuleManager
	{
		static string metaConn=null;
		static string uploadFileVirtualPath=null;
		static HybridDictionary libConns=new HybridDictionary(true);
		static HybridDictionary libConnTypes=new HybridDictionary(true);
		static HybridDictionary modelFactorys=new HybridDictionary(true);
		static DataAccessType daType;
		//��������
		public static void SetMetaConn(DataAccessType datype,string metaconn)
		{
			daType=datype;
			metaConn=metaconn;
			SetDB("0",metaConn,daType);
		}
		public static string UploadFileVirtualPath
		{
			set
			{
				uploadFileVirtualPath=value;
			}
			get
			{
				return uploadFileVirtualPath;
			}
		}
		public static void SetDB(string libId,string provider,string server,string database,string user,string pass)
		{	
			string connstr="";
			DataAccessType type=DataAccessType.OleDb;
			string prov=provider.ToUpper();
			if(prov.IndexOf("ORAOLEDB.ORACLE")>=0)
			{
				connstr="Provider="+prov+";Data Source="+server+";User Id="+user+";Password="+pass+";";
				type=DataAccessType.OleDb;
			}
			
			else if(prov.IndexOf("SQLOLEDB")>=0)
			{
				connstr="Provider="+prov+";Data Source="+server+";Initial Catalog="+database+";User Id="+user+";Password="+pass+";";
				type=DataAccessType.OleDb;
			}
			else if(prov.IndexOf("[ODBC.NET]")>=0)
			{
				prov=prov.Substring(10);
				connstr="DSN="+prov+";SRVR="+server+";DB="+database+";UID="+user+";PWD="+pass+";";
				type=DataAccessType.Odbc;
			}
			else if(prov.IndexOf("[ORACLE.NET]")>=0)
			{
				connstr="Data Source="+server+";User Id="+user+";Password="+pass+";";
				type=DataAccessType.Oracle;
			}
			else if(prov.IndexOf("[SQL.NET]")>=0)
			{
				connstr="Data Source="+server+";Initial Catalog="+database+";User Id="+user+";Password="+pass+";";
				type=DataAccessType.SQL;
			}
			else
			{
				connstr="";
				type=DataAccessType.OleDb;
			}
			SetDB(libId,connstr,type);
		}
		public static void SetDB(string libId,string connstr,DataAccessType type)
		{
			libConns[libId]=connstr;
			libConnTypes[libId]=type;
		}
		public static string GetDBConn(string libId)
		{
			return libConns[libId] as string;
		}
		public static DataAccessType GetDBType(string libId)
		{
			return (DataAccessType)libConnTypes[libId];
		}
		//��������
		public static IDataAccess NewDataAccess(string libId)
		{
			IDataAccess dataAccess=DataAccessFactory.Produce(GetDBType(libId),GetDBConn(libId));
			return dataAccess;
		}
		public static InputControl NewInputControl(string sqlstr,string libId)
		{
			InputControl input=new InputControl();
			input.DataAccessType=GetDBType(libId);
			input.ConnectionString=GetDBConn(libId);			
			input.SQL=sqlstr;
			return input;
		}
		public static StatisticControl NewStatisticControl(string sqlstr,string libId)
		{
			StatisticControl stat=new StatisticControl();
			stat.DataAccessType=GetDBType(libId);
			stat.ConnectionString=GetDBConn(libId);			
			stat.SQL=sqlstr;
			return stat;
		}
		public static void SetModelFactory(string id,IModelFactory val)
		{
			modelFactorys[id]=val;
		}
		public static IModelFactory GetModelFactory(string id)
		{
			return modelFactorys[id] as IModelFactory;
		}
		public static object GetModel(int id,NameValueCollection args)
		{
			Module m=GetModule(id);
			string modelId=m.Model;
			if(modelId==null)
				return null;
			int si=modelId.IndexOf("@");
			if(si>0)
			{
				string factory=modelId.Substring(0,si);
				string leftId=modelId.Substring(si+1);
				IModelFactory mf=GetModelFactory(factory);
				return mf.GetModel(leftId,args);
			}			
			return null;
		}
		//ģ����Ϣ������
		public static Module NewModule(string desc,string model,string ui)
		{
			Module m=new Module();
			m.ModuleID=GetNextCode();
			m.Description=desc;
			m.Model=model;
			m.UI=ui;
			return m;
		}
		public static void UpdateModule(Module m)
		{
			IDataAccess dataAccess=DataAccessFactory.Produce(daType,metaConn);
			string sqlstr="select * from MODULES where MODULE_ID='"+m.ModuleID+"'";
			DataSet ds=dataAccess.BeginUpdate(sqlstr);
			DataTable dt=ds.Tables[0];
			DataRow dr=null;
			if(dt.Rows.Count<=0)
			{
				dr=dt.NewRow();
				dt.Rows.Add(dr);
			}
			else
			{
				dr=dt.Rows[0];
				dr.BeginEdit();
			}
			dr["MODULE_ID"]=m.ModuleID;
			dr["DESCRIPTION"]=m.Description;
			dr["MODEL"]=m.Model;
			dr["UI"]=m.UI;
			dr.EndEdit();
			dataAccess.EndUpdate(ds);
			if(dataAccess.Error!=null)
				throw dataAccess.Error;
		}
		public static void DeleteModule(int id)
		{
			IDataAccess dataAccess=DataAccessFactory.Produce(daType,metaConn);
			string sqlstr="delete from MODULES where MODULE_ID="+id;
			dataAccess.ExecuteNonQuery(sqlstr);
		}
		public static Module GetModule(int id)
		{
			Module mi=new Module();
			IDataAccess dataAccess=DataAccessFactory.Produce(daType,metaConn);
			string sqlstr="select MODULE_ID,DESCRIPTION,MODEL,UI from MODULES where MODULE_ID="+id;
			IDataReader myReader=dataAccess.ExecuteDataReader(sqlstr);
			try
			{
				if(myReader.Read())
				{
					mi.ModuleID=(int)myReader["MODULE_ID"];
					mi.Description=myReader["DESCRIPTION"] as string;
					mi.Model=myReader["MODEL"] as string;
					mi.UI=myReader["UI"] as string;					
				}
			}
			finally
			{
				if(myReader!=null)
					myReader.Close();
			}
			return mi;
		}
		private static int GetNextCode()
		{
			IDataAccess dataAccess=DataAccessFactory.Produce(daType,metaConn);
			string sqlstr="select max(MODULE_ID) from MODULES";
			object o=dataAccess.ExecuteScalar(sqlstr);
			if(o==null || o==DBNull.Value)
				return 1;
			else
				return (int)o+1;
		}
	}
}
