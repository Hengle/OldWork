using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Dreaman.DataAccess;
using NUnit.Framework;

namespace Dreaman.InfoService
{
	public enum SQLParserClauseType
	{
		SELECT,
		FROM,
		WHERE,
		GROUPBYLIST,
		HAVING,
		ORDERBYLIST,
		PARENTHESES,
		EXPRESSION,
		COUNT,
		OTHER
	}
	public class SQLParserClause
	{
		public SQLParserClauseType Type=SQLParserClauseType.OTHER;
		public Queue Tokens=new Queue();
		public SQLParserClause()
		{}
		public SQLParserClause(SQLParserClauseType t)
		{
			Type=t;
		}
	}
	public class SQLParserContext : IParserContext
	{
		public SQLParserContext(string sentence)
		{
			StringCollection words=new StringCollection();
			sentence=sentence.Replace("\r"," ").Replace("\n"," ");
			sentence=sentence.Trim();
			int curPos=0;
			for(int i=0;i<sentence.Length;i++)
			{
				//���Կ��ַ�
				while(i<sentence.Length && (sentence[i]==' ' || sentence[i]=='	'))
				{
					i++;
				}
				curPos=i;
				//�����ַ���
				if(sentence[i]=='\'')
				{
					for(i++;;)
					{
						if(i<sentence.Length && sentence[i]!='\'')
							i++;
						else if(i+1<sentence.Length && sentence[i]=='\'' && sentence[i+1]=='\'')
							i+=2;
						else
							break;
					}
					words.Add(sentence.Substring(curPos,i-curPos+1));
				}
				else
				{
					while(i<sentence.Length && sentence[i]!=' ' && sentence[i]!='	' && Operators.IndexOf(sentence[i])<0)
						i++;
					if(i>curPos)
						words.Add(sentence.Substring(curPos,i-curPos));
					if(i<sentence.Length && Operators.IndexOf(sentence[i])>=0)
						words.Add(sentence[i].ToString());
				}
			}
			for(int i=0;i<words.Count;i++)
			{
				ParserToken t=new ParserToken();
				if(string.Compare(words[i],"select",true)==0)
				{
					t.Word="select";
					t.Type=ParserTokenType.KEYWORD;
				}
				else if(string.Compare(words[i],"from",true)==0)
				{
					t.Word="from";
					t.Type=ParserTokenType.KEYWORD;
				}
				else if(string.Compare(words[i],"where",true)==0)
				{
					t.Word="where";
					t.Type=ParserTokenType.KEYWORD;
				}
				else if(i+1<words.Count && string.Compare(words[i],"group",true)==0 && string.Compare(words[i+1],"by",true)==0)
				{
					t.Word="group by";
					t.Type=ParserTokenType.KEYWORD;
					i++;
				}
				else if(string.Compare(words[i],"having",true)==0)
				{
					t.Word="having";
					t.Type=ParserTokenType.KEYWORD;
				}
				else if(i+1<words.Count && string.Compare(words[i],"order",true)==0 && string.Compare(words[i+1],"by",true)==0)
				{
					t.Word="order by";
					t.Type=ParserTokenType.KEYWORD;
					i++;
				}
				else if(words[i]=="(" || words[i]==")")
				{
					t.Word=words[i];
					t.Type=ParserTokenType.OPERATOR;
				}
				else if(words[i]==",")
				{
					t.Word=words[i];
					t.Type=ParserTokenType.SEPARATOR;
				}
				else
				{
					t.Word=words[i];
					t.Type=ParserTokenType.IDENTIFY;
				}
				tokenList.Add(t);
				TokenPosition=0;
			}
		}
		public ParserToken PeekToken()
		{
			if(TokenPosition<tokenList.Count)
				return tokenList[TokenPosition] as ParserToken;
			return null;
		}
		public void AdvanceToken()
		{
			if(TokenPosition<tokenList.Count)
				TokenPosition++;
		}
		public int TokenPosition
		{
			get
			{
				return currentPosition;
			}
			set
			{
				currentPosition=value;
				if(currentPosition>tokenList.Count)
					currentPosition=tokenList.Count;
				if(currentPosition<0)
					currentPosition=0;
			}
		}
		public void PushClause(object arg)
		{
			SQLParserClauseType clauseType=SQLParserClauseType.OTHER;
			if(arg!=null)				
				clauseType=(SQLParserClauseType)arg;

			if(clauseType==SQLParserClauseType.PARENTHESES)
			{
				if(parenthesesLevel==0)
					parenthesesClause="";
				parenthesesLevel++;
			}
			if(clauseType==SQLParserClauseType.EXPRESSION)
			{
				if(expressionLevel==0)
					expClause="";
				expressionLevel++;
			}
			if(clauseType==SQLParserClauseType.COUNT)
			{
				tempLevel++;
			}
			if(clauseStack.Count<=3)
			{
				switch(clauseType)
				{
					case SQLParserClauseType.SELECT:
						tempClause="select";
						break;
					case SQLParserClauseType.FROM:
						tempClause="";
						break;
				}
			}
			if(clauseStack.Count<=7)
			{
				switch(clauseType)
				{
					case SQLParserClauseType.WHERE:
						tempClause="";
						break;
					case SQLParserClauseType.GROUPBYLIST:
						groupByListLevel++;
						tempClause="";
						break;
					case SQLParserClauseType.HAVING:
						tempClause="";
						break;
					case SQLParserClauseType.ORDERBYLIST:
						orderByListLevel++;
						tempClause="";
						break;
				}
			}

			SQLParserClause clause=new SQLParserClause(clauseType);
			clauseStack.Push(clause);
		}
		public void AppendToClause(ParserToken t)
		{
			if(clauseStack.Count>0)
			{
				SQLParserClause clause=clauseStack.Peek() as SQLParserClause;
				clause.Tokens.Enqueue(t);
			}
		}
		public void PopClause()
		{
			if(clauseStack.Count>0)
			{
				SQLParserClause clause=clauseStack.Pop() as SQLParserClause;
				string clauseStr="";
				while(clause.Tokens.Count>0)
				{
					ParserToken t=clause.Tokens.Dequeue() as ParserToken;
					string prestr=" ";
					if(clauseStr.Length>0)
					{
						char l=clauseStr[clauseStr.Length-1];
						char f=t.Word[0];
						if(NoSeperators.IndexOf(l)>=0 || NoSeperators.IndexOf(f)>=0)
							prestr="";
					}
					else
						prestr="";
					clauseStr+=prestr+t.Word;
				}
				if(clauseStr.Trim().Length>0)
				{
					string prestr=" ";
					if(tempClause.Length>0)
					{
						char l=tempClause[tempClause.Length-1];
						char f=clauseStr[0];
						if(NoSeperators.IndexOf(l)>=0 || NoSeperators.IndexOf(f)>=0)
							prestr="";
					}
					else
						prestr="";
					tempClause+=prestr+clauseStr;

					prestr=" ";
					if(parenthesesClause.Length>0)
					{
						char l=parenthesesClause[parenthesesClause.Length-1];
						char f=clauseStr[0];
						if(NoSeperators.IndexOf(l)>=0 || NoSeperators.IndexOf(f)>=0)
							prestr="";
					}
					else
						prestr="";
					parenthesesClause+=prestr+clauseStr;

					prestr=" ";
					if(expClause.Length>0)
					{
						char l=expClause[expClause.Length-1];
						char f=clauseStr[0];
						if(NoSeperators.IndexOf(l)>=0 || NoSeperators.IndexOf(f)>=0)
							prestr="";
					}
					else
						prestr="";
					expClause+=prestr+clauseStr;
				}
				if(clause.Type==SQLParserClauseType.PARENTHESES)
				{
					parenthesesLevel--;
					if(parenthesesLevel==0 && tempLevel==2 && parenthesesClause.Trim().Length>0)
					{
						Parentheses.Add(parenthesesClause.Trim());
					}
				}
				if(clause.Type==SQLParserClauseType.EXPRESSION)
				{
					expressionLevel--;
					if(expressionLevel==0 && expClause.Trim().Length>0)						
					{
						if(tempLevel==1)
						{
							Expressions.Add(expClause.Trim());
						}
						else if(tempLevel==3)
						{
							if(groupByListLevel==1)
							{
								GroupByExps.Add(expClause.Trim());
							}
							else if(orderByListLevel==1)
							{
								OrderByExps.Add(expClause.Trim());
							}
						}
					}
				}
				if(clause.Type==SQLParserClauseType.COUNT)
				{
					tempLevel--;
				}
				if(clauseStack.Count<=3)
				{
					switch(clause.Type)
					{
						case SQLParserClauseType.SELECT:
							SelectClause=tempClause.Trim();
							break;
						case SQLParserClauseType.FROM:
							FromClause=tempClause.Trim();
							break;
					}
				}
				if(clauseStack.Count<=7)
				{
					switch(clause.Type)
					{
						case SQLParserClauseType.WHERE:
							WhereClause=tempClause.Trim();
							break;
						case SQLParserClauseType.GROUPBYLIST:
							groupByListLevel--;
							GroupByList=tempClause.Trim();
							break;
						case SQLParserClauseType.HAVING:
							HavingClause=tempClause.Trim();
							break;
						case SQLParserClauseType.ORDERBYLIST:
							orderByListLevel--;
							OrderByList=tempClause.Trim();
							break;
					}
				}
			}
		}
		public string SelectClause
		{
			get
			{
				return selectClause;
			}
			set
			{
				selectClause=value;
			}
		}
		public string FromClause
		{
			get
			{
				return fromClause;
			}
			set
			{
				fromClause=value;
			}
		}
		public string WhereClause
		{
			get
			{
				return whereClause;
			}
			set
			{
				whereClause=value;
			}
		}
		public string GroupByList
		{
			get
			{
				return groupByList;
			}
			set
			{
				groupByList=value;
			}
		}
		public string HavingClause
		{
			get
			{
				return havingClause;
			}
			set
			{
				havingClause=value;
			}
		}
		public string OrderByList
		{
			get
			{
				return orderByList;
			}
			set
			{
				orderByList=value;
			}
		}
		public ArrayList Parentheses
		{
			get
			{
				return parentheses;
			}
		}
		public ArrayList Expressions
		{
			get
			{
				return expressions;
			}
		}
		public ArrayList GroupByExps
		{
			get
			{
				return groupByExps;
			}
		}
		public ArrayList OrderByExps
		{
			get
			{
				return orderByExps;
			}
		}
		public override string ToString()
		{
			string resultStr = "Select : "+SelectClause+"\r\n"
				+"From : "+FromClause+"\r\n"
				+"Where : "+WhereClause+"\r\n"
				+"Group By List : "+GroupByList+"\r\n"
				+"Having : "+HavingClause+"\r\n"
				+"Order By List : "+OrderByList+"\r\n";

			resultStr += "Expressions : \r\n";
			foreach(string s in Expressions)
			{
				resultStr+=s+"\r\n";
			}
			resultStr += "Parentheses : \r\n";
			foreach(string s in Parentheses)
			{
				resultStr+=s+"\r\n";
			}
			resultStr += "GroupByExps : \r\n";
			foreach(string s in groupByExps)
			{
				resultStr+=s+"\r\n";
			}
			resultStr += "OrderByExps : \r\n";
			foreach(string s in orderByExps)
			{
				resultStr+=s+"\r\n";
			}			
			return resultStr;
		}
		public static string Operators="<>=,.()+-*/^&|~!";
		public static string NoSeperators=",.";
					
		private string selectClause=null;
		private string fromClause=null;
		private string whereClause=null;
		private string groupByList=null;
		private string havingClause=null;
		private string orderByList=null;

		private ArrayList parentheses=new ArrayList();
		private ArrayList expressions=new ArrayList();
		private ArrayList groupByExps=new ArrayList();
		private ArrayList orderByExps=new ArrayList();

		private int tempLevel=0;
		private int expressionLevel=0;
		private int parenthesesLevel=0;
		private int groupByListLevel=0;
		private int orderByListLevel=0;

		private string tempClause="";
		private string expClause="";
		private string parenthesesClause="";

		private Stack clauseStack=new Stack();//�Ӿ�ջ(������ʱ������������Ӿ�)
		private ArrayList tokenList=new ArrayList();//�ʷ�������õ���ʶ���������
		private int currentPosition=0;//��ʶ��������е�ǰλ��
	}
	
	public class SQLSelectParser
	{
		public class StringLengthComparer : IComparer
		{
			#region IComparer ��Ա

			public int Compare(object x, object y)
			{
				string strx=x as string;
				string stry=y as string;
				if(strx==null || stry==null)
					return -1;
				if(strx.Length<stry.Length)
					return 1;
				else if(strx.Length>stry.Length)
					return -1;
				return 0;
			}

			#endregion

		}

		/// <summary>
		/// ���ﲻ����COMPUTE��UNION���㣬ʹ��COMPUTE�ĳ��ϲ�Ӧʹ�ñ��࣬���ʵ����Ҫʹ��UNION����Ҫд����ͼ��
		/// ��д��ͼ��ѯ����������MS SQL SERVER��ʹ��������ͼ��
		/// ����ֻ����SELECT���ĵ�һ��Ĺ��ɣ��������Ǽٶ�SQL����﷨����ȷ�ģ���Щ���Ժ������
		/// ϸ�ڵ��﷨���﷨�����EBNF���£���T��ʾ�սط���
		/// EXP::=[T*][(( '(' SELECT | (EXP ( , EXP )*) ')' ) | T)* ]
		/// EXPLIST::=EXP ( , EXP )*
		/// SELECT::=select EXPLIST from EXPLIST [where EXP][group by EXPLIST [having EXP]][order by EXPLIST]
		/// ��Ϊ���ǲ�ϣ��ʵ��ȫSQL�﷨�ķ����������ؼ���AS���﷨�г�Ϊ��������صģ��������ǲ����﷨�ж�������
		/// �����﷨������ɺ󣬶��﷨Ƭ�ϲ���������ʽ���ַ��������һ�������
		/// </summary>
		/// <param name="sqlstr"></param>
		public SQLSelectParser(string sqlstr)
		{
			context=new SQLParserContext(sqlstr);
			AndClause selectQuery=AbstractClause.NewAnd;

			OrClause exp=AbstractClause.NewOr;
			exp.Build(AbstractClause.Terminal.Repeat(),
				AbstractClause.Identic.And("(",
					selectQuery.Alternative(exp.And(AbstractClause.Identic.And(",",exp).Repeat())),
					")")[SQLParserClauseType.PARENTHESES].Alternative(AbstractClause.Terminal).Repeat()
			);//���ʽ�﷨����
			AbstractClause expression=exp[SQLParserClauseType.EXPRESSION];

			AbstractClause expList=expression.And(AbstractClause.Identic.And(",",expression).Repeat());//���ʽ�б��﷨����
			selectQuery.Build("select",
				expList[SQLParserClauseType.SELECT][SQLParserClauseType.COUNT],
				AbstractClause.Identic.And("from",expList)[SQLParserClauseType.FROM][SQLParserClauseType.COUNT][SQLParserClauseType.COUNT],
				AbstractClause.Nilpotent.Or(
					AbstractClause.Identic.And("where",expression[SQLParserClauseType.WHERE]),
					AbstractClause.Identic.And("group by",expList[SQLParserClauseType.GROUPBYLIST].Or("having",expression[SQLParserClauseType.HAVING])						
						),
					AbstractClause.Identic.And("order by",expList[SQLParserClauseType.ORDERBYLIST])
					)[SQLParserClauseType.COUNT][SQLParserClauseType.COUNT][SQLParserClauseType.COUNT]
				);//SELECT����﷨����

			selectQuery.Parse(context);
			//���������
			Parentheses.Sort(new StringLengthComparer());
			noParenthesesFrom=context.FromClause;
			int pi=0;
			foreach(string s in Parentheses)
			{
				noParenthesesFrom=noParenthesesFrom.Replace(s,"__EXP"+pi+"__");
				pi++;
			}
			string regex = @"
(?:
	from\s+(\w+(?:\s+(?!left)(?!rigth)(?!full)(?!inner)(?!outer)(?!join)\w+)?)\s*
		(?:
			(?:
				(?:
					(?:
						left|right|full|inner|outer|join
					)
					\s+
				)+
				(\w+(?:\s+(?!on)\w+)?)
				\s+on\s+(?:(?!left)(?!rigth)(?!full)(?!inner)(?!outer)(?!join)[^\s]+\s*)+
			)+
			|	
			(?:,\s*(\w+(?:\s+\w+)?)\s*)+
		)?
)";
			System.Text.RegularExpressions.RegexOptions options = ((System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace | System.Text.RegularExpressions.RegexOptions.Multiline) 
				| System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(regex, options);

			string regex0 = "(\\w+)(?:\\s+(\\w+))?";
			System.Text.RegularExpressions.Regex reg0 = new System.Text.RegularExpressions.Regex(regex0, options);

			Match m=reg.Match(noParenthesesFrom);
			if(m.Success)
			{
				for(int i=1;i<=3;i++)
				{
					Group g=m.Groups[i];
					foreach(Capture c in g.Captures)
					{
						Match m0=reg0.Match(c.Value);
						if(m0.Success)
						{
							if(m0.Groups[2].Captures.Count>0)
							{					
								tableAlias[m0.Groups[1].Value]=m0.Groups[2].Value;
							}
							else
							{
								tableAlias[m0.Groups[1].Value]=m0.Groups[1].Value;
							}
						}
					}
				}
			}
			//select �б���
			for(int i=0;i<Expressions.Count;i++)
			{
				string field=Expressions[i] as string;				
				//����[all | distinct][top n [percent] [with ties]]
				Regex regexp=new Regex("^(?:all|distinct)?\\s*(?:top\\s+[0-9]{0,3}\\s*(?:percent)?\\s*(?:with\\s+ties)?)?",options);
				Match match=regexp.Match(field.ToLower());
				if(match.Success)
				{
					field=field.Substring(match.Index+match.Length);
					Expressions[i]=field;
				}
				break;
			}
			//����SELECT�б������ֶ�ԭʼ��������Ķ�Ӧ��ϵ��SQL����е�*���޷����﷨����ʱ��չ�ɸ�������
			string regex1 = "(.*)(?:\\s+as\\s+(\\w+))?\\s* ";
			System.Text.RegularExpressions.RegexOptions options1 = (((System.Text.RegularExpressions.RegexOptions.RightToLeft | System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace) 
				| System.Text.RegularExpressions.RegexOptions.Multiline) 
				| System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			System.Text.RegularExpressions.Regex reg1 = new System.Text.RegularExpressions.Regex(regex1, options1);

			string regex2 = "(\\w+)\\.(\\w+)";
			System.Text.RegularExpressions.RegexOptions options2 = (((System.Text.RegularExpressions.RegexOptions.RightToLeft | System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace) 
				| System.Text.RegularExpressions.RegexOptions.Multiline) 
				| System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			System.Text.RegularExpressions.Regex reg2 = new System.Text.RegularExpressions.Regex(regex2, options2);
			
			for(int i=0;i<Expressions.Count;i++)
			{
				string field=Expressions[i] as string;
				Match match=reg1.Match(field);
				if(match.Success)
				{
					if(match.Groups[2].Captures.Count>0)
					{
						fieldAlias[match.Groups[1].Value]=match.Groups[2].Value;
						fieldNames[match.Groups[2].Value]=match.Groups[1].Value;
					}
					else
					{
						string alias=match.Groups[1].Value;
						Match mm=reg2.Match(alias);
						if(mm.Success)
						{
							alias=mm.Groups[2].Value;
						}
						fieldAlias[match.Groups[1].Value]=alias;
						fieldNames[alias]=match.Groups[1].Value;
					}
				}
			}
		}
		/// <summary>
		/// ȡ��ı���������ı�����SQL��ı�������ǰ�湹���Ӧ��ϵʱ��û�б����ı�ı�������ԭʼ����
		/// ����ͨ����������ķ��ز���Ϊnull
		/// </summary>
		/// <param name="tableName"></param>
		/// <returns></returns>
		public string TableAlias(string tableName)
		{
			return tableAlias[tableName];
		}
		/// <summary>
		/// ��ǰ�����Ǽ���ת��������,ȡSQL����ȷ����ı�����û�б������嵫�г�����SQL���
		/// SELECT��ʱ�����������в�������SELECT���򷵻ؿգ���select *Ҳ����ˣ�
		/// ���ؿ���Ҫ����GetGroupByList��GetOrderByList�Ĵ�����ֹ���Ƿ���δ�����ڽ������
		/// ����
		/// </summary>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public string FieldAlias(string fieldName)
		{
			return fieldAlias[fieldName];
		}
		/// <summary>
		/// ��ǰ�����Ǽ���ת��������,ȡSQL����ȷ����ı�����Ӧ��ԭʼ����û�б������嵫�г���
		/// ��SQL���SELECT��ʱ�����������в�������SELECT���򷵻ؿգ���select *Ҳ����ˣ�
		/// ���ؿ���Ҫ����GetGroupByList��GetOrderByList�Ĵ�����ֹ���Ƿ���δ�����ڽ������
		/// ����
		/// </summary>
		/// <param name="fieldAlias"></param>
		/// <returns></returns>
		public string FieldName(string fieldAlias)
		{
			return fieldNames[fieldAlias];
		}
		/// <summary>
		/// ���SQL����ȷ�����˱������򷵻���ȷ����ı�����Ӧ��ԭʼ�������򣬸��ݻ��������ֶ���������ȫ�޶�������
		/// ����SQL����е�*���޷����﷨����ʱ��չ�ɸ����������ʲ��Ϊ��ʱֱ�ӷ��ز���ֵ
		/// </summary>
		/// <param name="fieldAlias"></param>
		/// <param name="baseTable"></param>
		/// <param name="baseField"></param>
		/// <returns></returns>
		public string FieldName(string fieldAlias,string baseTable,string baseField)
		{
			string result=FieldName(fieldAlias);
			if(result!=null && string.Compare(result,fieldAlias,true)!=0)
				return result;
			if(baseTable!=null && baseField!=null)
			{
				string s=TableAlias(baseTable);
				if(s==null)
					return fieldAlias; 
				return s+"."+baseField;
			}
			return fieldAlias;
		}
		public bool HaveGroupBy()
		{
			if(GroupByExps.Count>0)
				return true;
			return false;
		}
		public string BuildSQL(string condition,string having,params string[] sorts)
		{
			string sqlstr=SelectClause+" "+FromClause;
			if(condition!=null && condition.Trim().Length>0)
			{
				if(WhereClause==null || WhereClause.Trim().Length<=0)
				{
					sqlstr+=" where "+condition;
				}
				else
				{
					sqlstr+=" where ( "+condition+" ) and ( "+WhereClause+" )";
				}
			}
			else if(WhereClause!=null && WhereClause.Trim().Length>0)
			{
				sqlstr+=" where "+WhereClause;
			}
			if(GroupByList!=null && GroupByList.Trim().Length>0)
			{
				sqlstr+=" group by "+GroupByList;
				if(having!=null && having.Trim().Length>0)
				{//�û�ָ�������������з���,���û�ָ�������ϲ���HAVING�Ӿ���
					if(HavingClause==null || HavingClause.Trim().Length<=0)
					{
						sqlstr+=" having "+having;
					}
					else
					{
						sqlstr+=" having ( "+having+" ) and ( "+HavingClause+" )";
					}
				}
				else if(HavingClause!=null && HavingClause.Trim().Length>0)
				{
					sqlstr+=" having "+HavingClause;
				}
			}
			if(sorts.Length>0)
			{
				sqlstr+=" "+string.Join(",",sorts);
			}
			else if(OrderByList!=null && OrderByList.Trim().Length>0)
			{
				sqlstr+=" order by "+OrderByList;
			}
			return sqlstr;
		}
		/// <summary>
		/// �õ�SQL��GROUP BY�Ӿ��е��ֶ��б�����б����Ĳ��ñ�����ʾ��Ҳ����SQL������е����ƣ�
		/// ����Ϊ�﷨����ʱ�޷�����select�Ӿ���*������У�����ķ��ؿ��ܻ᲻ȫ
		/// </summary>
		/// <returns></returns>
		public string[] GetGroupByList()
		{
			if(GroupByExps.Count<=0)
				return null;
			StringCollection retStrs=new StringCollection();
			foreach(string str in GroupByExps)
			{
				if(str.Trim().ToUpper()=="ALL")
					continue;
				string alias=FieldAlias(str.Trim());
				if(alias!=null)//��ʱ�п��ܲ������ڽ�����У��ʲ���������
					retStrs.Add(alias);
			}
			string[] rets=new string[retStrs.Count];
			retStrs.CopyTo(rets,0);
			return rets;
		}
		/// <summary>
		/// �õ�SQL��ORDER BY�Ӿ��е��ֶ��б���ASC��DESC���֣�������б����Ĳ��ñ�����ʾ��Ҳ����SQL������е����ƣ�
		/// ����Ϊ�﷨����ʱ�޷�����select�Ӿ���*������У�����ķ��ؿ��ܻ᲻ȫ
		/// </summary>
		/// <returns></returns>
		public string[] GetOrderByList()
		{
			if(OrderByExps.Count<=0)
				return null;
			StringCollection retStrs=new StringCollection();
			foreach(string str in OrderByExps)
			{
				string temp=str.Trim();
				string postfix="";
				if(temp.ToUpper().EndsWith("ASC"))
					temp=temp.Substring(0,temp.Length-3).Trim();
				else if(temp.ToUpper().EndsWith("DESC"))
				{
					temp=temp.Substring(0,temp.Length-4).Trim();
					postfix=" DESC";
				}
				string alias=FieldAlias(temp);
				if(alias!=null)
				{
					retStrs.Add(alias+postfix);
				}
				else
				{
					if(FieldName(temp)!=null)//���������Ѿ��Ǳ�����ֱ�Ӱ�������
					{
						retStrs.Add(temp+postfix);
					}
				}
			}
			string[] rets=new string[retStrs.Count];
			retStrs.CopyTo(rets,0);
			return rets;
		}
		public string SelectClause
		{
			get
			{
				return context.SelectClause;
			}
		}
		public string FromClause
		{
			get
			{
				return context.FromClause;
			}
		}
		public string WhereClause
		{
			get
			{
				return context.WhereClause;
			}
		}
		public string GroupByList
		{
			get
			{
				return context.GroupByList;
			}
		}
		public string HavingClause
		{
			get
			{
				return context.HavingClause;
			}
		}
		public string OrderByList
		{
			get
			{
				return context.OrderByList;
			}
		}
		public ArrayList Parentheses
		{
			get
			{
				return context.Parentheses;
			}
		}
		public ArrayList Expressions
		{
			get
			{
				return context.Expressions;
			}
		}
		public ArrayList GroupByExps
		{
			get
			{
				return context.GroupByExps;
			}
		}
		public ArrayList OrderByExps
		{
			get
			{
				return context.OrderByExps;
			}
		}
		public string NoParenthesesFrom
		{
			get
			{
				return noParenthesesFrom;
			}
		}
		public override string ToString()
		{
			string tastr="Table Alias : ";
			foreach(string s in tableAlias.Keys)
			{
				tastr+="\r\n"+s+" : "+tableAlias[s];
			}
			tastr+="\r\nField Alias : ";
			foreach(string s in fieldAlias.Keys)
			{
				tastr+="\r\n"+s+" : "+fieldAlias[s];
			}
			return context.ToString()+
				"NoParentheses From Clause : "+noParenthesesFrom+"\r\n"+
				tastr;
		}
		private SQLParserContext context=null;
		private string noParenthesesFrom=null;
		private StringDictionary tableAlias=new StringDictionary();
		private StringDictionary fieldAlias=new StringDictionary();
		private StringDictionary fieldNames=new StringDictionary();
	}

	[TestFixture]
	public class SQLSelectParserTest
	{
		[Test]
		public void TestParse()
		{
			Console.Out.WriteLine("--Simple--");
			SQLSelectParser parser=new SQLSelectParser("select distinct * from table where a=b group by a,b,c order by c,e,f");
			Console.Out.WriteLine(parser);
			Console.Out.WriteLine("Field:"+parser.FieldAlias("*"));
			Console.Out.WriteLine("--Alias--");
			parser=new SQLSelectParser("select all a as aa, b as bb, c as [cc] from [table] TableName where a=b and c is null group by a,b,c order by c,e,f");
			Console.Out.WriteLine(parser);
			Console.Out.WriteLine("Field:"+parser.FieldAlias("aa"));
			Console.Out.WriteLine("--No Where--");
			parser=new SQLSelectParser("select a,b,c,d from aaa a,bbb b,ccc c group by a,b,c order by c,e,f");
			Console.Out.WriteLine(parser);
			Console.Out.WriteLine("--No Where , No Group By--");
			parser=new SQLSelectParser("select top 100 * from aaa a,bbb b,ccc c order by c,e,f");
			Console.Out.WriteLine(parser);
			Console.Out.WriteLine("Field:"+parser.FieldAlias("*"));
			Console.Out.WriteLine("--No Group By--");
			parser=new SQLSelectParser("select a,b,c,d from aaa a,bbb b,ccc c  where a=b and c is null order by c,e,f");
			Console.Out.WriteLine(parser);
			Console.Out.WriteLine("--No Order By--");
			parser=new SQLSelectParser("select * from aaa a inner join bbb b on a.f=b.f and a.ff=b.ff Where a=b and c is null Group By a,b,c ");
			Console.Out.WriteLine(parser);
			Console.Out.WriteLine("--No Order By , cast , join , sub query--");		
			parser=new SQLSelectParser("select *,cast( a as b) from aaa a inner join bbb b on a.f=b.f and a.ff=b.ff Where a=b and c is null and d is (select * from t) Group By a,b,c ");
			Console.Out.WriteLine(parser);
			Console.Out.WriteLine("--Complex--");
			parser=new SQLSelectParser("select distinct top 100 *,a as b,1+4 as c,(5,6,7) as t,f(1,2,3) as tt from aaa a inner join (select * from bbb) b on a.f=b.f and a.ff=b.ff Where a=b and (c is null) and d in (select * from t where a=b group by c,d order by f) Group By all having a in (select * from tt) order by c,d,e");
			Console.Out.WriteLine(parser);

			Console.Out.WriteLine("* Field:"+parser.FieldAlias("*"));
		}
	}
}
