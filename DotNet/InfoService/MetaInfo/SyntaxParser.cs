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
using Dreaman.DataAccess;
using NUnit.Framework;

namespace Dreaman.InfoService
{
	/// <summary>
	/// ��ʶ������
	/// </summary>
	public enum ParserTokenType
	{
		KEYWORD,
		OPERATOR,
		IDENTIFY,
		SEPARATOR
	}
	/// <summary>
	/// ��ʶ���࣬һ����ʶ����Ҫ�����䵥�������ͣ���Щ��Ϣ�ڴʷ�����ʱȷ��
	/// </summary>
	public sealed class ParserToken
	{
		public string Word;
		public ParserTokenType Type;
		public ParserToken(){}
		public ParserToken(string word,ParserTokenType type)
		{
			Word=word;
			Type=type;
		}
	}
	/// <summary>
	/// ������ʹ�õ������Ľӿ�,��ҪĿ��:
	/// 1���ṩ�ʷ������Ľ��
	/// 2����������Ĵ���
	/// </summary>
	public interface IParserContext
	{
		ParserToken PeekToken();//��ȡ��ǰ��ʶ
		void AdvanceToken();//�ƽ���ʶ���������
		void PushClause(object arg);//ѹ��һ���ؼ����Ӿ䣬���������ѷ�����
									//ͨ��Ӧ�ô����Ӿ�����һ�����Ϣ
		void AppendToClause(ParserToken t);//��ջ���ؼ����Ӿ��м���һ����ʶ
		void PopClause();//����һ���ؼ����Ӿ䣬ͨ��������Ӧ��Դ��Ӿ���Щ����
						 //ͨ��Ӧ�ý�������д����һ�����Ӿ���
		int TokenPosition//��ʶ��������е�ǰλ��
		{
			get;set;
		}
	}
	/// <summary>
	/// �����Ӿ䣬�ṩ���Ӿ���Ϊ�Ľӿ��븴���Ӿ�Ļ��������뷽��
	/// </summary>
	public abstract class AbstractClause
	{
		/// <summary>
		/// ���������ӿڣ���������ί��ParseImplʵ��,���������������ƥ��ʧ��ʱ������Ҫ��
		/// ��ʶ������״̬����Ϊ�������������EBNF�﷨������������������֧�ֻ�����ȷ����
		/// ȷ�ԣ���EBNFͨ���ǿ���д��������ݵģ�����ǰ�LL��1���������EBNF���򱾷���
		/// ��������ݡ�����һ���������Ӿ�Ĵ��������������ɵȣ�����Щ��ί�е�
		/// IParserContext��ʵ���ࡣ
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public bool Parse(IParserContext context)
		{
			//�����ʶ��������е�״̬
			context.PushClause(ClauseArgument);
			try
			{
				int pos=context.TokenPosition;
				if(!ParseImpl(context))
				{
					//���ʧ���򽫱�ʶ����������趨��������Ӿ�ʱ��״̬
					context.TokenPosition=pos;
					return false;
				}
				return true;
			}
			finally
			{
				context.PopClause();
			}
		}
		/// <summary>
		/// �������ܵľ���ʵ��
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		protected abstract bool ParseImpl(IParserContext context);
		protected virtual object ClauseArgument
		{
			get
			{
				return null;
			}
		}
		/// <summary>
		/// ���ڴ洢����IParserContext::PushClause�Ĳ���
		/// </summary>
		public AbstractClause this[object arg]
		{
			get
			{
				return new ArgumentClause(this,arg);
			}
		}
		public AbstractClause Or(params object[] args)
		{
			object[] clauses=new object[args.Length+1];
			clauses[0]=this;
			for(int i=1;i<=args.Length;i++)
			{
				clauses[i]=args[i-1];
			}
			return new OrClause(clauses);
		}
		public AbstractClause Repeat()//{0,}�ظ�
		{
			return new RepeatClause(this,0,0);
		}		
		public AbstractClause Repeat(int n)//{n,}�ظ�
		{
			return new RepeatClause(this,n,n);
		}
		public AbstractClause Repeat(int m,int n)//{m,n}�ظ�
		{
			return new RepeatClause(this,m,n);
		}
		public AbstractClause And(params object[] args)
		{
			object[] clauses=new object[args.Length+1];
			clauses[0]=this;
			for(int i=1;i<=args.Length;i++)
			{
				clauses[i]=args[i-1];
			}
			return new AndClause(clauses);
		}
		public AbstractClause Alternative(params object[] args)
		{
			object[] clauses=new object[args.Length+1];
			clauses[0]=this;
			for(int i=1;i<=args.Length;i++)
			{
				clauses[i]=args[i-1];
			}
			return new AlternativeClause(clauses);
		}
		public static AbstractClause Nilpotent
		{
			get
			{
				return nilpotent;
			}
		}
		public static AbstractClause Identic
		{
			get
			{
				return identic;
			}
		}
		public static TerminalClause Terminal
		{
			get
			{
				return terminal;
			}
		}
		public static AndClause NewAnd
		{
			get
			{
				return new AndClause();
			}
		}
		public static OrClause NewOr
		{
			get
			{
				return new OrClause();
			}
		}
		public static AlternativeClause NewAlternative
		{
			get
			{
				return new AlternativeClause();
			}
		}
		protected static void BuildClauses(ArrayList clauseList,params object[] clauses)
		{
			foreach(object clause in clauses)
			{
				if(clause is String)
				{
					clauseList.Add(new KeywordClause(clause as string));
				}
				else if(clause is AbstractClause)
				{
					clauseList.Add(clause);
				}
				else
				{
					throw new Exception("����ֻ�����ַ�������AbstractClauseʵ����");
				}
			}
		}
		private static NilpotentClause nilpotent=new NilpotentClause();
		private static IdenticClause identic=new IdenticClause();
		private static TerminalClause terminal=new TerminalClause();
	}
	/// <summary>
	/// ��Ԫ�Ӿ䣬��������һ����ѡһ/���Ӿ�
	/// </summary>
	public sealed class NilpotentClause : AbstractClause
	{
		protected override bool ParseImpl(IParserContext context)
		{
			return false;
		}
	}
	/// <summary>
	/// ��Ԫ�Ӿ䣬��������һ�����Ӿ�
	/// </summary>
	public sealed class IdenticClause : AbstractClause
	{
		protected override bool ParseImpl(IParserContext context)
		{
			return true;
		}
	}
	/// <summary>
	/// �����Ӿ�,���ڷ��������Ӿ�ʱ�����ض��Ĳ�����IParserContext::PushClause��
	/// ��ָ���﷨���Ĺ���
	/// </summary>
	public sealed class ArgumentClause : AbstractClause
	{
		public ArgumentClause(AbstractClause _clause,object arg)
		{
			clause=_clause;
			clauseArg=arg;
		}
		protected override bool ParseImpl(IParserContext context)
		{
			return clause.Parse(context);
		}
		protected override object ClauseArgument
		{
			get
			{
				return clauseArg;
			}
		}
		private AbstractClause clause;
		/// <summary>
		/// ����IParserContext::PushClause�Ĳ���
		/// </summary>
		private object clauseArg=null;
	}
	/// <summary>
	/// ���Ӿ�����ִ�й��������Ӿ䣬���ҽ��������Ӿ�ƥ��ʱ����ƥ��
	/// </summary>
	public sealed class AndClause : AbstractClause
	{
		public AndClause(){}
		public AndClause(params object[] clauses)
		{
			Build(clauses);
		}
		protected override bool ParseImpl(IParserContext context)
		{
			foreach(AbstractClause clause in clauseList)
			{
				if(!clause.Parse(context))
					return false;
			}
			return true;
		}
		public void Build(params object[] clauses)
		{
			BuildClauses(clauseList,clauses);
		}
		private ArrayList clauseList=new ArrayList();
	}
	/// <summary>
	/// ��ѡһ�Ӿ�����ִ�й��������Ӿ䣬ֻҪ��һ���Ӿ�ƥ���򷵻�ƥ�䣨֮����Ӿ䲻��ִ�У�
	/// </summary>
	public sealed class AlternativeClause : AbstractClause
	{
		public AlternativeClause(){}
		public AlternativeClause(params object[] clauses)
		{
			Build(clauses);
		}
		protected override bool ParseImpl(IParserContext context)
		{
			foreach(AbstractClause clause in clauseList)
			{
				if(clause.Parse(context))
					return true;
			}
			return false;
		}
		public void Build(params object[] clauses)
		{
			BuildClauses(clauseList,clauses);
		}
		private ArrayList clauseList=new ArrayList();
	}
	/// <summary>
	/// ���Ӿ�����ִ�й��������Ӿ䣬�����Ӿ��Ƿ�ƥ�������ƥ��
	/// </summary>
	public sealed class OrClause : AbstractClause
	{
		public OrClause(){}
		public OrClause(params object[] clauses)
		{
			Build(clauses);
		}
		protected override bool ParseImpl(IParserContext context)
		{
			foreach(AbstractClause clause in clauseList)
			{
				clause.Parse(context);
			}
			return true;
		}
		public void Build(params object[] clauses)
		{
			BuildClauses(clauseList,clauses);
		}
		private ArrayList clauseList=new ArrayList();
	}
	/// <summary>
	/// �ظ��Ӿ�,�ظ��Ӿ䲻��Ҫ֧�ֵݹ�(���û��BUILD����)
	/// </summary>
	public sealed class RepeatClause : AbstractClause
	{
		public RepeatClause(AbstractClause _clause,int minnum,int maxnum)
		{
			clause=_clause;
			minNum=minnum;
			maxNum=maxnum;
		}
		protected override bool ParseImpl(IParserContext context)
		{
			//��Ҫִ�д���
			for(int i=0;i<minNum;i++)
			{
				if(!clause.Parse(context))
					return false;
			}
			//��ѡִ�д���
			for(int i=minNum;i<maxNum || maxNum<=minNum;i++)
			{
				if(!clause.Parse(context))
					return true;
			}
			return true;
		}
		private int minNum=0,maxNum=0;
		private AbstractClause clause;
	}
	/// <summary>
	/// �սط��Ӿ�ƥ��ǹؼ���/���/�ָ������սط�
	/// </summary>
	public sealed class TerminalClause : AbstractClause
	{
		protected override bool ParseImpl(IParserContext context)
		{
			ParserToken t=context.PeekToken();
			if(t==null)
			{
				return false;
			}
			else if(t.Type==ParserTokenType.KEYWORD || t.Type==ParserTokenType.OPERATOR || t.Type==ParserTokenType.SEPARATOR)
			{
				return false;
			}
			else
			{
				context.AdvanceToken();				
				context.AppendToClause(t);
			}
			return true;
		}
	}
	/// <summary>
	/// �ؼ����Ӿ�ƥ��ָ���Ĺؼ���/���/�ָ���
	/// </summary>
	public sealed class KeywordClause : AbstractClause
	{
		public KeywordClause(string key)
		{
			keyword=key;
		}
		protected override bool ParseImpl(IParserContext context)
		{
			ParserToken t=context.PeekToken();
			if(t==null)
				return false;
			else if(t.Type==ParserTokenType.KEYWORD || t.Type==ParserTokenType.OPERATOR || t.Type==ParserTokenType.SEPARATOR)
			{
				if(keyword==t.Word)
				{
					context.AdvanceToken();
					context.AppendToClause(t);
					return true;
				}
			}
			return false;
		}
		private string keyword=null;
	}
}
