// ScriptDllOld.h : ScriptDllOld DLL ����ͷ�ļ�
//

#pragma once

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// ������


// CScriptDllOldApp
// �йش���ʵ�ֵ���Ϣ������� ScriptDllOld.cpp
//

class CScriptDllOldApp : public CWinApp
{
public:
	CScriptDllOldApp();

// ��д
public:
	virtual BOOL InitInstance();

	DECLARE_MESSAGE_MAP()
};
