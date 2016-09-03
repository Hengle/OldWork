// test.cpp : �������̨Ӧ�ó������ڵ㡣
//

#include "stdafx.h"
#include <fstream>
#include <conio.h>
#define INCLUDE_COMPILE
#include "../Interpreter.h"

#ifdef _DEBUG
	#ifdef INCLUDE_COMPILE
	#pragma comment(lib,"../lib/debug/sc_fulld.lib")
	#else
	#pragma comment(lib,"../lib/debug/sc_vmd.lib")
	#endif
#else
	#ifdef INCLUDE_COMPILE
	#pragma comment(lib,"../lib/release/sc_full.lib")
	#else
	#pragma comment(lib,"../lib/release/sc_vm.lib")
	#endif
#endif

static char stdinbuf[1024*1024]={""};
int _tmain(int argc, _TCHAR* argv[])
{
#ifdef _DEBUG
	int tmpDbgFlag = _CrtSetDbgFlag(_CRTDBG_REPORT_FLAG);
    tmpDbgFlag |= _CRTDBG_DELAY_FREE_MEM_DF;
    tmpDbgFlag |= _CRTDBG_LEAK_CHECK_DF;
    _CrtSetDbgFlag(tmpDbgFlag);
#endif
	std::cout<<"��C�ű���������� V0.5 ��Ȩ����(C) 2005 �Ϸ�"<<std::endl;
	std::cout<<"�÷�:�����д�һ������������Ϊ�ű��ļ������߲��������ӱ�׼�������ű�"<<std::endl;
	std::cout<<"������CTRL+Z�����س����������������ض�����ļ���ȡ��"<<std::endl<<std::endl;

	char* buf=stdinbuf;
	if(argc<2)
	{
		std::cout<<"������ű����ݣ�����CTRL+Z�����س���������"<<std::endl;
		std::cin.read(stdinbuf,1024*1024);
		stdinbuf[std::cin.gcount()]=0;
	}
	else
	{
		std::ifstream file(argv[1],std::ios_base::in);
		file.read(buf,1024*1024);
		buf[file.gcount()]=0;
	}
#ifdef INCLUDE_COMPILE	
	printf("��ʼ����...\n");
	ScriptcRuntime vmachine;
	CString err=vmachine.Compile(buf,ScriptFile::ConvertPath("main.sc"));
	if(err.GetLength()>0)
	{
		std::cout<<err<<std::endl;
		std::cout<<"��������˳�..."<<std::endl;
		::getch();
		return -1;
	}
	else		
		vmachine.SaveFile("ss.sc","032CS999874341290_h469DNX8689_937Q1ON987650C315AY_86X41S90290P_850035182918");
	printf("�������.\n");
#endif
	printf("��ʼִ��...\n");
	ScriptcRuntime vmachine2;
	if(!vmachine2.LoadFile("ss.sc","032CS999874341290_h469DNX8689_937Q1ON987650C315AY_86X41S90290P_850035182918"))
	{
		std::cout<<"װ���ļ����󣬰�������˳�..."<<std::endl;
		::getch();
		return -2;
	}
	::CoInitialize(NULL);
	vmachine2.LoadLibrary();
	vmachine2.ExecScript();
	vmachine2.UnloadLibrary();
	printf("ִ�н���,��������˳�����.\n");
	::CoUninitialize();
	::getch();
	return 0;
}

