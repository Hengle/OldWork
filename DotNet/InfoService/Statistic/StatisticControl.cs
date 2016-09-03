using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing.Design;
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
using System.IO;
using Dreaman.DataAccess;

namespace Dreaman.InfoService
{
	public delegate void StatTableUseDynamicTable(DynamicTable dynamicTable);
	public delegate void ShowStatTable(Table table,Table statTable);
	public delegate bool ExportStatTable(Table table);
	/// <summary>
	/// StatisticControl ��ժҪ˵����
	/// </summary>
	[ToolboxData("<{0}:StatisticControl runat=server></{0}:StatisticControl>")]
	public class StatisticControl : System.Web.UI.WebControls.WebControl,INamingContainer,IMultiFacadeControl
	{		
		/// <summary>
		/// ���û�����WebDynamicTable������SQL������Ķ���ʹ��Ԫ���ݳ�ʼ��һЩ
		/// ���ԵĻ���,����ֵΪ�ձ���ʹ����ȱʡ���ഴ������ʹ��ȱʡ��Ԫ��������ʼ����
		/// </summary>
		public event CreateWebDynamicTable CreateWebDynamicTable;
		/// <summary>		
		/// ���¼����û��ڶ�̬�������ʼ���ú�һ�����������ԵĻ��ᣬ��һ�¼���Ҫ����
		/// ȡ��ԭGetWebDynamicTable���з��������ѱ�Ϊ�ڲ�������������Ϊ�ؼ����ø���״̬��
		/// ������ԣ�show,search��
		/// </summary>
		public event SharedWebDynamicTable SharedWebDynamicTable;
		/// <summary>
		/// ��ͳ�Ʊ�ʹ��DynamicTableǰ���û�һ������DynamicTable�Ļ���
		/// </summary>
		public event StatTableUseDynamicTable StatTableUseDynamicTable;
		/// <summary>
		/// ����ʾͳ�ƽ��ǰ�ṩ��ͳ�ƽ���ĵ������ᡣ��һ������Ϊ��ʾͳ�ƽ����ĸ�Ԫ�ر���
		/// ��������Ϊ��ʾͳ�ƽ���ı�
		/// </summary>
		public event ShowStatTable ShowStatTable;
		/// <summary>
		/// �ڵ���ͳ�ƽ����֮ǰ�ṩ��ͳ�ƽ����ĵ������ᡣ����ΪTable��
		/// </summary>
		public event ExportStatTable ExportStatTable;
		/// <summary>
		/// ����ʾ������¼����ǰ�ṩ�޸Ķ�̬�����ԵĻ���,
		/// ��һ������Ϊ��̬��ĸ�Ԫ�ر�,�ڶ�������Ϊ��̬��
		/// </summary>
		public event ShowSearch ShowSearch;
		/// <summary>
		/// �ڹ���������������ǰ�ṩ�����ڹ������������ݵĴ�������Ϊ��̬��
		/// </summary>
		public event BuildCondition BuildCondition;
		/// <summary>
		/// ���ݿ��������:Sql,OleDb,Oracle,Odbc
		/// </summary>
		public DataAccessType DataAccessType
		{
			get
			{
				return dataAccessType;
			}
			set
			{
				dataAccessType=value;
			}
		}
		/// <summary>
		/// ���ݿ����Ӵ�
		/// </summary>
		public string ConnectionString
		{
			get
			{
				return connStr;
			}
			set
			{
				connStr=value;
			}
		}
		/// <summary>
		/// ��ǰ¼����ڵ�����Դ,ͨ��Ϊselect * from tablename,�����ڲ�ѯʱ����Ϊ����select���
		/// </summary>
		public string SQL
		{
			get
			{
				return originalSql;
			}
			set
			{
				originalSql=value;
			}
		}
		/// <summary>
		/// ���ݱ��ȡ������¼��,Ĭ��Ϊ100
		/// </summary>
		public int MaxRecordNumber
		{
			get
			{
				return maxRecordNumber;
			}
			set
			{
				maxRecordNumber=value;
			}
		}
		/// <summary>
		/// �ֶ�������ʾ�������,��������...��ʾ,������ʾ��ʾȫ������.Ĭ��Ϊ8
		/// </summary>
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
		/// <summary>
		/// ��ʾͳ�Ʊ�ı�ͷ���ܼ��е���ɫ��
		/// </summary>
		public int BaseColor
		{
			get
			{
				return baseColor;
			}
			set
			{
				baseColor=value;
			}
		}
		/// <summary>
		/// ��ʾͳ�Ʊ�ĸ������е���ɫ����0x00HHSSVV��ÿ���ӷ������ɫ�ڸ������������ֵ��
		/// H: -120~120 S:-100~100 V:-100~100 Hֵ��ʾɫ���������Sֵ��100��ʾ���Ͷȵ�������Vֵ��100��ʾ���ȵ�������
		/// </summary>
		public int DeltaColor
		{
			get
			{
				return deltaColor;
			}
			set
			{
				deltaColor=value;
			}
		}
		/// <summary>
		/// ��ʾ��Щ������ť,Ĭ����ȫ����ʾ
		/// </summary>
		public DisplayButtons DisplayButtons
		{
			get
			{
				return displayBtns;
			}
			set
			{
				displayBtns=value;
			}
		}
		/// <summary>
		/// �ϴ��ļ�����������·��.
		/// </summary>
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
		[Editor(typeof(System.Web.UI.Design.ImageUrlEditor), typeof(UITypeEditor))]
		public string SearchIcon
		{
			get
			{
				return searchIcon;
			}
			set
			{
				searchIcon=value;
			}
		}
		[Editor(typeof(System.Web.UI.Design.ImageUrlEditor), typeof(UITypeEditor))]
		public string ReturnIcon
		{
			get
			{
				return returnIcon;
			}
			set
			{
				returnIcon=value;
			}
		}
		[Editor(typeof(System.Web.UI.Design.ImageUrlEditor), typeof(UITypeEditor))]
		public string ExportIcon
		{
			get
			{
				return exportIcon;
			}
			set
			{
				exportIcon=value;
			}
		}		
		public string ExportToolTip
		{
			get
			{
				return exportToolTip;
			}
			set
			{
				exportToolTip=value;
			}
		}
		/// <summary>
		/// ����WEB��̬�����ʽ���ļ�
		/// </summary>
		[Editor(typeof(System.Web.UI.Design.ImageUrlEditor), typeof(UITypeEditor))]
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
		/// <summary>
		/// ����WEB��̬��Ŀͻ��˽ű��ļ�
		/// </summary>
		[Editor(typeof(System.Web.UI.Design.ImageUrlEditor), typeof(UITypeEditor))]
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
		/// <summary>
		/// ��������ʹ�õ�ContentType��Ӧͷ��ֵ,ȱʡΪapplication/vnd.ms-excel
		/// </summary>
		public string ExportContentType
		{
			get
			{
				return exportContentType;
			}
			set
			{
				exportContentType=value;
			}
		}
		/// <summary>
		/// ��������ʹ�õ�Content-Disposition��Ӧͷ��ֵ,ȱʡΪattachment;filename=datagrid.xls
		/// </summary>
		public string ExportContentDisposition
		{
			get
			{
				return exportContentDisposition;
			}
			set
			{
				exportContentDisposition=value;
			}
		}
		/// <summary>
		/// ��������ʹ�õ��ַ���������,ȱʡΪGB2312
		/// </summary>
		public string ExportContentEncoding
		{
			get
			{
				return exportContentEncoding;
			}
			set
			{
				exportContentEncoding=value;
			}
		}
		[Browsable(false)]
		public Table GetStatTable()
		{
			string state=HttpContext.Current.Request["StatisticControlState"];
			if(showTable==null)
			{
				showTable=new Table();
				showTable.Width=Unit.Percentage(100.0);
				showTable.CssClass="LanStatTable";
				showTable.Style.Add("border-collapse","collapse");
			}
			return showTable;
		}
		[Browsable(false)]
		public string BuildRequestUrl(string state)
		{
			return ControlStateUrl.BuildStateRequestUrl("StatisticControlState",state);
		}
		/// <summary>
		/// ��Ϊ״̬�ı���Ҫ���webDynaTable,������������ؼ��ڲ�ʹ��,����Ϊ����(���Ѿ�������ȷ�ķ��ص�ǰ״̬����
		/// �������ԵĶ�̬��,��Ϊ�Ժ���ܱ����������ʵ����),ʵ��ʹ����Ҫ���ö�̬�����Ե���ʹ���¼�����.
		/// </summary>
		/// <returns></returns>
		[Browsable(false)]
		internal WebDynamicTable GetWebDynamicTable()
		{
			string state=HttpContext.Current.Request["StatisticControlState"];
			if(webDynaTable==null)
			{
				webDynaTable=OnCreateWebDynamicTable(SQL,DataAccessType,ConnectionString);
				if(webDynaTable==null)
				{
					webDynaTable=new WebDynamicTable(SQL,DataAccessType,ConnectionString);
					webDynaTable.SetDefaultMetaInfo();
				}
				OnSharedWebDynamicTable(webDynaTable);
			}
			return webDynaTable;
		}
		internal WebDynamicTable OnCreateWebDynamicTable(string sql,DataAccessType datype,string connstr)
		{
			if(CreateWebDynamicTable!=null)
				return CreateWebDynamicTable(sql,datype,connstr);
			return null;
		}
		internal void OnSharedWebDynamicTable(WebDynamicTable dynamicTable)
		{
			if(SharedWebDynamicTable!=null)
				SharedWebDynamicTable(dynamicTable);
		}
		internal void OnStatTableUseDynamicTable(DynamicTable dynamicTable)
		{
			if(StatTableUseDynamicTable!=null)
				StatTableUseDynamicTable(dynamicTable);
		}
		internal void OnShowStatTable(Table table,Table statTable)
		{
			if(ShowStatTable!=null)
				ShowStatTable(table,statTable);
		}
		internal bool OnExportStatTable(Table statTable)
		{
			if(ExportStatTable!=null)
				return ExportStatTable(statTable);
			return true;
		}
		internal void OnShowSearch(WebDynamicTable dynamicTable)
		{
			if(ShowSearch!=null)
				ShowSearch(dynamicTable);
		}
		internal void OnBuildCondition(WebDynamicTable dynamicTable)
		{
			if(BuildCondition!=null)
				BuildCondition(dynamicTable);
		}
		internal string UniqueControlID
		{
			get
			{
				string connCode="";
				if(ConnectionString!=null)
					connCode=((uint)this.ConnectionString.GetHashCode()).ToString();
				string sqlCode="";
				if(SQL!=null)
					sqlCode=((uint)this.SQL.GetHashCode()).ToString();
				return HttpContext.Current.Session.SessionID+((uint)HttpContext.Current.Request.RawUrl.GetHashCode()).ToString()+connCode+sqlCode;
			}
		}
		/// <summary>
		/// ���ӵĲ�ѯ��������SQL�еĲ�ѯ������ͬ��������������Ծ����������ܸı䡣
		/// </summary>
		public string WhereCondition
		{
			get
			{
				return HttpContext.Current.Cache["WhereCondition"+UniqueControlID] as string;
			}
			set
			{
				if(value==null)
				{
					HttpContext.Current.Cache.Remove("WhereCondition"+UniqueControlID);
					return;
				}
				HttpContext.Current.Cache["WhereCondition"+UniqueControlID]=value;
			}
		}
		/// <summary>
		/// ���ӵĲ�ѯ��������SQL�еĲ�ѯ������ͬ��������������Ծ����������ܸı䡣
		/// </summary>
		public string HavingCondition
		{
			get
			{
				return HttpContext.Current.Cache["HavingCondition"+UniqueControlID] as string;
			}
			set
			{
				if(value==null)
				{
					HttpContext.Current.Cache.Remove("HavingCondition"+UniqueControlID);
					return;
				}
				HttpContext.Current.Cache["HavingCondition"+UniqueControlID]=value;
			}
		}
		/// <summary>
		/// �����ֶ��б���ע������ֶε�˳��Ӧ���������е�˳��һ�£�ǰ����һ�£���
		/// </summary>
		public string[] Groups
		{
			get
			{
				return HttpContext.Current.Cache["Groups"+UniqueControlID] as string[];
			}
			set
			{
				if(value==null)
				{
					HttpContext.Current.Cache.Remove("Groups"+UniqueControlID);
					return;
				}
				HttpContext.Current.Cache["Groups"+UniqueControlID]=value;
			}
		}
		/// <summary>
		/// �����ֶ��б�����ǽ������ֶ���д��"�ֶ��� DESC"��
		/// </summary>
		public string[] Sorts
		{
			get
			{
				return HttpContext.Current.Cache["Sorts"+UniqueControlID] as string[];
			}
			set
			{
				if(value==null)
				{
					HttpContext.Current.Cache.Remove("Sorts"+UniqueControlID);
					return;
				}
				HttpContext.Current.Cache["Sorts"+UniqueControlID]=value;
			}
		}
		public StatisticFieldType GetStatisticFieldType(string fname)
		{
			HybridDictionary ht=HttpContext.Current.Cache["StatisticFieldTypes"+UniqueControlID] as HybridDictionary;
			if(ht==null)
				return StatisticFieldType.AUTO;
			else
			{
				object o=ht[fname];
				if(o==null)
					return StatisticFieldType.AUTO;
				else
					return (StatisticFieldType)o;
			}
		}
		public void SetStatisticFieldType(string fname,StatisticFieldType sft)
		{
			HybridDictionary ht=HttpContext.Current.Cache["StatisticFieldTypes"+UniqueControlID] as HybridDictionary;
			if(ht==null)
			{
				ht=new HybridDictionary(true);
				HttpContext.Current.Cache["StatisticFieldTypes"+UniqueControlID]=ht;
			}
			ht[fname]=sft;
		}
		internal DataTable Data
		{
			get
			{
				if(ConnectionString==null || SQL==null)
					return null;
				SQLSelectParser parser=new SQLSelectParser(SQL);
				string sqlstr=parser.BuildSQL(WhereCondition,HavingCondition);

				if(Groups==null || Groups.Length==0)
				{
					string[] groups=parser.GetGroupByList();
					if(groups!=null && groups.Length>0)
					{
						Groups=groups;
					}
				}

				if(Sorts==null || Sorts.Length==0)
				{
					string[] sorts=parser.GetOrderByList();
					if(sorts!=null && sorts.Length>0)
					{
						Sorts=sorts;
					}
					else if(Groups!=null && Groups.Length>0)
					{	
						Sorts=new string[Groups.Length];
						Groups.CopyTo(Sorts,0);
					}
				}

				QueryResult result=new QueryResult(sqlstr,GetWebDynamicTable(),UploadFileVirtualPath,MaxRecordNumber);
				return result.GetData();

				//				IDataAccess dataAccess=DataAccessFactory.Produce(DataAccessType,ConnectionString);
				//				SQLSelectParser parser=new SQLSelectParser(SQL);
				//				string sqlstr=parser.BuildSQL(SearchCondition);
				//				DataTable dt=dataAccess.GetSchemaStruct(sqlstr);
				//				IDataReader myReader=dataAccess.ExecuteDataReader(sqlstr);
				//				for(int i=0;i<MaxRecordNumber && myReader.Read();i++)
				//				{
				//					DataRow dr=dt.NewRow();
				//					foreach(DataColumn dc in dt.Columns)
				//					{
				//						dr[dc.ColumnName]=myReader[dc.ColumnName];
				//					}
				//					dr.EndEdit();
				//					dt.Rows.Add(dr);
				//				}
				//				myReader.Close();
				//				return dt;
			}
		}
		/// <summary>
		/// ���ؼ����뵽���ؼ���,ֻҪ��ʹ�ý��������,Ӧ���ǵ��ô˷��������ؼ����뵽ҳ����,
		/// �˷����������ؼ�״̬��ʵ�ʴ��������ɿؼ���ǰ���档
		/// ����ֵ�����Ƿ����������棬Ϊ��ʱӦ����ֹ�������κν�����صĲ�������ʱ��Ӧ���ѹرգ���
		/// </summary>
		/// <param name="parent"></param>
		public bool AddToParent(Control parent)
		{
			return AdjustControlEnvironment(parent,true);
		}
		/// <summary>
		/// �����ؼ���ʾ�������Ļ���,ʹ�ý��������ʱ,Ӧ���ô˷���,��Ϊ�ؼ���ĳЩ״̬������Ҫ������
		/// ҳ������ʾ���ؼ���Ҫ��ҳ������Ӧ������ʹ�ý��������ʱ����һ�������ɡ�
		/// ����,�ؼ��Ŀͻ�����ԴURLҲ��Ҫ��˴�������ֵ�����Ƿ����������棬Ϊ��ʱӦ����ֹ��
		/// �����κν�����صĲ�������ʱ��Ӧ���ѹرգ���
		/// </summary>
		public bool AdjustControlEnvironment()
		{
			return AdjustControlEnvironment(this.Parent,false);
		}
		private bool AdjustControlEnvironment(Control parent,bool forAdd)
		{
			if(ControlStateUrl.HandleResourceRequest())
				return true;
			if(parent.Page==null)//������ؼ�����ͬһҳ����ʱ,��һ���α���ǰ��Ŀؼ��Ѿ��������ض�״̬����ʾ,���ﲻ���ٴ���
				//������ؼ�����ͬһҳ����ʱ,�ض�״̬�����ɵ�һ���ؼ�������
			{
				return false;
			}
			string state=HttpContext.Current.Request["StatisticControlState"];
			if(forAdd)
			{
				this.Width=Unit.Percentage(100.0);
				parent.Controls.Add(this);
			}
			return false;
		}
		/// <summary>
		/// �ı�ؼ���ָ��״̬
		/// </summary>
		/// <param name="state">״̬����show,add,modify,view,search,upload,htmleditor</param>
		/// <param name="args">�������ϣ�����/ֵ�ԣ�</param>
		public void ChangeState(string state,NameValueCollection args)
		{
			string argstr="",prestr="";
			foreach(string s in args.AllKeys)
			{
				argstr+=prestr+HttpContext.Current.Server.UrlEncode(s)+"="+HttpContext.Current.Server.UrlEncode(args[s]);
				prestr="&";
			}			
			ChangeState(state,argstr);
		}
		/// <summary>
		/// �ı�ؼ���ָ��״̬
		/// </summary>
		/// <param name="state">״̬����show,add,modify,view,search,upload,htmleditor</param>
		/// <param name="args">�������飬����Ϊ������ֵ������ֵ��...</param>
		public void ChangeState(string state,params string[] args)
		{
			string argstr="",prestr="";
			if(args.Length==1)
			{
				argstr=args[0];
			}
			else
			{
				for(int i=0;i<args.Length-1;i+=2)
				{
					argstr+=prestr+HttpContext.Current.Server.UrlEncode(args[i])+"="+HttpContext.Current.Server.UrlEncode(args[i+1]);
					prestr="&";
				}
			}
			StateArgs=argstr;
			if(State!=state)//״̬�ı���ղ�����SQL�����Ϣ
			{
				webDynaTable=null;
			}
			State=state;
			if(State=="show")
			{
				controlState=new StatShowControlState();
				controlState.Handle(this,StateArgs);
			}
			else if(State=="search")
			{
				controlState=new StatSearchControlState();
				controlState.Handle(this,StateArgs);
			}
		}
		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			string state=Page.Request["StatisticControlState"];
			if(state!=null)
				State=state;
			ChangeState(State,StateArgs);
		}
		private string State
		{
			get
			{
				if(HttpContext.Current.Cache["State"+UniqueControlID]==null)
					return "show";
				return HttpContext.Current.Cache["State"+UniqueControlID] as string;
			}
			set
			{
				if(value==null)
				{
					HttpContext.Current.Cache.Remove("State"+UniqueControlID);
					return;
				}
				HttpContext.Current.Cache["State"+UniqueControlID]=value;
			}
		}
		private string StateArgs
		{
			get
			{
				return HttpContext.Current.Cache["StateArgs"+UniqueControlID] as string;
			}
			set
			{
				if(value==null)
				{
					HttpContext.Current.Cache.Remove("StateArgs"+UniqueControlID);
					return;
				}
				HttpContext.Current.Cache["StateArgs"+UniqueControlID]=value;
			}
		}
		private StatControlState controlState=null;
		private WebDynamicTable webDynaTable=null;
		private Table showTable=null;

		private DataAccessType dataAccessType=DataAccessType.OleDb;
		private string connStr=null;
		private string originalSql=null;
		private int maxRecordNumber=100;
		private int maxFieldWidth=16;

		private int baseColor=0x00FFFF99;
		private int deltaColor=0x001E0000;

		private DisplayButtons displayBtns=Dreaman.InfoService.DisplayButtons.QUERY;
		private string uploadFileVPath="../uploadFiles";

		private string exportContentType="application/vnd.ms-excel";
		private string exportContentDisposition="attachment;filename=datagrid.xls";
		private string exportContentEncoding="GB2312";//"GB18030";
		private string exportToolTip="����ǰ���ݵ�����EXCEL��...";

		private string searchIcon="../images/search.gif";
		private string returnIcon="../images/return.gif";
		private string exportIcon="../images/export.gif";
		private string cssUrl="../html/TreeTable.css";
		private string scriptUrl="../html/calendar.js";
	}
	/// <summary>
	/// �������Ͽؼ�ĳ��������״̬
	/// </summary>
	internal abstract class StatControlState : IControlState
	{
		public void Handle(IMultiFacadeControl statctrl,string stateargs)
		{
			statControl=(StatisticControl)statctrl;
			if(stateargs!=null)
			{
				string[] args=stateargs.Split('&');
				foreach(string pair in args)
				{
					string[] nameVal=pair.Split('=');
					if(nameVal.Length==2)
						stateArgs[HttpContext.Current.Server.UrlDecode(nameVal[0])]=HttpContext.Current.Server.UrlDecode(nameVal[1]);
				}
			}
			statControl.Controls.Clear();
			Handle();
		}
		protected abstract void Handle();
		protected StatisticControl StatisticControl
		{
			get
			{
				return statControl;
			}
		}
		protected NameValueCollection StateArgs
		{
			get
			{
				return stateArgs;
			}
		}
		private StatisticControl statControl=null;
		private NameValueCollection stateArgs=new NameValueCollection();
	}
	internal class StatShowControlState : StatControlState
	{
		protected override void Handle()
		{
			dynamicTable=StatisticControl.GetWebDynamicTable();
			StatisticControl.OnStatTableUseDynamicTable(dynamicTable);
			DataTable data=StatisticControl.Data;
			DataView dataView=new DataView(data);
			string[] sorts=StatisticControl.Sorts;
			if(sorts!=null)
				dataView.Sort=string.Join(",",sorts);
			string[] groups=StatisticControl.Groups;
			int groupNum=0;
			if(groups!=null)
				groupNum=groups.Length;
			StringCollection sc=new StringCollection();
			foreach(DataColumn dc in dataView.Table.Columns)
			{
				sc.Add(dc.ColumnName);
			}
			table=StatisticControl.GetStatTable();
			StatisticRow statRow=null,lastRow=null;
			if(statRow==null)
			{
				statRow=StatisticRow.CreateTotalRow(table,groupNum,dynamicTable,StatisticControl.MaxFieldWidth,StatisticControl.BaseColor,StatisticControl.DeltaColor);
				lastRow=statRow;
			}
			for(int i=0;i<groupNum;i++)
			{
				string field=groups[i];
				sc.Remove(field);
				lastRow=lastRow.CreateSubGroup(dataView.Table.Columns.IndexOf(field));
			}
			foreach(string field in sc)
			{
				DataColumn dc=dataView.Table.Columns[field];
				lastRow=statRow;
				while(lastRow!=null)
				{
					lastRow.AddField(dc,StatisticControl.GetStatisticFieldType(field));
					lastRow=lastRow.SubGroup;
				}
			}
			statRow.DisplayHead(statRow,dataView);
			for(int ii=0;ii<dataView.Count;ii++)
			{
				DataRowView drv=dataView[ii];
				statRow.ExecStatistic(drv,false);
				if(ii==dataView.Count-1)
				{
					statRow.ExecStatistic(drv,true);//����ͳ��
				}
			}
			
			Table t=new Table();
			t.Width=Unit.Percentage(100.0);
			TableRow tr=new TableRow();
			TableCell tc=new TableCell();
			tr.Cells.Add(tc);
			t.Rows.Add(tr);
			
			if((StatisticControl.DisplayButtons & DisplayButtons.EXPORT) == DisplayButtons.EXPORT)
			{
				ImageButton exportBtn=new ImageButton();
				exportBtn.ID="ExportBtn";
				exportBtn.ImageUrl=ControlStateUrl.BuildResourceRequestUrl(StatisticControl.ExportIcon);
				exportBtn.ToolTip=StatisticControl.ExportToolTip;
				exportBtn.CommandName="Export";
				exportBtn.Click+=new ImageClickEventHandler(this.ButtonClick);

				tc.Controls.Add(exportBtn);
			}			
			if((StatisticControl.DisplayButtons & DisplayButtons.SEARCH) == DisplayButtons.SEARCH)
			{
				ImageButton searchBtn=new ImageButton();
				searchBtn.ID="SearchBtn";
				searchBtn.ImageUrl=ControlStateUrl.BuildResourceRequestUrl(StatisticControl.SearchIcon);
				searchBtn.ToolTip="��������";
				searchBtn.CommandName="Search";
				searchBtn.Click+=new ImageClickEventHandler(this.ButtonClick);
				
				tc.Controls.Add(searchBtn);
			}
			tc.HorizontalAlign=HorizontalAlign.Right;

			tr=new TableRow();
			tc=new TableCell();
			tr.Cells.Add(tc);
			t.Rows.Add(tr);
			tc.Controls.Add(table);

			StatisticControl.Controls.Add(t);
			StatisticControl.OnShowStatTable(t,table);
			
			StatisticControl.Page.RegisterClientScriptBlock("InfoServiceCss","<LINK href='"+StatisticControl.CssUrl+"' type='text/css' rel='stylesheet'>");
			StatisticControl.Page.RegisterClientScriptBlock("ShowControlClientScript",@"
		<script language='javascript'>
			<!--
			function openHtml(obj)
			{
				var pobj=obj.parentElement;
				if(pobj)
				{
					try
					{
						var w=window.open('about:blank','HTMLVIEW','resizable=yes,scrollbars=yes');
						w.document.write(pobj.dreamanHtml);
					}
					catch(eee)
					{
						alert('��������������ֹ�˵������ڣ��޷��鿴HTML���ݣ�');
					}
					event.returnValue=false;
				}
			}
			-->
		</script>
				"
				);
		}
		private void ButtonClick(object sender,ImageClickEventArgs e)
		{
			ImageButton ib=(ImageButton)sender;
			if(ib.CommandName=="Export")
			{
				DataTable data=StatisticControl.Data;
				DataView dataView=new DataView(data);
				string[] sorts=StatisticControl.Sorts;
				if(sorts!=null)
					dataView.Sort=string.Join(",",sorts);
				string[] groups=StatisticControl.Groups;
				int groupNum=0;
				if(groups!=null)
					groupNum=groups.Length;
				StringCollection sc=new StringCollection();
				foreach(DataColumn dc in dataView.Table.Columns)
				{
					sc.Add(dc.ColumnName);
				}
				Table export=new Table();				
				export.Width=table.Width;
				export.Font.Size=FontUnit.Point(10);
				foreach(string key in table.Style.Keys)
				{
					export.Style.Add(key,table.Style[key]);
				}
				export.CssClass=table.CssClass;
				
				StatisticRow statRow=null,lastRow=null;
				if(statRow==null)
				{
					statRow=StatisticRow.CreateTotalRow(export,groupNum,dynamicTable,StatisticControl.MaxFieldWidth,StatisticControl.BaseColor,StatisticControl.DeltaColor,true);
					lastRow=statRow;
				}
				for(int i=0;i<groupNum;i++)
				{
					string field=groups[i];
					sc.Remove(field);
					lastRow=lastRow.CreateSubGroup(dataView.Table.Columns.IndexOf(field));
				}
				foreach(string field in sc)
				{
					DataColumn dc=dataView.Table.Columns[field];
					lastRow=statRow;
					while(lastRow!=null)
					{
						lastRow.AddField(dc,StatisticControl.GetStatisticFieldType(field));
						lastRow=lastRow.SubGroup;
					}
				}
				statRow.DisplayHead(statRow,dataView);
				for(int ii=0;ii<dataView.Count;ii++)
				{
					DataRowView drv=dataView[ii];
					statRow.ExecStatistic(drv,false);
					if(ii==dataView.Count-1)
					{
						statRow.ExecStatistic(drv,true);//����ͳ��
					}
				}

				if(StatisticControl.OnExportStatTable(export))
				{
					System.IO.StringWriter tw = new System.IO.StringWriter();
					export.RenderControl(new System.Web.UI.HtmlTextWriter(tw));
					tw.Close();

					HttpResponse res = HttpContext.Current.Response; 
					res.Clear();
					res.ContentType = StatisticControl.ExportContentType;
					res.ContentEncoding = System.Text.Encoding.GetEncoding(StatisticControl.ExportContentEncoding);

					res.AppendHeader("Content-Disposition", StatisticControl.ExportContentDisposition);
					res.Write(tw.ToString());   
					res.End();
				}

				StatisticControl.ChangeState("show");
			}
			else if(ib.CommandName=="Search")
			{
				StatisticControl.ChangeState("search");
			}
		}
		private Table table=null;
		private DynamicTable dynamicTable=null;
	}
	/// <summary>
	/// ��������״̬
	/// </summary>
	internal class StatSearchControlState : StatControlState
	{
		protected override void Handle()
		{
			Table t=new Table();
			TableRow tr=new TableRow();
			TableCell tc=new TableCell();
			tr.Cells.Add(tc);
			t.Rows.Add(tr);

			dynaTable=StatisticControl.GetWebDynamicTable();
			dynaTable.CssUrl=ControlStateUrl.BuildResourceRequestUrl(StatisticControl.CssUrl);
			dynaTable.ScriptUrl=ControlStateUrl.BuildResourceRequestUrl(StatisticControl.ScriptUrl);
			dynaTable.MaxFieldWidth=StatisticControl.MaxFieldWidth;
			dynaTable.UseGroups=true;
			if(StatisticControl.Groups!=null && StatisticControl.Groups.Length>0)
			{
				foreach(string g in StatisticControl.Groups)
				{
					dynaTable.SetFieldGroup(g,true);
				}
			}
			if(StatisticControl.Sorts!=null && StatisticControl.Sorts.Length>0)
			{
				for(int i=1;i<=StatisticControl.Sorts.Length;i++)
				{
					int sort=i;
					string field=StatisticControl.Sorts[i-1].Trim();
					if(field.ToUpper().EndsWith("DESC"))
					{
						sort=-sort;
						field=field.Substring(0,field.Length-4).Trim();
					}
					dynaTable.SetFieldSort(field,sort);
				}
			}
			foreach(string s in dynaTable.KeyFields)
			{
				dynaTable[s].StatMethod=StatisticControl.GetStatisticFieldType(s);
			}
			foreach(string s in dynaTable.NormalFields)
			{
				dynaTable[s].StatMethod=StatisticControl.GetStatisticFieldType(s);				
			}
			dynaTable.UI=WebDynamicTableUI.QUERY;
			StatisticControl.OnShowSearch(dynaTable);
			LiteralControl lc=new LiteralControl(dynaTable.GenerateHtml());

			dynaTable.RegisterCss(StatisticControl.Page);
			dynaTable.RegisterScript(StatisticControl.Page);

			tc.Controls.Add(lc);

			tr=new TableRow();
			tc=new TableCell();
			tr.Cells.Add(tc);
			t.Rows.Add(tr);

			ImageButton okBtn=new ImageButton();
			okBtn.ID="OK";
			okBtn.ImageUrl=ControlStateUrl.BuildResourceRequestUrl(StatisticControl.SearchIcon);
			okBtn.ToolTip="��ʼ������������������";
			okBtn.CommandName="Ok";
			okBtn.Click+=new ImageClickEventHandler(this.ButtonClick);

			tc.Controls.Add(okBtn);

			ImageButton returnBtn=new ImageButton();
			returnBtn.ID="Return";
			returnBtn.ImageUrl=ControlStateUrl.BuildResourceRequestUrl(StatisticControl.ReturnIcon);
			returnBtn.ToolTip="�������ݱ�";
			returnBtn.CommandName="Return";
			returnBtn.Click+=new ImageClickEventHandler(this.ButtonClick);

			tc.Controls.Add(returnBtn);
			tc.HorizontalAlign=HorizontalAlign.Center;

			StatisticControl.Controls.Add(t);
		}
		void ButtonClick(object sender,ImageClickEventArgs e)
		{
			ImageButton ib=(ImageButton)sender;
			if(ib.CommandName=="Ok")
			{
				StatisticControl.OnBuildCondition(dynaTable);
				string where=null;
				string having=null;
				string[] sorts=dynaTable.GetQuerySql(out where,out having);
				string[] groups=dynaTable.GetGroupInfo(StatisticControl);
				StatisticControl.WhereCondition=where;
				StatisticControl.HavingCondition=having;
				if(sorts.Length>0)
					StatisticControl.Sorts=sorts;
				else
					StatisticControl.Sorts=null;
				if(groups.Length>0)
					StatisticControl.Groups=groups;
				else
					StatisticControl.Groups=null;
				StatisticControl.ChangeState("show");
			}
			else if(ib.CommandName=="Return")
			{
				StatisticControl.ChangeState("show");
			}
		}
		private WebDynamicTable dynaTable=null;
	}
}
