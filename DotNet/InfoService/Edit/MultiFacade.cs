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
using Microsoft.Win32;
using System.Reflection;

namespace Dreaman.InfoService
{
	/// <summary>
	/// IMultiFacadeControl ��ժҪ˵����
	/// </summary>
	public interface IMultiFacadeControl
	{
		/// <summary>
		/// ���ؼ����뵽���ؼ���,ֻҪ��ʹ�ý��������,Ӧ���ǵ��ô˷��������ؼ����뵽ҳ����,
		/// �˷����������ؼ�״̬��ʵ�ʴ��������ɿؼ���ǰ���档
		/// ����ֵ�����Ƿ����������棬Ϊ��ʱӦ����ֹ�������κν�����صĲ�������ʱ��Ӧ���ѹرգ���
		/// </summary>
		/// <param name="parent"></param>
		bool AddToParent(Control parent);
		/// <summary>
		/// �����ؼ���ʾ�������Ļ���,ʹ�ý��������ʱ,Ӧ���ô˷���,��Ϊ�ؼ���ĳЩ״̬������Ҫ������
		/// ҳ������ʾ���ؼ���Ҫ��ҳ������Ӧ������ʹ�ý��������ʱ����һ�������ɡ�
		/// ����,�ؼ��Ŀͻ�����ԴURLҲ��Ҫ��˴�������ֵ�����Ƿ����������棬Ϊ��ʱӦ����ֹ��
		/// �����κν�����صĲ�������ʱ��Ӧ���ѹرգ���
		/// </summary>
		bool AdjustControlEnvironment();
		/// <summary>
		/// �ı�ؼ���ָ��״̬
		/// </summary>
		/// <param name="state">״̬��</param>
		/// <param name="args">�������ϣ�����/ֵ�ԣ�</param>
		void ChangeState(string state,NameValueCollection args);
		/// <summary>
		/// �ı�ؼ���ָ��״̬
		/// </summary>
		/// <param name="state">״̬��</param>
		/// <param name="args">�������飬����Ϊ������ֵ������ֵ��...</param>
		void ChangeState(string state,params string[] args);
	}
	public interface IControlState
	{
		/// <summary>
		/// �ؼ�״̬�ӿڣ���һ������Ϊ��Ͽؼ�����Ͽؼ�����״̬����֮���ǽ����
		/// ������һ�������ǻ�����ת�͵��������Ͽؼ����ò�����������һ���Ϻõ�
		/// ѡ�񡣣���Ͽؼ��е�״̬���󹤳���ְ��ͬʱΪ״̬�����ṩ�����ģ�
		/// </summary>
		/// <param name="ctrl"></param>
		/// <param name="stateargs"></param>
		void Handle(IMultiFacadeControl ctrl,string stateargs);
	}
	public sealed class ContentEncoder
	{
		private ContentEncoder()
		{}
		/// <summary>
		/// �����ͻ���HTML���ݵ��ַ����ô˷�������(�ͻ���Դ���п�������������)
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string HtmlEncode(string s)
		{
			string str=HttpContext.Current.Server.HtmlEncode(s);
			return str.Replace("'","&#39;");
		}
		/// <summary>
		/// �����ͻ��˽ű��������ַ����ô˷�������(��Щ�ַ����ڿͻ���Դ�������ǳ����ַ���,�ͻ���Դ���п�������������)
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string StringEncode(string s)
		{
			return s.Replace("'","\\'").Replace("\"","\\\"\\\"");
		}
		/// <summary>
		/// ����URL��������ֵ���ַ����ı���
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string UrlEncode(string s)
		{
			string str=HttpContext.Current.Server.UrlEncode(s);
			return str;
		}
	}
	public sealed class ControlStateUrl
	{		
		private ControlStateUrl()
		{}
		/// <summary>
		/// ���������ض�״̬�����URL
		/// </summary>
		public static string BuildStateRequestUrl(string stateKey,string stateVal)
		{		
			UriBuilder ub=new UriBuilder(HttpContext.Current.Request.Url);
			if(ub.Query==null || ub.Query.Trim().Length<=0)
			{
				ub.Query=stateKey+"="+stateVal;
			}
			else
			{
				string val=HttpContext.Current.Request[stateKey];
				if(val==null)
					ub.Query=ub.Query.Substring(1)+"&"+stateKey+"="+stateVal;
				else
					ub.Query.Replace(stateKey+"="+val,stateKey+"="+stateVal);
			}
			return ub.Uri.ToString();
		}
		/// <summary>
		/// ������Դ����URL
		/// </summary>
		/// <param name="resName"></param>
		/// <returns></returns>
		public static string BuildResourceRequestUrl(string resName)
		{
			if(resName.ToLower().StartsWith("res://"))
				return BuildStateRequestUrl("getresource",resName.Substring(6));
			else
				return resName;
		}
		/// <summary>
		/// ���������������Ϊ��Դ��������յ�ǰ��Ӧ��д����Դ��Ӧ����ֹ��Ӧ��
		/// </summary>
		/// <returns></returns>
		internal static bool HandleResourceRequest()
		{
			string resName=HttpContext.Current.Request["getresource"];
			if(resName!=null)
			{
				RequestEmbedResource(resName,typeof(ControlStateUrl).Assembly);
				return true;
			}
			return false;
		}
		/// <summary>
		/// ���������������Ϊ��Դ��������յ�ǰ��Ӧ��д����Դ��Ӧ����ֹ��Ӧ��
		/// </summary>
		/// <returns></returns>
		public static bool HandleResourceRequest(Assembly assembly)
		{
			string resName=HttpContext.Current.Request["getresource"];
			if(resName!=null)
			{
				RequestEmbedResource(resName,assembly);
				return true;
			}
			return false;
		}
		public static void RequestEmbedResource(string resName,Assembly assembly)
		{
			HttpResponse response=HttpContext.Current.Response;
			int li=resName.LastIndexOf('.');
			string ext=".bin";
			if(li>0)
				ext=resName.Substring(li).Trim();
			RegistryKey reg=Registry.ClassesRoot;
			RegistryKey subKey=reg.OpenSubKey(ext);
			string s=subKey.GetValue("Content Type") as string;
			if(s!=null)
				response.ContentType=s;
			else
				response.ContentType="application/octet-stream";

			string[] resNames=assembly.GetManifestResourceNames();
			foreach(string res in resNames)
			{
				if(string.Compare(res,resName,true)==0)
				{
					Stream stream=assembly.GetManifestResourceStream(res);
					response.Clear();
					if(stream==null)
					{
						response.StatusCode=404;
						response.End();
					}
					else
					{
						response.Clear();
						byte[] buf=new byte[stream.Length];
						stream.Read(buf,0,buf.Length);
						stream.Close();
						response.BinaryWrite(buf);
						response.End();
					}
					break;
				}
			}
		}
	}
}
