#include "StdAfx.h"
#include ".\debugthread.h"
#include "HtmlWindow.h"

DebugThread* DebugThread::Start(void)
{
	//�����ȴ��߳��������¼�
	HANDLE stEve=::CreateEvent(NULL,FALSE,FALSE,NULL);
	//���¼�����������������߳�,�߳��ڴ����¼���㲻����ʹ��������Ϊ�ֲ�������
	DWORD arg=(DWORD)stEve;
	HANDLE ht=(HANDLE)::_beginthreadex(NULL,0,&DebugThread::Loop,(LPVOID)&arg,0,NULL);
	if(ht)
	{
		::WaitForSingleObject(stEve,INFINITE);
		((DebugThread*)arg)->hThread=ht;
	}
	else
	{
		arg=NULL;
	}
	::CloseHandle(stEve);
	return (DebugThread*)arg;
}

UINT WINAPI DebugThread::Loop(LPVOID obj)
{	
	//�õ������ȴ��¼����
	HANDLE stEve=*(HANDLE*)obj;
	DebugThread dt;
	//����ǰ�߳���ص���ʵ���ش������������������������߳���ֹ��㲻��ʹ�ø�ʵ������Ϊ�ֲ�������
	*(DebugThread**)obj=&dt;
	//���ߵȴ��������������Է�����
	::SetEvent(stEve);
	while(!dt.isTerm)
	{		
		dt.OnceWork();
	}
	return 0;
}

void DebugThread::Terminate(DebugThread*& pObj)
{
	if(pObj==NULL)
		return;
	HANDLE ht=pObj->hThread;
	pObj->TerminateSignal();
	pObj=NULL;//ȷ���߳���ֹ��������ʵ������
		
	::WaitForSingleObject(ht,INFINITE);
	::CloseHandle(ht);
}

DebugThread::DebugThread(void)
{
	isTerm=false;
	hEvent=::CreateEvent(NULL,FALSE,FALSE,NULL);
}

DebugThread::~DebugThread(void)
{
	::CloseHandle(hEvent);
}

void DebugThread::TerminateSignal(void)
{
	isTerm=true;
	Signal();
}

void DebugThread::Signal(void)
{
	::SetEvent(hEvent);
}

void DebugThread::OnceWork(void)
{
	DWORD r=::WaitForSingleObject(hEvent,INFINITE);
	if(isTerm)
		return;
	dlg.SendMessage(WM_MY_RUN_REQUESTS,0,0);
}
