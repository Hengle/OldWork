/*****************************************************************************
 
    SDError.cpp
  
******************************************************************************/

#include "SDError.h"
#include "SDToken.h"
#include "SDLog.h"
#include "SDString.h"

#define GET_SYMBOL_NAME    SDGetSymbolName

namespace DataScript
{
	short SDError::mismatch ( short terminal, short token )
	{
		printf("[�� %d ]��������%s�����ǵõ���%s��\n",tokens->getLineNumber(),GET_SYMBOL_NAME(terminal),GET_SYMBOL_NAME(token));
		mHasError=true;
		return  token;
	}

	short SDError::no_entry ( short nonterminal, short token, int level )
	{
		printf("[�� %d ]���﷨���󣺡�%s�������ԡ�%s����ʼ���������롮%s��\n",tokens->getLineNumber(),GET_SYMBOL_NAME(nonterminal),GET_SYMBOL_NAME(token),tokens->getCurToken().c_str());
	    
		token = tokens->get();       // advance the input
		mHasError=true;
		return  token;
	}

	void SDError::input_left ( void )
	{
		printf("[�� %d ]���﷨�Ѿ�������ʣ�ಿ���޷�����������\n",tokens->getLineNumber());
		mHasError=true;
	}

	SDError::SDError ( SDToken& tokens, SDLog& log )
	{
		SDError::log = &log;
		SDError::tokens = &tokens;
		mHasError = false;
	}
}