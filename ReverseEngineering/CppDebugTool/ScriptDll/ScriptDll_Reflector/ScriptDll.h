// ScriptDll.h : ScriptDll DLL ����ͷ�ļ�
//

#pragma once

#ifndef __AFXWIN_H__
	#error "�ڰ������ļ�֮ǰ������stdafx.h�������� PCH �ļ�"
#endif

#include "resource.h"		// ������


// CScriptDllApp
// �йش���ʵ�ֵ���Ϣ������� ScriptDll.cpp
//

class CScriptDllApp : public CWinApp
{
public:
	CScriptDllApp();

// ��д
public:
	virtual BOOL InitInstance();

	DECLARE_MESSAGE_MAP()
};
