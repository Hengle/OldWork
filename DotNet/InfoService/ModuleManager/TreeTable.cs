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

[ assembly:TagPrefix("Dreaman.InfoService", "InfoService") ]
namespace Dreaman.InfoService
{
	/// <summary>
	/// TreeTable ��ժҪ˵����
	/// </summary>
	[ToolboxData("<{0}:TreeTable runat=server></{0}:TreeTable>")]
	public class TreeTable : WebControl
	{
		public string rootImg="../treeimage/treetop.gif";
		public string openImg="../treeimage/openfolder.gif";
		public string closeImg="../treeimage/folder.gif";
		public string open2Img="../treeimage/yellow.gif";
		public string close2Img="../treeimage/green.gif";
		public string minusImg="../treeimage/minus.gif";
		public string plusImg="../treeimage/plus.gif";
		public string emptyImg="../treeimage/blank.gif";
		public string lineImg="../treeimage/blank.gif";

		public string cssUrl="../html/TreeTable.css";//"res://InfoService.html.TreeTable.css"
		public string scpUrl="../html/TreeTable.js";//"res://InfoService.html.treetable.js"
		
		public TreeTable()
		{
		}
		public NameValueCollection TableAttrs
		{
			get
			{
				return tableAttrs;
			}
		}
		/// <summary>
		/// �ṩһ�ְ���������˳����ӽ��ķ���,������ʱ�������EndAppend���������������ݡ�
		/// </summary>
		/// <param name="level"></param>
		/// <param name="useSecondImage"></param>
		/// <param name="name"></param>
		/// <param name="rowAttrs"></param>
		public void AppendNode(int level,bool useSecondImage,string name,params string[] rowAttrs)
		{
			NameValueCollection attrs=new NameValueCollection();
			for(int i=0;i<rowAttrs.Length-1;i+=2)
			{
				string n=rowAttrs[i];
				string v=rowAttrs[i+1];
				if(n==null || v==null)
					continue;
				attrs[n]=v;
			}
			AppendNode(level,useSecondImage,name,attrs);
		}
		/// <summary>
		/// �ṩһ�ְ���������˳����ӽ��ķ���,������ʱ�������EndAppend���������������ݡ�
		/// </summary>
		/// <param name="level"></param>
		/// <param name="useSecondImage"></param>
		/// <param name="name"></param>
		/// <param name="rowAttrs"></param>
		public void AppendNode(int level,bool useSecondImage,string name,NameValueCollection rowAttrs)
		{
			nodes.Push(new NodeInfo(level,useSecondImage,name,rowAttrs));
		}
		public void EndAppend()
		{
			while(nodes.Count>0)
			{
				NodeInfo ni=nodes.Pop() as NodeInfo;
				PushNode(ni.Level,ni.UseSecondImage,ni.Name,ni.RowAttrs);
			}
		}
		public void PushNode(int level,bool useSecondImage,string name,params string[] rowAttrs)
		{
			NameValueCollection attrs=new NameValueCollection();
			for(int i=0;i<rowAttrs.Length-1;i+=2)
			{
				string n=rowAttrs[i];
				string v=rowAttrs[i+1];
				if(n==null || v==null)
					continue;
				attrs[n]=v;
			}
			PushNode(level,useSecondImage,name,attrs);
		}
		/// <summary>
		/// ������������һ�����Ĵ��룬����������ϴε��õ�����Ϊ��׼�����ٶ��ϴε��ü���Ľ��
		/// Ϊ��ε��õĽ�������ϵĺ�һ����㣬Ҳ���ǵ����������ʱ��������������Ϲ��죬����
		/// ����ϡ���ָ��������Ӧ�ı��е��е����¡���
		/// ÿ��������Ӧһ��,���϶����˼�������:isExtend,level,useSecondImage,�������ⶨ�Ƶ�����
		/// �ɲ���rowAttrs��������.
		/// ÿһ����һ��һ�ж��еı���,ÿ����Ԫ�����һ��IMG��SPAN,IMG��SPAN�϶�����һ������:itag
		/// </summary>
		/// <param name="level"></param>
		/// <param name="useSecondImage"></param>
		/// <param name="name"></param>
		/// <param name="rowAttrs"></param>
		public void PushNode(int level,bool useSecondImage,string name,NameValueCollection rowAttrs)
		{
			const int width=20;
			//const int height=14;
			string line="";			
			string linetag="";
			string img=closeImg;
			if(lineList.Count>level)lineList[level]=false;
			if(lastLevel<level)
			{
				if(lastLevel>1)
				{
					if(lineList.Count>lastLevel)
						lineList[lastLevel-1]=true;
					else
					{
						for(int i=lineList.Count;i<lastLevel-1;i++)
						{
							lineList.Add(false);
						}
						lineList.Add(true);
					}
				}
				line=lineImg;
				linetag="imgimg";
			}
			else if(lastLevel==level)
			{
				line=lineImg;
				linetag="imgimg";
			}				
			else
			{
				line=minusImg;
				linetag="imgext";
			}
			if(useSecondImage)img=close2Img;
			string firstPart="<tr isExtend='true' level='"+level+"' useSecondImage='"+(useSecondImage?"true":"false")+"' ";
			foreach(string key in rowAttrs.AllKeys)
			{
				firstPart+=key+"='"+rowAttrs[key]+"' ";
			}
			string secondPart=		">\r\n"+
								"<td >\r\n"+
									"<table border=0 width=100% cellspacing=0 cellpadding=0 style='border-width:0px' >\r\n"+
										"<tr >\r\n";
			for(int i=0;i<level-2;i++)
			{
				string pad=emptyImg; 
				if(i<lineList.Count&&(bool)lineList[i]==true)
				{
					pad=lineImg;
				}
				secondPart+="<td width="+width+" style='font-size:1px' >\r\n"+
								"<img src='"+pad+"' >\r\n"+
							"</td>\r\n";
			}
			string thirdPart=				"<td width="+width+" style='font-size:1px' >\r\n"+
												"<img itag='"+linetag+"' src='"+line+"' >\r\n"+
											"</td>\r\n"+
											"<td width="+width+" style='font-size:1px' >\r\n"+											
												"<img itag='imgoc' src='"+img+"' >\r\n"+			
											"</td>\r\n"+
											"<td nowrap>\r\n"+
												"<span itag='txtfont' class='LanMouseOut'>\r\n"+
													name+
												"</span>\r\n"+
											"</td>\r\n"+
										"</tr>\r\n"+
									"</table>\r\n"+
								"</td>\r\n"+
							"</tr>\r\n";
			if(level==1)
			{
				thirdPart=
					"<td width="+width+" style='font-size:1px' >\r\n"+											
					"<img itag='imgext' src='"+rootImg+"' >\r\n"+			
					"</td>\r\n"+
					"<td nowrap>\r\n"+
					"<span itag='txtfont' class='LanMouseOut'>\r\n"+
					name+
					"</span>\r\n"+
					"</td>\r\n"+
					"</tr>\r\n"+
					"</table>\r\n"+
					"</td>\r\n"+
					"</tr>\r\n";
			}
			lastLevel=level;
			nodeStack.Push(firstPart+secondPart+thirdPart);
		}
		
		/// <summary>
		/// ���ؼ����뵽���ؼ���,ֻҪ��ʹ�ý��������,Ӧ���ǵ��ô˷�����������ؼ����뵽ҳ����,
		/// �˷����������ؼ�״̬��ʵ�ʴ��������ɿؼ���ǰ���档����ֵ�����Ƿ����������棬Ϊ��ʱӦ����ֹ��
		/// �����κν�����صĲ�������ʱ��Ӧ���ѹرգ���
		/// </summary>
		/// <param name="parent"></param>
		public bool AddToParent(Control parent)
		{
			if(EnsureControlEnvironment(parent))
				return true;
			parent.Controls.Add(this);
			return false;
		}
		/// <summary>
		/// ȷ���ؼ���ʾ�������Ļ���,ʹ�ý��������ʱ,Ӧ���ô˷���,��Ϊ�ؼ���ĳЩ״̬������Ҫ������
		/// ҳ������ʾ���ؼ���Ҫ��ҳ������Ӧ������ʹ�ý��������ʱ����һ�������ɣ������Զ����ô˷�
		/// ��������,�ؼ��Ŀͻ�����ԴURLҲ��Ҫ��˴�������ֵ�����Ƿ����������棬Ϊ��ʱӦ����ֹ��
		/// �����κν�����صĲ�������ʱ��Ӧ���ѹرգ���
		/// </summary>
		/// <param name="parent"></param>
		public bool EnsureControlEnvironment(Control parent)
		{
			if(ControlStateUrl.HandleResourceRequest())
				return true;
			if(parent.Page==null)
				return false;
			string refScript="<LINK href='"+ControlStateUrl.BuildResourceRequestUrl(cssUrl)+"' type='text/css' rel='stylesheet'>\r\n"+
				"<script language='javascript' src="+ControlStateUrl.BuildResourceRequestUrl(scpUrl)+"></script>\r\n"+
				@"
				<script language='javascript'>
					function getTree(obj)
					{
						if(!obj.tree)
						{
							var treeObj=new TreeTable('',obj);"+
				"			treeObj.rootImage='"+rootImg+"';\r\n"+
				"			treeObj.openImage='"+openImg+"';\r\n"+				
				"			treeObj.closeImage='"+closeImg+"';\r\n"+
				"			treeObj.open2Image='"+open2Img+"';\r\n"+				
				"			treeObj.close2Image='"+close2Img+"';\r\n"+
				"			treeObj.line='"+lineImg+"';\r\n"+
				"			treeObj.minus='"+minusImg+"';\r\n"+
				"			treeObj.plus='"+plusImg+"';\r\n"+
				"			treeObj.empty='"+emptyImg+"';\r\n"+
				@"			return treeObj;
						}
						return obj.tree;
					}
				</script>

				";
			parent.Page.RegisterClientScriptBlock("TreeTableClientScript",refScript);
			return false;
		}
		protected override void Render(HtmlTextWriter writer)
		{
			writer.Write(PopTable());
		}
		private string PopTable()
		{		
			tableHead="<table cellSpacing=0 cellPadding=0 border=0 ";
			foreach(string key in TableAttrs.AllKeys)
			{
				tableHead+=key+"='"+TableAttrs[key]+"' ";
			}
			tableHead+=">\r\n";	
			tableTail="</table>\r\n";

			StringBuilder sb=new StringBuilder();
			sb.Append(tableHead);
			while(nodeStack.Count>0)
				sb.Append(nodeStack.Pop() as string);
			sb.Append(tableTail);
			return sb.ToString();
		}
		private class NodeInfo
		{
			public int Level;
			public bool UseSecondImage;
			public string Name;
			public NameValueCollection RowAttrs;

			public NodeInfo(int level,bool useSecondImage,string name,NameValueCollection rowAttrs)
			{
				Level=level;
				UseSecondImage=useSecondImage;
				Name=name;
				RowAttrs=rowAttrs;
			}
		}
		private Stack nodes=new Stack();

		private NameValueCollection tableAttrs=new NameValueCollection();
		private Stack nodeStack=new Stack();
		private string tableHead=null;
		private string tableTail=null;		
		private int lastLevel=0;
		private ArrayList lineList=new ArrayList();	
	}
}
