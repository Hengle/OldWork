// ScriptDllOld.cpp : ���� DLL �ĳ�ʼ�����̡�
//

#include "stdafx.h"
#include "ScriptDllOld.h"

#include "MainForm.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//
//	ע�⣡
//
//		����� DLL ��̬���ӵ� MFC
//		DLL���Ӵ� DLL ������
//		���� MFC ���κκ����ں�������ǰ��
//		��������� AFX_MANAGE_STATE �ꡣ
//
//		����:
//
//		extern "C" BOOL PASCAL EXPORT ExportedFunction()
//		{
//			AFX_MANAGE_STATE(AfxGetStaticModuleState());
//			// �˴�Ϊ��ͨ������
//		}
//
//		�˺������κ� MFC ����
//		������ÿ��������ʮ����Ҫ������ζ��
//		��������Ϊ�����еĵ�һ�����
//		���֣������������ж������������
//		������Ϊ���ǵĹ��캯���������� MFC
//		DLL ���á�
//
//		�й�������ϸ��Ϣ��
//		����� MFC ����˵�� 33 �� 58��
//

// CScriptDllOldApp

BEGIN_MESSAGE_MAP(CScriptDllOldApp, CWinApp)
END_MESSAGE_MAP()


// CScriptDllOldApp ����

CScriptDllOldApp::CScriptDllOldApp()
{
	// TODO: �ڴ˴���ӹ�����룬
	// ��������Ҫ�ĳ�ʼ�������� InitInstance ��
}


// Ψһ��һ�� CScriptDllOldApp ����

CScriptDllOldApp theApp;


// CScriptDllOldApp ��ʼ��

BOOL CScriptDllOldApp::InitInstance()
{
	CWinApp::InitInstance();

	return TRUE;
}

extern "C" __declspec(dllexport) void __cdecl Init(void)
{
	::ShowMainForm();
}
