using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using Dreaman.DataAccess;
using Dreaman.InfoService;

namespace Dreaman.InfoService.Report
{
	/// <summary>
	/// IDataBlock ��ժҪ˵����
	/// </summary>
	public interface IDataBlock
	{
		/// <summary>
		/// �õ�ǰ���ݿ��ֵ�滻ָ��ģ�������е����ݱ�ǩ,һ��ֵ��Ӧһ�����ݱ�ǩ
		/// ,����ı�ǩ���滻Ϊ��,�����ֵ��ʹ��
		/// ��һ������Ҫ���ڻ���ģ��ı���,ģ��ͨ�����ݱ�ǩ������Ϊ�������ݿ�,
		/// ÿ�����ݿ��Ӧһ��IDataBlock,ģ�����ֵ���̱��Ϊģ����������������
		/// ��IDataBlock������(�ܵ�-������ģʽ)
		/// </summary>
		/// <param name="templateContent"></param>
		/// <param name="dataTag">
		/// ���ݱ�ǩ��ʽ���£�(���ݱ�ǩ���ǳ����ڵ�Ԫ���У�ǰ��Ҫ�пո�,>�����ڱ�ǩ�������滻Ϊ��Ԫ�������ݵ�����)
		/// >$dataTag!fieldName$
		/// ��
		/// >$dataTag$
		/// 
		/// ʶ��������ʽ:
		/// 1���ض����ݱ�ǩ
		/// >\$dataTag(?:!(\w+))*\$
		/// 2�����з��ϸ�ʽ�Ŀ��ܱ�ǩ
		/// \$(\w+)(?:!(\w+))*\$
		/// </param>
		/// <returns></returns>
		string Filter(string templateContent,string dataTag);
		/// <summary>
		/// ���ڶ�ȡ��ǰ���ݿ�UI��HTML����,����ٶ����ݿ�֪�����Լ�Ӧ�����ʾ,
		/// ����ί�и���ʾ����,�������Ǽٶ����ݿ���һЩʵ������ָ������Ҫ����
		/// �ĸ���ʾ����,�������ݿ�������Ӧ����ʾ�����������ʾ��������߼�
		/// �������������ݿ��Լ���֪���������������ʾ�������仰˵�������MVC
		/// �ĽǶȿ��ǣ����ݿ����ģ�ͣ����ǿ�����������MVCȫ�����ݿ飩
		/// ��һ�������ڲ�ʹ��ģ������ݿ���߶�Ӧ��ģ���е�һ�����ݿ��ǩ(��
		/// ��������������,һ��ģ��ȷ���ܵĲ���,�������ÿ�����ݱ�ǩ��Ӧһ����
		/// �ݿ�,���������ݿ鸺���Լ�����ʾ)
		/// ���ڿ�������ҪӦ����������Ϊģ��������ݿ��ǩ���滻������ģ���ǻ�
		/// ��HTML�ģ�������������EXCEL����ģ�壬���ΪHTML�ļ�����MHTML�ļ���
		/// ��˿���Ƕ��ͼƬ֮��������ļ�����Ϊģ���ļ�
		/// </summary>
		string GetDataBlockUI(string[] fieldNames);
		/// <summary>
		/// �������ݱ�ǩδָ�е����Σ����õ�����Ĭ�ϵ���ʾUI
		/// </summary>
		/// <returns></returns>
		string GetDataBlockUI();
	}
	/// <summary>
	/// �����滻һ�����ݱ�ǩ���¼�����,fieldNameΪ�ձ�����ǩ��û��ָ��fieldName,indexΪ
	/// ��ӦfieldName�ı�ǩ���
	/// </summary>
	public delegate string FilterDataLabel(string fieldName,int index);

	public sealed class DataBlockUtility
	{
		public static SQLDataBlock NewSQLDataBlock(string sql,DataAccessType datype,string connstr)
		{
			return new SQLDataBlock(sql,datype,connstr);
		}
		public HybridDictionary DataLabels
		{
			get
			{
				return dataLabels;
			}
		}
		public void ParseTemplate(string templateContent)
		{			
			templateContent=PreprocessTemplate(templateContent);
			//ʹ��������ʽ���г�ģ�������п��ܵ����ݱ�ǩ
			string regex = @"\$(\w+)(?:!(\w+))*\$";
			System.Text.RegularExpressions.RegexOptions options = ((System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace | System.Text.RegularExpressions.RegexOptions.Multiline) 
				| System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(regex, options);
			MatchCollection mc=reg.Matches(templateContent);
			foreach(Match m in mc)
			{
				if(m.Groups[1].Captures.Count>0)
				{
					string tag=m.Groups[1].Value;
					StringCollection sc=DataLabels[tag] as StringCollection;
					if(sc==null)
					{
						sc=new StringCollection();
						DataLabels[tag]=sc;
					}
					foreach(Capture c in m.Groups[2].Captures)
					{
						string field=c.Value;
						if(sc.IndexOf(field)<0)
						{
							sc.Add(field);
						}
					}
				}
			}
		}

		public event FilterDataLabel FilterDataLabel;
		public string Filter(string templateContent, string dataTag)
		{
			templateContent=PreprocessTemplate(templateContent);
			lock(this)
			{				
				indexes=new HybridDictionary(true);
				nullIndex=0;
				//ʹ��������ʽ��ʶ����ϸ�ʽ�����ݱ�ǩ,���ݱ�ǩ�������>�ǵ�Ԫ��TD��ǩ��һ����,�����趨��Ԫ����������,
				//�� x:num>,�滻ʱӦ����" x:num>"+fieldValue��">"+fieldValue����ʽ
				string regex = ">\\$"+dataTag+"(?:!(\\w+))*\\$";
				RegexOptions options = (RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
				Regex reg = new Regex(regex,options);
				return reg.Replace(templateContent,new MatchEvaluator(this.MatchFilter));
			}
		}

		private string OnFilterDataLabel(string fieldName,int index)
		{
			if(FilterDataLabel!=null)
				return FilterDataLabel(fieldName,index);
			return ">";
		}

		//�滻һ�����ݱ�ǩ
		private string MatchFilter(Match match)
		{
			//��ӦFilter�����е�������ʽ���ڶ����������ݱ�ǩ���fieldName���֣�������Ϊ0����1�����߶��
			//Ϊ1����ͨ���İ����滻��Ϊ0���������ݿ�Ĭ��UI�滻��Ϊ������ö������ݿ�UI�滻��
			int ct=match.Groups[1].Captures.Count;
			if(ct>1)
			{
				string fieldName="",prestr="";
				foreach(Capture c in match.Groups[1].Captures)
				{
					fieldName+=prestr+c.Value;
					prestr=",";
				}

				object o=indexes[fieldName];
				if(o==null)
				{
					indexes[fieldName]=0;
					return OnFilterDataLabel(fieldName,0);
				}
				else
				{
					int i=(int)o+1;
					indexes[fieldName]=i;
					return OnFilterDataLabel(fieldName,i);
				}
			}
			else if(ct==1)
			{
				string fieldName=match.Groups[1].Value;

				object o=indexes[fieldName];
				if(o==null)
				{
					indexes[fieldName]=0;
					return OnFilterDataLabel(fieldName,0);
				}
				else
				{
					int i=(int)o+1;
					indexes[fieldName]=i;
					return OnFilterDataLabel(fieldName,i);
				}
			}
			else
			{
				return OnFilterDataLabel(null,nullIndex++);
			}
		}
		private string PreprocessTemplate(string templateContent)
		{
			//����ʽ���嵥Ԫ����������
			string regex0 = @"
<td
	(?:
		(?:\s+(?!x:fmla)(?:\w+(?::\w+)?)(?:=(?:""[^""]*""|'[^']*'|\w*))?)*
		\s+x:fmla=(?:""[^""]*""|'[^']'|\w*)
		(?:\s+(?!x:fmla)(?:\w+(?:\:\w+)?)(?:=(?:""[^""]*""|'[^']*'|\w*))?)*
	)+
\s*>
(
	(?:
		<(?:(?!td)\w+)[^>]*>
		|
		</\s*(?:(?!td)\w+)\s*>
		|
		[^<]+
	)*
)
</\s*td\s*>";
			System.Text.RegularExpressions.RegexOptions options0 = ((System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace | System.Text.RegularExpressions.RegexOptions.Multiline) 
				| System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			System.Text.RegularExpressions.Regex reg0 = new System.Text.RegularExpressions.Regex(regex0, options0);
			templateContent=reg0.Replace(templateContent,new MatchEvaluator(this.ClearFormulaValue));
			//������ݱ�ǩ�е�Ƕ��HTML��ǩ,��Ϊд��һ��������ʽ��ĳЩ�ĵ��ᵹ��������ʽ�������������ǽ����ֳ����ɸ����������
			//�ҳ�<td></td>����$��ͷ��$��β��ģʽ����$��$֮���п�����<td></td>���ڿ�ʼ<td>���һ��$��ֻ�з�<td></td>HTML��ǩ��&nbsp;
			//�ڵڶ���$�����</td>֮��Ҳֻ�з�<td></td>HTML��ǩ��&nbsp;
			string regex = @"
<td[^>]*>
	(
		(?:
			(?:</\s*(?:(?!td)\w+)\s*>)
			|
			\s*&nbsp;\s*
		)*
	)
	(
		\$[^\$]+
	)
	(
		\$
		(?:
			<(?:(?!td)\w+)[^>]*>
			|
			(?:</\s*(?:(?!td)\w+)\s*>)
			|
			\s*&nbsp;\s*
		)*
	)
</\s*td\s*>";
			System.Text.RegularExpressions.RegexOptions options = ((System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace | System.Text.RegularExpressions.RegexOptions.Multiline) 
				| System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(regex, options);
			return reg.Replace(templateContent,new MatchEvaluator(this.RemoveHtmlTag));
		}
		private string ClearFormulaValue(Match match)
		{
			if(match.Groups[1].Value.Length<=0)
				return match.Value;
			return match.Value.Replace(match.Groups[1].Value," ");
		}
		private string RemoveHtmlTag(Match match)
		{
			string tdContent=match.Value;

			string frontSpace=match.Groups[1].Value;
			string dataLabel=match.Groups[2].Value;
			string followSpace=match.Groups[3].Value;
			string repTarget=frontSpace+dataLabel+followSpace;
			//�������$֮��Ĳ����Ƿ����һ�����ݱ�ǩ��ģʽ�������˷�<td></td>HTML��ǩ�⣬Ϊ����ʶ�� �� �� �� =\r\n�� �����
			string regex0 = @"
\$
(?:
	(?:
		<(?:(?!td)\w+)[^>]*>
		|
		(?:</\s*(?:(?!td)\w+)\s*>)
	)*
	(?:
		!
		|
		\w+
		|
		=[\r\n]+
	)+
	(?:
		<(?:(?!td)\w+)[^>]*>
		|
		(?:</\s*(?:(?!td)\w+)\s*>)
	)*
)+";
			System.Text.RegularExpressions.RegexOptions options0 = ((System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace | System.Text.RegularExpressions.RegexOptions.Multiline) 
				| System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			System.Text.RegularExpressions.Regex reg0 = new System.Text.RegularExpressions.Regex(regex0, options0);

			Match m=reg0.Match(dataLabel);
			if(!(m.Success && m.Index==0 && m.Length==dataLabel.Length))
				return tdContent;

			string regex = @"<[^>]*>";
			System.Text.RegularExpressions.RegexOptions options = ((System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace | System.Text.RegularExpressions.RegexOptions.Multiline) 
				| System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(regex, options);

			if(frontSpace.Trim().Length>0)
			{
				frontSpace=reg.Replace(frontSpace,"");
			}
			else
			{
				frontSpace="";
			}
			if(followSpace.Trim().Length>0)
			{
				followSpace=reg.Replace(followSpace,"");
			}
			else
			{
				followSpace="";
			}
			dataLabel=reg.Replace(dataLabel,"");
			//MHTML�ļ����ܻ���=\r\n���������ݱ�ǩ�У�ɾ������
			dataLabel=dataLabel.Replace("=","");
			dataLabel=dataLabel.Replace("\r","");
			dataLabel=dataLabel.Replace("\n","");
			return tdContent.Replace(repTarget,frontSpace+dataLabel+followSpace);
		}

		//���ڼ�¼ģ�崦������е��ֶ�ֵ����
		private HybridDictionary indexes=null;
		private int nullIndex=0;
		//���ڼ�¼ģ���е����ݱ�ǩ����Ϣ
		private HybridDictionary dataLabels=new HybridDictionary(true);
	}
}
