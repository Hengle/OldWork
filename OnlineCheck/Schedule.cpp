#include "StdAfx.h"
#include ".\schedule.h"
#import <msxml3.dll> raw_interfaces_only raw_native_types no_implementation

#include "CheckThread.h"
#include "IPCPipe.h"

#define WM_ONLINECHECK			WM_APP+1
#define MAX_TIME_COUNT			0x80000000
#define UI_UPDATE_INTERVAL		2
#define OUTPUT_INTERVAL			20
#define MIN_NAME_LENGTH			3
#define MAX_NAME_LENGTH			16
#define MAX_OUTPUT_LINES		100*10000
#define SLEEP_PER_LINK			50

#define CHECK_NUMBER			5
#define CHECK_INTERVAL			10

int __cdecl nullprintf(const char *, ...)
{
	return 0;
}

CString GetErrorMsg(DWORD err)
{
	LPSTR lpMsgBuf;
	if (!::FormatMessage( 
		FORMAT_MESSAGE_ALLOCATE_BUFFER | 
		FORMAT_MESSAGE_FROM_SYSTEM | 
		FORMAT_MESSAGE_IGNORE_INSERTS,
		NULL,
		err,
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		(LPSTR)&lpMsgBuf,
		0,
		NULL ))
	{
		return "";
	}
	CString r(lpMsgBuf);
	LocalFree((LPVOID)lpMsgBuf);
	return r;
}

CString GetLastErrorMsg(void)
{
	return GetErrorMsg(::GetLastError());
}

void PrintErrorMsg(DWORD err,LPCSTR title)
{
#ifdef NO_CONSOLE_MSG
	err,title;
#else
	if(title==NULL)
		PRINT_CONSOLE_MSG("\r\n%u,%s",err,GetErrorMsg(err));
	else
		PRINT_CONSOLE_MSG("\r\n%s:%u,%s",title,err,GetErrorMsg(err));
#endif
}

void PrintLastErrorMsg(LPCSTR title)
{
#ifdef NO_CONSOLE_MSG
	title;
#else
	DWORD lastErr=::GetLastError();
	PrintErrorMsg(lastErr,title);
#endif
}

Schedule::Schedule(void):threadNum(1000),proxyNum(0),connPerProxy(1),connPerThread(3),isTerminate(false),isSuspend(false),
	directConnNum(256),directConnStatus(0),directSuccessfulCount(0),ipcEvent(NULL),fileNum(0),fileSizes(NULL),readPoses(NULL),
	uiUpdateInterval(UI_UPDATE_INTERVAL),outputInterval(OUTPUT_INTERVAL),totalSuccessfulCount(0),totalOutputCount(0),
	maxOutputLines(MAX_OUTPUT_LINES),totalBadNameCount(0),addPostfix(0),sleepPerLink(SLEEP_PER_LINK),
	averPerformance(0),checkNum(CHECK_NUMBER),checkInterval(CHECK_INTERVAL)
{
	proxyStatus=new ProxyStatus[MAX_PROXY_NUMBER];
	proxyNum=0;
	usedProxyNum=0;
	existIndex=0;

	::AfxSocketInit();
	::CoInitialize(NULL);
	targetUrl="http://api.oscar.aol.com/SOA/key=PandorasBoxGoodUntilJan2006/presence/[USERNAME]";
	outputFile="OnlineStatus.txt";
	errorFile="WrongFormat.txt";
	postfixStr="@aol.com";
	postfixLen=postfixStr.GetLength();

	CComPtr<MSXML2::IXMLDOMDocument2> pDom;
	HRESULT hr=pDom.CoCreateInstance(L"MSXML2.DOMDocument");
	VARIANT_BOOL r;
	hr=pDom->load(CComVariant(L"OnlineCheck.xml"),&r);
	if(!r)
	{
		::MessageBox(NULL,"�޷��������ļ�OnlineCheck.xml!","",MB_OK);
	}
	if(1)
	{
		CComPtr<MSXML2::IXMLDOMNodeList> pNodeList; 
		hr=pDom->selectNodes(L"//InputFile",&pNodeList);
		if(hr==S_OK)
		{
			hr=pNodeList->reset();
			while(hr==S_OK)
			{
				CComQIPtr<MSXML2::IXMLDOMNode> pNode;
				hr=pNodeList->nextNode(&pNode);
				if(hr==S_FALSE)
					break;
				CComQIPtr<MSXML2::IXMLDOMElement> pElement(pNode);
				CComVariant val;
				HRESULT rAttr=pElement->getAttribute(L"Value",&val);
				if(val.vt==VT_BSTR && val.bstrVal)
				{
					inputFiles.push_back(CString(val.bstrVal));
				}
			}
		}
	}
	if(2)
	{
		CComPtr<MSXML2::IXMLDOMNodeList> pNodeList; 
		hr=pDom->selectNodes(L"//StatusInfo",&pNodeList);
		if(hr==S_OK)
		{
			hr=pNodeList->reset();
			while(hr==S_OK)
			{
				CComQIPtr<MSXML2::IXMLDOMNode> pNode;
				hr=pNodeList->nextNode(&pNode);
				if(hr==S_FALSE)
					break;
				CComQIPtr<MSXML2::IXMLDOMElement> pElement(pNode);
				CComVariant status,name,url;
				HRESULT rAttr1=pElement->getAttribute(L"Status",&status);
				HRESULT rAttr2=pElement->getAttribute(L"Name",&name);
				HRESULT rAttr3=pElement->getAttribute(L"Url",&url);
				if(rAttr1==S_OK && rAttr2==S_OK && rAttr3==S_OK)
				{
					status.ChangeType(VT_I4);
					StatusInfo si;
					si.Status=status.lVal;
					si.Name=CString(name.bstrVal);
					si.Url=CString(url.bstrVal);
					statusInfos.push_back(si);
				}
			}
		}
	}
	if(3)
	{
		CComPtr<MSXML2::IXMLDOMNodeList> pNodeList; 
		hr=pDom->selectNodes(L"//ProxyInfo",&pNodeList);
		if(hr==S_OK)
		{
			hr=pNodeList->reset();
			while(hr==S_OK)
			{
				CComQIPtr<MSXML2::IXMLDOMNode> pNode;
				hr=pNodeList->nextNode(&pNode);
				if(hr==S_FALSE)
					break;
				CComQIPtr<MSXML2::IXMLDOMElement> pElement(pNode);
				CComVariant ip,port;
				HRESULT rAttr1=pElement->getAttribute(L"IP",&ip);
				HRESULT rAttr2=pElement->getAttribute(L"Port",&port);
				if(rAttr1==S_OK && rAttr2==S_OK)
				{
					port.ChangeType(VT_I4);
					CString ipStr(ip.bstrVal);
					proxyStatus[proxyNum].IP=::inet_addr(ipStr);
					proxyStatus[proxyNum].Port=port.lVal;
					proxyNum++;
				}
			}
		}
	}
	if(4)
	{
		CComPtr<MSXML2::IXMLDOMNode> pNode;
		hr=pDom->selectSingleNode(L"//Arguments",&pNode);
		CComQIPtr<MSXML2::IXMLDOMElement> pElement(pNode);
		if(hr==S_OK)
		{
			CComVariant target,output,threads,connperproxy,connperthread,directconnnum,proxynum,uiupdateint;
			HRESULT rAttr=pElement->getAttribute(L"TargetUrl",&target);
			if(rAttr==S_OK)
			{
				targetUrl=CString(target.bstrVal);
			}
			rAttr=pElement->getAttribute(L"OutputFile",&output);
			if(rAttr==S_OK)
			{
				outputFile=CString(output.bstrVal);
			}
			rAttr=pElement->getAttribute(L"ThreadNum",&threads);
			if(rAttr==S_OK)
			{
				threads.ChangeType(VT_I4);
				threadNum=threads.lVal;
			}
			rAttr=pElement->getAttribute(L"ConnPerProxy",&connperproxy);
			if(rAttr==S_OK)
			{
				connperproxy.ChangeType(VT_I4);
				connPerProxy=connperproxy.lVal;
			}
			rAttr=pElement->getAttribute(L"ConnPerThread",&connperthread);
			if(rAttr==S_OK)
			{
				connperthread.ChangeType(VT_I4);
				connPerThread=connperthread.lVal;
			}
			rAttr=pElement->getAttribute(L"DirectConnNum",&directconnnum);
			if(rAttr==S_OK)
			{
				directconnnum.ChangeType(VT_I4);
				directConnNum=directconnnum.lVal;
			}
			rAttr=pElement->getAttribute(L"ProxyNum",&proxynum);
			if(rAttr==S_OK)
			{
				proxynum.ChangeType(VT_UI4);
				if(proxyNum>proxynum.ulVal)
					proxyNum=proxynum.ulVal;
			}
			rAttr=pElement->getAttribute(L"UIUpdateInterval",&uiupdateint);
			if(rAttr==S_OK)
			{
				uiupdateint.ChangeType(VT_I4);
				if(uiupdateint.lVal>=1)
					uiUpdateInterval=uiupdateint.lVal;
			}		
			CComVariant outputinterval,errorfile,maxoutputlines;
			rAttr=pElement->getAttribute(L"OutputInterval",&outputinterval);
			if(rAttr==S_OK)
			{
				outputinterval.ChangeType(VT_I4);
				if(outputinterval.lVal>=5)
					outputInterval=outputinterval.lVal;
			}		
			rAttr=pElement->getAttribute(L"ErrorFile",&errorfile);
			if(rAttr==S_OK)
			{
				errorFile=CString(errorfile.bstrVal);
			}		
			rAttr=pElement->getAttribute(L"MaxOutputLines",&maxoutputlines);
			if(rAttr==S_OK)
			{
				maxoutputlines.ChangeType(VT_I4);
				if(maxoutputlines.lVal>=1000)
					maxOutputLines=maxoutputlines.lVal;
			}		
			CComVariant addaolcom,sleepperlink,checknum,checkinterval;
			rAttr=pElement->getAttribute(L"Add_AOL_COM",&addaolcom);
			if(rAttr==S_OK)
			{
				addaolcom.ChangeType(VT_I4);
				if(addaolcom.lVal>=0)
					addPostfix=addaolcom.lVal;
			}		
			rAttr=pElement->getAttribute(L"SleepPerLink",&sleepperlink);
			if(rAttr==S_OK)
			{
				sleepperlink.ChangeType(VT_I4);
				if(sleepperlink.lVal>=0)
					sleepPerLink=sleepperlink.lVal;
			}		
			rAttr=pElement->getAttribute(L"CheckNum",&checknum);
			if(rAttr==S_OK)
			{
				checknum.ChangeType(VT_I4);
				if(checknum.lVal>=0)
					checkNum=checknum.lVal;
			}		
			rAttr=pElement->getAttribute(L"CheckInterval",&checkinterval);
			if(rAttr==S_OK)
			{
				checkinterval.ChangeType(VT_I4);
				if(checkinterval.lVal>=5)
					checkInterval=checkinterval.lVal;
			}		
		}
	}

	if(proxyNum>0)
	{
		for(DWORD i=0;i<proxyNum;i++)
		{
			unsigned __int64 v=GetProxyID(proxyStatus[i].IP,proxyStatus[i].Port);
			proxyInfo[existIndex][v]=i;
		}
		if(threadNum*connPerThread>proxyNum*connPerProxy)
		{
			threadNum=proxyNum*connPerProxy/connPerThread;
			if(proxyNum*connPerProxy % connPerThread != 0)
				threadNum++;
		}
		usedProxyNum=threadNum*connPerThread/connPerProxy;
		if(threadNum*connPerThread % connPerProxy != 0)
			usedProxyNum++;
		if(usedProxyNum>proxyNum)
			usedProxyNum=proxyNum;
		for(i=0;i<usedProxyNum;i++)
		{
			unsigned __int64 id=GetProxyID(proxyStatus[i].IP,proxyStatus[i].Port);
			usedProxyQueue.push_back(id);
			proxyStatus[i].InUse=1;
		}
		for(;i<proxyNum;i++)
		{
			unsigned __int64 id=GetProxyID(proxyStatus[i].IP,proxyStatus[i].Port);
			unusedProxyQueue.push(id);
			proxyStatus[i].InUse=0;
		}
		for(;i<MAX_PROXY_NUMBER;i++)
		{
			unusedProxyIndex.push(i);
		}
	}
	else if(threadNum*connPerThread>directConnNum)
	{
		threadNum=directConnNum/connPerThread;
		if(directConnNum % connPerThread != 0)
			threadNum++;
	}

	threadHandles=new HANDLE[threadNum];
	threadStatus=new ThreadStatus[threadNum];

	::InitializeCriticalSectionAndSpinCount(&readCS,0x80000400);
	::InitializeCriticalSectionAndSpinCount(&writeCS,0x80000400);
	::InitializeCriticalSectionAndSpinCount(&errorCS,0x80000400);
	::InitializeCriticalSectionAndSpinCount(&threadHandleCS,0x80000400);
	::InitializeCriticalSectionAndSpinCount(&proxyReadCS,0x80000400);
	::InitializeCriticalSectionAndSpinCount(&proxyWriteCS,0x80000400);
	::InitializeCriticalSectionAndSpinCount(&proxySuccessCS,0x80000400);

	LARGE_INTEGER f;
	if(::QueryPerformanceFrequency(&f))
		countFrequency=f.QuadPart;
	else
		countFrequency=0;

	fileNum=inputFiles.size();
	if(fileNum>0)
	{
		fileSizes=new DWORD[fileNum];
		readPoses=new DWORD[fileNum];
		::ZeroMemory(fileSizes,sizeof(DWORD)*fileNum);
		::ZeroMemory(readPoses,sizeof(DWORD)*fileNum);
	}
	inputBuffer=new char[FILE_BUFFER_SIZE+1];
	outputBuffer=new char[FILE_BUFFER_SIZE+1];
	::ZeroMemory(inputBuffer,FILE_BUFFER_SIZE+1);
	::ZeroMemory(outputBuffer,FILE_BUFFER_SIZE+1);
	inputPtr=0;
	inputSize=0;
	outputPtr=0;
	outputSize=0;
}

Schedule::~Schedule(void)
{
	delete[] inputBuffer;
	delete[] outputBuffer;
	if(fileNum>0)
	{
		delete[] fileSizes;
		delete[] readPoses;
	}
	::DeleteCriticalSection(&threadHandleCS);
	::DeleteCriticalSection(&proxyReadCS);
	::DeleteCriticalSection(&proxyWriteCS);
	::DeleteCriticalSection(&proxySuccessCS);
	::DeleteCriticalSection(&readCS);
	::DeleteCriticalSection(&writeCS);
	::DeleteCriticalSection(&errorCS);
	delete[] proxyStatus;
	delete[] threadStatus;
	delete[] threadHandles;
	::CoUninitialize();
}
void Schedule::PushAThreadInstance(HANDLE* pEvents)
{
	::EnterCriticalSection(&threadHandleCS);
	instances.push_back(pEvents);
	::LeaveCriticalSection(&threadHandleCS);
}
void Schedule::EraseAThreadInstance(HANDLE* pEvents)
{
	::EnterCriticalSection(&threadHandleCS);
	ThreadInstances::iterator it=std::find(instances.begin(),instances.end(),pEvents);
	if(it!=instances.end())
		instances.erase(it);
	::LeaveCriticalSection(&threadHandleCS);
}
void Schedule::SignalThreadEvents(void)
{
	::EnterCriticalSection(&threadHandleCS);
	ThreadInstances::iterator it=instances.begin();
	while(it!=instances.end())
	{
		HANDLE* pEvents=*it;
		for(DWORD i=0;i<connPerThread;i++)
		{
			::SetEvent(pEvents[i]);
		}
		it++;
	}
	::LeaveCriticalSection(&threadHandleCS);
}
void Schedule::SetIPCEvent(HANDLE pEvent)
{
	ipcEvent=pEvent;
}
void Schedule::SignalIPCEvent(void)
{
	if(ipcEvent)
		::SetEvent(ipcEvent);
}
void Schedule::Suspend(void)
{
	isSuspend=true;
	SignalThreadEvents();
}
void Schedule::Resume(void)
{
	isSuspend=false;
}
DWORD Schedule::PopName(LPSTR namebuf)
{
	::EnterCriticalSection(&readCS);
	if(inputPtr>=inputSize)
	{
		inputPtr=0;

		int ix=-1;
		for(DWORD i=0;i<fileNum;i++)
		{
			DWORD size=GetFileSize(inputFiles[i]);
			if(size==INVALID_FILESIZE)
			{
				fileSizes[i]=readPoses[i];
			}
			else
			{
				fileSizes[i]=size;
				if(readPoses[i]<size && ix<0)
				{
					ix=i;
				}
			}
		}
		if(ix>=0)
		{
			std::ifstream file;
			file.open(inputFiles[ix],std::ios_base::in);
			if(file.good())
			{
				char flag[3];
				file.seekg(0);
				file.read(flag,3);
				bool isUnicode=false;
				if(flag[0]=='\xff' && flag[1]=='\xfe')//unicode
				{
					isUnicode=true;
				}
				else if(flag[0]=='\xef' && flag[1]=='\xbb' && flag[2]=='\xbf')//utf-8
				{
					isUnicode=true;
				}
				else if(flag[0]=='\xfe' && flag[1]=='\xff')//big endian unicode
				{
					isUnicode=true;
				}
				if(isUnicode)
				{
					::printf("\r\nfile %s is UNICODE encode , we can only read ASCII text file ! ( you can use nodepad open the file and save as ASCII text file )",inputFiles[ix]);
				}
				else
				{
					file.seekg(readPoses[ix]);
					file.read(inputBuffer,FILE_BUFFER_SIZE);
					DWORD size=file.gcount();
					inputBuffer[size]=0;
					readPoses[ix]=file.tellg();
					for(inputSize=size;inputSize>1;inputSize--)
					{
						if(inputBuffer[inputSize-1]=='\r' || inputBuffer[inputSize-1]=='\n')
						{
							inputBuffer[inputSize]=0;
							break;
						}
					}
					if(inputSize<size)
					{
						readPoses[ix]-=size-inputSize;
					}
				}
				file.close();
			}
			else
			{
				::printf("\r\ncan't open file %s !",inputFiles[ix]);
			}
		}
		else
		{
			::printf("\r\nall exist files handled !");
		}
	}	
	namebuf[0]=0;
	for(DWORD len=0;inputPtr<inputSize;inputPtr++)
	{		
		if(inputBuffer[inputPtr]=='\r' || inputBuffer[inputPtr]=='\n' || len>=BUFFER_USERNAME-1 || inputBuffer[inputPtr]=='@')
		{
			namebuf[len]=0;
			if(len>=BUFFER_USERNAME-1 || inputBuffer[inputPtr]=='@')//�������ȵ���@����ֱ��\r\n�Ĳ��ֺ���
			{
				while(inputBuffer[inputPtr]!='\r' && inputBuffer[inputPtr]!='\n' && inputPtr<inputSize)
					inputPtr++;
			}
			while((inputBuffer[inputPtr]=='\r' || inputBuffer[inputPtr]=='\n') && inputPtr<inputSize)
				inputPtr++;			
			if(::strpbrk(namebuf,"!#$%")==0 && len>=MIN_NAME_LENGTH && len<=MAX_NAME_LENGTH)
			{
				break;
			}
			else
			{
				PushError(namebuf);
				namebuf[0]=0;
				len=0;
				continue;
			}
		}
		else
		{
			namebuf[len]=inputBuffer[inputPtr];
			len++;
		}
	}
	if(namebuf[len]!=0)
		namebuf[len]=0;
	::LeaveCriticalSection(&readCS);
	return len;
}
void Schedule::PushSelect(LPCSTR userName)
{
	DWORD addLen=0;
	if(addPostfix)
		addLen=postfixLen;
	DWORD len=::strlen(userName);
	::EnterCriticalSection(&writeCS);
	if(outputPtr>FILE_BUFFER_SIZE-len-addLen-2)
	{
		::LeaveCriticalSection(&writeCS);
		FlushOutput();
		::EnterCriticalSection(&writeCS);
	}
	for(DWORD i=0;i<len && outputPtr<FILE_BUFFER_SIZE-addLen-2;i++,outputPtr++)
	{
		outputBuffer[outputPtr]=userName[i];
	}
	for(i=0;i<addLen && outputPtr<FILE_BUFFER_SIZE-2;i++,outputPtr++)
	{
		outputBuffer[outputPtr]=postfixStr.GetAt(i);
	}
	outputBuffer[outputPtr++]='\r';
	outputBuffer[outputPtr++]='\n';
	outputBuffer[outputPtr]=0;
	outputSize++;
	::LeaveCriticalSection(&writeCS);
}
void Schedule::PushError(LPCSTR userName)
{
	::printf("\r\nillegal name:%s",userName);
	::EnterCriticalSection(&errorCS);
	errorQueue.push(userName);
	::LeaveCriticalSection(&errorCS);
}
void Schedule::FlushOutput(void)
{
	char fname[1024];
	char* fn;
	int n=totalOutputCount/maxOutputLines;
	if(n>0)
	{
		fn=(LPSTR)(LPCSTR)outputFile;
		char* dot=::strrchr(fn,'.');
		if(dot!=0)
		{
			int size=dot-fn;
			::strncpy(fname,fn,size);
			fname[size]=0;
		}
		else
			::strcpy(fname,fn);
		::sprintf(fname,"%s%u.txt",fname,n);
		fn=fname;
	}
	else
	{
		fn=(LPSTR)(LPCSTR)outputFile;
	}
	::EnterCriticalSection(&writeCS);
	std::ofstream file;
	if(::PathFileExists(fn))
        file.open(fn,std::ios_base::app|std::ios_base::binary);
	else
		file.open(fn,std::ios_base::out|std::ios_base::binary);
	file.write(outputBuffer,outputPtr);
	file.close();
	totalOutputCount+=outputSize;
	outputPtr=0;
	outputSize=0;
	::LeaveCriticalSection(&writeCS);
	::EnterCriticalSection(&errorCS);
	DWORD size=errorQueue.size();
	totalBadNameCount+=size;
	if(::PathFileExists(errorFile))
        file.open(errorFile,std::ios_base::app|std::ios_base::binary);
	else
		file.open(errorFile,std::ios_base::out|std::ios_base::binary);
	for(DWORD i=0;i<size;i++)
	{		
		CString& r=errorQueue.front();
		int len=r.GetLength();
		file.write(r,len);
		errorQueue.pop();
		file.put('\r');
		file.put('\n');
	}
	file.close();
	::LeaveCriticalSection(&errorCS);
}
void Schedule::CalcCount(const unsigned __int64& id,DWORD v)
{
	DWORD index=GetProxyIndex(id);
	if(index==PROXYINDEX_INVALID)
		return;
	ProxyStatus& ps=proxyStatus[index];
	ps.Total+=v;
	ps.Count++;
	if(ps.Count>0)
		ps.Interval=ps.Total/ps.Count;
	if(ps.Count>=0xffffffff)
	{
		ps.Total=0;
		ps.Count=0;
	}
}
struct HeapSortCompare
{
	Schedule* pSchedule;
	//���ڶ�����ÿ����ѡInterval���ģ�Ҳ���Ƿ�Ӧ�����ģ�
	bool operator () (const unsigned __int64& id1,const unsigned __int64& id2) const
	{
		DWORD ix1=pSchedule->GetProxyIndex(id1,false);
		DWORD ix2=pSchedule->GetProxyIndex(id2,false);
		if(ix1==PROXYINDEX_INVALID)
			return false;
		else if(ix2==PROXYINDEX_INVALID)
			return true;
		if(pSchedule->proxyStatus[ix1].Interval<pSchedule->proxyStatus[ix2].Interval)
			return true;
		else
			return false;
	}
	HeapSortCompare():pSchedule(NULL)
	{}
};
void Schedule::StartProxyLoad(void)
{
	::EnterCriticalSection(&proxyWriteCS);
}
//���δ�ô�����У���ʹ��������Ȳ�����װ��Ĵ���
void Schedule::DoReload(void)
{
	//����������б����ϴ��Ѿ�ɾ���Ĵ���ɾ��
	DWORD disabledIndex=1-existIndex;
	if(!proxyInfo[disabledIndex].empty())
	{
		//�������ô����б�
		for(DWORD i=0;i<usedProxyQueue.size();)
		{
			UsedProxyQueue::iterator pit0=usedProxyQueue.begin();
			std::advance(pit0,i);
			const unsigned __int64& id=*pit0;
			ProxyID_Index::iterator pit=proxyInfo[disabledIndex].find(id);
			if(pit!=proxyInfo[disabledIndex].end())
			{
				//ɾ��ʱָ�뱣��ԭ��λ��
				usedProxyQueue.erase(pit0);
			}
			else
			{
				//��ɾ����ָ�����һλ
				i++;
			}
		}
		//������ô������
		DWORD qsize=unusedProxyQueue.size();
		for(i=0;i<qsize;i++)
		{
			unsigned __int64 id=unusedProxyQueue.front();
			unusedProxyQueue.pop();
			ProxyID_Index::iterator pit=proxyInfo[disabledIndex].find(id);
			if(pit==proxyInfo[disabledIndex].end())
			{
				unusedProxyQueue.push(id);
			}
		}
	}
	//��ս��ô����б�
	ProxyID_Index::iterator dit=proxyInfo[disabledIndex].begin();
	while(dit!=proxyInfo[disabledIndex].end())
	{
		DWORD ix=dit->second;
		//���ճ���������¼��δ���������ȶ�����
		unusedProxyIndex.push(ix);
		dit++;
	}
	proxyInfo[disabledIndex].clear();
	//�������ô����������ȶ���
	proxyNum=proxyInfo[existIndex].size();
	UsedProxyIndex usedProxyIndex;
	ProxyID_Index::iterator eit=proxyInfo[existIndex].begin();
	while(eit!=proxyInfo[existIndex].end())
	{
		DWORD ix=eit->second;
		usedProxyIndex.push(ix);
		eit++;
	}
	//����������Ϣ�ռ�
	while(!usedProxyIndex.empty() && !unusedProxyIndex.empty())
	{
		DWORD ix=usedProxyIndex.top();
		DWORD nullix=unusedProxyIndex.top();
		//��С�������������������С������߶Ե�
		if(ix>nullix)
		{
			usedProxyIndex.pop();
			unusedProxyIndex.pop();
			usedProxyIndex.push(nullix);
			unusedProxyIndex.push(ix);
			
			proxyStatus[nullix]=proxyStatus[ix];
			
			unsigned __int64 id=GetProxyID(proxyStatus[nullix].IP,proxyStatus[nullix].Port);
			proxyInfo[existIndex][id]=nullix;
		}
		else
		{
			//��С�������������������������������
			break;
		}
	}
	//��������/���ڴ����б��Ա�Ӻ����Ĳ����дӽ����б�������ǰҪ�õĴ����ʣ���������õĴ���
	existIndex=disabledIndex;
}
void Schedule::PushProxy(DWORD ip,DWORD port)
{
	unsigned __int64 id=GetProxyID(ip,port);
	//��������Ѿ����Ѵ��ڴ����б��У����������(�Ѵ����б����ڱ�������װ��ʱ��պ�ѡ������ģ����
	//���ִ�����Ĵ��������У�����װ������б����ظ����򵥶�������)
	ProxyID_Index::iterator it1=proxyInfo[existIndex].find(id);
	if(it1!=proxyInfo[existIndex].end())
	{
		return;
	}
	//��������Ѿ��ڴ���ѡ���ô����б��У������ӽ����б�ת���Ѵ����б�
	DWORD disabledIndex=1-existIndex;
	ProxyID_Index::iterator it2=proxyInfo[disabledIndex].find(id);
	if(it2!=proxyInfo[disabledIndex].end())
	{
		proxyInfo[existIndex][it2->first]=it2->second;
		proxyInfo[disabledIndex].erase(it2);
		return;
	}
	//�����ӵĴ�����Ҫʹ���µ������ռ�
	if(!unusedProxyIndex.empty())
	{
		//��δ�������л����С��һ������
		DWORD ix=unusedProxyIndex.top();
		unusedProxyIndex.pop();
		//����������Ӧ�Ĵ����¼�����ö���β
		unusedProxyQueue.push(id);
		proxyInfo[existIndex][id]=ix;

		ProxyStatus ps;
		ps.IP=ip;
		ps.Port=port;
		proxyStatus[ix]=ps;
		proxyNum++;
	}
	else
	{
		ASSERT(FALSE);
	}
}
void Schedule::EndProxyLoad(void)
{
	::LeaveCriticalSection(&proxyWriteCS);
}
void Schedule::CheckProxy(void)
{
	::EnterCriticalSection(&proxyWriteCS);
	::EnterCriticalSection(&proxyReadCS);
	/*::EnterCriticalSection(&proxySuccessCS);	
	if(!successSet.empty())
	{
		UINT64Set::iterator sit0=successSet.begin();
		while(sit0!=successSet.end())
		{
			const unsigned __int64& id=*sit0;
			DWORD ix=GetProxyIndex(id,false);
			if(ix!=PROXYINDEX_INVALID)
			{
				if(proxyStatus[ix].InUse==0)
				{
					proxyStatus[ix].InUse=1;
					usedProxyQueue.push_back(id);
				}
			}
			sit0++;
		}
		DWORD qsize=unusedProxyQueue.size();
		for(DWORD i=0;i<qsize;i++)
		{
			unsigned __int64 id=unusedProxyQueue.front();
			unusedProxyQueue.pop();
			UINT64Set::iterator sit=successSet.find(id);
			if(sit==successSet.end())
			{
				unusedProxyQueue.push(id);
			}
		}
	}
	::LeaveCriticalSection(&proxySuccessCS);*/
	HeapSortCompare hsc;
	hsc.pSchedule=this;
	std::make_heap(usedProxyQueue.begin(),usedProxyQueue.end(),hsc);
	//�Ӵ��ô����б���ɾ����Ӧ�����ļ�����������δ�ô������
	DWORD size1=usedProxyQueue.size();
	for(DWORD i=0;i<checkNum && i<usedProxyNum && !usedProxyQueue.empty();i++)
	{
		const unsigned __int64& id=usedProxyQueue.front();
		DWORD ix=GetProxyIndex(id,false);
		if(ix!=PROXYINDEX_INVALID)
		{
			proxyStatus[ix].InUse=0;
			unusedProxyQueue.push(id);
		}
		std::pop_heap(usedProxyQueue.begin(),usedProxyQueue.end(),hsc);
		usedProxyQueue.pop_back();
	}
	DWORD size2=usedProxyQueue.size();
	//��δ�ô�����������ô����б�����Ӵ���ֱ�����ô����������Ҫ���δ�ô���Ϊ��
	while(usedProxyQueue.size()<usedProxyNum && !unusedProxyQueue.empty())
	{
		const unsigned __int64& id=unusedProxyQueue.front();
		DWORD ix=GetProxyIndex(id,false);
		if(ix!=PROXYINDEX_INVALID)
		{
			proxyStatus[ix].InUse=1;
			usedProxyQueue.push_back(id);
		}
		unusedProxyQueue.pop();
	}
	::LeaveCriticalSection(&proxyReadCS);
	::LeaveCriticalSection(&proxyWriteCS);
}
void Schedule::Loop(void)
{
	if(::PathFileExists(outputFile))
	{
		if(IDYES==::MessageBox(NULL,"Ouput file exist , overwrite it ? (or program exit)","confirm",MB_YESNO))
			::DeleteFile(outputFile);
		else
			return;
	}
	if(fileNum<=0)
	{
		::MessageBox(NULL,"No input file , program can't continue !","alert",MB_OK);
		return;
	}
	DWORD ipcThreadID=0;
	HANDLE ipcThread=(HANDLE)::_beginthreadex(NULL,0,&IPCPipe::CommandThread,(LPVOID*)this,0,(UINT*)&ipcThreadID);
	if(ipcThread==NULL)
	{
		::PrintLastErrorMsg("create IPC thread");
		return;
	}
	for(DWORD i=0;i<threadNum;i++)
	{
		threadHandles[i]=(HANDLE)::_beginthreadex(NULL,0,&AccessSocket::CheckThread,(LPVOID*)this,0,(UINT*)&(threadStatus[i].ThreadID));
		if(threadHandles[i]==NULL)
		{
			CString temp;
			temp.Format("create thread failed:%d,error code:%u,%s!",i,::GetLastError(),::GetLastErrorMsg());
			::MessageBox(NULL,temp,"",MB_OK);
			for(DWORD j=0;j<i;j++)
				::CloseHandle(threadHandles[j]);
			return;
		}
		else
		{
			threadid_index[threadStatus[i].ThreadID]=i;
		}
	}

	DWORD timeCount=0;
	for(;;)
	{
		::Sleep(1000);
		HWND hwnd=::FindWindow(NULL,"OnlineStatusCheck");
		if(!::IsWindow(hwnd))
		{
			//֪ͨ���߳��˳����ȴ����߳��˳�����ֹ����
			isTerminate=true;
			SignalThreadEvents();
			for(i=0;i<threadNum;i+=MAXIMUM_WAIT_OBJECTS)
			{
				int num=MAXIMUM_WAIT_OBJECTS;
				if(i>threadNum-MAXIMUM_WAIT_OBJECTS || threadNum<MAXIMUM_WAIT_OBJECTS)num=threadNum-i;
				DWORD dwStatus = ::WaitForMultipleObjects(num,threadHandles+i,TRUE,INFINITE);
				if (dwStatus == WAIT_FAILED)
				{
					CString temp;
					temp.Format("wait thread terminate failed:%u,%s!",::GetLastError(),::GetLastErrorMsg());
					::MessageBox(NULL,temp,"",MB_OK);
					break;
				}
			}
			for(i=0;i<threadNum;i++)
				::CloseHandle(threadHandles[i]);
			SignalIPCEvent();
			::WaitForSingleObject(ipcThread,INFINITE);
			::CloseHandle(ipcThread);
			break;
		}
		else if(!isSuspend)
		{
			//����������Ϣ�����߳������״̬��ʾ
			if(timeCount % uiUpdateInterval == 0)
			{
				COPYDATASTRUCT ts;
				ts.dwData=COPYDATA_THREAD_STATUS;
				ts.cbData=sizeof(ThreadStatus)*threadNum;
				ts.lpData=(LPVOID)threadStatus;
				::SendMessage(hwnd,WM_COPYDATA,NULL,(LPARAM)&ts);

				if(proxyNum>0)
				{
					COPYDATASTRUCT ps;
					ps.dwData=COPYDATA_PROXY_STATUS;
					ps.cbData=sizeof(ProxyStatus)*proxyNum;
					ps.lpData=(LPVOID)proxyStatus;
					::SendMessage(hwnd,WM_COPYDATA,NULL,(LPARAM)&ps);
				}
			}

			::PostMessage(hwnd,WM_ONLINE_CHECK,WPARAM_TOTAL_SUCCESS,totalSuccessfulCount);
			::PostMessage(hwnd,WM_ONLINE_CHECK,WPARAM_TOTAL_OUTPUT_NAMES,totalOutputCount);
			::PostMessage(hwnd,WM_ONLINE_CHECK,WPARAM_TOTAL_BAD_NAMES,totalBadNameCount);

			if(timeCount % outputInterval == 0)
			{
				::PostMessage(hwnd,WM_ONLINE_CHECK,WPARAM_AVER_PERFORMANCE,averPerformance);
				FlushOutput();
			}
			if(timeCount % checkInterval == 0)
			{
				if(proxyNum>0)
					CheckProxy();
			}
		}
		timeCount=(timeCount+1) % MAX_TIME_COUNT;
	}
	FlushOutput();
}
