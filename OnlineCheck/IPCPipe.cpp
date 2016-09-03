#include "StdAfx.h"
#include ".\ipcpipe.h"
#include "Schedule.h"

IPCPipe::IPCPipe(Schedule* ptr,HANDLE event)
{
	pSchedule=ptr;
	hEvent=event;
	hPipe=::CreateNamedPipe("\\\\.\\pipe\\OnlineCheck",PIPE_ACCESS_DUPLEX | FILE_FLAG_OVERLAPPED,PIPE_TYPE_MESSAGE | PIPE_READMODE_MESSAGE,1,PIPE_BUFFER_SIZE,PIPE_BUFFER_SIZE,1000,NULL);
	if(hPipe==INVALID_HANDLE_VALUE)
	{
		::PrintLastErrorMsg("CreateNamedPipe");
		pSchedule->isTerminate=true;
	}
	readPtr=0;writePtr=0;
	::ZeroMemory(&readBuffer,PIPE_BUFFER_SIZE);
	::ZeroMemory(&writeBuffer,PIPE_BUFFER_SIZE);
	//�������Ӽ���	
	::ResetEvent(hEvent);
	::ZeroMemory(&overlapped,sizeof(OVERLAPPED));
	overlapped.hEvent=hEvent;
	if(!::ConnectNamedPipe(hPipe,&overlapped))
	{
		DWORD lastErr=::GetLastError();
		if(lastErr==ERROR_PIPE_CONNECTED)
		{			
			::SetEvent(hEvent);
		}
		else if(lastErr!=ERROR_IO_PENDING)
		{
			::PrintErrorMsg(lastErr,"ConnectNamedPipe");
			return;
		}
	}
	status=PIPE_STATUS_CONNECT;
}

IPCPipe::~IPCPipe(void)
{
	if(!::DisconnectNamedPipe(hPipe))
	{
		::PrintLastErrorMsg("DisconnectNamedPipe");
	}
	if(hPipe!=INVALID_HANDLE_VALUE)
		::CloseHandle(hPipe);
}
void IPCPipe::DoRead(void)
{
	readPtr=0;
	if(!::ReadFile(hPipe,readBuffer,PIPE_BUFFER_SIZE,&readPtr,&overlapped))
	{
		DWORD lastErr=::GetLastError();
		if(lastErr!=ERROR_IO_PENDING)
		{
			::PrintErrorMsg(lastErr,"ReadFile");
			return;
		}
	}
}
void IPCPipe::DoWrite(void)
{
	//��ǰ������д�������������¼���Ϊ�ź�״̬��ֱ��ת����һ״̬
	::SetEvent(hEvent);
	if(readPtr>=sizeof(CommandPackageHead))
	{
		DWORD cmd=*(DWORD*)readBuffer;
		switch(cmd)
		{
		case COMMAND_PAUSE:
			{
				pSchedule->Suspend();
				PRINT_CONSOLE_MSG2("\r\nPAUSE COMMAND");
			}
			break;
		case COMMAND_CONTINUE:
			{
				pSchedule->Resume();
				PRINT_CONSOLE_MSG2("\r\nCONTINUE COMMAND");
			}
			break;
		case COMMAND_PROXY_RELOAD:
			{
				if(pSchedule->proxyNum>0)
				{
					CommandPackageHead* ptr=(CommandPackageHead*)readBuffer;
					ProxyData* pData=(ProxyData*)(readBuffer+sizeof(CommandPackageHead));
					pSchedule->StartProxyLoad();
					pSchedule->DoReload();
					for(DWORD i=0;i<ptr->Size && (i+1)*sizeof(ProxyData)+sizeof(CommandPackageHead)<=readPtr;i++)
					{
						pSchedule->PushProxy(pData[i].IP,pData[i].Port);
					}
					pSchedule->EndProxyLoad();
				}
				PRINT_CONSOLE_MSG2("\r\nRELOAD COMMAND");
				/*
				writePtr=sizeof(CommandPackageHead);
				CommandPackageHead* pHead=(CommandPackageHead*)writeBuffer;
				pHead->Command=COMMAND_PROXY_RELOAD;
				pHead->Size=0;
				::ResetEvent(hEvent);
				::ZeroMemory(&overlapped,sizeof(OVERLAPPED));
				overlapped.hEvent=hEvent;
				if(!::WriteFile(hPipe,writeBuffer,writePtr,&writePtr,&overlapped))
				{
					DWORD lastErr=::GetLastError();
					if(lastErr!=ERROR_IO_PENDING)
					{
						::PrintErrorMsg(lastErr,"WriteFile");
						return;
					}
				}
				*/
			}
			break;
		case COMMAND_PROXY_APPEND:
			{
				if(pSchedule->proxyNum>0)
				{
					CommandPackageHead* ptr=(CommandPackageHead*)readBuffer;
					ProxyData* pData=(ProxyData*)(readBuffer+sizeof(CommandPackageHead));
					pSchedule->StartProxyLoad();
					for(DWORD i=0;i<ptr->Size && (i+1)*sizeof(ProxyData)+sizeof(CommandPackageHead)<=readPtr;i++)
					{					
						pSchedule->PushProxy(pData[i].IP,pData[i].Port);
					}
					pSchedule->EndProxyLoad();
				}
				PRINT_CONSOLE_MSG2("\r\nAPPEND COMMAND");
				/*
				writePtr=sizeof(CommandPackageHead);
				CommandPackageHead* pHead=(CommandPackageHead*)writeBuffer;
				pHead->Command=COMMAND_PROXY_APPEND;
				pHead->Size=0;
				::ResetEvent(hEvent);
				::ZeroMemory(&overlapped,sizeof(OVERLAPPED));
				overlapped.hEvent=hEvent;
				if(!::WriteFile(hPipe,writeBuffer,writePtr,&writePtr,&overlapped))
				{
					DWORD lastErr=::GetLastError();
					if(lastErr!=ERROR_IO_PENDING)
					{
						::PrintErrorMsg(lastErr,"WriteFile");
						return;
					}
				}
				*/
			}
			break;
		}
	}
}
void IPCPipe::DoConnect(void)
{
	//���ȶϿ���ǰ������
	if(!::DisconnectNamedPipe(hPipe))
	{
		::PrintLastErrorMsg("DisconnectNamedPipe");
	}
	::ResetEvent(hEvent);
	::ZeroMemory(&overlapped,sizeof(OVERLAPPED));
	overlapped.hEvent=hEvent;
	if(!::ConnectNamedPipe(hPipe,&overlapped))
	{
		DWORD lastErr=::GetLastError();
		if(lastErr==ERROR_PIPE_CONNECTED)
		{			
			::SetEvent(hEvent);
		}
		else if(lastErr!=ERROR_IO_PENDING)
		{
			::PrintErrorMsg(lastErr,"ConnectNamedPipe");
			::SetEvent(hEvent);
			return;
		}
	}
}
void IPCPipe::Exec(void)
{
	//���ִ�й���״̬�Ľ��
	DWORD size=0;
	if(!::GetOverlappedResult(hPipe,&overlapped,&size,FALSE))
	{
		DWORD lastErr=::GetLastError();
		PRINT_CONSOLE_MSG("\r\nstate %u,GetOverlappedResult failed:%u,%s",status,lastErr,::GetErrorMsg(lastErr));
		//�κ�״̬�����ת���̿�����״̬
		status=PIPE_STATUS_CONNECT;
	}
	else
	{
		//ת��״̬
		status=(status+1)%PIPE_STATUS_NUM;
	}
	::ResetEvent(hEvent);
	::ZeroMemory(&overlapped,sizeof(OVERLAPPED));
	overlapped.hEvent=hEvent;
	//ִ����״̬
	switch(status)
	{
	case PIPE_STATUS_READ:
		DoRead();
		break;
	case PIPE_STATUS_WRITE:
		DoWrite();
		break;
	case PIPE_STATUS_CONNECT:
		DoConnect();
		break;
	}
}

UINT WINAPI IPCPipe::CommandThread(LPVOID obj)
{
	Schedule* pSchedule=static_cast<Schedule*>(obj);
	HANDLE pipeEvent=::CreateEvent(NULL,TRUE,FALSE,NULL);
	if(!pipeEvent)
	{
		::PrintLastErrorMsg("CreateEvent");
		pSchedule->isTerminate=true;
		return 0;
	}
	pSchedule->ipcEvent=pipeEvent;
	IPCPipe* pPipe=new IPCPipe(pSchedule,pipeEvent);
	while(!pSchedule->isTerminate)
	{		
		//�ȴ���ǰ״̬�Ĳ������
		DWORD r=::WaitForSingleObject(pipeEvent,INFINITE);
		if(r!=WAIT_OBJECT_0)
		{
			::PrintLastErrorMsg("\r\nWaitForSingleObject");
			continue;
		}
		if(pSchedule->isTerminate)
			break;
		//ִ��״̬ת����ִ����״̬�Ĳ���
		pPipe->Exec();
	}
	delete pPipe;
	if(pipeEvent)
		::CloseHandle(pipeEvent);
	return 0;
}