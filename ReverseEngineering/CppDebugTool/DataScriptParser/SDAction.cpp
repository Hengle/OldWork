/******************************************************************************

    SDAction.cpp

******************************************************************************/
#include <iostream>
#include "SDAction.h"
#include "SDToken.h"
#include "SDLog.h"

#define OUTPUT_SYNTAX 0

// -----------------------------------------------------------------------
// Semantic stack action code
// -----------------------------------------------------------------------
namespace DataScript
{	
	void SDAction::setExternScript( void )
	{
		FunctionExData& funcEx=getLastFunction();
		funcEx.setScript(mScanner->getLastToken());
	}
	void SDAction::buildOperator ( void )
	{
		int type=FunctionData::ID_TOKEN;
		std::string name=pop(&type);
		
		StatementData arg=popStatement();
		
		StatementData newStatement;
		newStatement.setLine(mScanner->getLastLineNumber());
		mStatementSemanticStack.push(newStatement);
		
		StatementData& statement=getCurStatement();
		FunctionExData& funcEx=getLastFunction();
		if(!funcEx.isValid())
		{
			funcEx.setParamClass(FunctionData::PARAM_CLASS_OPERATOR);

			funcEx.setHaveId(true);
			funcEx.setId(name);
			funcEx.setType(type);

			if(arg.isValid())
				funcEx.addParam(arg);
		}
	}
	void SDAction::buildFirstTernaryOperator ( void )
	{
		int type=FunctionData::ID_TOKEN;
		std::string name=pop(&type);
		
		StatementData arg=popStatement();
		
		StatementData newStatement;
		newStatement.setLine(mScanner->getLastLineNumber());
		mStatementSemanticStack.push(newStatement);
		
		StatementData& statement=getCurStatement();
		FunctionExData& funcEx=getLastFunction();
		if(!funcEx.isValid())
		{
			funcEx.setParamClass(FunctionData::PARAM_CLASS_TERNARY_OPERATOR);
			funcEx.setExtentClass(FunctionExData::EXTENT_CLASS_STATEMENT);

			funcEx.setHaveId(true);
			funcEx.setId(name);
			funcEx.setType(type);

			if(arg.isValid())
				funcEx.addParam(arg);
		}
	}
	void SDAction::buildSecondTernaryOperator ( void )
	{
		int type=FunctionData::ID_TOKEN;
		std::string name=pop(&type);
		
		StatementData& statement=getCurStatement();
		FunctionExData newFuncEx;
		statement.addSubsequentFunction(newFuncEx);

		FunctionExData& funcEx=getLastFunction();
		if(!funcEx.isValid())
		{
			funcEx.setParamClass(FunctionData::PARAM_CLASS_TERNARY_OPERATOR);
			funcEx.setExtentClass(FunctionExData::EXTENT_CLASS_STATEMENT);

			funcEx.setHaveId(true);
			funcEx.setId(name);
			funcEx.setType(type);
		}
	}
	//--------------------------------------------------------------------------------------
	void SDAction::beginStatement ( void )
	{
		StatementData statement;
		statement.setLine(mScanner->getLastLineNumber());
		mStatementSemanticStack.push(statement);
	}
	void SDAction::endStatement ( void )
	{
		StatementData statement=popStatement();
		if(mStatementSemanticStack.empty())
		{
			if(!statement.isValid())
			{
				//_epsilon_���ʽ���������
				return;
			}
			//����Ԫ�ؽ���
			ScriptData scriptData(statement);
			scriptData.setLoaded(true);
			mScriptDatas->insert(std::make_pair(scriptData.getId(),scriptData));
	#if OUTPUT_SYNTAX
			std::cout<<scriptData.toString()<<std::endl;
	#endif
		}
		else
		{
			FunctionExData& funcEx=getLastFunction();
			switch(funcEx.getExtentClass())
			{
			case FunctionExData::EXTENT_CLASS_NOTHING:
				{
					if(funcEx.getParamClass()==FunctionData::PARAM_CLASS_OPERATOR && !statement.isValid())
						return;//�������Ͳ�֧�ֿ������������
					//�����������������䣬���ڱ��Ĭ��״̬(��������a()��a[]�����ǻ���һ����������)��
					funcEx.addParam(statement);
				}
				break;
			case FunctionExData::EXTENT_CLASS_STATEMENT:
				{
					if(!statement.isValid())
					{
						//_epsilon_���ʽ���������
						return;
					}
					//������չ��䲿��
					funcEx.addStatement(statement);
				}
				break;
			}
		}
	}
	//--------------------------------------------------------------------------------------
	void SDAction::beginFunction ( void )
	{
		StatementData& statement=getCurStatement();
		FunctionExData& funcEx=getLastFunction();
		if(funcEx.isValid())
		{
			//���Ķ�Ԫ����������Ԫ������
			FunctionExData newFuncEx;
			statement.addSubsequentFunction(newFuncEx);
		}
	}
	void SDAction::setFunctionId ( void )
	{
		int type=FunctionData::ID_TOKEN;
		std::string name=pop(&type);
		FunctionExData& funcEx=getLastFunction();
		if(!funcEx.isValid())
		{
			funcEx.setHaveId(true);
			funcEx.setId(name);
			funcEx.setType(type);
		}
	}
	void SDAction::setMemberId ( void )
	{
		int type=FunctionData::ID_TOKEN;
		std::string name=pop(&type);
		if(type==FunctionData::ID_TOKEN)
			type=FunctionData::STRING_TOKEN;
		FunctionExData& funcEx=getLastFunction();
		if(!funcEx.isValid())
		{
			funcEx.setHaveId(true);
			funcEx.setId(name);
			funcEx.setType(type);
		}
	}
	void SDAction::endFunction ( void )
	{
	}
	void SDAction::buildHighOrderFunction ( void )
	{
		//�߽׺������죨��ǰ��������һ��������
		FunctionExData& funcEx=getLastFunction();
		FunctionExData temp=funcEx;
		funcEx.clear();
		funcEx.setFunctionAsName(temp);
	}
	void SDAction::markParenthesisParam ( void )
	{
		FunctionExData& funcEx=getLastFunction();
		funcEx.setParamClass(FunctionData::PARAM_CLASS_PARENTHESIS);
	}
	void SDAction::markBracketParam ( void )
	{
		FunctionExData& funcEx=getLastFunction();
		funcEx.setParamClass(FunctionData::PARAM_CLASS_BRACKET);
	}
	void SDAction::markPeriodParam ( void )
	{
		FunctionExData& funcEx=getLastFunction();
		funcEx.setParamClass(FunctionData::PARAM_CLASS_PERIOD);
	}
	void SDAction::markPeriodParenthesisParam ( void )
	{
		FunctionExData& funcEx=getLastFunction();
		funcEx.setParamClass(FunctionData::PARAM_CLASS_PERIOD_PARENTHESIS);
	}
	void SDAction::markPeriodBracketParam ( void )
	{
		FunctionExData& funcEx=getLastFunction();
		funcEx.setParamClass(FunctionData::PARAM_CLASS_PERIOD_BRACKET);
	}
	void SDAction::markPeriodBraceParam ( void )
	{
		FunctionExData& funcEx=getLastFunction();
		funcEx.setParamClass(FunctionData::PARAM_CLASS_PERIOD_BRACE);
	}
	void SDAction::markHaveStatement ( void )
	{
		FunctionExData& funcEx=getLastFunction();
		funcEx.setExtentClass(FunctionExData::EXTENT_CLASS_STATEMENT);
	}
	void SDAction::markHaveExternScript ( void )
	{
		FunctionExData& funcEx=getLastFunction();
		funcEx.setExtentClass(FunctionExData::EXTENT_CLASS_EXTERN_SCRIPT);
	}
	//--------------------------------------------------------------------------------------
	void SDAction::pushTrue ( void )
	{
		push("true",FunctionData::BOOL_TOKEN);
	}
	void SDAction::pushFalse ( void )
	{
		push("false",FunctionData::BOOL_TOKEN);
	}
	//��ʶ��
	void SDAction::pushId( void )
	{
		push(mScanner->getLastToken(),FunctionData::ID_TOKEN);
	}
	void SDAction::pushNum( void )
	{
		push(mScanner->getLastToken(),FunctionData::NUM_TOKEN);
	}
	void SDAction::pushStr( void )
	{
		push(mScanner->getLastToken(),FunctionData::STRING_TOKEN);
	}
	//--------------------------------------------------------------------------------------
	void SDAction::push(const std::string& v,int val)
	{
		mSemanticStack.push(SemanticInfo(v,val));
	}
	std::string SDAction::pop(int* pVal)
	{
		if(pVal!=NULL)
			*pVal=0;
		if(mSemanticStack.empty())
			return "null_stack_!!!";
		SemanticInfo info=mSemanticStack.top();
		mSemanticStack.pop();
		if(pVal!=NULL)
			*pVal=info.value;
		return info.token;
	}
	StatementData SDAction::popStatement(void)
	{
		if(mStatementSemanticStack.empty())
		{
			StatementData data("null_element_stack_!!!",FunctionData::ID_TOKEN);
			return data;
		}
		StatementData cdata=mStatementSemanticStack.top();
		mStatementSemanticStack.pop();
		return cdata;
	}
	StatementData& SDAction::getCurStatement(void)
	{
		if(mStatementSemanticStack.empty())
			return nullStatementInfoRef();
		StatementData& topData=mStatementSemanticStack.top();
		return topData;
	}
	FunctionExData& SDAction::getLastFunction(void)
	{
		StatementData& statement=getCurStatement();
		FunctionExDatas& funcExes=statement.getSubsequentFunctions();
		if(funcExes.size()==0)
			return statement;
		else
			return funcExes.back();
	}
}