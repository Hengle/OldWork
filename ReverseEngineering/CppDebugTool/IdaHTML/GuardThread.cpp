#include "StdAfx.h"
#include ".\guardthread.h"
#include "HtmlWindow.h"

GuardThread* GuardThread::Start(void)
{
	//�����ȴ��߳��������¼�
	HANDLE stEve=::CreateEvent(NULL,FALSE,FALSE,NULL);
	//���¼�����������������߳�,�߳��ڴ����¼���㲻����ʹ��������Ϊ�ֲ�������
	DWORD arg=(DWORD)stEve;
	HANDLE ht=(HANDLE)::_beginthreadex(NULL,0,&GuardThread::Loop,(LPVOID)&arg,0,NULL);
	if(ht)
	{
		::WaitForSingleObject(stEve,INFINITE);
		((GuardThread*)arg)->hThread=ht;
	}
	else
	{
		arg=NULL;
	}
	::CloseHandle(stEve);
	return (GuardThread*)arg;
}

UINT WINAPI GuardThread::Loop(LPVOID obj)
{	
	//�õ������ȴ��¼����
	HANDLE stEve=*(HANDLE*)obj;
	GuardThread dt;
	//����ǰ�߳���ص���ʵ���ش������������������������߳���ֹ��㲻��ʹ�ø�ʵ������Ϊ�ֲ�������
	*(GuardThread**)obj=&dt;
	//���ߵȴ��������������Է�����
	::SetEvent(stEve);
	while(!dt.isTerm)
	{		
		dt.OnceWork();
	}
	return 0;
}

void GuardThread::Terminate(GuardThread*& pObj)
{
	if(pObj==NULL)
		return;
	HANDLE ht=pObj->hThread;
	pObj->TerminateSignal();
	pObj=NULL;//ȷ���߳���ֹ��������ʵ������
		
	::WaitForSingleObject(ht,INFINITE);
	::CloseHandle(ht);
}

GuardThread::GuardThread(void)
{
	isTerm=false;
	hEvent=::CreateEvent(NULL,FALSE,FALSE,NULL);
}

GuardThread::~GuardThread(void)
{
	::CloseHandle(hEvent);
}

void GuardThread::TerminateSignal(void)
{
	isTerm=true;
	Signal();
}

void GuardThread::Signal(void)
{
	::SetEvent(hEvent);
}

void GuardThread::OnceWork(void)
{
	DWORD r=::WaitForSingleObject(hEvent,INFINITE);
	if(isTerm)
		return;
	dlg.SendMessage(WM_MY_RUN_REQUESTS,0,0);
}
