using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text;
using Dreaman.DataAccess;
using NUnit.Framework;

namespace Dreaman.InfoService
{
	/// <summary>
	/// ָ�����ʾ����
	/// </summary>
	public enum ShowAttr : int
	{
		UNKNOWN=-1,
		MINVALUE=0,
		HIDDEN=0,
		VISIBLE=1,
		COMBOBOX=2,
		CHECKBOX=3,
		FILE=4,
		HTML=5,
		NOVALUE=6,
		RAWVALUE=7,
		USECODECOMBOBOX=8,
		USECODECHECKBOX=9,
		OUTEREDITHTML=10,
		MAXVALUE=10
	}
	/// <summary>
	/// ָ�����ݽṹ
	/// </summary>	
	public class Guide
	{
		public string Qualifier;
		public string ID;
		public string Name;
		public string Unit;
		public ShowAttr ShowAttr;
		public string Description;
		public string Format;
		public int Indent;
		public void Copy(Guide g)
		{
			Qualifier=g.Qualifier;
			ID=g.ID;
			Name=g.Name;
			Unit=g.Unit;
			ShowAttr=g.ShowAttr;
			Description=g.Description;
			Format=g.Format;
			Indent=g.Indent;
		}
		public override string ToString()
		{
			return "Qualifier:"+Qualifier+" ID:"+ID+" Name:"+Name+" Unit:"+Unit+" ShowAttr:"+ShowAttr+" Description:"+Description+" Format:"+Format+" Indent:"+Indent;
		}
	}
	public class GuideInfo : DataAccessInfo
	{
		public string UpdateGuide(Guide guide)
		{		
			if(guide.Qualifier==null)
			{
				guide.Qualifier="__global__";
			}	
			IDataAccess dataAccess=DataAccessFactory.Produce(DAType,ConnectionString);
			DataSet ds=dataAccess.BeginUpdate("select * from ����_ָ�� where �޶�ID = '"+guide.Qualifier+"' and ָ��ID = '"+guide.ID+"'");
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
			dr["�޶�ID"]=guide.Qualifier;
			dr["ָ��ID"]=guide.ID;
			dr["ָ������"]=guide.Name;
			dr["ָ�굥λ"]=guide.Unit;
			dr["��ʾ����"]=guide.ShowAttr;
			dr["ָ������"]=guide.Description;
			dr["��ʾ��ʽ"]=guide.Format;
			dr["����"]=guide.Indent;
			dr.EndEdit();
			dataAccess.EndUpdate(ds);
			if(dataAccess.Error!=null)
				return dataAccess.Error.Message;
			else
			{
				ResetMetadata();
				return null;
			}
		}
		public string DeleteGuide(string qualifier,string id)
		{
			if(qualifier==null)
			{
				qualifier="__global__";
			}
			IDataAccess dataAccess=DataAccessFactory.Produce(DAType,ConnectionString);
			dataAccess.ExecuteNonQuery("delete from ����_ָ�� where  �޶�ID = '"+qualifier+"' and ָ��ID = '"+id+"'");
			if(dataAccess.Error!=null)
				return dataAccess.Error.Message;
			else
			{
				ResetMetadata();
				return null;
			}
		}
		public Guide GetGuide(string qualifier,string id)
		{
			Guide guide=new Guide();
			if(id==null)
			{
				guide.Qualifier=null;
				guide.ID=null;
				guide.Name=null;
				guide.Unit=null;
				guide.ShowAttr=ShowAttr.UNKNOWN;
				guide.Description=null;
				guide.Format=null;
				guide.Indent=0;
				return guide;
			}
			else
			{
				if(myDataSet!=null)
				{
					string filter;
					if(qualifier==null)
					{
						filter="�޶�ID = '__global__' and ָ��ID = '"+id+"'";
					}
					else
					{
						filter="�޶�ID = '"+qualifier+"' and ָ��ID = '"+id+"'";
					}
					DataTable dt=myDataSet.Tables[0];
					DataRow[] dr=dt.Select(filter);
					if(dr.Length<=0 && qualifier!=null)
					{
						dr=dt.Select("�޶�ID = '__global__' and ָ��ID = '"+id+"'");
					}
					if(dr.Length>0)
					{
						guide.Qualifier=qualifier;
						guide.ID=id;
						guide.Name=dr[0]["ָ������"] as string;
						guide.Unit=dr[0]["ָ�굥λ"] as string;
						object o=dr[0]["��ʾ����"];
						if(o==null || o==DBNull.Value)
							guide.ShowAttr=ShowAttr.VISIBLE;
						else
							guide.ShowAttr=(ShowAttr)o;
						guide.Description=dr[0]["ָ������"] as string;
						guide.Format=dr[0]["��ʾ��ʽ"] as string;
						o=dr[0]["����"];
						if(o==null || o==DBNull.Value)
							guide.Indent=0;
						else
							guide.Indent=(int)o;
						return guide;
					}
				}
				guide.Qualifier=qualifier;
				guide.ID=id;
				guide.Name=id;
				guide.ShowAttr=ShowAttr.UNKNOWN;
				guide.Description=null;
				guide.Format=null;
				guide.Indent=0;
				return guide;
			}
		}
		public void GenerateMetadata(string oledbConnStr)
		{
			MetadataUtility.OleDbConnectionString=oledbConnStr;
			DataTable tables=MetadataUtility.GetTables(null,"TABLE");
			DataTable dt=MetadataUtility.GetColumns(null,null);

			IDataAccess dataAccess=DataAccessFactory.Produce(DAType,ConnectionString);
			DataSet ds=dataAccess.BeginUpdate("select * from ����_ָ��");
			DataTable dt0=ds.Tables[0];
			foreach(DataRow dr in dt.Rows)
			{
				DataRow[] drs=tables.Select("TABLE_NAME = '"+dr["TABLE_NAME"]+"'");
				if(drs.Length<=0)continue;

				drs=dt0.Select("�޶�ID = '__global__' and ָ��ID = '"+dr["COLUMN_NAME"]+"'");
				if(drs.Length<=0)
				{
					DataRow dr0=dt0.NewRow();
					dt0.Rows.Add(dr0);
					dr0["�޶�ID"]="__global__";
					dr0["ָ��ID"]=dr["COLUMN_NAME"];
					dr0["ָ������"]=dr["COLUMN_NAME"];
				
					object leno=dr["CHARACTER_MAXIMUM_LENGTH"];
					long len=0;
					if(leno!=null && leno!=DBNull.Value)
					{
						len=(long)Convert.ChangeType(leno,typeof(long));
					}
					dr0["��ʾ����"]=1;

					object o=dr["DATA_TYPE"];
					int type=0;
					if(o!=null && o!=DBNull.Value)
					{
						type=(int)Convert.ChangeType(o,typeof(int));
					}
					if(type==128)//128=binary,varbinary,image
					{
						dr0["��ʾ����"]=3;
					}
					else if(type==129 || type==130)//129=char,varchar,text;130=nchar,nvarchar,ntext;
					{
						if(len>0x7fff)
						{
							dr0["��ʾ����"]=4;
						}
					}
					dr0.EndEdit();
				}
				drs=dt0.Select("�޶�ID = '"+dr["TABLE_NAME"]+"' and ָ��ID = '"+dr["COLUMN_NAME"]+"'");
				if(drs.Length<=0)
				{
					DataRow dr1=dt0.NewRow();
					dt0.Rows.Add(dr1);
					dr1["�޶�ID"]=dr["TABLE_NAME"];
					dr1["ָ��ID"]=dr["COLUMN_NAME"];
					dr1["ָ������"]=dr["COLUMN_NAME"];
				
					object leno=dr["CHARACTER_MAXIMUM_LENGTH"];
					long len=0;
					if(leno!=null && leno!=DBNull.Value)
					{
						len=(long)Convert.ChangeType(leno,typeof(long));
					}
					dr1["��ʾ����"]=1;

					object o=dr["DATA_TYPE"];
					int type=0;
					if(o!=null && o!=DBNull.Value)
					{
						type=(int)Convert.ChangeType(o,typeof(int));
					}
					if(type==128)//128=binary,varbinary,image
					{
						dr1["��ʾ����"]=3;
					}
					else if(type==129 || type==130)//129=char,varchar,text;130=nchar,nvarchar,ntext;
					{
						if(len>0x7fff)
						{
							dr1["��ʾ����"]=4;
						}
					}
					dr1.EndEdit();
				}
			}
			dataAccess.EndUpdate(ds);
			if(dataAccess.Error!=null)
			{
				//Error
			}
		}
		public GuideInfo(DataAccessType type,string connstr):base(type,connstr)
		{
			ResetMetadata();
		}		
		private void ResetMetadata()
		{
			//�����SQL�����ȷ�г����ֶ����ڼ��Ԫ���ݱ�Ľṹ
			string sqlstr="select �޶�ID, ָ��ID, ָ������, ָ�굥λ, ��ʾ����, ָ������, ��ʾ��ʽ, ���� from ����_ָ��";
			IDataAccess dataAccess=DataAccessFactory.Produce(DAType,ConnectionString);
			myDataSet=dataAccess.ExecuteDataSet(sqlstr);
			if(myDataSet==null && !alreadyCheckMetadata)
			{//����Ԫ���ݱ�
				alreadyCheckMetadata=true;
				object o=dataAccess.ExecuteScalar("select count(*) from ����_ָ��");
				if(o!=null && o!=DBNull.Value)
				{
					int recNo=(int)o;
					if(recNo<=0)
					{
						//���������û�м�¼����ɾ��
						string dropDDL="DROP TABLE ����_ָ��";
						dataAccess.ExecuteNonQuery(dropDDL);
					}
					else
					{
						throw new Exception("����_ָ���Ľṹ�뵱ǰҪ�󲻷�,�����Ѿ����ڲ����м�¼,������������ִ�б����������µĸ���_ָ���!");
					}
				}
				//�����±�
				string sqlDDL=@"
					CREATE TABLE ����_ָ��
					(
						�޶�ID varchar (255) NOT NULL ,
						ָ��ID varchar (255) NOT NULL ,
						ָ������ varchar (255) NULL ,
						ָ�굥λ varchar (255) NULL ,
						��ʾ���� int NULL ,
						ָ������ varchar (255) NULL ,
						��ʾ��ʽ varchar (255) NULL ,
						���� int NULL ,
						CONSTRAINT pk_����_ָ�� PRIMARY KEY (�޶�ID,ָ��ID)
					)
					";
				dataAccess.ExecuteNonQuery(sqlDDL);
				//
				myDataSet=dataAccess.ExecuteDataSet(sqlstr);
				if(myDataSet==null)
				{
					//ERROR
				}
			}
		}
		private DataSet myDataSet=new DataSet();
		private bool alreadyCheckMetadata=false;
	}
	[TestFixture]
	public class GuideInfoTest
	{
		[Test]
		public void SimpleTest()
		{
			Guide g=new Guide();
			g.ID="test1";
			g.Name="test2";
			g.Description="test3";
			g.ShowAttr=ShowAttr.VISIBLE;
			GuideInfo gm=new GuideInfo(DataAccessType.OleDb,"provider=SQLOLEDB;data source=lan;initial catalog=huairou;user id=sa;password=sa;");
			gm.UpdateGuide(g);
			Guide g2=gm.GetGuide(null,"test1");
			Assert.AreEqual(g.Name,g2.Name,"UpdateGuide or GetGuide test failed.");
			Console.Out.WriteLine(g2);
			gm.DeleteGuide(null,"test1");
			Guide g3=gm.GetGuide(null,"test1");
			Assert.IsNull(g3.Name,"DeleteGuide test failed.");			
		}
	}
}
