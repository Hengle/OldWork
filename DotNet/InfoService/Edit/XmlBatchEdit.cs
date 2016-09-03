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
using System.Xml;
using Dreaman.DataAccess;

namespace Dreaman.InfoService
{
	/// <summary>
	/// XmlBatchEdit ��ժҪ˵������Ϊ��Ҫͨ��XML����ϵ������UI��Field��OldValues�������ֵ��ѯ���ȡ�û��������ʱ�Ż��õ���
	/// ���״̬��Ҫ��Update�����ã���ΪUpdate����Ҫ��GetInputValue��GetInputValue����һ��һ����ȷ���Ƿ����״̬�������Գ�
	/// GetInputValue��Select�����������״̬��
	/// </summary>
	public class XmlBatchEdit : SQLInfo
	{
		#region ϵ������
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
		public new BatchEditField this[string fieldName]
		{
			get
			{
				return tableFields[fieldName] as BatchEditField;
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
					while(this[s].OldValues.Count<count)
						this[s].OldValues.Add(null);
				}
				foreach(string s in SQLFields)
				{
					while(this[s].Values.Count>count)
						this[s].Values.RemoveAt(count);
					while(this[s].OldValues.Count>count)
						this[s].OldValues.RemoveAt(count);
				}
			}
		}
		#endregion
		public void SetFieldInitVal(string s,int index,string val)
		{
			if(this[s]==null)
				return;			
			if(index<0)
				return;
			while(this[s].Values.Count<=index)
				this[s].Values.Add(null);
			this[s].Values[index]=val;
			while(this[s].OldValues.Count<=index)
				this[s].OldValues.Add(null);
			this[s].OldValues[index]=val;
		}
		/// <summary>
		/// ����ָ��ĳ����¼�Ƿ���ȷ�Ĵ������״̬���ڻ�ȡ����ֵʱ��ȷ�Ĵ������״̬�ļ�¼����¼��ֵ���Ǿ�ֵ
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		private bool AppendState(int index)
		{
			if(appendStates.Count<=index)
				return true;
			else
				return (bool)appendStates[index];
		}
		private void AppendState(int index,bool append)
		{
			while(appendStates.Count<=index)
			{
				appendStates.Add(true);
			}
			appendStates[index]=append;
		}
		public XmlBatchEdit(string sqlstr,DataAccessType datype,string connstr):base(sqlstr,datype,connstr)
		{}		
		protected override TableFieldBase CreateField()
		{
			return new BatchEditField();
		}
		/// <summary>
		/// �����趨�õ��ֶγ�ʼֵ�������������ļ�¼,��ʱ��¼���Ѿ��趨Ϊĳ��ֵ
		/// </summary>
		/// <returns></returns>
		public string SelectRecord()
		{
			int count=RecordCount;
			for(int ii=0;ii<count;ii++)
			{
				string condition=GetSelectCondition(ii);
				string sqlstr=parser.BuildSQL(condition,null);
				string connstr=ConnectionString;
				if(connstr==null)throw new Exception("ϵͳ����");
				IDataAccess dataAccess=DataAccessFactory.Produce(DAType,connstr);
				IDataReader myReader=dataAccess.ExecuteDataReader(sqlstr);
				try
				{
					if(myReader.Read())//�ҵ���¼��д��ֵ
					{
						foreach(string key in SQLFields)
						{
							if(this[key].Type.IsArray)
							{							
								//�����༭��֧���ļ��ֶ�
								continue;
							}
							else
							{
								this[key].Values[ii]=myReader[key];
							}
						}
						if(myReader.Read())//���ڶ����¼ʱ��δ���ֵ�ֶ�д��ֵ
						{
							foreach(string key in SQLFields)
							{
								if(this[key].OldValues[ii]==null || this[key].OldValues[ii].Trim().Length<=0)
									this[key].Values[ii]=null;
							}
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
			
			StringWriter sw=new StringWriter();
			XmlTextWriter xmlWriter=new XmlTextWriter(sw);

			xmlWriter.WriteStartElement("DATATABLE");
			for(int ii=0;ii<count;ii++)
			{
				xmlWriter.WriteStartElement("ROW");
				foreach(string key in SQLFields)
				{
					if(this[key].Type.IsArray)
					{
						//�����༭��֧���ļ��ֶ�
						continue;
					}
					xmlWriter.WriteStartElement(this[key].ID);					
					xmlWriter.WriteString(this[key].GetStringValue(ii));
					xmlWriter.WriteEndElement();
					xmlWriter.WriteStartElement(this[key].ID+"old");					
					xmlWriter.WriteString(this[key].GetStringValue(ii));
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteEndElement();
			}				

			xmlWriter.WriteEndElement();
			xmlWriter.Close();
			sw.Close();
			return sw.ToString();
		}
		/// <summary>
		/// ��ָ���������˵�ǰ����¼�������һ��XML�����Ľ����
		/// </summary>
		/// <param name="condition">����</param>
		/// <returns>
		/// ��XML�ļ����ʾ�Ľ��������Ԫ��ΪDATATABLE����Ԫ��ΪROW�����ֶ�Ԫ��Ϊ�ֶ�ID��
		/// InnerTextΪ��ֵ��ÿ���ֶ�����һ����ֵֵ��Ԫ��Ϊ�ֶ�ID+old
		/// </returns>
		public string SelectRecord(string condition)
		{
			string sqlstr=parser.BuildSQL(condition,null);
			return SelectCompatibleRecord(sqlstr);
		}
		/// <summary>
		/// ��ָ���������SQL��乹������¼�����ݣ�����һ��XML�����Ľ����
		/// </summary>
		/// <param name="sqlstr">SQL���</param>
		/// <returns>
		/// ��XML�ļ����ʾ�Ľ��������Ԫ��ΪDATATABLE����Ԫ��ΪROW�����ֶ�Ԫ��Ϊ�ֶ�ID��
		/// InnerTextΪ��ֵ��ÿ���ֶ�����һ����ֵֵ��Ԫ��Ϊ�ֶ�ID+old
		/// </returns>
		public string SelectCompatibleRecord(string sqlstr)
		{
			RecordCount=0;
			string connstr=ConnectionString;
			if(connstr==null)throw new Exception("ϵͳ����");
			IDataAccess dataAccess=DataAccessFactory.Produce(DAType,connstr);
			IDataReader myReader=dataAccess.ExecuteDataReader(sqlstr);
		
			StringWriter sw=new StringWriter();
			XmlTextWriter xmlWriter=new XmlTextWriter(sw);

			xmlWriter.WriteStartElement("DATATABLE");
			while(myReader.Read())//�ҵ���¼��д��ֵ
			{
				xmlWriter.WriteStartElement("ROW");
				foreach(string key in SQLFields)
				{
					if(this[key].Type.IsArray)
					{
						//�����༭��֧���ļ��ֶ�
						continue;
					}
					xmlWriter.WriteStartElement(this[key].ID);
					xmlWriter.WriteString(BatchEditField.ValueToString(myReader[key],null));
					xmlWriter.WriteEndElement();
					xmlWriter.WriteStartElement(this[key].ID+"old");
					xmlWriter.WriteString(BatchEditField.ValueToString(myReader[key],null));
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteEndElement();
			}				

			xmlWriter.WriteEndElement();
			xmlWriter.Close();
			sw.Close();

			myReader.Close();		
			if(dataAccess.Error!=null)
				throw new Exception("��ѯ����"+dataAccess.Error.Message);
			return sw.ToString();
		}	
		public string UpdateRecord(string dataLand,bool bOverWrite)
		{
			GetInputValue(dataLand);
			CheckInputValue();
			return UpdateHelper(bOverWrite);
		}
		private void GetInputValue(string dataLand)
		{
			XmlDocument xmlDoc=new XmlDocument();
			xmlDoc.LoadXml(dataLand);

			XmlNodeList nodeList=xmlDoc.SelectNodes("//ROW");
			if(nodeList==null)
				return;
			for(int ii=0;ii<nodeList.Count;ii++)
			{
				AppendState(ii,false);
			}
			foreach(string key in KeyFields)
			{
				for(int ii=0;ii<nodeList.Count;ii++)
				{
					XmlNode node=nodeList[ii];
					XmlNode cnode=node.SelectSingleNode(this[key].ID);
					if(cnode==null)
					{
						this[key].Values.Add(null);
					}
					else
					{
						this[key].Values.Add(cnode.InnerText);
					}
					XmlNode onode=node.SelectSingleNode(this[key].ID+"old");
					if(onode==null)
					{
						if(cnode==null)
							this[key].OldValues.Add(null);
						else
						{
							AppendState(ii,true);//�ؼ��־�ֵΪ����������¼ӵļ�¼
						}
					}
					else
					{
						this[key].OldValues.Add(onode.InnerText);
					}
				}
			}
			foreach(string key in NormalFields)
			{
				for(int ii=0;ii<nodeList.Count;ii++)
				{
					XmlNode node=nodeList[ii];
					XmlNode cnode=node.SelectSingleNode(this[key].ID);
					if(cnode==null)
					{
						this[key].Values.Add(null);
					}
					else
					{
						this[key].Values.Add(cnode.InnerText);
					}
					XmlNode onode=node.SelectSingleNode(this[key].ID+"old");
					if(onode==null)
					{
						this[key].OldValues.Add(null);
					}
					else
					{
						this[key].OldValues.Add(onode.InnerText);
					}
				}
			}
			//�Դ������״̬�ļ�¼����¼��ֵ���Ƶ���ֵ
			for(int ii=0;ii<nodeList.Count;ii++)
			{
				if(AppendState(ii))
				{
					foreach(string s in SQLFields)
					{
						this[s].OldValues[ii]=this[s].GetStringValue(ii);
					}
				}
			}
		}
		/// <summary>
		/// ���¼�����ݵ���Ч��,��ǰֻ��鲻ӦΪ�յ���,���Կ��Ǽ��϶�����,��ֵ,�ַ����������ݵļ��.
		/// </summary>
		/// <returns></returns>
		/// 
		private void CheckInputValue()
		{
			int count=RecordCount;
			errors.Clear();
			for(int i=0;i<count;i++)
			{
				errors.Add("");
			}
			foreach(string key in SQLFields)
			{
				if(!this[key].FromUpdateTable)
					continue;
				if(this[key].Type.IsArray)continue;//�������ֶβ������
				if(this[key].ReadOnly)continue;//ֻ���ֶβ���¼��
				if(this[key].ShowType==ShowType.FILEUPLOAD || this[key].ShowType==ShowType.HTMLEDIT || this[key].ShowType==ShowType.HTMLOUTEREDIT || this[key].ShowType==ShowType.FILE || this[key].ShowType==ShowType.HTML)
					continue;//�ļ���HTML�༭���ֶβ�������
				if(this[key].ShowType==ShowType.RAWVALUE || this[key].ShowType==ShowType.NOVALUE)
					continue;//û�б༭�ؼ����߲���ʾֵ���ֶβ�����飨����ʹ���ǲ�Ӧ��ִ�е�����ģ�����һ�ַǷ�״̬��飩
				for(int ii=0;ii<count;ii++)
				{
					string val=this[key].GetStringValue(ii);
					bool nullable=this[key].Nullable;
					if(!nullable && (val==null || val.Trim().Length<=0))
					{
						string prestr="";
						if(errors[ii].Length>0)
							prestr="\n";
						errors[ii]+=prestr+this[key].ShowName+"---����Ϊ�գ�";
					}
					if(val!=null && val.Trim().Length>0)
					{
						val=val.Trim();		
						Type type=this[key].Type;
						if(type==typeof(string))
						{					
							int byteLen=this[key].Encoding.GetByteCount(val);
							if(byteLen>this[key].Length)
							{
								string prestr="";
								if(errors[ii].Length>0)
									prestr="\n";
								errors[ii]+=prestr+this[key].ShowName+"---�ı���������ǰ�ֽ�����"+byteLen+" > ��������ֽ�����"+this[key].Length+"��ͨ��һ������ռ2���ֽڣ�";
							}
						}
						else if(type==typeof(DateTime))
						{
							try
							{
								DateTime.Parse(val);
							}
							catch(FormatException)
							{
								string prestr="";
								if(errors[ii].Length>0)
									prestr="\n";
								errors[ii]+=prestr+this[key].ShowName+"---���ڸ�ʽ����ӦΪyyyy-MM-dd��";						
							}
						}
						else if(!type.IsArray && type!=typeof(DateTime))
						{
							try
							{
								Convert.ChangeType(val,type);
							}
							catch(FormatException)
							{
								string prestr="";
								if(errors[ii].Length>0)
									prestr="\n";
								errors[ii]+=prestr+this[key].ShowName+"---��ֵ��ʽ����";
							}
						}
						else
						{				
							int byteLen=this[key].Encoding.GetByteCount(val);
							if(byteLen>this[key].Length)
							{
								string prestr="";
								if(errors[ii].Length>0)
									prestr="\n";
								errors[ii]+=prestr+this[key].ShowName+"---�ı���������ǰ�ֽ�����"+byteLen+" > ��������ֽ�����"+this[key].Length+"��ͨ��һ������ռ2���ֽڣ�";
							}
						}
					}
				}	
			}
		}
		private string UpdateHelper(bool bOverWrite)
		{
			string connstr=ConnectionString;
			if(connstr==null)throw new Exception("ϵͳ����");
			for(int ii=0;ii<errors.Count;ii++)
			{
				if(errors[ii].Length>0)
					continue;

				string condition=GetUpdateCondition(ii);
				string sqlstr=updateParser.BuildSQL(condition,null);
				IDataAccess dataAccess=DataAccessFactory.Produce(DAType,connstr);
				DataSet ds=dataAccess.BeginUpdate(sqlstr);
				DataTable dt=ds.Tables[0];
				if(!(hasAutoIncrementKey && AppendState(ii)) && dt.Rows.Count>1)//�������������ļ�¼���ܸ���
				{
					dataAccess.EndUpdate(ds);
					string prestr="";
					if(errors[ii].Length>0)
						prestr="\n";
					errors[ii]+=prestr+"�ж������������ļ�¼,���ܸ��£�";
				}
				else if(!(hasAutoIncrementKey && AppendState(ii)) && dt.Rows.Count==1)
				{
					if(bOverWrite)
					{
						DataRow dr=dt.Rows[0];
						dr.BeginEdit();
						foreach(string s in KeyFields)
						{
							if(!this[s].FromUpdateTable)
								continue;
							if(this[s].ReadOnly)
							{
								dt.Columns[this[s].BaseField].ReadOnly=true;
								continue;//ֻ���ֶβ���¼��
							}
							if(this[s].ValueIsNotNull(ii))
							{
								if(this[s].GetStringValue(ii).Length<=0)
									dr[this[s].BaseField]=DBNull.Value;
								else
									dr[this[s].BaseField]=this[s].Values[ii];
							}
						}
						foreach(string s in NormalFields)
						{
							if(!this[s].FromUpdateTable)
								continue;
							if(this[s].Type.IsArray)
							{
								//�����༭��֧���ļ��ֶ�
								continue;
							}
							if(this[s].ReadOnly)
							{
								dt.Columns[this[s].BaseField].ReadOnly=true;
								continue;//ֻ���ֶβ���¼��
							}
							if(this[s].ValueIsNotNull(ii))
							{
								if(this[s].GetStringValue(ii).Length<=0)
									dr[this[s].BaseField]=DBNull.Value;
								else
									dr[this[s].BaseField]=this[s].Values[ii];
							}
						}
						dr.EndEdit();
						dataAccess.EndUpdate(ds);
						if(dataAccess.Error!=null)
						{
							string prestr="";
							if(errors[ii].Length>0)
								prestr="\n";
							errors[ii]+=prestr+"���´���"+dataAccess.Error.Message+"��";
						}
					}
					else
					{
						string prestr="";
						if(errors[ii].Length>0)
							prestr="\n";
						errors[ii]+=prestr+"��¼�Ѿ����ڣ�";
					}
				}
				else
				{
					DataRow dr=dt.NewRow();
					foreach(string s in KeyFields)
					{
						if(!this[s].FromUpdateTable)
							continue;
						if(this[s].ReadOnly)
						{
							dt.Columns[this[s].BaseField].ReadOnly=true;
							continue;//ֻ���ֶβ���¼��
						}
						if(this[s].ValueIsNotNull(ii))
						{
							if(this[s].GetStringValue(ii).Length<=0)
								dr[this[s].BaseField]=DBNull.Value;
							else
								dr[this[s].BaseField]=this[s].Values[ii];
						}
					}
					foreach(string s in NormalFields)
					{
						if(!this[s].FromUpdateTable)
							continue;
						if(this[s].Type.IsArray)
						{
							//�����༭��֧���ļ��ֶ�
							continue;
						}
						if(this[s].ReadOnly)
						{
							dt.Columns[this[s].BaseField].ReadOnly=true;
							continue;//ֻ���ֶβ���¼��
						}
						if(this[s].ValueIsNotNull(ii))
						{
							if(this[s].GetStringValue(ii).Length<=0)
								dr[this[s].BaseField]=DBNull.Value;
							else
								dr[this[s].BaseField]=this[s].Values[ii];
						}
					}
					dt.Rows.Add(dr);
					dataAccess.EndUpdate(ds);
					if(dataAccess.Error!=null)
					{
						string prestr="";
						if(errors[ii].Length>0)
							prestr="\n";
						errors[ii]+=prestr+"��Ӵ���"+dataAccess.Error.Message+"��";
					}
				}	
			}

			int ct=FieldCount;
			if(ct<=0)return "";

			StringWriter sw=new StringWriter();
			XmlTextWriter xmlWriter=new XmlTextWriter(sw);

			xmlWriter.WriteStartElement("DATATABLE");
			bool noError=true;
			for(int ii=0;ii<errors.Count;ii++)
			{
				if(errors[ii].Length==0)
					continue;
				noError=false;
				xmlWriter.WriteStartElement("ROW");
				foreach(string key in SQLFields)
				{
					if(this[key].Type.IsArray)
					{
						//�����༭��֧���ļ��ֶ�
						continue;
					}
					xmlWriter.WriteStartElement(this[key].ID);					
					xmlWriter.WriteString(this[key].GetStringValue(ii));
					xmlWriter.WriteEndElement();
					if(this[key].OldValues[ii]!=null && this[key].OldValues[ii].Length>0)
					{
						xmlWriter.WriteStartElement(this[key].ID+"old");					
						xmlWriter.WriteString(this[key].OldValues[ii]);
						xmlWriter.WriteEndElement();						
					}
				}
				xmlWriter.WriteStartElement("ERROR");					
				xmlWriter.WriteString(errors[ii]);
				xmlWriter.WriteEndElement();

				xmlWriter.WriteEndElement();
			}				

			xmlWriter.WriteEndElement();
			xmlWriter.Close();
			sw.Close();
			if(noError)
				return null;
			else
				return sw.ToString();
		}
		/// <summary>
		/// ��������õĲ�ѯ������ֱ��ʹ�ø��±���ֶ������졣
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		private string GetUpdateCondition(int index)
		{
			string condition="";
			//����ѡ���¼������
			bool nullKey=false;
			string prestr="";
			foreach(string fname in KeyFields)
			{
				if(!this[fname].FromUpdateTable)
					continue;
				string val=this[fname].OldValues[index];
				if(val!=null && val.Length>0)
				{
					if(this[fname].Type==typeof(string))
						condition+=prestr+EscapeName(this[fname].BaseField)+" = '"+EscapeValue(val)+"' ";
					else if(this[fname].Type==typeof(DateTime))
					{
						if(DBType==DataBaseType.ORACLE)
						{							
							condition+=prestr+EscapeName(this[fname].BaseField)+" = to_date('"+val+"','yyyy-MM-dd') ";
						}
						else
						{
							condition+=prestr+EscapeName(this[fname].BaseField)+" = '"+val+"' ";
						}
					}
					else if(this[fname].Type==typeof(bool))
					{							
						if(val=="True")
							condition+=prestr+EscapeName(this[fname].BaseField)+" = 1 ";
						else
							condition+=prestr+EscapeName(this[fname].BaseField)+" = 0 ";
					}
					else
					{
						condition+=prestr+EscapeName(this[fname].BaseField)+" = "+val+" ";
					}
					prestr="and ";
				}
				else
					nullKey=true;
			}
			//���û�йؼ��ֻ����еĹؼ���û��ָ��ֵ����ʹ��ȫ���ֶι����ѯ����
			if(KeyFields.Count<=0 || nullKey)
			{
				foreach(string fname in NormalFields)
				{
					if(!this[fname].FromUpdateTable)
						continue;
					if(this[fname].Type.IsArray)continue;//�������ֶβ�������
					if(this[fname].ShowType==ShowType.FILEUPLOAD || this[fname].ShowType==ShowType.HTMLEDIT || this[fname].ShowType==ShowType.HTMLOUTEREDIT || this[fname].ShowType==ShowType.FILE || this[fname].ShowType==ShowType.HTML)
						continue;//�ļ���HTML�༭���ֶβ�������
					string val=this[fname].OldValues[index];
					if(val!=null && val.Length>0)
					{
						if(this[fname].Type==typeof(string))
							condition+=prestr+EscapeName(this[fname].BaseField)+" = '"+EscapeValue(val)+"' ";
						else if(this[fname].Type==typeof(DateTime))
						{
							if(DBType==DataBaseType.ORACLE)
							{							
								condition+=prestr+EscapeName(this[fname].BaseField)+" = to_date('"+val+"','yyyy-MM-dd') ";
							}
							else
							{
								condition+=prestr+EscapeName(this[fname].BaseField)+" = '"+val+"' ";
							}
						}
						else if(this[fname].Type==typeof(bool))
						{							
							if(val=="True")
								condition+=prestr+EscapeName(this[fname].BaseField)+" = 1 ";
							else
								condition+=prestr+EscapeName(this[fname].BaseField)+" = 0 ";
						}
						else
						{
							condition+=prestr+EscapeName(this[fname].BaseField)+" = "+val+" ";
						}
						prestr="and ";
					}
				}
			}
			return condition;
		}
		/// <summary>
		/// ��������select�Ĳ�ѯ����������ʹ��ԭʼSQL�е��ֶ������������������ʱ��һ����
		/// ���ڸ����ֶ�������������ʱ���ֶΣ����ﹹ����������ܻ�鲻��ʵ�ʵļ�¼�����Ǹ�BUG��
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		private string GetSelectCondition(int index)
		{
			string condition="";
			//����ѡ���¼������
			bool nullKey=false;
			string prestr="";
			foreach(string fname in KeyFields)
			{
				string val=this[fname].OldValues[index];
				if(val!=null && val.Length>0)
				{
					if(this[fname].Type==typeof(string))
						condition+=prestr+EscapeName(this[fname].Alias)+" = '"+EscapeValue(val)+"' ";
					else if(this[fname].Type==typeof(DateTime))
					{
						if(DBType==DataBaseType.ORACLE)
						{							
							condition+=prestr+EscapeName(this[fname].Alias)+" = to_date('"+val+"','yyyy-MM-dd') ";
						}
						else
						{
							condition+=prestr+EscapeName(this[fname].Alias)+" = '"+val+"' ";
						}
					}
					else if(this[fname].Type==typeof(bool))
					{							
						if(val=="True")
							condition+=prestr+EscapeName(this[fname].Alias)+" = 1 ";
						else
							condition+=prestr+EscapeName(this[fname].Alias)+" = 0 ";
					}
					else
					{
						condition+=prestr+EscapeName(this[fname].Alias)+" = "+val+" ";
					}
					prestr="and ";
				}
				else
					nullKey=true;
			}
			//���û�йؼ��ֻ����еĹؼ���û��ָ��ֵ����ʹ��ȫ���ֶι����ѯ����
			if(KeyFields.Count<=0 || nullKey)
			{
				foreach(string fname in NormalFields)
				{
					if(this[fname].Type.IsArray)continue;//�������ֶβ�������
					if(this[fname].ShowType==ShowType.FILEUPLOAD || this[fname].ShowType==ShowType.HTMLEDIT || this[fname].ShowType==ShowType.HTMLOUTEREDIT || this[fname].ShowType==ShowType.FILE || this[fname].ShowType==ShowType.HTML)
						continue;//�ļ���HTML�༭���ֶβ�������
					string val=this[fname].OldValues[index];
					if(val!=null && val.Length>0)
					{
						if(this[fname].Type==typeof(string))
							condition+=prestr+EscapeName(this[fname].Alias)+" = '"+EscapeValue(val)+"' ";
						else if(this[fname].Type==typeof(DateTime))
						{
							if(DBType==DataBaseType.ORACLE)
							{							
								condition+=prestr+EscapeName(this[fname].Alias)+" = to_date('"+val+"','yyyy-MM-dd') ";
							}
							else
							{
								condition+=prestr+EscapeName(this[fname].Alias)+" = '"+val+"' ";
							}
						}
						else if(this[fname].Type==typeof(bool))
						{							
							if(val=="True")
								condition+=prestr+EscapeName(this[fname].Alias)+" = 1 ";
							else
								condition+=prestr+EscapeName(this[fname].Alias)+" = 0 ";
						}
						else
						{
							condition+=prestr+EscapeName(this[fname].Alias)+" = "+val+" ";
						}
						prestr="and ";
					}
				}
			}
			return condition;
		}
		private string uploadFileVPath="../uploadFiles";

		private StringCollection errors=new StringCollection();
		private ArrayList appendStates=new ArrayList();
	}
	public class WebXmlBatchEdit : XmlBatchEdit
	{
		public bool IsMaxLengthShow
		{
			get
			{
				return showLength;
			}
			set
			{
				showLength=value;
			}
		}
		private bool showLength=false;//�Ƿ���ʾ�ַ����ֶ�������
		public WebXmlBatchEdit(string sqlstr,DataAccessType datype,string connstr):base(sqlstr,datype,connstr)
		{}
		/// <summary>
		/// ��ָ�������������Լ���ʼֵ����������HTML���漰�ű�
		/// </summary>
		/// <returns></returns>
		public string GenerateHtml()
		{
			return GenerateHtml(SelectRecord(),false);
		}
		/// <summary>
		/// ָ������������������¼�����HTML���������ݰ󶨣�POST�ű���COPY/PASTE�ű��Լ�¼����ű�
		/// </summary>
		/// <param name="condition"></param>
		/// <returns></returns>
		public string GenerateHtmlByFilter(string condition)
		{
			return GenerateHtml(SelectRecord(condition),false);
		}
		/// <summary>
		/// ָ���������SQL��������¼�����HTML���������ݰ󶨣�POST�ű���COPY/PASTE�ű��Լ�¼����ű�
		/// </summary>
		/// <param name="sqlstr"></param>
		/// <returns></returns>
		public string GenerateHtmlBySql(string sqlstr)
		{
			return GenerateHtml(SelectCompatibleRecord(sqlstr),false);
		}
		/// <summary>
		/// �ύ����¼�룬�����ɴ���У������HTML���������ݰ󶨣�POST�ű���COPY/PASTE�ű��Լ�¼����ű�
		/// </summary>
		/// <param name="overwrite"></param>
		/// <returns></returns>
		public string UpdateRecord(bool overwrite)
		{
			string id="DT"+GUID;
			string xml=HttpContext.Current.Request[id];
			string errXml=UpdateRecord(xml,overwrite);
			if(errXml==null)
				return null;
			return GenerateHtml(errXml,true);
		}
		private string GenerateHtml(string xml,bool post)
		{		
			string id="DT"+GUID;
	
			StringWriter sw=new StringWriter();
			sw.WriteLine(GenerateCssScript());
			sw.WriteLine(GenerateAllInternalScript());			
			sw.WriteLine("<input id='"+id+"' name='"+id+"' type=hidden>");
			sw.WriteLine("<OBJECT id='"+id+"_dso' style='WIDTH:0px;HEIGHT:0px;DISPLAY:none' classid='clsid:184F68E7-DAFA-4C92-B5FF-10880B1F6AA9'></OBJECT>");
			sw.WriteLine("<XML id='"+id+"_xml'>");
			sw.WriteLine(xml);
			sw.WriteLine("</XML>");
			foreach(string s in SQLFields)
			{
				if(this[s].ShowType==ShowType.COMBOBOX)
				{
					string xmlLand=GetClassXml(s);
					sw.WriteLine("<XML id='"+this[s].ID+"_xml'>"+xmlLand+"</XML>");
				}
			}
			sw.WriteLine("<table id='"+id+"_table' class='LanXmlEditTable' onkeydown='javascript:keydownHandle(this)' style='border-collapse:collapse;'><thead><tr>");
			foreach(string s in SQLFields)
			{
				if(this[s].ShowType==ShowType.HIDDEN)
					continue;
				sw.WriteLine("<td class='LanXmlEditHeaderCell' title='"+HttpContext.Current.Server.HtmlEncode(GetFieldDescription(s))+"'>"+this[s].ShowName+"</td>");
			}
			if(post)
				sw.WriteLine("<td class='LanXmlEditHeaderCell'>������Ϣ</td>");
			sw.WriteLine("</tr><tr>");
			foreach(string s in SQLFields)
			{
				if(this[s].ShowType==ShowType.HIDDEN)
					continue;
				sw.WriteLine("<td class='LanXmlEditBodyCell'>"+GetFieldAdditional(s)+"</td>");
			}
			if(post)
				sw.WriteLine("<td class='LanXmlEditBodyCell'></td>");
			sw.WriteLine("</tr></thead><tbody><tr>");
			foreach(string s in SQLFields)
			{
				if(this[s].ShowType==ShowType.HIDDEN)
					continue;
				Type type=this[s].Type;
				if(this[s].ShowType==ShowType.COMBOBOX)
				{
					if(this[s].NotEditable)
					{
						sw.WriteLine("<td class='LanXmlEditBodyCell'>");
						sw.WriteLine("<span style='width:100%'></span>");
						sw.WriteLine("<input type=hidden datafld='"+this[s].ID+"' onpropertychange=\"javascript:notEditableClassChange(this,'"+this[s].ID+"')\" style='border:0px white groove;background-color:transparent;'>");
						sw.WriteLine("</td>");
					}
					else
					{
						NameValueCollection vals=GetClassVals(s);
						HtmlSelect ctrl=new HtmlSelect();
						ctrl.Attributes.Add("datafld",this[s].ID);
						ctrl.Attributes.Add("class","LanSelect");
						ctrl.Style.Add("border","0px white groove");
						if(vals!=null)
						{
							if(this[s].Nullable)
							{
								ctrl.Items.Add(new ListItem("",""));
							}
							foreach(string key in vals.Keys)
							{
								if(this[s].UseClassValue)
								{
									ctrl.Items.Add(new ListItem(vals[key],vals[key].Trim()));
								}
								else
								{
									ctrl.Items.Add(new ListItem(vals[key],key));
								}
							}
						}
						StringWriter selectCtrlCode=new StringWriter();
						ctrl.RenderControl(new HtmlTextWriter(selectCtrlCode));
						sw.WriteLine("<td class='LanXmlEditBodyCell'>"+selectCtrlCode.ToString()+"</td>");
					}
				}
				else if(this[s].ShowType==ShowType.READONLYTEXTBOX || this[s].ShowType==ShowType.READONLYTEXTAREA)
				{
					sw.WriteLine("<td class='LanXmlEditBodyCell'><span datafld='"+this[s].ID+"' style='width:100%'></span></td>");
				}
				else if(type==typeof(string))
				{
					sw.WriteLine("<td class='LanXmlEditBodyCell'><input type=text datafld='"+this[s].ID+"' name='editable_input' onchange='javascript:checkStr(this.value,"+this[s].Length+")' style='border:0px white groove;background-color:transparent;'></td>");
				}
				else if(!type.IsArray && type!=typeof(DateTime))
				{
					sw.WriteLine("<td class='LanXmlEditBodyCell'><input type=text datafld='"+this[s].ID+"' name='editable_input' onchange='javascript:checkNum(this.value)' style='border:0px white groove;background-color:transparent;'></td>");
				}
				else if(type==typeof(DateTime))
				{
					sw.WriteLine("<td class='LanXmlEditBodyCell'><input type=text title='˫��ѡ������' datafld='"+this[s].ID+"' name='editable_input' onchange='javascript:checkDate(this.value)' ondblclick='javascript:calendar()' style='border:0px white groove;background-color:transparent;'></td>");
				}			
				else
					sw.WriteLine("<td class='LanXmlEditBodyCell'><input type=text datafld='"+this[s].ID+"' name='editable_input' style='border:0px white groove;background-color:transparent;'></td>");
			}
			if(post)
				sw.WriteLine("<td class='LanXmlEditBodyCell'><span datafld='ERROR' style='color:red;font-size:14px'></span></td>");
			sw.WriteLine("</tr></tbody></table>");
			sw.WriteLine("<script>");
			sw.WriteLine(@"
					var "+id+@"_cols=new Array(");
			string prestr="";
			foreach(string s in SQLFields)
			{
				sw.WriteLine(prestr+"'"+this[s].ID+"'");
				prestr=",";
				sw.WriteLine(prestr+"'"+this[s].ID+"old'");
				prestr=",";
			}
			if(post)
			{
				sw.WriteLine(prestr+"'ERROR'");
				prestr=",";
			}
			sw.WriteLine(");");
			sw.WriteLine("var "+id+"_datatable=new ActiveXObject('OSPDSO.DataTable');");
			sw.WriteLine("var "+id+"_xmldoc=new ActiveXObject('Msxml2.DOMDocument');");
			sw.WriteLine(@"				
					function initDataBind()
					{
						var t="+id+@"_datatable;
						var dso=document.getElementById('"+id+@"_dso');
						var tb=document.getElementById('"+id+@"_table');
						var dataLand=document.getElementById('"+id+@"_xml');
						if(!dataLand.XMLDocument)
							return;
						var xmlElement=dataLand.XMLDocument.documentElement;
						if(!xmlElement)
							return;
						
						tb.dataSrc='';
						dso.RemoveTable('');
						t.InitUpdatable(true);
						t.BeginArrayInit();
						t.AddColsFromXml(xmlElement,"+id+@"_cols,'//ROW');
						t.EndArrayInit();
						dso.AddTable('',t);
						tb.dataSrc='#"+id+@"_dso';");
			if(post)
			{
				sw.WriteLine(@"if(t.GetRowCount()<=0)
						{
							alert('�༭��¼��ȫ���ɹ����£���ȷ���鿴�༭�������');
							var url=document.location.href;
							if(url.indexOf('?')>0)
								document.location.href=url+'&refresh_time='+new Date();
							else
								document.location.href=url+'?refresh_time='+new Date();								
						}");			
			}
			sw.WriteLine(@"			
					}
					
					function buildPostXml()
					{
						var t="+id+@"_datatable;
						var xmlDoc="+id+@"_xmldoc;
						xmlDoc.async=false;
						xmlDoc.loadXML('<DATATABLE/>');
						for(var i=1;i<=t.GetRowCount();i++)
						{
							var xmlRow=xmlDoc.createElement('ROW');
							xmlDoc.documentElement.appendChild(xmlRow);
							for(var j=1;j<=t.GetColCount();j++)
							{
								var xmlCol=xmlDoc.createElement("+id+@"_cols[j-1]);
								xmlRow.appendChild(xmlCol);
								xmlCol.text=t.GetCellValue(i,j);						
							}
						}
						var postField=document.getElementById('"+id+@"');
						postField.value=xmlDoc.documentElement.xml;
					}
					function copyForExcel()
					{
						var t="+id+@"_datatable;
						var lines=new Array();
						var rowCount=t.GetRowCount();
						var colCount=t.GetColCount();
						for(var i=1;i<=rowCount;i++)
						{
							var line=new Array();
							for(var j=1;j<=colCount;j+=2)
							{
								line.push(t.GetCellValue(i,j));
							}
							lines.push(line.join('	'));
						}
						window.clipboardData.setData('Text',lines.join('\r\n'));
					}
					function pasteFromExcel()
					{
						var t="+id+@"_datatable;
						var content=window.clipboardData.getData('Text');
						if(!content || content=='')
							return;
						var lines=content.split('\r\n');
						var stLine=1;
						var colCount=t.GetColCount();
						for(var i=0;i<lines.length;i++)
						{
							if(lines[i]=='')
								continue;
							var newLine=false;
							if(stLine>t.GetRowCount())
							{
								t.InsertRows(stLine,1);
								newLine=true;
							}
							var line=lines[i].split('	');
							for(var j=0;j<line.length && (j*2+2)<=colCount;j++)
							{
								t.SetCellValue(stLine,j*2+1,line[j]);
								if(newLine)
									t.SetCellValue(stLine,j*2+2,line[j]);//���и��ֶεľ�ֵ����ֵ��ͬ
							}
							stLine++;
						}
					}
					function notEditableClassChange(obj,id)
					{
						if(event.propertyName=='value')
						{	
							var val=obj.value;
							var dataLand=document.getElementById(id+'_xml');
							if(!dataLand.XMLDocument)
								return;
							var xmlElement=dataLand.XMLDocument.documentElement;
							if(!xmlElement)
								return;");
				sw.WriteLine("var xpath='//ROW[CLASSCODE=\"'+val.replace('\"','\\\\\\\"')+'\"]/CLASSVALUE';");
				sw.WriteLine(@"var node=xmlElement.selectSingleNode(xpath);
							if(!node)
								return;
							obj.previousSibling.innerText=node.text;
						}
					}
				");
			sw.WriteLine("</script>");
			sw.Close();
			return sw.ToString();
		}
		private string GetClassXml(string field)
		{
			StringWriter sw=new StringWriter();
			XmlTextWriter xmlWriter=new XmlTextWriter(sw);
			xmlWriter.WriteStartElement("DATACLASS");

			NameValueCollection vals=GetClassVals(field);
			if(vals!=null)
			{
				bool useClassValue=this[field].UseClassValue;
				foreach(string key in vals)
				{
					xmlWriter.WriteStartElement("ROW");

					xmlWriter.WriteStartElement("CLASSVALUE");
					xmlWriter.WriteString(vals[key]);
					xmlWriter.WriteEndElement();

					xmlWriter.WriteStartElement("CLASSCODE");
					if(useClassValue)
						xmlWriter.WriteString(vals[key].Trim());
					else
						xmlWriter.WriteString(key);
					xmlWriter.WriteEndElement();

					xmlWriter.WriteEndElement();
				}
			}

			xmlWriter.WriteEndElement();
			xmlWriter.Close();
			sw.Close();
			return sw.ToString();
		}
		/// <summary>
		/// �õ�һ�����������ص�����������ֵ����е�λ���õ�λ����
		/// </summary>
		/// <param name="f"></param>
		/// <returns></returns>
		public string GetFieldAdditional(string f)
		{
			if(this[f]==null)
				return "";
			if(this[f].ShowType==ShowType.NOVALUE || this[f].ShowType==ShowType.RAWVALUE 
				|| this[f].ShowType==ShowType.FILE || this[f].ShowType==ShowType.HTML)
			{
				return "";
			}
			else
			{
				string nullable="";
				if(!this[f].Nullable)
				{
					nullable="[����]";
				}
				string type="";
				if(this[f].Unit!=null && this[f].Unit.Trim().Length>0)
				{
					type=this[f].Unit.Trim();
				}
				string length="";
				if(this[f].Type==typeof(string) && (this[f].ShowType==ShowType.TEXTBOX || this[f].ShowType==ShowType.TEXTAREA))
				{
					if(type.Length<=0)
						type="����";
					if(IsMaxLengthShow)
						length="[�ֽ���<="+this[f].Length+"]";
				}
				return type+nullable+length;
			}
		}
		/// <summary>
		/// �õ�һ���������
		/// </summary>
		/// <param name="f"></param>
		/// <returns></returns>
		public string GetFieldDescription(string f)
		{
			if(this[f]==null)
				return "";
			if(this[f].ShowType==ShowType.HIDDEN || this[f].ShowType==ShowType.READONLYTEXTBOX || this[f].ShowType==ShowType.READONLYTEXTAREA 
				|| this[f].ShowType==ShowType.NOVALUE || this[f].ShowType==ShowType.RAWVALUE 
				|| this[f].ShowType==ShowType.FILE || this[f].ShowType==ShowType.HTML)
			{
				return "";
			}
			else
			{
				string desc=this[f].Description;
				if(desc==null)
					desc="";
				else
					desc=desc.Trim();
				return desc;
			}
		}
		public string GenerateCssScript()
		{
			return "<LINK href='"+CssUrl+"' type='text/css' rel='stylesheet'>";
		}
		/// <summary>
		/// ��������,���ɳ���׼�¼�������������ű�.
		/// </summary>
		/// <returns></returns>
		private string GenerateAllInternalScript()
		{
			string retVal="<script src='"+ScriptUrl+"'></script>\r\n"+GenerateEditCursorScript()+GenerateNumCheckScript()+GenerateStrCheckScript()+GeneratorDateCheckScript();
			return retVal;
		}
		private string GenerateEditCursorScript()
		{
			string retVal=@"
		<script language='javascript'>
			<!--
				function keydownHandle(tb)
				{
					//37 �� 38 �� 39 �� 40 �� 13 �س� 27 ESC 16 SHIFT 17 CTRL 18 ALT
					//33 PgUp 34 PgDn 36 Home 35 End
					var obj=event.srcElement;
					var t=obj.tagName;
					if(t!='INPUT')
						return;					
					var keyCode=event.keyCode;
					if(keyCode==13 || keyCode==34)
					{
						var inputs=document.getElementsByName('editable_input');
						var len=inputs.length;
						for(var i=0;i<inputs.length-1;i++)
						{
							if(inputs[i]==obj)
							{
								inputs[i+1].select();
								break;
							}
						}
						event.cancelBubble=true;
						event.returnValue=false;
					}
					else if(keyCode==27 || keyCode==33)
					{
						var inputs=document.getElementsByName('editable_input');
						var len=inputs.length;
						for(var i=1;i<inputs.length;i++)
						{
							if(inputs[i]==obj)
							{
								inputs[i-1].select();
								break;
							}
						}
						event.cancelBubble=true;
						event.returnValue=false;
					}
					else if(keyCode==38)
					{
						var cell=obj.parentElement;
						var row=obj.parentElement.parentElement;
						if(row.rowIndex>tb.tHead.rows.length)
						{
							//Ŀǰ�ı���췽��ȷ��û����������,���Կ��԰�ȫ��ʹ��rowIndex,cellIndex,�������������,����ʹ����
							var nextObj=tb.rows[row.rowIndex-1].cells[cell.cellIndex].firstChild;
							nextObj.select();
						}
						event.cancelBubble=true;
						event.returnValue=false;
					}
					else if(keyCode==40)
					{
						var cell=obj.parentElement;
						var row=obj.parentElement.parentElement;
						if(row.rowIndex<tb.rows.length-1)
						{
							//Ŀǰ�ı���췽��ȷ��û����������,���Կ��԰�ȫ��ʹ��rowIndex,cellIndex,�������������,����ʹ����
							var nextObj=tb.rows[row.rowIndex+1].cells[cell.cellIndex].firstChild;
							nextObj.select();
						}
						event.cancelBubble=true;
						event.returnValue=false;
					}
				}
			-->
		</script>
			";
			return retVal;
		}
	}
}
