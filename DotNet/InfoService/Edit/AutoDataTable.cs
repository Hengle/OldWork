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
	public delegate byte[] ReadField(int size);
	public delegate void WriteField(byte[] content);
	/// <summary>
	/// ���ڸ�����д�ļ��ֶε���,��ǰ�����ǳߴ������ļ�,���������Ҫ����,����ǰ�����˼·,
	/// Ӧ���趨��д�¼�,�����ṩ���ֶζ�����������д�ص�����.
	/// </summary>
	public sealed class FileField
	{
		private FileField()
		{}
		public static void GenFilePath(string path,out string subPath1,out string subPath2)
		{
			//����Ҫ������Ŀ¼
			subPath1=DateTime.Now.ToString("yyyy-MM");
			if(!Directory.Exists(path))
				Directory.CreateDirectory(path);
			//Ѱ�����ʹ�õ�Ŀ¼���ߵ����Ŀ¼�ļ�������100ʱ�����µ���Ŀ¼
			if(!Directory.Exists(path+"\\"+subPath1))
				Directory.CreateDirectory(path+"\\"+subPath1);
			subPath2="1";
			if(!Directory.Exists(path+"\\"+subPath1+"\\"+subPath2))
			{
				Directory.CreateDirectory(path+"\\"+subPath1+"\\"+subPath2);
			}
			for(int i=1;;i++)
			{
				if(!Directory.Exists(path+"\\"+subPath1+"\\"+i))
				{
					int fileCount=Directory.GetFiles(path+"\\"+subPath1+"\\"+(i-1)).Length;
					if(fileCount>=100)
					{
						subPath2=""+i;
					}
					else
					{
						subPath2=""+(i-1);
					}
					break;
				}
			}
			if(!Directory.Exists(path+"\\"+subPath1+"\\"+subPath2))
			{
				Directory.CreateDirectory(path+"\\"+subPath1+"\\"+subPath2);
			}
		}
		public static string GenCacheFilePath(string path,string fileName)
		{
			if(!Directory.Exists(path))
				Directory.CreateDirectory(path);
			if(!Directory.Exists(path+"\\CacheFiles"))
				Directory.CreateDirectory(path+"\\CacheFiles");
			return path+"\\CacheFiles\\"+fileName;
		}
		public static string Content(byte[] content,string virtualPath)
		{
			int size=content.Length;
			if(size>256)
			{
				int len=0;
				for(;len<256;len++)
				{
					if(content[len]==(byte)0)break;
				}
				//������һ���ڳ����޸��ر����IIS�������������»���ס����֪���Ǻ�ԭ����ȫ�־�̬�����йأ���
				string fileName=System.Text.Encoding.GetEncoding(54936).GetString(content,0,len);
				string filePath=HttpContext.Current.Server.MapPath(virtualPath);
				string fullName=GenCacheFilePath(filePath,fileName);
				FileInfo fi=new FileInfo(fullName);
				if(!fi.Exists || fi.Length!=size-256)
				{	
					FileStream fs=null;
					if(File.Exists(fullName))
					{
						fs=File.OpenWrite(fullName);
					}
					else
					{
						fs=File.Create(fullName);
					}						
					using(fs)
					{
						fs.Write(content,256,size-256);
						fs.Close();
					}
				}
				return "<a href='"+virtualPath+"/CacheFiles/"+fileName+"' target='_blank'>"+Path.GetFileNameWithoutExtension(fileName)+"</a>";
			}
			return "<a href='about:blank' target='_blank'>Can't Read File!</a>";
		}
		public static byte[] Content(string fileNameWithoutExtension,string virtualPath)
		{
			string filePath=HttpContext.Current.Server.MapPath(virtualPath);
			FileInfo fi=new FileInfo(filePath);
			if(fi.Exists)
			{
				FileStream fs=File.OpenRead(filePath);
				byte[] content=new byte[fi.Length+256];
				byte[] fileName=System.Text.Encoding.GetEncoding(54936).GetBytes(fileNameWithoutExtension);
				for(int i=0;i<256;i++)
				{
					if(i<fileName.Length)
					{
						content[i]=fileName[i];
					}
					else
						content[i]=(byte)0;
				}
				fs.Read(content,256,(int)fi.Length);
				fs.Close();
				return content;
			}
			return null;
		}

		public static event ReadField ReadField;
		public static event WriteField WriteField;
		public static string ReadBigContent(string filePath,string virtualPath)
		{
			return "<a href='about:blank' target='_blank'>Can't Read File!</a>";
		}
		public static void WriteBigContent(string fileNameWithoutExtension,string virtualPath)
		{
			string filePath=HttpContext.Current.Server.MapPath(virtualPath);
		}
	}
	/// <summary>
	/// �����������ʾ�ؼ���ʽ
	/// </summary>
	public enum ShowType:int
	{
		TEXTBOX,
		TEXTAREA,
		COMBOBOX,
		CHECKBOX,
		FILEUPLOAD,
		HTMLEDIT,
		HTMLOUTEREDIT,
		DATETIME,
		READONLYTEXTBOX,
		READONLYTEXTAREA,
		HIDDEN,
		FILE,
		HTML,
		NOVALUE,
		RAWVALUE
	}
	public enum LayoutStyle:int
	{
		DEFAULT
	}
	/// <summary>
	/// ����һ���������Ϣ,��ͬ�����ݲ�ı���,��������������ʾ��Ϣ��
	/// </summary>
	public class TableFieldBase
	{
		public static string ValueToString(object val,string format)
		{
			if(val==null || val==DBNull.Value)
				return "";
			else if(format!=null)
			{
				if(val is float)
					return ((float)val).ToString(format);
				else if(val is decimal)
					return ((decimal)val).ToString(format);
				else if(val is double)
					return ((double)val).ToString(format);
				else if(val is DateTime)
				{
					DateTime dt=(DateTime)val;
					return dt.ToString(format);
				}
				else
					return val.ToString().Trim();
			}
			else
			{			
				if(val is float)
					return ((float)val).ToString("###########0.########");
				else if(val is decimal)
					return ((decimal)val).ToString("###########0.########");
				else if(val is double)
					return ((double)val).ToString("###########0.########");
				else if(val is DateTime)
				{
					DateTime dt=(DateTime)val;
					if(dt.Hour==0 && dt.Minute==0 && dt.Second==0)
						return dt.ToString("yyyy-MM-dd");
					else if(dt.Year==0 && dt.Month==0 && dt.Day==0)
						return dt.ToString("hh:mm:ss");
					else
						return dt.ToString("yyyy-MM-dd hh:mm:ss");
				}
				else
					return val.ToString().Trim();
			}
		}

		public string ID=null;
		public string Name=null;
		public Type Type=null;
		public int Length=0;
		public bool Nullable=true;
		public string BaseTable=null;
		public string BaseField=null;
		public string Alias=null;
		public bool ReadOnly=false;
		public Encoding Encoding=null;

		public string ShowName=null;
		public string Unit=null;
		public string Description=null;
		public string Format=null;
		public int Indent=0;
		public ShowType ShowType=ShowType.TEXTBOX;
		/// <summary>
		/// �����Ƿ�ʹ�÷����������ֶ����ֶ�ֵ,����ʹ�÷����ı������ֶ�ֵ.
		/// ������ʾʼ��ʹ������.
		/// </summary>
		public bool UseClassValue=true;
		public bool FromUpdateTable=false;

		public string GetIndentString()
		{
			if(Indent>6)
				Indent=6;
			if(Indent<0)
				Indent=0;
			return indentString.Substring(0,Indent*singleIndent.Length);
		}
		private static string singleIndent="&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
		private static string indentString="&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
	}
	/// <summary>
	/// ����һ���������Ϣ,��ͬ�����ݲ�ı���,��������������ʾ��Ϣ��
	/// </summary>
	public class TableField : TableFieldBase
	{
		public bool ComboBoxAsCheckBox=true;//����ѡ���๹��Ϊ���ѡ����һ���Ĳ�ѯ����,���Բ�ѯ����Ĺ�����Ч,�༭���治ʹ������
		public bool IsGroup=false;//�Ƿ����������
		public StatisticFieldType StatMethod=StatisticFieldType.AUTO;//ͳ�Ʒ���
		public int SortOrder=0;//�������,0����������
		public string WhereClause=null;//where�������ʽ,��������

		public object Value=null;//�ֶ�ֵ,�������ݿ����
		public string OldValue=null;//�ֶ�ԭʼֵ,�������ݿ����ʱ����������

		public bool ValueIsNotNull()
		{
			if(Value==null || Value==DBNull.Value)
				return false;
			return true;
		}
		public string GetStringValue()
		{
			return ValueToString(Value,null);
		}
		public string GetFormatValue()
		{
			return ValueToString(Value,Format);
		}
		public TableField Clone()
		{
			TableField tf=new TableField();
			tf.ID=ID;
			tf.Name=Name;
			tf.Type=Type;
			tf.Length=Length;
			tf.Nullable=Nullable;
			tf.BaseTable=BaseTable;
			tf.BaseField=BaseField;
			tf.Alias=Alias;
			tf.ReadOnly=ReadOnly;
			tf.Encoding=Encoding;

			tf.ShowName=ShowName;
			tf.Unit=Unit;
			tf.Description=Description;
			tf.Format=Format;
			tf.Indent=Indent;
			tf.ShowType=ShowType;
			tf.UseClassValue=UseClassValue;
			tf.FromUpdateTable=FromUpdateTable;

			tf.ComboBoxAsCheckBox=ComboBoxAsCheckBox;
			tf.IsGroup=IsGroup;
			tf.StatMethod=StatMethod;
			tf.SortOrder=SortOrder;
			tf.WhereClause=WhereClause;

			tf.Value=Value;
			tf.OldValue=OldValue;
			return tf;
		}
	}
	/// <summary>
	/// �û��ṩ����ֵ�ķ��࣬�����ķ������һ������/ֵ�б�
	/// </summary>
	public class NameValueClass
	{
		public NameValueClass(NameValueCollection nameVals)
		{
			classes=nameVals;
		}
		public NameValueCollection GetClasses()
		{
			return classes;
		}
		private NameValueCollection classes;
	}
	/// <summary>
	/// ��SQL���Ľ����������ֵ�ķ��࣬��ͬ�ڷ�����ṩ���ݵķ��࣬������SQL���
	/// ֻ�ṩһ�������ֵ��
	/// </summary>
	public class SQLSourceClass
	{
		public SQLSourceClass(string valCol,string _codeCol,string _sql,DataAccessType _daType,string connStr)
		{
			valueCol=valCol;
			codeCol=_codeCol;
			sql=_sql;
			daType=_daType;
			connectionString=connStr;
		}
		public NameValueCollection GetClasses()
		{
			IDataAccess dataAccess=DataAccessFactory.Produce(daType,connectionString);
			DataSet myDataSet=dataAccess.ExecuteDataSet(sql);
			DataTable myTable=myDataSet.Tables[0];
			NameValueCollection retVal=new NameValueCollection();
			foreach(DataRow dr in myTable.Rows)
			{
				if(dr[codeCol]==null || dr[codeCol]==DBNull.Value || dr[valueCol]==null || dr[valueCol]==DBNull.Value)
					continue;
				//�����������ȥ��ǰ��ո�,������ֵ����ǰ��Ŀո��������˹����η���(���ֲ�ν�������ʾ����)
				retVal[dr[codeCol].ToString().Trim()]=dr[valueCol].ToString().TrimEnd(null);
			}
			if(dataAccess.Error!=null)
				throw new Exception("��ѯ�������"+dataAccess.Error.Message);
			return retVal;
		}
		private string valueCol;
		private string codeCol;
		private string sql;
		private DataAccessType daType;
		private string connectionString;
	}
	/// <summary>
	/// ��׼���࣬ʹ��ָ����SQL��ѯ������ṩ�������ֵ��
	/// </summary>
	public class StandardClasses
	{
		public StandardClasses(string valCol,string _codeCol,string _filterCol,string _sql,DataAccessType _daType,string connStr)
		{
			valueCol=valCol;
			codeCol=_codeCol;
			filterCol=_filterCol;
			parser=new SQLSelectParser(_sql);
			daType=_daType;
			connectionString=connStr;
		}
		public NameValueCollection GetClasses(string name)
		{			
			IDataAccess dataAccess=DataAccessFactory.Produce(daType,connectionString);
			string sql=parser.BuildSQL(filterCol+" = '"+name+"'",null);
			DataSet myDataSet=dataAccess.ExecuteDataSet(sql);
			DataTable myTable=myDataSet.Tables[0];
			NameValueCollection retVal=new NameValueCollection();
			foreach(DataRow dr in myTable.Rows)
			{
				if(dr[codeCol]==null || dr[codeCol]==DBNull.Value || dr[valueCol]==null || dr[valueCol]==DBNull.Value)
					continue;
				//�����������ȥ��ǰ��ո�,������ֵ����ǰ��Ŀո��������˹����η���(���ֲ�ν�������ʾ����)
				retVal[dr[codeCol].ToString().Trim()]=dr[valueCol].ToString().TrimEnd(null);
			}
			if(dataAccess.Error!=null)
				throw new Exception("��ѯ�������"+dataAccess.Error.Message);
			return retVal;
		}
		private string valueCol;
		private string codeCol;
		private string filterCol;
		private SQLSelectParser parser;
		private DataAccessType daType;
		private string connectionString;
	}
	/// <summary>
	/// ��ȡ��һ��SQL��ص���Ϣ���������ڲ�ѯ����µ���Ϣ����SQL��������Զ��ű�ʱ��SQLInfo��������һ�ű�Ϊ���ݹ��������Ϣ��
	/// Ĭ�ϵ�ѡ��SQL������ֶ������ĵ�һ�������ǵ�����ѯ�ɸ��µı�ͨ��ֻ��������ϸ�ı�KeyFieldsֻȡ���±���ص��ֶΣ����ֶ�����Ե�һ�γ���Ϊ׼����
	/// </summary>
	public class SQLInfo : DataAccessInfo
	{
		#region ����
		public TableFieldBase this[string s]
		{
			get
			{
				return tableFields[s] as TableFieldBase;
			}
			set
			{
				tableFields[s]=value;
			}
		}
		public int FieldCount
		{
			get
			{
				return tableFields.Count;
			}
		}
		public StringCollection SQLFields
		{
			get
			{
				return sqlFields;
			}
		}
		public StringCollection KeyFields
		{
			get
			{
				return keyFields;
			}
		}
		public StringCollection NormalFields
		{
			get
			{
				return normalFields;
			}
		}
		public StringCollection AutoIncrementFields
		{
			get
			{
				return autoIncrementFields;
			}
		}
		public string UpdateTableName
		{
			get
			{
				return updateTableName;
			}
		}
		public LayoutStyle Layout
		{
			get
			{
				return layoutStyle;
			}
			set
			{
				layoutStyle=value;
			}
		}		
		public string GUID
		{
			get{return guid;}
		}
		public string EditIcon
		{
			get
			{
				return editIcon;
			}
			set
			{
				editIcon=value;
			}
		}
		public string BrowseIcon
		{
			get
			{
				return browseIcon;
			}
			set
			{
				browseIcon=value;
			}
		}
		public string FilterIcon
		{
			get
			{
				return filterIcon;
			}
			set
			{
				filterIcon=value;
			}
		}
		public string OpenIcon
		{
			get
			{
				return openIcon;
			}
			set
			{
				openIcon=value;
			}
		}
		public string CloseIcon
		{
			get
			{
				return closeIcon;
			}
			set
			{
				closeIcon=value;
			}
		}
		public string FileUploadUrl
		{
			get
			{
				return fileUploadUrl;
			}
			set
			{
				fileUploadUrl=value;
			}
		}
		public string HtmlEditUrl
		{
			get
			{
				return htmlEditUrl;
			}
			set
			{
				htmlEditUrl=value;
			}
		}
		/// <summary>
		/// HTML�༭���������ļ�Ŀ¼
		/// </summary>
		public string HtmlEditorConfigFile
		{
			get
			{
				return htmlEditorConfigFile;
			}
			set
			{
				htmlEditorConfigFile=value;
			}
		}		
		public string CssUrl
		{
			get
			{
				return cssUrl;
			}
			set
			{
				cssUrl=value;
			}
		}
		public string ScriptUrl
		{
			get
			{
				return scriptUrl;
			}
			set
			{
				scriptUrl=value;
			}
		}
		public int MaxSelectItems
		{
			get
			{
				return maxSelectItems;
			}
			set
			{
				maxSelectItems=value;
			}
		}
		public int MaxFieldWidth
		{
			get
			{
				return maxFieldWidth;
			}
			set
			{
				maxFieldWidth=value;
			}
		}
		public int TextAreaRows
		{
			get
			{
				return textAreaRows;
			}
			set
			{
				textAreaRows=value;
			}
		}
		public int CheckBoxesCols
		{
			get
			{
				return checkBoxesCols;
			}
			set
			{
				checkBoxesCols=value;
			}
		}
		public int QueryCheckBoxesCols
		{
			get
			{
				return queryCheckBoxesCols;
			}
			set
			{
				queryCheckBoxesCols=value;
			}
		}
		public int MaxCheckBoxesRows
		{
			get
			{
				return maxCheckBoxesRows;
			}
			set
			{
				maxCheckBoxesRows=value;
			}
		}
		public int MaxSort
		{
			get
			{
				return maxSort;
			}
			set
			{
				maxSort=value;
			}
		}
		public bool UseSorts
		{
			get
			{
				return useSorts;
			}
			set
			{
				useSorts=value;
			}
		}
		public bool UseGroups
		{
			get
			{
				return useGroups;
			}
			set
			{
				useGroups=value;
			}
		}
		#endregion
		/// <summary>
		/// �趨�����õķ������ơ�
		/// </summary>
		/// <param name="refcol"></param>
		/// <param name="className"></param>
		/// <param name="showtype"></param>
		public void SetFieldClass(string refcol,string className,ShowType showtype)
		{
			if(this[refcol]==null)
				return;
			//�趨�����еı༭�ؼ���ʽ
			if(showtype!=ShowType.COMBOBOX && showtype!=ShowType.CHECKBOX)
				this[refcol].ShowType=ShowType.COMBOBOX;
			else
				this[refcol].ShowType=showtype;
			//�趨���չ�ϵ
			this.classCol_ClassName[refcol]=className;
		}
		/// <summary>
		/// �����еķ���洢��ʽ,�Ǵ洢�����ֵ���Ǳ���,ȱʡ�洢ֵ(useClassValue����Ϊtrue)
		/// </summary>
		/// <param name="refcol"></param>
		/// <param name="useClassValue"></param>
		public void SetFieldClassUsage(string refcol,bool useClassValue)
		{
			if(this[refcol]==null)
				return;
			this[refcol].UseClassValue=useClassValue;
		}
		/// <summary>
		/// ����һ�����࣬����ֵ�ɣ����룬ֵ�����룬ֵ��...)�ṩ��
		/// </summary>
		/// <param name="name"></param>
		/// <param name="nameVals"></param>
		public void SetClass(string name,params string[] nameVals)
		{
			NameValueCollection vals=new NameValueCollection();
			for(int i=0;i<nameVals.Length-1;i+=2)
			{
				string c=nameVals[i];
				string v=nameVals[i+1];
				if(c==null || v==null)
					continue;
				vals[c]=v;
			}
			nameValueClasses[name]=new NameValueClass(vals);
		}
		/// <summary>
		/// ����һ�����࣬����ֵ������(����)/ֵ���ṩ��
		/// </summary>
		/// <param name="name"></param>
		/// <param name="vals"></param>
		public void SetClass(string name,NameValueCollection vals)
		{
			nameValueClasses[name]=new NameValueClass(vals);
		}
		/// <summary>
		/// ����һ�����࣬����ֵ��SQL����ṩ�����еĴ���/ֵ�ֶ��ɲ���ָ����
		/// </summary>
		/// <param name="name"></param>
		/// <param name="valCol"></param>
		/// <param name="codeCol"></param>
		/// <param name="sqlstr"></param>
		public void SetClass(string name,string valCol,string codeCol,string sqlstr)
		{
			sqlSourceClasses[name]=new SQLSourceClass(valCol,codeCol,sqlstr,DAType,ConnectionString);
		}
		/// <summary>
		/// ����һ�����࣬����ֵ��SQL����ṩ�����еĴ���/ֵ�ֶ��ɲ���ָ����SQL���ʵ����ݿ�Ҳ�ɲ���ָ����
		/// </summary>
		/// <param name="name"></param>
		/// <param name="valCol"></param>
		/// <param name="codeCol"></param>
		/// <param name="sqlstr"></param>
		/// <param name="datype"></param>
		/// <param name="connstr"></param>
		public void SetClass(string name,string valCol,string codeCol,string sqlstr,DataAccessType datype,string connstr)
		{
			sqlSourceClasses[name]=new SQLSourceClass(valCol,codeCol,sqlstr,datype,connstr);
		}
		/// <summary>
		/// ���ñ�׼����������Ϣ,ʹ�����ѯ��ͬ�������ᴮ���ʷ�������
		/// </summary>
		/// <param name="valCol"></param>
		/// <param name="_codeCol"></param>
		/// <param name="_filterCol"></param>
		/// <param name="_sql"></param>
		public void SetStandardClass(string valCol,string _codeCol,string _filterCol,string _sql)
		{
			standardClasses=new StandardClasses(valCol,_codeCol,_filterCol,_sql,DAType,ConnectionString);
		}
		/// <summary>
		/// ���ñ�׼�������
		/// </summary>
		/// <param name="sc"></param>
		public void SetStandardClass(StandardClasses sc)
		{
			standardClasses=sc;
		}
		/// <summary>
		/// �趨�����ʾ��
		/// </summary>
		/// <param name="f"></param>
		/// <param name="showname"></param>
		public void SetFieldShowName(string f,string showname)
		{
			if(this[f]==null)
				return;
			this[f].ShowName=showname;
		}
		/// <summary>
		/// ������ĵ�λ
		/// </summary>
		/// <param name="f"></param>
		/// <param name="unit"></param>
		public void SetFieldUnit(string f,string unit)
		{
			if(this[f]==null)
				return;
			this[f].Unit=unit;
		}
		/// <summary>
		/// �����������
		/// </summary>
		/// <param name="f"></param>
		/// <param name="description"></param>
		public void SetFieldDescription(string f,string description)
		{
			if(this[f]==null)
				return;
			this[f].Description=description;
		}
		/// <summary>
		/// ���������ʾ��ʽ
		/// </summary>
		/// <param name="f"></param>
		/// <param name="format"></param>
		public void SetFieldFormat(string f,string format)
		{
			if(this[f]==null)
				return;
			this[f].Format=format;
		}
		/// <summary>
		/// �����������
		/// </summary>
		/// <param name="f"></param>
		/// <param name="indent"></param>
		public void SetFieldIndent(string f,int indent)
		{
			if(this[f]==null)
				return;
			this[f].Indent=indent;
		}
		/// <summary>
		/// ���������ʾ��ʽ��
		/// </summary>
		/// <param name="fieldname"></param>
		/// <param name="showType"></param>
		public void SetFieldShowType(string fieldname,ShowType showType)
		{
			if(this[fieldname]==null)
				return;
			this[fieldname].ShowType=showType;
		}
		public void SetFieldFile(string fieldname)
		{
			if(this[fieldname]==null)
				return;
			this[fieldname].ShowType=ShowType.FILEUPLOAD;
		}
		/// <summary>
		/// �趨һ��HTML�༭��
		/// </summary>
		/// <param name="fieldname"></param>
		/// <param name="useOuterEditor"></param>
		public void SetFieldHtmlEdit(string fieldname,bool useOuterEditor)
		{
			if(this[fieldname]==null)
				return;
			if(useOuterEditor)
				this[fieldname].ShowType=ShowType.HTMLOUTEREDIT;
			else
				this[fieldname].ShowType=ShowType.HTMLEDIT;
		}
		/// <summary>
		/// �趨������
		/// </summary>
		/// <param name="fieldname"></param>
		public void SetFieldHidden(string fieldname)
		{
			if(this[fieldname]==null)
				return;	
			this[fieldname].ShowType=ShowType.HIDDEN;
		}
		/// <summary>
		/// �趨ֻ����
		/// </summary>
		/// <param name="fieldname"></param>
		public void SetFieldReadOnly(string fieldname)
		{
			if(this[fieldname]==null)
				return;
			if(this[fieldname].ShowType==ShowType.NOVALUE 
				|| this[fieldname].ShowType==ShowType.RAWVALUE
				|| this[fieldname].ShowType==ShowType.FILE
				|| this[fieldname].ShowType==ShowType.HTML
				|| this[fieldname].ShowType==ShowType.FILEUPLOAD
				|| this[fieldname].ShowType==ShowType.HTMLEDIT
				|| this[fieldname].ShowType==ShowType.HTMLOUTEREDIT
				|| this[fieldname].ShowType==ShowType.HIDDEN)
				return;
			if(this[fieldname].ShowType==ShowType.TEXTAREA)
				this[fieldname].ShowType=ShowType.READONLYTEXTAREA;
			else
				this[fieldname].ShowType=ShowType.READONLYTEXTBOX;
		}
		public void SetFieldRawValueOnly(string fieldname)
		{
			if(this[fieldname]==null)
				return;
			if(this[fieldname].ShowType==ShowType.FILEUPLOAD)
				this[fieldname].ShowType=ShowType.FILE;
			else if(this[fieldname].ShowType==ShowType.HTMLEDIT || this[fieldname].ShowType==ShowType.HTMLOUTEREDIT)
				this[fieldname].ShowType=ShowType.HTML;
			else if(this[fieldname].ShowType!=ShowType.HIDDEN && this[fieldname].ShowType!=ShowType.NOVALUE)
				this[fieldname].ShowType=ShowType.RAWVALUE;
		}
		public void SetFieldNameOnly(string fieldname)
		{
			if(this[fieldname]==null)
				return;
			this[fieldname].ShowType=ShowType.NOVALUE;
		}
		public void SetFieldEncoding(string fieldname,Encoding encoding)
		{
			if(this[fieldname]==null)
				return;
			if(this[fieldname].Type==typeof(string) || this[fieldname].Type.IsArray)//ֻ���ַ����ֶ���������ֶβ���Ҫָ������
				this[fieldname].Encoding=encoding;
		}
		public void SetUpdateTableName(string tbname)
		{			
			updateTableName=tbname;
			//�ֱ��������ͨ��
			KeyFields.Clear();
			NormalFields.Clear();

			if(tbname!=null)
			{
				DataTable dt=GetFieldsSchema("select * from "+tbname+" where 1=2");
				//����������Ϣ,��Ϊ����SQL������ѯSQL���Ľ�����ֶ������ܲ�ͬ���п����б�����,����ֻ��һ��һ����ƥ��				
				DataRow[] columns=dt.Select("","ColumnOrdinal");
				foreach(DataRow dr in columns)
				{
					string ttt = dr["ColumnName"].ToString();
					foreach(string s in SQLFields)
					{
						if(this[s].BaseField==ttt && this[s].BaseTable!=null)//��Դ��ͬ��ԭʼ������ʶ�����Դ��ģ���һ���ǹ����ֶΣ�
						{
							this[s].BaseTable=dr["BaseTableName"].ToString();
							this[s].Nullable=(bool)dr["AllowDBNull"];//ȡ���Ƿ�ɿ�
							this[s].Alias=parser.FieldName(this[s].Name,this[s].BaseTable,this[s].BaseField);
							//��Ӽ���
							if((bool)dr["IsKey"])
							{
								if(KeyFields.IndexOf(s)<0)
									KeyFields.Add(s);
							}
							break;
						}
					}
				}
			}
			//ȡ��ͨ��
			foreach(string s in SQLFields)
			{
				if(KeyFields.IndexOf(s)<0)
					NormalFields.Add(s);
			}
			//ʶ��������������־
			hasAutoIncrementKey=false;
			foreach(string s in KeyFields)
			{
				if(this[s].BaseTable==tbname && AutoIncrementFields.IndexOf(s)>=0)
				{
					hasAutoIncrementKey=true;
					break;
				}
			}
			//�����е�ֻ����Ϣ
			foreach(string s in KeyFields)
			{
				if(tbname==null || this[s].BaseTable!=tbname)
				{
					this[s].FromUpdateTable=false;
					SetFieldReadOnly(s);
				}
				else
				{
					this[s].FromUpdateTable=true;

				}
			}
			foreach(string s in NormalFields)
			{
				if(tbname==null || this[s].BaseTable!=tbname)
				{
					this[s].FromUpdateTable=false;
					SetFieldReadOnly(s);
				}
				else
				{
					this[s].FromUpdateTable=true;
				}
			}
			//�������SQL������
			if(tbname!=null)
				updateParser=new SQLSelectParser("select * from "+tbname);
		}
		public string BuildSelectSQL(string where,string having,params string[] sorts)
		{
			return parser.BuildSQL(where,having,sorts);
		}
		public string BuildUpdateSQL(string where,string having,params string[] sorts)
		{
			return updateParser.BuildSQL(where,having,sorts);
		}
		public static string BuildSQL(string sql,string where,string having,params string[] sorts)
		{
			SQLSelectParser sqlParser=new SQLSelectParser(sql);
			return sqlParser.BuildSQL(where,having,sorts);
		}
		public SQLInfo(string sqlstr,DataAccessType datype,string connstr):base(datype,connstr)
		{
			sqlFields=new StringCollection();
			keyFields=new StringCollection();
			normalFields=new StringCollection();
			autoIncrementFields=new StringCollection();
						
			guid=((uint)sqlstr.GetHashCode()).ToString();
			originalSql=sqlstr;
			parser=new SQLSelectParser(sqlstr);

			string tempTableName=null;

			string newSql=parser.BuildSQL("1=2",null);//����һ�������ؼ�¼�Ľ����,�����ڻ�ȡ��ͷ��Ϣ
			
			//ȷ��SQL���select�Ӿ��г��ֵ���
			IDataAccess dataAccess=DataAccessFactory.Produce(datype,connstr);
			DataSet ds=dataAccess.ExecuteDataSet(newSql);
			if(dataAccess.Error!=null)
				throw dataAccess.Error;
			DataTable sqlTable=ds.Tables[0];
			foreach(DataColumn dc in sqlTable.Columns)
			{
				if(sqlFields.IndexOf(dc.ColumnName)<0)
					sqlFields.Add(dc.ColumnName);
			}
			DataTable dt=GetFieldsSchema(newSql);
			DataRow[] columns=dt.Select("","ColumnOrdinal");
			//ȡ������Ϣ
			int order=0;
			foreach(DataRow dr in columns)
			{
				string colName=dr["ColumnName"].ToString();
				if(tableFields[colName]!=null || sqlFields.IndexOf(colName)<0)//�ظ��л򲻳�����SQL���select�Ӿ��е�������
					continue;
				TableFieldBase tf=CreateField();
				tf.ID="DT"+GUID+"_"+order.ToString();//�趨��ʾID
				order++;
				tf.Name=colName;
				tf.ShowName=colName;

				tf.Type=dr["DataType"] as Type;//ȡ������
				tf.Nullable=(bool)dr["AllowDBNull"];//ȡ���Ƿ�ɿ�
				tf.BaseTable=dr["BaseTableName"] as String;//�����Եı�
				tf.BaseField=dr["BaseColumnName"] as String;//�е�ԭʼ��
				try
				{
					if(DBType!=DataBaseType.ORACLE)
					{						
						bool autoIncrement=(bool)dr["IsAutoIncrement"];//������������ֻ����
						if(autoIncrement)
						{
							tf.ShowType=ShowType.HIDDEN;
							tf.ReadOnly=true;
							AutoIncrementFields.Add(colName);
						}
						else
							tf.ReadOnly=false;
					}
				}
				catch(Exception)
				{
					tf.ReadOnly=false;
				}
				try
				{
					if(!tf.ReadOnly)
						tf.ReadOnly=(bool)dr["IsReadOnly"];//���Ƿ�ֻ����
				}
				catch(Exception)
				{
					tf.ReadOnly=false;
				}
				if(tf.BaseTable!=null && tf.BaseField!=null)
				{
					tf.Alias=parser.FieldName(tf.Name,tf.BaseTable,tf.BaseField);
				}
				else
				{
					tf.Alias=parser.FieldName(tf.Name,null,null);
				}
				int len=0;
				object o=dr["ColumnSize"];//ȡ�г���
				if(o is System.DBNull)
				{
					len=0;
				}
				else
				{
					len=(int)Convert.ChangeType(o,typeof(int));
				}
				tf.Length=len;//����
				//��Ҫȷ����ʾ����
				if(tf.ShowType!=ShowType.TEXTBOX)
				{
					//ǰ���Ѿ���������ʾ����,������������
				}
				else if(tf.Type.IsArray)
				{
					//����Լ��image�ֶ�һ���Ǵ���ļ��ģ�����ͷ256���ֽڴ洢�ļ�������һԼ����ȻӰ�����ݿ����ơ�
					tf.ShowType=ShowType.FILEUPLOAD;
				}
				else if(tf.Type==typeof(System.DateTime))
				{
					tf.ShowType=ShowType.DATETIME;	
				}
				else if(len>0x7fff)
				{
					//���봿����ַ����ֶ�Ӧ�ò�������0x7fff���ַ���ô�������Ա���������Ӧ�þ���text�ֶΣ�
					//����Լ��text�ֶ�һ���Ǵ洢HTML�ġ���������Ӧ�ÿ�������������ݿ⡣
					tf.ShowType=ShowType.HTMLEDIT;
				}
				else if(len>0xff)
				{
					tf.ShowType=ShowType.TEXTAREA;
				}
				else
				{
					tf.ShowType=ShowType.TEXTBOX;
				}
				//�����ַ������ļ���Ĭ�ϱ��뷽ʽ
				if(tf.Type==typeof(string))
				{
					tf.Encoding=Encoding.GetEncoding("GB2312");
				}
				else if(tf.Type.IsArray)
				{
					tf.Encoding=Encoding.UTF8;
				}
				//���ò���ֵ�ķ������
				if(tf.Type==typeof(bool))
				{
					SetFieldClass(tf.Name,"������",tf.ShowType);
				}
				//����ʶ����ĸ������Ԫ������Ϣ
				if(tf.BaseTable!=null && tf.BaseField!=null && tempTableName==null)
				{
					tempTableName=tf.BaseTable;
				}
				tableFields.Add(tf.Name,tf);
			}
			//���Ƕ��ʽHTML�༭���ĸ���,�������2�����Ϊ�ⲿHTML�༭��
			int htmlEditorCount=0;
			foreach(string s in sqlFields)
			{
				if(this[s].ShowType==ShowType.HTMLEDIT)
					htmlEditorCount++;
			}
			if(htmlEditorCount>2)
			{
				foreach(string s in sqlFields)
				{
					if(this[s].ShowType==ShowType.HTMLEDIT)
						this[s].ShowType=ShowType.HTMLOUTEREDIT;
				}
			}	
			SetUpdateTableName(tempTableName);//ȫ���ж�����ٸ�ֵ�Ա�֤��ؼ�����ȷ		
		}
		~SQLInfo()
		{
			SQLFields.Clear();
			KeyFields.Clear();
			NormalFields.Clear();
			AutoIncrementFields.Clear();

			tableFields.Clear();
			classCol_ClassName.Clear();
			nameValueClasses.Clear();
			sqlSourceClasses.Clear();
		}
		protected virtual TableFieldBase CreateField()
		{
			return new TableFieldBase();
		}
		public bool HaveGroupBy()
		{
			return parser.HaveGroupBy();			
		}
		public override string ToString()
		{
			string t="<p>[�ؼ��ֶ��б�:]</p>  ";
			foreach(object s in KeyFields)
				t+="<p>"+s+" ";
			t+="<p>[��ͨ�ֶ��б�:]</p>  ";
			foreach(object s in NormalFields)
				t+="<p>"+s+" </p>";
			return t;
		}
		public NameValueCollection GetClassVals(string colname)
		{
			if(this[colname]==null)
				return null;
			string className=classCol_ClassName[colname] as string;
			if(className==null || className.Trim().Length<=0)
				return null;
			NameValueClass _class1=nameValueClasses[className] as NameValueClass;
			if(_class1!=null)
				return _class1.GetClasses();
			SQLSourceClass _class2=sqlSourceClasses[className] as SQLSourceClass;
			if(_class2!=null)
				return _class2.GetClasses();
			return standardClasses.GetClasses(className);
		}
		/// <summary>
		/// ָ��ʹ��Ȩȱʡ�ķ����ṹ��ָ����Ϣ��ṹ,������ЩԪ������Ϣ���Ե�ǰҪ���ʵ����ݿ�
		/// </summary>
		public void SetDefaultMetaInfo()
		{
			SetDefaultClassTable(DAType,ConnectionString);
			SetDefaultGuideInfo(DAType,ConnectionString);
		}
		/// <summary>
		/// ָ��ʹ��ȱʡ�ķ����ṹ,�ҷ��������ڲ���ָ�������ݿ���
		/// </summary>
		/// <param name="daType"></param>
		/// <param name="connStr"></param>
		public void SetDefaultClassTable(DataAccessType daType,string connStr)
		{
			string sqlstr="select ��������,����,����,��� from ����_����";
			IDataAccess dataAccess=DataAccessFactory.Produce(daType,connStr);
			IDataReader myReader=dataAccess.ExecuteDataReader(sqlstr);
			try
			{
				if(myReader==null && !alreadyCheckClassTable)
				{//����Ԫ���ݱ�
					alreadyCheckClassTable=true;
					object o=dataAccess.ExecuteScalar("select count(*) from ����_����");
					if(o!=null && o!=DBNull.Value)
					{
						int recNo=(int)o;
						if(recNo<=0)
						{
							//���������û�м�¼����ɾ��
							string dropDDL="DROP TABLE ����_����";
							dataAccess.ExecuteNonQuery(dropDDL);
						}
						else
						{
							throw new Exception("����_�����Ľṹ�뵱ǰҪ�󲻷�,�����Ѿ����ڲ����м�¼,������������ִ�б����������µĸ���_�����!");
						}
					}
					//�����±�
					string sqlDDL=@"
					CREATE TABLE ����_���� 
					(
						�������� varchar (255) NOT NULL ,
						���� varchar (255) NOT NULL ,
						���� varchar (255) NULL ,
						��� int NULL ,
						CONSTRAINT PK_����_���� PRIMARY KEY (��������,����)
					)
					";
					dataAccess.ExecuteNonQuery(sqlDDL);
					myReader=dataAccess.ExecuteDataReader(sqlstr);
					if(myReader==null)
					{
						//ERROR
					}
				}
				SetClass("������","True","True","False","False");
				standardClasses=new StandardClasses("����","����","��������"," select * from ����_���� order by ���",daType,connStr);
			}
			finally
			{
				if(myReader!=null)
					myReader.Close();
			}
		}
				
		/// <summary>
		/// ָ��ʹ��ȱʡ��ָ����Ϣ��ṹ,��ָ����Ϣ������ڲ���ָ�������ݿ���
		/// </summary>
		/// <param name="daType"></param>
		/// <param name="connStr"></param>
		public void SetDefaultGuideInfo(DataAccessType daType,string connStr)
		{
			foreach(DictionaryEntry de in tableFields)
			{
				TableFieldBase tf=de.Value as TableFieldBase;
				//�趨��ʾ��
				GuideInfo gi=new GuideInfo(daType,connStr);
				Guide guide=gi.GetGuide(tf.BaseTable,tf.Name);				
				tf.ShowName=guide.Name;//��ʾ��	
				tf.Unit=guide.Unit;
				tf.Description=guide.Description;
				if(guide.ShowAttr==ShowAttr.HIDDEN)
				{
					tf.ShowType=ShowType.HIDDEN;
				}
				else if(guide.ShowAttr==ShowAttr.COMBOBOX || guide.ShowAttr==ShowAttr.USECODECOMBOBOX)
				{
					tf.ShowType=ShowType.COMBOBOX;
					if(guide.ShowAttr==ShowAttr.USECODECOMBOBOX)
						tf.UseClassValue=false;
				}
				else if(guide.ShowAttr==ShowAttr.CHECKBOX || guide.ShowAttr==ShowAttr.USECODECHECKBOX)
				{
					tf.ShowType=ShowType.CHECKBOX;
					if(guide.ShowAttr==ShowAttr.USECODECHECKBOX)
						tf.UseClassValue=false;
				}
				else if(guide.ShowAttr==ShowAttr.FILE)
				{
					tf.ShowType=ShowType.FILEUPLOAD;
				}
				else if(guide.ShowAttr==ShowAttr.HTML)
				{
					tf.ShowType=ShowType.HTMLEDIT;
				}
				else if(guide.ShowAttr==ShowAttr.OUTEREDITHTML)
				{
					tf.ShowType=ShowType.HTMLOUTEREDIT;
				}
				else if(guide.ShowAttr==ShowAttr.NOVALUE)
				{
					tf.ShowType=ShowType.NOVALUE;
				}
				else if(guide.ShowAttr==ShowAttr.RAWVALUE)
				{
					tf.ShowType=ShowType.RAWVALUE;
				}
				tf.Indent=guide.Indent;
				if(guide.Format!=null && guide.Format.Trim().Length>0)
				{
					string str=guide.Format.Trim();
					if(tf.ShowType==ShowType.COMBOBOX || tf.ShowType==ShowType.CHECKBOX)
					{
						if(str.StartsWith("SQL@"))
						{
							str=str.Substring(4);
							int si=str.IndexOf(':');
							if(si>0)
							{
								string head=str.Substring(0,si);
								string sql=str.Substring(si+1);

								string[] args=head.Split('@');
								if(args.Length==3)
								{
									SetClass(args[0],args[1],args[2],sql);
									SetFieldClass(tf.Name,args[0],tf.ShowType);
								}
							}
						}
						else
							SetFieldClass(tf.Name,str,tf.ShowType);
					}
					else
						tf.Format=str;
				}
			}
		}

		
		#region �û��ӿڿͻ��˽ű�
		protected string GeneratePositionScript()
		{
			return @"
		<script>	
			//��ȡ���Ե�topֵ			function getAbsoluteTop(elem,win)
			{
				var topPosition = 0;
				while (elem)
				{
					if (elem.tagName == 'BODY')
					{
						break;
					}
					topPosition += elem.offsetTop;
					elem = elem.offsetParent;
				}
				if(win && win.frameElement)
				{		
					topPosition+=getAbsoluteTop(win.frameElement,win.parent);
				}
				return topPosition;
			}
			//��ȡ���Ե�left
			function getAbsoluteLeft(elem,win)
			{
				var leftPosition = 0;
				while (elem)
				{
					if (elem.tagName == 'BODY')
					{
						break;
					}
					leftPosition += elem.offsetLeft;
					elem = elem.offsetParent;
				}
				if(win && win.frameElement)
				{
					leftPosition+=getAbsoluteLeft(win.frameElement,win.parent);
				}
				return leftPosition;
			}
		</script>
				";
		}
		protected string GenerateSearchSelectScript()
		{
			return @"
		<script language='javascript'>
			<!--
			function filterClass(id)
			{
				var closeObj=document.getElementById(id+'_close');
				var classListObj=document.getElementById(id+'_div');
				var filterObj=document.getElementById(id+'_filter');
				var listObj=document.getElementById(id+'_list');
				for(var i=0;i<listObj.rows.length;i++)
				{
					var cellObj=listObj.rows[i].cells[0];
					if(!cellObj)continue;
					if(cellObj.innerText.indexOf(filterObj.value)>=0)
					{
						cellObj.style.display='';
					}
					else
					{
						cellObj.style.display='none';
					}
				}
			}
			function openClass(id)
			{
				var openObj=document.getElementById(id+'_open');
				openObj.style.display='none';
				var findObj=document.getElementById(id+'_find');
				findObj.style.display='';
				var closeObj=document.getElementById(id+'_close');
				closeObj.style.display='';
				var classListObj=document.getElementById(id+'_div');

				var filterObj=document.getElementById(id+'_filter');
				filterObj.value='';
				/*
				var al=getAbsoluteLeft(filterObj,null);
				var at=getAbsoluteTop(filterObj,null);
				classListObj.style.pixelLeft=al;
				classListObj.style.pixelTop=at+filterObj.offsetHeight;
				*/
				classListObj.style.display='';
			}
			function closeClass(id)
			{
				var openObj=document.getElementById(id+'_open');
				openObj.style.display='';
				var findObj=document.getElementById(id+'_find');
				findObj.style.display='none';
				var closeObj=document.getElementById(id+'_close');
				closeObj.style.display='none';
				var classListObj=document.getElementById(id+'_div');
				classListObj.style.display='none';

				var filterObj=document.getElementById(id+'_filter');
				var listObj=document.getElementById(id+'_list');
				if(listObj.selObj)
				{
					if(listObj.selObj.innerText=='---��---')
						filterObj.value='';
					else
						filterObj.value=listObj.selObj.innerText.replace(/^\s*/,'').replace(/%s*$/,'');
				}
			}
			function selectClass(id)
			{
				var srcObj=event.srcElement;
				if(srcObj.tagName!='TD')return;
				var filterObj=document.getElementById(id+'_filter');
				var listObj=document.getElementById(id+'_list');
				var valObj=document.getElementById(id);
				if(srcObj.innerText=='---��---')
					filterObj.value='';
				else
					filterObj.value=srcObj.innerText.replace(/^\s*/,'').replace(/%s*$/,'');
				valObj.value=srcObj.val;
				if(listObj.selObj)
					listObj.selObj.className='LanSelectItemCell';
				listObj.selObj=srcObj;		
				listObj.selObj.className='LanSelectItemCellSelect';						
			}
			-->
		</script>
				";
		}
		protected string GenerateSearchCheckBoxesScript()
		{
			return @"
		<script language='javascript'>
			<!--
			function filterMultiClass(id)
			{
				var classListObj=document.getElementById(id+'_div');
				classListObj.style.display='';
				var filterObj=document.getElementById(id+'_filter');
				var listObj=document.getElementById(id+'_list');
				//�ָ�����ʼ����
				for(var i=0;i<listObj.rows.length;i++)
				{
					var tempRow=listObj.rows[i];
					for(var j=1;j<tempRow.cells.length;j++)
					{
						var cellObj=tempRow.cells[j];
						if(!cellObj)continue;
						var oi=cellObj.orgRow;
						var oj=cellObj.orgCell;
						
						if(oi>=0 && oj>=1 && (oi!=i || oj!=j))
						{
							//alert(''+i+','+j+'<->'+oi+','+oj);
							cellObj.swapNode(listObj.rows[oi].cells[oj]);
						}
					}
				}
				//���ı�Ĳ���
				var curRow=0,curCell=1;
				for(var i=0;i<listObj.rows.length;i++)
				{
					var tempRow=listObj.rows[i];
					for(var j=1;j<tempRow.cells.length;j++)
					{
						var cellObj=tempRow.cells[j];
						if(!cellObj)continue;
						if(cellObj.val.indexOf(filterObj.value)>=0)
						{
							cellObj.style.display='';
							if(curRow!=i || curCell!=j)
							{
								cellObj.orgRow=i;
								cellObj.orgCell=j;
								var tCell=listObj.rows[curRow].cells[curCell];
								tCell.orgRow=curRow;
								tCell.orgCell=curCell;
								//alert(''+i+','+j+'<->'+curRow+','+curCell);
								tCell.swapNode(cellObj);
							}
							if(curCell<listObj.rows[curRow].cells.length-1)
								curCell++;
							else if(curRow<listObj.rows.length-1)
							{
								curRow++;
								curCell=1;
							}
							else
							{
							}
						}
						else
						{
							cellObj.style.display='none';
						}
					}
				}
			}
			function openMultiClass(id)
			{
				var openObj=document.getElementById(id+'_open');
				openObj.style.display='none';
				var findObj=document.getElementById(id+'_find');
				findObj.style.display='';
				var closeObj=document.getElementById(id+'_close');
				closeObj.style.display='';
				var classListObj=document.getElementById(id+'_div');

				var filterObj=document.getElementById(id+'_filter');
				filterObj.value='';
				/*
				var al=getAbsoluteLeft(filterObj,null);
				var at=getAbsoluteTop(filterObj,null);
				classListObj.style.pixelLeft=al;
				classListObj.style.pixelTop=at+filterObj.offsetHeight;
				*/
				classListObj.style.display='';
			}
			function closeMultiClass(id,ct)
			{
				var openObj=document.getElementById(id+'_open');
				openObj.style.display='';
				var findObj=document.getElementById(id+'_find');
				findObj.style.display='none';
				var closeObj=document.getElementById(id+'_close');
				closeObj.style.display='none';
				var classListObj=document.getElementById(id+'_div');
				classListObj.style.display='none';
				
				var filterObj=document.getElementById(id+'_filter');
				var prestr='',vals='';
				for(var i=0;i<ct;i++)
				{
					var checkObj=document.getElementById(id+'_'+i);
					if(checkObj.checked)
					{
						vals+=prestr+checkObj.parentElement.val;
						prestr=',';
					}
				}
				filterObj.value=vals;
			}
			function cancelSelect(id,ct)
			{
				for(var i=0;i<ct;i++)
				{
					var checkObj=document.getElementById(id+'_'+i);
					checkObj.checked=false;
				}
			}
			-->
		</script>
				";
		}

		protected string GenerateFileUploadScript()
		{
			string retVal=@"
		<script language='javascript'>
			<!--
				function addLinkToParent(obj,fileUrl,docName)
				{
					if(obj)
					{
						var pobj=obj.parentElement;
						if(pobj)
						{
							var link=document.createElement('a');
							link.href=fileUrl;
				"+
				"			if(docName.length>"+(MaxFieldWidth+1)+")\r\n"+
				"			{\r\n"+
				"				link.innerText=docName.substr(0,"+MaxFieldWidth+")+'...';\r\n"+
				"			}\r\n"+
				@"
							else
								link.innerText=docName;
							link.title=docName;
							link.target='_blank';
							obj.value='<a target=\'_blank\' href=\''+fileUrl+'\' title=\''+docName+'\'>'+link.innerText+'</a>';							
							var cobj=pobj.firstChild;
							if(cobj)
							{
								if(cobj.tagName=='A')
									cobj.replaceNode(link);
								else
									pobj.insertBefore(link,cobj);
							}
							else
								pobj.appendChild(link);
						}
					}
				}
				function selectDoc(strtag)
				{	x=window.event.screenX;
					y=window.event.screenY;
					w=screen.availWidth;
					h=screen.availHeight;
					if(x>w-270)x=w-270;
					if(y>h-140)y=h-140;
				"+
				"	window.open('"+FileUploadUrl+"',strtag,'left='+x+',top='+y+',height=100,width=270,status=no,toolbar=no,menubar=no,location=no');\r\n"+
				@"
				}
			-->
		</script>
			";
			return retVal;
		}

		protected string GenerateHtmlEditorScript()
		{
			string retVal=@"
		<script language='javascript'>
			<!--
				function addHtmlLinkToParent(obj)
				{
					if(obj)
					{
						var pobj=obj.parentElement;
						if(pobj)
						{
							var link=document.createElement('a');
							link.href='javascript:openHtml(\''+obj.id+'\')';
							link.innerText='�鿴����';
							link.title='��˲鿴HTML�ֶε�����...';
							var cobj=pobj.firstChild;
							if(cobj)
							{
								if(cobj.tagName=='A')
									cobj.replaceNode(link);
								else
									pobj.insertBefore(link,cobj);
							}
							else
								pobj.appendChild(link);
						}
					}
				}
				function openHtml(id)
				{
					try
					{
						var obj=document.getElementById(id);
						if(!obj)return;
						var w=window.open('about:blank','HTMLVIEW','resizable=yes,scrollbars=yes');						
						w.document.write(obj.value);
					}
					catch(eee)
					{
						alert('��������������ֹ�˵������ڣ��޷��鿴HTML���ݣ�');
					}
				}
				function editHtml(strtag)
				{	
					x=window.event.screenX;
					y=window.event.screenY;
					w=screen.availWidth;
					h=screen.availHeight;
					if(x>w-570)x=w-570;
					if(y>h-440)y=h-440;
				"+
				"	window.open('"+HtmlEditUrl+"',strtag,'left='+x+',top='+y+',height=400,width=570,status=no,toolbar=no,menubar=no,location=no,resizable=yes,scrollbars=yes');\r\n"+
				@"
				}
			-->
		</script>
			";
			return retVal;
		}

		protected string GenerateStrCheckScript()
		{
			string retVal=@"
		<script language='javascript'>
			<!--
				function checkStr(str,l)
				{
					var ll=str.length;
					if(ll>l)alert(str+'---�ı�����,���Ȳ�����'+l+'!');
				}
			-->
		</script>
			";
			return retVal;
		}

		protected string GenerateNumCheckScript()
		{
			string retVal=@"
		<script language='javascript'>
			<!--
				function checkNum(num)
				{
					var pt=/([0-9]*).([0-9]*)/;
					var r=num.match(pt);
					var l=num.length;
					if(l>0)
					{
						if(r.index!=0||r.lastIndex!=l)alert(num+'---��ֵ��ʽ����!');
					}
				}
			-->
		</script>
			";
			return retVal;
		}			
		
		protected string GeneratorDateCheckScript()
		{
			return @"
		<script language='javascript' >
			<!--
				function checkDate(val)
				{  if(val=='')return;
					exp=new RegExp('^\\s*(\\d{4})(?:[-])(\\d{1,2})(?:[-])(\\d{1,2})\\s*$');
					m=val.match(exp);
					if(m==null)
					{
						alert(val+'---���ڸ�ʽ����,��ʽӦΪ:yyyy-MM-dd.');
						return;
					}
					var year=parseInt('0'+RegExp.$1,10);
					var month=parseInt('0'+RegExp.$2,10);
					var day=parseInt('0'+RegExp.$3,10);
					if(year<1)
					{
						alert(val+'---��ݴ�!');
						return;
					}
					switch(month)
					{
						case 1:
						case 3:
						case 5:
						case 7:
						case 8:
						case 10:
						case 12:
							if(day<1 || day>31)
							{
								alert(val+'---���ڴ�!');
								return;
							}
							break;
						case 4:
						case 6:
						case 9:
						case 11:
							if(day<1 || day>30)
							{
								alert(val+'---���ڴ�!');
								return;
							}
							break;
						case 2:
							{
								if(  (year % 100!=0 && year%4==0)
									||
									(year%100==0 &&year%400==0) )
								{
									if(day<1|| day>29)
									{
										alert(val+'---���ڴ�!');
										return;
									}
								}
								else
								{
									if(day<1 || day>28)
									{
										alert(val+'---���ڴ�!');
										return;
									}
								}
							}
							break;
						default:
							alert(val+'---�·ݴ�!');
					}
				}
			-->
		</script>
			";
		}
		#endregion
		protected SQLSelectParser parser=null;//SQL������
		protected SQLSelectParser updateParser=null;//SQL������
		protected HybridDictionary tableFields=new HybridDictionary(true);//������Ϣ,���ô�Сд�����еıȽ��Ա��ڲ���		
		protected bool hasAutoIncrementKey=false;//��ǰ���Ƿ����������ֶ�������

		private string originalSql=null;//ԭʼSQL
		private string updateTableName=null;//���²�����Ŀ�����
		private LayoutStyle layoutStyle;//������ʽ
		private string guid;//ΨһID
		private StringCollection sqlFields;//SQL��������е��ֶ�
		private StringCollection keyFields;//�����б�
		private StringCollection normalFields;//��ͨ���б�
		private StringCollection autoIncrementFields;//���������б�

		private HybridDictionary classCol_ClassName=new HybridDictionary(true);//�в��յķ�����,���ô�Сд�����еıȽ��Ա��ڲ���
		private HybridDictionary nameValueClasses=new HybridDictionary(true);//��/ֵ�Լ��Ϲ��ɵķ����,���ô�Сд�����еıȽ��Ա��ڲ���
		private HybridDictionary sqlSourceClasses=new HybridDictionary(true);//SQL��������ɵķ����,���ô�Сд�����еıȽ��Ա��ڲ���
		private StandardClasses standardClasses=null;
		
		private bool alreadyCheckClassTable=false;
		//
		private string editIcon="../images/edit.gif";
		private string browseIcon="../images/browse.gif";
		private string filterIcon="../images/filter.gif";
		private string closeIcon="../images/close.gif";
		private string openIcon="../images/open.gif";

		private string fileUploadUrl="UpLoad.aspx";
		private string htmlEditUrl="HtmlEditor.aspx";
		private string htmlEditorConfigFile="DreamanHtmlEditor/s_coolblue/";
		private string cssUrl="../html/TreeTable.css";
		private string scriptUrl="../html/calendar.js";

		private int maxSelectItems=16;

		private int maxFieldWidth=8;
		private int textAreaRows=3;
		private int checkBoxesCols=2;
		private int queryCheckBoxesCols=2;
		private int maxCheckBoxesRows=2;
		private int maxSort=10;

		private bool useSorts=false;//�Ƿ�������ʱ���û�ָ����ؼ�������
		private bool useGroups=false;//�Ƿ�������ʱ���û�ָ���������(��Ҫ����ͳ��)
	}
	/// <summary>
	/// Summary description for DynamicTable.
	/// �ṩΪ��Ӧ���ݱ����ɽ���������Ϣ��
	/// ���״̬Ĭ��Ϊ�棬��Select�����ò��ҷ��ּ�¼ʱ�Żᱻ���óɼ٣����ȷ����Ҫ�޸Ķ�������ӣ�
	/// ���ߵ���Select����ֱ������AppendState��ָ����InputControl���޸�״̬�ڳ���ʱ��������ָ���ģ���
	/// </summary>
	public class DynamicTable : SQLInfo
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
		public new TableField this[string fieldName]
		{
			get
			{
				return tableFields[fieldName] as TableField;
			}
			set
			{
				tableFields[fieldName]=value;
			}
		}
		/// <summary>
		/// ָ����ǰ��ʾ�ļ�¼�Ƿ���ȷ�Ĵ������״̬���ڻ�ȡ����ֵʱ��ȷ�Ĵ������״̬�ļ�¼����¼��ֵ���Ǿ�ֵ
		/// </summary>
		public bool AppendState
		{
			get
			{
				return appendState;
			}
			set
			{
				appendState=value;
			}
		}
		#endregion		
		/// <summary>
		/// ����һ��COMBOBOX�ֶεĲ�ѯ���湹�취��ָ���Ƿ���Ϊ��CHECKBOXһ���Ĳ�ѯ����(�������ѡ��һ�������Ӧ�������ѡ)��
		/// </summary>
		/// <param name="refcol"></param>
		/// <param name="asCheckBox"></param>
		public void SetFieldComboBoxUsage(string refcol,bool asCheckBox)
		{
			if(this[refcol]==null)
				return;
			this[refcol].ComboBoxAsCheckBox=asCheckBox;
		}
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
		/// <summary>
		/// ������ĳ�ʼֵ��
		/// </summary>
		/// <param name="fieldname"></param>
		/// <param name="val"></param>
		public void SetFieldInitVal(string fieldname,string val)
		{
			if(this[fieldname]==null)
				return;
			this[fieldname].Value=val.Trim();
			this[fieldname].OldValue=val.Trim();
		}
		
		public DynamicTable(string sqlstr,DataAccessType datype,string connstr):base(sqlstr,datype,connstr)
		{}
		protected override TableFieldBase CreateField()
		{
			return new TableField();
		}

		public void SelectRecord()
		{
			string condition=GetSelectCondition();
			SelectRecord(condition);
		}		
		public void UpdateRecord(bool bOverWrite)
		{
			string ttt=CheckInputValue();
			if(ttt!=null&&ttt.Length>0)
			{
				throw new Exception(ttt);
			}
			UpdateHelper(bOverWrite);			
		}
		public void DeleteRecord()
		{
			string condition=GetUpdateCondition();
			string sqlstr=updateParser.BuildSQL(condition,null);
			string connstr=ConnectionString;
			if(connstr==null)throw new Exception("ϵͳ����");
			IDataAccess dataAccess=DataAccessFactory.Produce(DAType,connstr);
			DataSet ds=dataAccess.BeginUpdate(sqlstr);
			DataTable dt=ds.Tables[0];
			if(dt.Rows.Count>1)//�������������ļ�¼����ɾ��
			{
				dataAccess.EndUpdate(ds);
			}
			else if(dt.Rows.Count==1)
			{
				DataRow dr=dt.Rows[0];
				foreach(string s in NormalFields)
				{
					if(this[s].Type.IsArray)continue;//�ļ��洢�����ݿ�������Ҫɾ���ļ�
					if(this[s].ShowType==ShowType.FILEUPLOAD || this[s].ShowType==ShowType.FILE)
					{
						string link=dr[this[s].BaseField] as string;
						if(link!=null && link.Trim().Length>0)
						{
							XmlDocument xmlDoc=new XmlDocument();
							xmlDoc.LoadXml("<?xml version='1.0' ?>"+link);
							string url=xmlDoc.DocumentElement.GetAttribute("href");
							string docName=xmlDoc.DocumentElement.InnerText;
							string path=HttpContext.Current.Server.MapPath(url);
							File.Delete(path);
						}
					}
				}
				dt.Rows[0].Delete();
				dataAccess.EndUpdate(ds);
			}
			if(dataAccess.Error!=null)
				throw new Exception("ɾ������"+dataAccess.Error.Message);
		}
		protected void SelectRecord(string condition)
		{
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
							//ע�⣺��ǰ�ٶ����ݿⲻ�洢�������ļ��������Ҫ֧�ֶ������ļ���Ӧ�޸���δ��롣
							//�������Ƶ��뷨�Ƕ������ļ���洢���ļ�������Ϣ���������ȡ��Щ��Ϣ�������
							//�ܻ�ȡ�ļ����ݵ�URL���󣬲������һ��HTML���ӷ��ص��ͻ��ˣ���Ϊ���ֶε�ֵ��ʾ��
							int i=myReader.GetOrdinal(key);
							if(!myReader.IsDBNull(i))
							{
								long size=myReader.GetBytes(i,0,null,0,0);
								byte[] content=new byte[size];
								myReader.GetBytes(i,0,content,0,(int)size);
								this[key].Value=FileField.Content(content,UploadFileVirtualPath);
							}
							else
								this[key].Value=null;
						}
						else
						{
							this[key].Value=myReader[key];
						}
					}
					if(myReader.Read())//���ڶ����¼ʱ��δ���ֵ�ֶ�д��ֵ
					{
						foreach(string key in SQLFields)
						{
							if(this[key].OldValue==null || this[key].OldValue.Trim().Length<=0)
								this[key].Value=null;
						}
						AppendState=true;//�޷�ȷ����¼������Ҳ������ȷ�����״̬�Դ�
					}				
					else
					{
						//����ѯ����ֵд���ֵ�����ڸ���
						foreach(string key in SQLFields)
						{
							if(this[key].ValueIsNotNull())
								this[key].OldValue=this[key].GetStringValue();
							else
								this[key].OldValue=null;
						}
						AppendState=false;//������ȷ�����״̬��
					}
				}
				else
				{
					AppendState=true;
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
		/// <summary>
		/// ���¼�����ݵ���Ч��,��ǰֻ��鲻ӦΪ�յ���,���Կ��Ǽ��϶�����,��ֵ,�ַ����������ݵļ��.
		/// </summary>
		/// <returns></returns>
		/// 
		protected string CheckInputValue()
		{
			string retVal="",prestr="";
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
				string val=this[key].GetStringValue();
				bool nullable=this[key].Nullable;
				if(!nullable && (val==null || val.Trim().Length<=0))
				{
					retVal+=prestr+this[key].ShowName+"---����Ϊ�գ�";
					prestr="\n";
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
							retVal+=prestr+this[key].ShowName+"---�ı���������ǰ�ֽ�����"+byteLen+" > ��������ֽ�����"+this[key].Length+"��ͨ��һ������ռ2���ֽڣ�";
							prestr="\n";
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
							retVal+=prestr+this[key].ShowName+"---���ڸ�ʽ����ӦΪyyyy-MM-dd��";						
							prestr="\n";
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
							retVal+=prestr+this[key].ShowName+"---��ֵ��ʽ����";
							prestr="\n";
						}
					}
					else
					{	
						int byteLen=this[key].Encoding.GetByteCount(val);
						if(byteLen>this[key].Length)
						{
							retVal+=prestr+this[key].ShowName+"---�ı���������ǰ�ֽ�����"+byteLen+" > ��������ֽ�����"+this[key].Length+"��ͨ��һ������ռ2���ֽڣ�";
							prestr="\n";
						}
					}
				}
			}			
			return retVal;
		}
		protected void UpdateHelper(bool bOverWrite)
		{			
			string condition=GetUpdateCondition();
			string sqlstr=updateParser.BuildSQL(condition,null);
			string connstr=ConnectionString;
			if(connstr==null)throw new Exception("ϵͳ����");
			IDataAccess dataAccess=DataAccessFactory.Produce(DAType,connstr);
			DataSet ds=dataAccess.BeginUpdate(sqlstr);
			DataTable dt=ds.Tables[0];
			if(!(hasAutoIncrementKey && AppendState) && dt.Rows.Count>1)//�������������ļ�¼���ܸ���
			{
				dataAccess.EndUpdate(ds);
				throw new Exception("�ж������������ļ�¼,���ܸ��£�");
			}
			else if(!(hasAutoIncrementKey && AppendState) && dt.Rows.Count==1)
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
						if(this[s].ValueIsNotNull())
						{
							if(this[s].GetStringValue().Length<=0)
								dr[this[s].BaseField]=DBNull.Value;
							else
								dr[this[s].BaseField]=this[s].Value;
						}
					}
					foreach(string s in NormalFields)
					{
						if(!this[s].FromUpdateTable)
							continue;
						if(this[s].Type.IsArray)
						{
							//��ǰ����ƿ����ǵ�ǰ�ֶ�ֵ��һ��HTML���ӣ��ݴ˿����ҵ��ļ����ڴ�
							//��ȡ�ļ����ݲ�д�����ݿ�
							if(this[s].ValueIsNotNull())
							{
								string link=this[s].GetStringValue();
								if(link.Length<=0)
								{
									dr[this[s].BaseField]=DBNull.Value;
								}
								else
								{
									XmlDocument xmlDoc=new XmlDocument();
									xmlDoc.LoadXml("<?xml version='1.0' ?>"+link);
									string url=xmlDoc.DocumentElement.GetAttribute("href");
									string docName=xmlDoc.DocumentElement.InnerText;
									/*
									XmlTextReader xmlReader=new XmlTextReader(link,XmlNodeType.Element,null);
									xmlReader.MoveToContent();
								
									string url=xmlReader.GetAttribute("href");
									string docName=xmlReader.ReadString();
									*/
									if(url!=null && docName!=null)
										dr[this[s].BaseField]=FileField.Content(docName.Trim(),url.Trim());
									else
										dr[this[s].BaseField]=DBNull.Value;
								}
							}
							continue;
						}
						if(this[s].ReadOnly)
						{
							dt.Columns[this[s].BaseField].ReadOnly=true;
							continue;//ֻ���ֶβ���¼��
						}
						if(this[s].ValueIsNotNull())
						{
							if(this[s].ShowType==ShowType.FILEUPLOAD || this[s].ShowType==ShowType.FILE)
							{
								string link=dr[this[s].BaseField] as string;
								//¼��ֵ�����ݿ��е�ֵ���ȱ��������ϴ����ļ�������δ�����ļ�
								if(string.Compare(link.Trim(),this[s].GetStringValue().Trim(),true)!=0)
								{
									if(link!=null && link.Trim().Length>0)
									{
										XmlDocument xmlDoc=new XmlDocument();
										xmlDoc.LoadXml("<?xml version='1.0' ?>"+link);
										string url=xmlDoc.DocumentElement.GetAttribute("href");
										string docName=xmlDoc.DocumentElement.InnerText;
										string path=HttpContext.Current.Server.MapPath(url);
										File.Delete(path);
									}
								}
							}
							if(this[s].GetStringValue().Length<=0)
								dr[this[s].BaseField]=DBNull.Value;
							else
								dr[this[s].BaseField]=this[s].Value;
						}
					}
					dr.EndEdit();
					dataAccess.EndUpdate(ds);
					if(dataAccess.Error!=null)
						throw new Exception("���´���"+dataAccess.Error.Message);
				}
				else
					throw new Exception("��¼�Ѿ����ڣ�");
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
					if(this[s].ValueIsNotNull())
					{
						if(this[s].GetStringValue().Length<=0)
							dr[this[s].BaseField]=DBNull.Value;
						else
							dr[this[s].BaseField]=this[s].Value;
					}
				}
				foreach(string s in NormalFields)
				{
					if(!this[s].FromUpdateTable)
						continue;
					if(this[s].Type.IsArray)
					{
						//��ǰ����ƿ����ǵ�ǰ�ֶ�ֵ��һ��HTML���ӣ��ݴ˿����ҵ��ļ����ڴ�
						//��ȡ�ļ����ݲ�д�����ݿ�
						if(this[s].ValueIsNotNull())
						{
							string link=this[s].GetStringValue();
							if(link.Length<=0)
							{
								dr[this[s].BaseField]=DBNull.Value;
							}
							else
							{
								XmlDocument xmlDoc=new XmlDocument();
								xmlDoc.LoadXml("<?xml version='1.0' ?>"+link);
								string url=xmlDoc.DocumentElement.GetAttribute("href");
								string docName=xmlDoc.DocumentElement.InnerText;
								/*
								XmlTextReader xmlReader=new XmlTextReader(link,XmlNodeType.Element,null);
								xmlReader.MoveToContent();
							
								string url=xmlReader.GetAttribute("href");
								string docName=xmlReader.ReadString();
								*/								
								if(url!=null && docName!=null)
									dr[this[s].BaseField]=FileField.Content(docName.Trim(),url.Trim());
								else
									dr[this[s].BaseField]=DBNull.Value;
							}
						}
						continue;
					}
					if(this[s].ReadOnly)
					{
						dt.Columns[this[s].BaseField].ReadOnly=true;
						continue;//ֻ���ֶβ���¼��
					}
					if(this[s].ValueIsNotNull())
					{
						if(this[s].GetStringValue().Length<=0)
							dr[this[s].BaseField]=DBNull.Value;
						else
							dr[this[s].BaseField]=this[s].Value;
					}
				}
				dt.Rows.Add(dr);
				dataAccess.EndUpdate(ds);
				if(dataAccess.Error!=null)
					throw new Exception("��Ӵ���"+dataAccess.Error.Message);
			}		
			//���³ɹ�ʱ��ֵ������Ϊ��¼���ֵ
			foreach(string key in SQLFields)
			{
				if(!this[key].FromUpdateTable)
					continue;
				if(this[key].ValueIsNotNull())
					this[key].OldValue=this[key].GetStringValue();
				else
					this[key].OldValue=null;
			}
		}
		/// <summary>
		/// �������ڸ�����ɾ���Ĳ�ѯ����������ֱ��ʹ�ø��±���ֶ�����������
		/// </summary>
		/// <returns></returns>
		private string GetUpdateCondition()
		{
			string condition="";
			//����ѡ���¼������
			bool nullKey=false;
			string prestr="";
			foreach(string fname in KeyFields)
			{
				if(!this[fname].FromUpdateTable)
					continue;
				string val=this[fname].OldValue;
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
					string val=this[fname].OldValue;
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
		/// ���ڸ����ֶ��������������ֶΣ����ﹹ����������ܻ�鲻��ʵ�ʵļ�¼�����Ǹ�BUG��
		/// </summary>
		/// <returns></returns>
		private string GetSelectCondition()
		{
			string condition="";
			//����ѡ���¼������
			bool nullKey=false;
			string prestr="";
			foreach(string fname in KeyFields)
			{
				string val=this[fname].OldValue;
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
					string val=this[fname].OldValue;
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
		private bool appendState=true;
	}
	public enum WebDynamicTableUI : int
	{
		EDIT,
		QUERY,
		VIEW
	}
	/// <summary>
	/// Summary description for WebDynamicTable.
	/// </summary>
	public class WebDynamicTable:DynamicTable
	{
		/// <summary>
		/// ָʾ��ǰ�����UI���ͣ��༭����ѯ����ϸ��Ϣ
		/// </summary>
		public WebDynamicTableUI UI
		{
			get
			{
				return ui;
			}
			set
			{
				ui=value;
			}
		}	
		public int EditCols
		{
			get
			{
				return editCols;
			}
			set
			{
				editCols=value;
			}
		}
		public int QueryCols
		{
			get
			{
				return queryCols;
			}
			set
			{
				queryCols=value;
			}
		}
		public int ViewCols
		{
			get
			{
				return viewCols;
			}
			set
			{
				viewCols=value;
			}
		}
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
		public HtmlTable ShowTable
		{
			get
			{
				if(showTable==null)
				{
					showTable=new HtmlTable();
					showTable.Border=0;
					showTable.CellPadding=0;
					showTable.CellSpacing=0;
					if(UI==WebDynamicTableUI.VIEW)
						showTable.Attributes.Add("class","LanViewTable");
					else if(UI==WebDynamicTableUI.QUERY)
						showTable.Attributes.Add("class","LanQueryTable");
					else
						showTable.Attributes.Add("class","LanInputTable");
					if(UI!=WebDynamicTableUI.VIEW)
						showTable.Attributes.Add("onkeydown","javascript:keydownHandle(this)");
				}
				return showTable;
			}
		}
		public HybridDictionary FieldDisplays
		{
			get
			{
				return fieldDisplays;
			}
		}

		private HybridDictionary fieldDisplays=new HybridDictionary(true);
		private HtmlTable showTable=null;
		private WebDynamicTableUI ui=WebDynamicTableUI.EDIT;//��ǰʵ��Ҫ�����UI����
		private int editCols=1;//���/�޸ļ�¼ʱһ����ʾ�����ֶ�
		private int queryCols=2;//������������һ����ʾ�����ֶ�
		private int viewCols=3;//��ʾ��¼ʱһ����ʾ�����ֶ�
		private bool showLength=false;//�Ƿ���ʾ�ַ����ֶ�������

		public WebDynamicTable(string sqlstr,DataAccessType datype,string connstr):base(sqlstr,datype,connstr)
		{
			//
			// TODO: Add constructor logic here
			//
		}
		public string GenerateHtml()
		{
			BuildDisplay();
			StringWriter sw=new StringWriter();
			ShowTable.RenderControl(new HtmlTextWriter(sw));
			sw.Close();
			return sw.ToString();
		}
		public void ResetFieldDisplays()
		{
			if(UI==WebDynamicTableUI.QUERY)
			{
				foreach(string s in KeyFields)
				{
					IQueryFieldDisplay fd=QueryFieldDisplay.Create(this[s],this);
					FieldDisplays[s]=fd;
				}
				foreach(string s in NormalFields)
				{
					IQueryFieldDisplay fd=QueryFieldDisplay.Create(this[s],this);
					FieldDisplays[s]=fd;
				}
			}
			else
			{
				foreach(string s in KeyFields)
				{
					IEditFieldDisplay fd=EditFieldDisplay.Create(EditField.Create(this[s]),this);
					FieldDisplays[s]=fd;
				}
				foreach(string s in NormalFields)
				{
					IEditFieldDisplay fd=EditFieldDisplay.Create(EditField.Create(this[s]),this);
					FieldDisplays[s]=fd;
				}
			}
		}
		public bool GetInputValue()
		{	
			bool bRet=true;
			foreach(string s in KeyFields)
			{
				if(!this[s].FromUpdateTable)
					continue;
				IEditFieldDisplay fd=FieldDisplays[s] as IEditFieldDisplay;
				if(fd==null)continue;
				fd.BuildFieldValue();
				if(!this[s].ReadOnly && this[s].GetStringValue().Length<=0)
					bRet=false;
				if(this[s].OldValue==null || this[s].OldValue.Trim().Length<=0)
				{
					if(this[s].ValueIsNotNull())
					{
						AppendState=true;//�ؼ��־�ֵΪ�ձ���������¼�¼
					}
				}
			}
			foreach(string s in NormalFields)
			{
				if(!this[s].FromUpdateTable)
					continue;
				IEditFieldDisplay fd=FieldDisplays[s] as IEditFieldDisplay;
				if(fd==null)continue;
				fd.BuildFieldValue();
			}
			if(AppendState)
			{
				int ft=FieldCount;
				if(ft>0)
				{
					string[] fields=new string[ft];
					KeyFields.CopyTo(fields,0);
					NormalFields.CopyTo(fields,KeyFields.Count);
					foreach(string s in fields)
					{
						this[s].OldValue=this[s].GetStringValue();
					}
				}
			}
			return bRet;
		}
		protected virtual StringCollection DecideLayout()
		{
			StringCollection strs=new StringCollection();
			foreach(string s in KeyFields)
			{
				if(UI==WebDynamicTableUI.EDIT && !this[s].FromUpdateTable)
					continue;
				strs.Add(s);
			}
			foreach(string s in NormalFields)
			{
				if(UI==WebDynamicTableUI.EDIT && !this[s].FromUpdateTable)
					continue;
				strs.Add(s);
			}
			return strs;
		}		
		private void BuildDisplay()
		{
			if(UI==WebDynamicTableUI.QUERY)
			{
				StringCollection fields=DecideLayout();
				HtmlTable table=ShowTable;
				table.Rows.Clear();

				int rowCount=0;
				int colCount=0;
				//ͨ����Ҫ��ʾ���м���ȷ����Ҫʹ�ü�������ʾ,��������Щ��,
				//�ڱ����Ѿ�������Ҫ��������������Щ����Ͱһ�������ڰ��ֶ�˳��������ÿ��Ͱ�з����ֶμ���
				//ʵ�ְ��������и��ֶΣ������ֶη���ǰһ����ʾ�ֶ����ڵ�Ͱ�У�
				//(Ŀǰ��Ҫ����˳�����)
				foreach(string s in fields)
				{
					IQueryFieldDisplay fd=QueryFieldDisplay.Create(this[s],this);
					FieldDisplays[s]=fd;
					fd.BuildDisplayControl();
					if(fd.Visible && !fd.Hidden)
					{
						if((colCount % QueryCols)==0)
						{
							HtmlTableRow tr0=new HtmlTableRow();
							table.Rows.Add(tr0);
							rowCount++;
						}
						colCount++;
					}
				}
				if(rowCount==2 && QueryCols-colCount%QueryCols>1)
				{
					table.Rows.Clear();
				}
				int ct=0;
				int rowi=0;
				HtmlTableRow tr=null;
				foreach(string s in fields)
				{
					IQueryFieldDisplay fd=FieldDisplays[s] as IQueryFieldDisplay;
					if(fd.Visible)
					{
						if(rowCount!=2 || QueryCols-colCount%QueryCols<=1)
						{
							tr=table.Rows[rowi];
							if(!fd.Hidden)
								rowi=(rowi+1)%rowCount;
						}
						else 
						{
							if(ct==0)//��һ���ұ���û����ʱ���Ǵ���һ��,�����Ƿ��������ֶ�
							{
								if(table.Rows.Count<=0)
								{
									tr=new HtmlTableRow();
									table.Rows.Add(tr);
								}
								if(!fd.Hidden)
									ct++;
							}
							else if(!fd.Hidden)
							{
								if(ct%QueryCols==0)
								{
									tr=new HtmlTableRow();
									table.Rows.Add(tr);								
								}
								ct++;
							}
						}

						HtmlTableCell cell=new HtmlTableCell();
						tr.Cells.Add(cell);
						cell.NoWrap=true;
						string showTxt=null;
						if(QueryCols>1)
						{
							showTxt=this[s].ShowName;
						}
						else
						{
							showTxt=this[s].GetIndentString()+this[s].ShowName;
						}
						cell.Controls.Add(new LiteralControl(showTxt));
						cell.Attributes.Add("title",GetFieldAdditional(s));
						cell.Attributes.Add("class","LanQueryHeaderCell");
						if(fd.Hidden)
							cell.Style.Add("display","none");

						if(UseGroups || UseSorts)
						{
							cell=new HtmlTableCell();
							tr.Cells.Add(cell);
							cell.NoWrap=true;
							if(UseGroups)
							{
								cell.Controls.Add(fd.GroupControl);
							}
							cell.Controls.Add(fd.SortControl);
							if(UseGroups)
							{
								cell.Attributes.Add("title","������ָ���Ƿ���������ܡ�������ܵķ����Լ���ؼ�������");
							}
							else
							{
								cell.Attributes.Add("title","������ָ����ؼ�������");
							}
							cell.Attributes.Add("class","LanQueryBodyCell");
							if(fd.Hidden)
								cell.Style.Add("display","none");
						}

						cell=new HtmlTableCell();
						tr.Cells.Add(cell);
						cell.NoWrap=true;
						cell.Controls.Add(fd.QueryControl);
						cell.Attributes.Add("class","LanQueryBodyCell");
						if(fd.Hidden)
							cell.Style.Add("display","none");
					}
				}
			}
			else if(UI==WebDynamicTableUI.VIEW)
			{
				string headerCellStyle="LanViewHeaderCell";
				string bodyCellStyle="LanViewBodyCell";
				StringCollection fields=DecideLayout();
				HtmlTable table=ShowTable;
				table.Rows.Clear();

				int rowCount=0;
				int colCount=0;
				//ͨ����Ҫ��ʾ���м���ȷ����Ҫʹ�ü�������ʾ,��������Щ��,
				//�ڱ����Ѿ�������Ҫ��������������Щ����Ͱһ�������ڰ��ֶ�˳��������ÿ��Ͱ�з����ֶμ���
				//ʵ�ְ��������и��ֶΣ������ֶη���ǰһ����ʾ�ֶ����ڵ�Ͱ�У�
				//(Ŀǰ��Ҫ����˳�����)
				foreach(string s in fields)
				{
					IEditFieldDisplay fd=EditFieldDisplay.Create(EditField.Create(this[s]),this);
					FieldDisplays[s]=fd;
					fd.BuildDisplayControl();
					if(fd.Visible && !fd.Hidden)
					{
						if((colCount % ViewCols)==0)
						{
							HtmlTableRow tr0=new HtmlTableRow();
							table.Rows.Add(tr0);
							rowCount++;
						}
						colCount++;
					}
				}
				if(rowCount==2 && ViewCols-colCount%ViewCols>1)
				{
					table.Rows.Clear();
				}
				int ct=0;
				int rowi=0;
				HtmlTableRow tr=null;
				foreach(string s in fields)
				{
					IEditFieldDisplay fd=FieldDisplays[s] as IEditFieldDisplay;
					if(fd.Visible && !fd.Hidden)//��ϸ��Ϣ���ѯ��༭��һ��,�����ֶβ�����UI
					{
						if(rowCount!=2 || ViewCols-colCount%ViewCols<=1)
						{
							tr=table.Rows[rowi];
							rowi=(rowi+1)%rowCount;
						}
						else 
						{
							if(ct%ViewCols==0)
							{
								tr=new HtmlTableRow();
								table.Rows.Add(tr);
							}
							ct++;
						}

						HtmlTableCell cell=new HtmlTableCell();
						tr.Cells.Add(cell);
						cell.NoWrap=true;
						string showTxt=null;
						if(ViewCols>1)
							showTxt=this[s].ShowName;
						else
							showTxt=this[s].GetIndentString()+this[s].ShowName;
						cell.Controls.Add(new LiteralControl(showTxt));
						cell.Attributes.Add("class",headerCellStyle);

						cell=new HtmlTableCell();
						BuildFieldView(s,cell);
						cell.Attributes.Add("class",bodyCellStyle);
						tr.Cells.Add(cell);
					}
				}
			}
			else if(UI==WebDynamicTableUI.EDIT)
			{
				string headerCellStyle="LanInputHeaderCell";
				string bodyCellStyle="LanInputBodyCell";
				StringCollection fields=DecideLayout();
				HtmlTable table=ShowTable;
				table.Rows.Clear();

				int rowCount=0;
				int colCount=0;
				//ͨ����Ҫ��ʾ���м���ȷ����Ҫʹ�ü�������ʾ,��������Щ��,
				//�ڱ����Ѿ�������Ҫ��������������Щ����Ͱһ�������ڰ��ֶ�˳��������ÿ��Ͱ�з����ֶμ���
				//ʵ�ְ��������и��ֶΣ������ֶη���ǰһ����ʾ�ֶ����ڵ�Ͱ�У�
				//(Ŀǰ��Ҫ����˳�����)
				foreach(string s in fields)
				{
					IEditFieldDisplay fd=EditFieldDisplay.Create(EditField.Create(this[s]),this);
					FieldDisplays[s]=fd;
					fd.BuildDisplayControl();
					if(fd.Visible && !fd.Hidden)
					{
						if((colCount % EditCols)==0)
						{
							HtmlTableRow tr0=new HtmlTableRow();
							table.Rows.Add(tr0);
							rowCount++;
						}
						colCount++;
					}
				}
				if(rowCount==2 && EditCols-colCount%EditCols>1)
				{
					table.Rows.Clear();
				}
				int ct=0;
				int rowi=0;
				HtmlTableRow tr=null;
				foreach(string s in fields)
				{
					IEditFieldDisplay fd=FieldDisplays[s] as IEditFieldDisplay;
					if(fd.Visible)
					{
						if(rowCount!=2 || EditCols-colCount%EditCols<=1)
						{
							tr=table.Rows[rowi];
							if(!fd.Hidden)
								rowi=(rowi+1)%rowCount;
						}
						else 
						{
							if(ct==0)//��һ���ұ���û����ʱ���Ǵ���һ��,�����Ƿ��������ֶ�
							{
								if(table.Rows.Count<=0)
								{
									tr=new HtmlTableRow();
									table.Rows.Add(tr);
								}
								if(!fd.Hidden)
									ct++;
							}
							else if(!fd.Hidden)
							{
								if(ct%EditCols==0)
								{
									tr=new HtmlTableRow();								
									table.Rows.Add(tr);
								}
								ct++;
							}
						}

						HtmlTableCell cell=new HtmlTableCell();
						tr.Cells.Add(cell);
						cell.NoWrap=true;
						string showTxt=null;
						if(EditCols>1)
						{
							showTxt=this[s].ShowName;
						}
						else
						{
							showTxt=this[s].GetIndentString()+this[s].ShowName;
						}
						cell.Controls.Add(new LiteralControl(showTxt));
						cell.Attributes.Add("class",headerCellStyle);
						if(fd.Hidden)
							cell.Style.Add("display","none");
						
						cell=new HtmlTableCell();
						tr.Cells.Add(cell);
						cell.NoWrap=true;
						cell.InnerHtml=GetFieldAdditional(s);
						cell.Attributes.Add("class",bodyCellStyle);
						if(fd.Hidden)
							cell.Style.Add("display","none");

						cell=new HtmlTableCell();
						tr.Cells.Add(cell);
						cell.NoWrap=true;
						cell.Controls.Add(fd.EditControl);
						cell.Attributes.Add("class",bodyCellStyle);
						if(fd.Hidden)
							cell.Style.Add("display","none");

						cell=new HtmlTableCell();
						tr.Cells.Add(cell);
						cell.InnerHtml=GetFieldDescription(s);
						cell.Attributes.Add("class",bodyCellStyle);
						if(fd.Hidden)
							cell.Style.Add("display","none");
					}
				}
			}
		}
		/// <summary>
		/// get query sql string
		/// </summary>
		public string[] GetQuerySql(out string where,out string having)
		{
			int order=0;
			SortedList sList=new SortedList();
			string whereRet="",whereAnd="";
			string havingRet="",havingAnd="";
			foreach(string s in KeyFields)
			{
				IQueryFieldDisplay fd=FieldDisplays[s] as IQueryFieldDisplay;
				if(fd==null)continue;
				fd.BuildFieldValue();
				if(this[s].WhereClause!=null && this[s].WhereClause.Length>0)
				{
					if(HaveGroupBy())
					{
						havingRet+=havingAnd+this[s].WhereClause;
						havingAnd=" and ";
					}
					else
					{
						whereRet+=whereAnd+this[s].WhereClause;
						whereAnd=" and ";
					}
				}
				//����������Ϣ
				int sort=this[s].SortOrder;
				if(sort!=0)
				{
					string postfix="";
					if(sort<0)
					{
						sort=-sort;
						postfix=" DESC";
					}
					sList[sort*MaxSort+order]=this[s].Name+postfix;	
					order++;
				}
			}
			foreach(string s in NormalFields)
			{
				IQueryFieldDisplay fd=FieldDisplays[s] as IQueryFieldDisplay;
				if(fd==null)continue;
				fd.BuildFieldValue();
				if(this[s].WhereClause!=null && this[s].WhereClause.Length>0)
				{
					if(HaveGroupBy())
					{
						havingRet+=havingAnd+this[s].WhereClause;
						havingAnd=" and ";
					}
					else
					{
						whereRet+=whereAnd+this[s].WhereClause;
						whereAnd=" and ";
					}
				}
				//����������Ϣ
				int sort=this[s].SortOrder;
				if(sort!=0)
				{
					string postfix="";
					if(sort<0)
					{
						sort=-sort;
						postfix=" DESC";
					}
					sList[sort*MaxSort+order]=this[s].Name+postfix;
					order++;
				}
			}
			where=whereRet;
			having=havingRet;
			string[] strs=new string[sList.Count];
			sList.Values.CopyTo(strs,0);
			return strs;
		}
		public string[] GetGroupInfo(StatisticControl statCtrl)
		{
			SortedList groupFields=new SortedList();
			int order=0;
			foreach(string s in KeyFields)
			{
				IQueryFieldDisplay fd=FieldDisplays[s] as IQueryFieldDisplay;
				if(fd==null)continue;
				fd.BuildFieldValue();
				//����������Ϣ
				int sort=this[s].SortOrder;
				if(sort!=0)
				{
					if(sort<0)
					{
						sort=-sort;
					}
					//���������Ϣ
					if(this[s].IsGroup)
					{
						groupFields[sort*MaxSort+order]=this[s].Name;
					}					
					order++;
				}
				if(!(this[s].IsGroup && sort!=0))
				{
					statCtrl.SetStatisticFieldType(this[s].Name,this[s].StatMethod);
				}
			}
			foreach(string s in NormalFields)
			{
				IQueryFieldDisplay fd=FieldDisplays[s] as IQueryFieldDisplay;
				if(fd==null)continue;
				fd.BuildFieldValue();
				//����������Ϣ
				int sort=this[s].SortOrder;
				if(sort!=0)
				{
					if(sort<0)
					{
						sort=-sort;
					}
					//���������Ϣ
					if(this[s].IsGroup)
					{
						groupFields[sort*MaxSort+order]=this[s].Name;
					}	
					order++;
				}
				if(!(this[s].IsGroup && sort!=0))
				{
					statCtrl.SetStatisticFieldType(this[s].Name,this[s].StatMethod);
				}
			}
			string[] groups=new string[groupFields.Count];
			groupFields.Values.CopyTo(groups,0);
			return groups;
		}
		/// <summary>
		/// ����쿴���ĳ����¼ʱ��ĳ�ֶε���ʾ,��Ҫ�����ֶ�ֵ��ת�����ʽ��.
		/// </summary>
		/// <param name="f">�ֶ���</param>
		/// <param name="cell">������ʾ�ֶ�ֵ��HtmlTableCell</param>
		public void BuildFieldView(string f,HtmlTableCell cell)
		{
			TableField tf=this[f];
			if(tf==null)
				return;
			if(!tf.UseClassValue)
			{
				if(tf.ShowType==ShowType.COMBOBOX)
				{
					NameValueCollection vals=GetClassVals(tf.Name);
					string v=tf.GetStringValue();
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
						string val=tf.GetStringValue();
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
					cell.InnerHtml=tf.GetFormatValue();
				}
			}
			else
				cell.InnerHtml=tf.GetFormatValue();

			string desc=GetFieldDescription(f);
			if(desc.Length>0)
				cell.InnerHtml+=" "+desc;
			else if(tf.Type==typeof(string) && (tf.ShowType==ShowType.TEXTAREA || tf.ShowType==ShowType.READONLYTEXTAREA))
			{
				//cell.InnerHtml="<pre>"+cell.InnerHtml+"</pre>";
			}

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
		/// <summary>
		/// �õ�һ�����������ص�����������ֵ����е�λ���õ�λ����
		/// </summary>
		/// <param name="f"></param>
		/// <returns></returns>
		public string GetFieldAdditional(string f)
		{
			if(this[f]==null)
				return "";
			if(UI==WebDynamicTableUI.VIEW || this[f].ShowType==ShowType.NOVALUE || this[f].ShowType==ShowType.RAWVALUE 
				|| this[f].ShowType==ShowType.FILE || this[f].ShowType==ShowType.HTML)
			{
				return "";
			}
			else
			{
				string type="";
				if(this[f].Unit!=null && this[f].Unit.Trim().Length>0)
				{
					type=this[f].Unit.Trim();
				}
				return type;
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
			if(UI==WebDynamicTableUI.VIEW || this[f].ShowType==ShowType.HIDDEN || this[f].ShowType==ShowType.READONLYTEXTBOX || this[f].ShowType==ShowType.READONLYTEXTAREA 
				|| this[f].ShowType==ShowType.NOVALUE || this[f].ShowType==ShowType.RAWVALUE 
				|| this[f].ShowType==ShowType.FILE || this[f].ShowType==ShowType.HTML)
			{
				if(this[f].Unit!=null && this[f].Unit.Trim().Length>0)
					return "<span style='color:blue'>"+this[f].Unit+"</span>";
				else
					return "";
			}
			else
			{
				string nullable="";
				if(!this[f].Nullable)
					nullable="<span style='color:red'>[����]</span>";
				string desc=this[f].Description;
				if(desc==null)
					desc="";
				else
					desc=desc.Trim();
				if(UI==WebDynamicTableUI.QUERY)
				{
					return desc;
				}
				string length="";
				if(this[f].Type==typeof(string) && (this[f].ShowType==ShowType.TEXTBOX || this[f].ShowType==ShowType.TEXTAREA))
				{
					if(IsMaxLengthShow)
						length="<span style='color:blue'>[�ֽ���<="+this[f].Length+"]</span>";
				}
				return nullable+length+desc;
			}
		}
		public new void UpdateRecord(bool bOverWrite)
		{
			if(GetInputValue())
			{
				CheckAndUpdate(bOverWrite);
			}
			else
				throw new Exception("����д�����");
		}
		public new void SelectRecord()
		{		
			GetInputValue();
			base.SelectRecord();
		}
		public new void DeleteRecord()
		{	
			GetInputValue();
			base.DeleteRecord();
		}
		public void CheckAndUpdate(bool bOverWrite)
		{
			string ttt=CheckInputValue();
			if(ttt!=null&&ttt.Length>0)
			{
				throw new Exception(ttt);
			}
			base.UpdateHelper(bOverWrite);
		}
		public void RegisterCss(Page page)
		{
			page.RegisterClientScriptBlock("InfoServiceCss",GenerateCssScript());
		}
		public void RegisterScript(Page page)
		{
			page.RegisterClientScriptBlock("WebDynamicTableClientScript",GenerateAllInternalScript());
		}
		private string GenerateCssScript()
		{
			return "<LINK href='"+CssUrl+"' type='text/css' rel='stylesheet'>";
		}
		/// <summary>
		/// ��������,���ɳ���׼�¼�������������ű�.
		/// </summary>
		/// <returns></returns>		
		private string GenerateAllInternalScript()
		{
			string retVal="<script src='"+ScriptUrl+"'></script>\r\n"+GenerateEditCursorScript()+GeneratePositionScript()+GenerateSearchSelectScript()+GenerateSearchCheckBoxesScript()+GenerateNumCheckScript()+GenerateStrCheckScript()+GenerateHtmlEditorScript()+
				GenerateFileUploadScript()+GeneratorDateCheckScript();
			return retVal;
		}
		private string GenerateEditCursorScript()
		{
			string retVal=@"
		<script language='javascript'>
			<!--
				function keydownHandle(conObj)
				{
					//37 �� 38 �� 39 �� 40 �� 13 �س� 27 ESC 16 SHIFT 17 CTRL 18 ALT
					//33 PgUp 34 PgDn 36 Home 35 End
					var obj=event.srcElement;
					var t=obj.tagName;
					if(t!='INPUT')
						return;
					var keyCode=event.keyCode;
					if(keyCode==13 || keyCode==34 || keyCode==40)
					{
						var elements=conObj.getElementsByTagName('INPUT');
						var len=elements.length;
						for(var i=0;i<elements.length-1;i++)
						{
							if(elements[i]==obj)
							{
								for(var j=i+1;j<elements.length-1;j++)
								{
									if(elements[j].type=='text')
									{
										elements[j].select();
										break;
									}					
								}
							}
						}
						event.cancelBubble=true;
						event.returnValue=false;
					}
					else if(keyCode==27 || keyCode==33 || keyCode==38)
					{
						var elements=conObj.getElementsByTagName('INPUT');
						var len=elements.length;
						for(var i=1;i<elements.length-1;i++)
						{
							if(elements[i]==obj)
							{
								for(var j=i-1;j>=0;j--)
								{
									if(elements[j].type=='text')
									{
										elements[j].select();
										break;
									}					
								}
							}
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
