using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Dreaman.DataAccess;
using Dreaman.InfoService;

namespace Dreaman.InfoService.Report
{
	/// <summary>
	/// ����һ���������Ϣ,��ͬ�����ݲ�ı���,��������������ʾ��Ϣ��
	/// </summary>
	public class QueryField : TableFieldBase
	{
		public bool IsGroup=false;//�Ƿ����������
		public StatisticFieldType StatMethod=StatisticFieldType.AUTO;//ͳ�Ʒ���
		public int SortOrder=0;//�������,0����������
		public string WhereClause=null;//where�������ʽ,��������

		public ArrayList Values=new ArrayList();//�ֶ�ֵ,�������ݿ����

		public bool ValueIsNotNull(int index)
		{
			if(index<0 || index>=Values.Count)
				return false;
			if(Values[index]==null || Values[index]==DBNull.Value)
				return false;
			return true;
		}
		public string GetStringValue(int index)
		{
			if(index<0 || index>=Values.Count)
				return "";
			object val=Values[index];	
			return ValueToString(val,null);
		}
		public string GetFormatValue(int index)
		{
			if(index<0 || index>=Values.Count)
				return "";
			object val=Values[index];	
			return ValueToString(val,Format);
		}
		public QueryField Clone()
		{
			QueryField qf=new QueryField();
			qf.ID=ID;
			qf.Name=Name;
			qf.ShowName=ShowName;
			qf.Type=Type;
			qf.Length=Length;
			qf.Unit=Unit;
			qf.Nullable=Nullable;
			qf.ShowType=ShowType;
			qf.Indent=Indent;
			qf.BaseTable=BaseTable;
			qf.BaseField=BaseField;
			qf.Alias=Alias;
			qf.Format=Format;
			qf.ReadOnly=ReadOnly;
			qf.UseClassValue=UseClassValue;
			qf.FromUpdateTable=FromUpdateTable;

			qf.IsGroup=IsGroup;
			qf.StatMethod=StatMethod;
			qf.SortOrder=SortOrder;
			qf.WhereClause=WhereClause;

			for(int i=0;i<qf.Values.Count;i++)
			{
				qf.Values.Add(Values[i]);
			}
			return qf;
		}
	}
	/// <summary>
	/// ����һ��SQL������γɵ����ݿ飬����ְ�����������ܣ���ǰδʵ��,������ܵ�������Ҫ
	/// ����µ�������ṹ�����������ܻ�����StatisticField��ķ�ʽ����ֵ����
	/// </summary>
	public class SQLDataBlock : SQLInfo,IDataBlock
	{
		#region IDataBlock ��Ա

		public string Filter(string templateContent, string dataTag)
		{
			DataBlockUtility dataBlock=new DataBlockUtility();
			dataBlock.FilterDataLabel+=new FilterDataLabel(dataBlock_FilterDataLabel);
			return dataBlock.Filter(templateContent,dataTag);
		}


		public string GetDataBlockUI(string[] fieldNames)
		{
			int ct=RecordCount;
			if(ct<=0)
				return "";
			HtmlTable table=new HtmlTable();
			if(!IsFormStyle)
			{
				for(int i=0;i<ct;i++)
				{
					HtmlTableRow tr=new HtmlTableRow();
					table.Rows.Add(tr);
					foreach(string s in fieldNames)
					{
						HtmlTableCell tc=new HtmlTableCell();
						tr.Cells.Add(tc);
						BuildFieldView(i,s,tc);
					}
				}
			}
			else
			{
				foreach(string s in fieldNames)
				{
					HtmlTableRow tr=new HtmlTableRow();
					table.Rows.Add(tr);
					for(int i=0;i<ct;i++)
					{
						HtmlTableCell tc=new HtmlTableCell();
						tr.Cells.Add(tc);
						BuildFieldView(i,s,tc);
					}
				}
			}
			StringWriter sw=new StringWriter();
			table.RenderControl(new HtmlTextWriter(sw));
			sw.Close();
			return sw.ToString();
		}
		public string GetDataBlockUI()
		{
			return "";
		}

		#endregion
		#region ϵ������
		public bool IsFormStyle
		{
			get
			{
				return isFormStyle;
			}
			set
			{
				isFormStyle=value;
			}
		}
		public string UploadFileVirtualPath
		{
			get
			{
				return uploadFileVPath;
			}
			set
			{
				uploadFileVPath=value;
			}
		}
		public new QueryField this[string fieldName]
		{
			get
			{
				return tableFields[fieldName] as QueryField;
			}
			set
			{
				tableFields[fieldName]=value;
			}
		}
		public int RecordCount
		{
			get
			{
				int ct=FieldCount;
				if(ct<=0)return 0;
				return this[SQLFields[0]].Values.Count;
			}
			set
			{
				int count=value;
				if(count<0)count=0;
				int ct=FieldCount;
				if(ct<=0)return;
				foreach(string s in SQLFields)
				{
					while(this[s].Values.Count<count)
						this[s].Values.Add(null);
				}
				foreach(string s in SQLFields)
				{
					while(this[s].Values.Count>count)
						this[s].Values.RemoveAt(count);
				}
			}
		}
		#endregion
		/// <summary>
		/// �������Ƿ���������Ϣ�������˳���������������
		/// </summary>
		public void SetFieldGroup(string fieldname,bool isGroup)
		{
			if(this[fieldname]==null)
				return;
			this[fieldname].IsGroup=isGroup;
		}
		/// <summary>
		/// �������������Ϣ��
		/// </summary>
		public void SetFieldSort(string fieldname,int sort)
		{
			if(this[fieldname]==null)
				return;
			this[fieldname].SortOrder=sort;
		}
		public SQLDataBlock(string sqlstr,DataAccessType datype,string connstr):base(sqlstr,datype,connstr)
		{}
		protected override TableFieldBase CreateField()
		{
			return new QueryField();
		}
		public void SelectRecord(string condition)
		{
			string sqlstr=parser.BuildSQL(condition,null);
			RecordCount=0;
			string connstr=ConnectionString;
			if(connstr==null)throw new Exception("ϵͳ����");
			IDataAccess dataAccess=DataAccessFactory.Produce(DAType,connstr);
			IDataReader myReader=dataAccess.ExecuteDataReader(sqlstr);
			try
			{
				while(myReader.Read())//�ҵ���¼��д��ֵ
				{
					foreach(string key in SQLFields)
					{
						if(this[key].Type.IsArray)
						{
							//ע�⣺��ǰ�ٶ����ݿⲻ�洢�������ļ��������Ҫ֧�ֶ������ļ���Ӧ�޸���δ��롣
							//�������Ƶ��뷨�Ƕ������ļ���洢���ļ�������Ϣ���������ȡ��Щ��Ϣ�������
							//�ܻ�ȡ�ļ����ݵ�URL���󣬲������һ��HTML���ӷ��ص��ͻ��ˣ���Ϊ���ֶε�ֵ��ʾ��
							int i=myReader.GetOrdinal(key);
							if(!myReader.IsDBNull(i))
							{
								long size=myReader.GetBytes(i,0,null,0,0);
								byte[] content=new byte[size];
								myReader.GetBytes(i,0,content,0,(int)size);
								string strFile=FileField.Content(content,UploadFileVirtualPath);
								this[key].Values.Add(strFile);
							}
							else
							{
								this[key].Values.Add(null);
							}
							continue;
						}
						this[key].Values.Add(QueryField.ValueToString(myReader[key],null));
					}
				}
			}
			finally
			{
				if(myReader!=null)
					myReader.Close();
			}	
			if(dataAccess.Error!=null)
				throw new Exception("��ѯ����"+dataAccess.Error.Message);
		}
		private string uploadFileVPath="../uploadFiles";
		private bool isFormStyle=false;

		/// <summary>
		/// �����UI��ĳ����¼ʱ��ĳ�ֶε���ʾ,��Ҫ�����ֶ�ֵ��ת�����ʽ��.
		/// </summary>
		/// <param name="index">��¼����</param>
		/// <param name="f">�ֶ���</param>
		/// <param name="cell">������ʾ�ֶ�ֵ��HtmlTableCell</param>
		private void BuildFieldView(int index,string f,HtmlTableCell cell)
		{
			QueryField tf=this[f];
			if(tf==null)
				return;
			if(!tf.UseClassValue)
			{
				if(tf.ShowType==ShowType.COMBOBOX)
				{
					NameValueCollection vals=GetClassVals(tf.Name);
					string v=tf.GetStringValue(index);
					if(vals!=null && vals[v]!=null)
					{
						v=vals[v].Trim();
					}
					cell.InnerHtml=v;
				}
				else if(tf.ShowType==ShowType.CHECKBOX)
				{
					NameValueCollection vals=GetClassVals(tf.Name);
					if(vals!=null)
					{
						string val=tf.GetStringValue(index);
						string[] keys=val.Split(',');
						for(int i=0;i<keys.Length-1;i++)
						{
							string v=vals[keys[i]];
							if(v!=null)
								keys[i]=v.Trim();
						}
						cell.InnerHtml=string.Join(",",keys,0,keys.Length-1);
					}
				}
				else
				{
					cell.InnerHtml=tf.GetFormatValue(index);
				}
			}
			else
				cell.InnerHtml=tf.GetFormatValue(index);

			if(tf.Type==typeof(string) || tf.Type.IsArray || tf.ShowType==ShowType.FILEUPLOAD || tf.ShowType==ShowType.FILE || tf.Type==typeof(bool))
			{}
			else if(tf.Type==typeof(System.DateTime))
			{
				cell.Attributes.Add("align","center");
			}
			else
			{
				cell.Attributes.Add("align","right");
			}
		}
		private string dataBlock_FilterDataLabel(string fieldName, int index)
		{
			string tdAttr=">";
			if(fieldName==null)
				return tdAttr+GetDataBlockUI();
			else if(fieldName.IndexOf(',')<=0)
			{
				QueryField qf=this[fieldName];
				if(qf.Type!=typeof(string) && qf.Type!=typeof(DateTime) && qf.Type!=typeof(bool) && !qf.Type.IsArray && qf.Type.IsValueType)
				{
					tdAttr=" x:num>";
				}
				if(qf!=null)
				{
					return tdAttr+qf.GetFormatValue(index);
				}
				else
				{
					return tdAttr;
				}
			}
			else
			{
				string[] fields=fieldName.Split(',');
				return tdAttr+GetDataBlockUI(fields);
			}
		}
	}
}
