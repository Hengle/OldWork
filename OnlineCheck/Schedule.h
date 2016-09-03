#pragma once
#define FILE_BUFFER_SIZE			1024*1024
#define WAIT_PROXY_TIME				1*1000
#define WAIT_QUEUE_TIME				60*1000
#define INVALID_FILESIZE			0xFFFFFFFF
#define MAX_PROXY_NUMBER			10000

#define BUFFER_USERNAME				33
//
#define NO_CONSOLE_MSG

#ifdef NO_CONSOLE_MSG
	#define PRINT_CONSOLE_MSG		::nullprintf
#else
	#define PRINT_CONSOLE_MSG		::printf
#endif

#ifdef NO_CONSOLE_MSG2
	#define PRINT_CONSOLE_MSG2		::nullprintf
#else
	#define PRINT_CONSOLE_MSG2		::printf
#endif

#define WM_ONLINE_CHECK						WM_APP+1
#define WPARAM_TOTAL_SUCCESS				0xFFFF0001
#define WPARAM_TOTAL_OUTPUT_NAMES			0xFFFF0002
#define WPARAM_TOTAL_BAD_NAMES				0xFFFF0003
#define WPARAM_AVER_PERFORMANCE				0xFFFF0004

#define COPYDATA_THREAD_STATUS				0
#define COPYDATA_PROXY_STATUS				1

extern int __cdecl  nullprintf(const char *, ...);
extern CString GetErrorMsg(DWORD err);
extern CString GetLastErrorMsg(void);
extern void PrintErrorMsg(DWORD err,LPCSTR title=NULL);
extern void PrintLastErrorMsg(LPCSTR title=NULL);

typedef std::queue<CString> ErrorQueue;
typedef std::vector<CString> InputFiles;
struct StatusInfo
{
	DWORD Status;
	CString Name;
	CString Url;
};
typedef std::vector<StatusInfo> StatusInfos;

#define THREADINDEX_INVALID		0xffffffff
#define PROXYINDEX_INVALID		0xffffffff
#define DIRECT_CONNECT			0xffffffffffffffff
#define NOUSABLE_PROXY			0xfffffffffffffffe

struct ProxyStatus
{
	volatile DWORD IP;
	volatile DWORD Port;
	volatile DWORD ConnectionCount;
	volatile DWORD SuccessfulCount;
	volatile DWORD Interval;
	volatile DWORD InUse;
	volatile DWORD Total;
	volatile DWORD Count;
	ProxyStatus():IP(0),Port(0),ConnectionCount(0),SuccessfulCount(0),InUse(0),Interval(0x7FFFFFFF),
		Total(0),Count(0)
	{}
};
struct ThreadStatus
{
	DWORD ThreadID;
	volatile DWORD SuccessfulCount;
	ThreadStatus():ThreadID(0),SuccessfulCount(0)
	{}
};
typedef stdext::hash_map<DWORD,DWORD> ThreadID_Index;
typedef std::vector<unsigned __int64> UsedProxyQueue;
typedef std::queue<unsigned __int64> UnusedProxyQueue;
typedef stdext::hash_map<unsigned __int64,DWORD> ProxyID_Index;
typedef stdext::hash_set<unsigned __int64> UINT64Set;
typedef std::priority_queue<DWORD,std::vector<DWORD>,std::greater<DWORD> > UnusedProxyIndex;
typedef std::priority_queue<DWORD,std::vector<DWORD>,std::less<DWORD> > UsedProxyIndex;

typedef std::vector<HANDLE*> ThreadInstances;

class Schedule
{
public:
	Schedule(void);
	virtual ~Schedule(void);
	void Loop(void);
	DWORD PopName(LPSTR namebuf);
	void PushSelect(LPCSTR userName);
	void PushError(LPCSTR userName);
	void FlushOutput(void);
	void CalcCount(const unsigned __int64& id,DWORD v);
	void StartProxyLoad(void);
	void DoReload(void);
	void PushProxy(DWORD ip,DWORD port);
	void EndProxyLoad(void);
	void CheckProxy(void);
	static inline DWORD GetFileSize(const char * fileName)
    {
		WIN32_FILE_ATTRIBUTE_DATA fileAttrib;
		if(!::GetFileAttributesEx(fileName,GetFileExInfoStandard,&fileAttrib))
			return INVALID_FILESIZE;
		if(fileAttrib.nFileSizeHigh)return INVALID_FILESIZE;
		return fileAttrib.nFileSizeLow;
    }
	static inline unsigned __int64 GetProxyID(DWORD ip,DWORD port)
	{
		union
		{
			struct
			{
				DWORD ip;
				DWORD port;
			}ipport;
			unsigned __int64 val;
		}v;
		v.ipport.ip=ip;
		v.ipport.port=port;
		return v.val;
	}
	static inline DWORD GetProxyIP(const unsigned __int64& id)
	{
		union
		{
			struct
			{
				DWORD ip;
				DWORD port;
			}ipport;
			unsigned __int64 val;
		}v;
		v.val=id;
		return v.ipport.ip;
	}
	static inline DWORD GetProxyPort(const unsigned __int64& id)
	{
		union
		{
			struct
			{
				DWORD ip;
				DWORD port;
			}ipport;
			unsigned __int64 val;
		}v;
		v.val=id;
		return v.ipport.port;
	}
public:
	DWORD GetThreadIndex(DWORD id)
	{
		ThreadID_Index::iterator it=threadid_index.find(id);
		if(it!=threadid_index.end())
		{
			return it->second;
		}
		return THREADINDEX_INVALID;
	}
	DWORD GetProxyIndex(const unsigned __int64& id,bool lock=true)
	{
		DWORD ix=PROXYINDEX_INVALID;
		if(lock)
			::EnterCriticalSection(&proxyWriteCS);
		DWORD disabledIndex=1-existIndex;
		ProxyID_Index::iterator it=proxyInfo[existIndex].find(id);
		if(it!=proxyInfo[existIndex].end())
			ix=it->second;
		else
		{
			ProxyID_Index::iterator it2=proxyInfo[disabledIndex].find(id);
			if(it2!=proxyInfo[disabledIndex].end())
				ix=it2->second;
		}
		if(lock)
			::LeaveCriticalSection(&proxyWriteCS);
		return ix;
	}
	void IncreaseThreadCount(DWORD id,const unsigned __int64& proxyID)
	{
		DWORD ix=GetThreadIndex(id);
		if(ix==THREADINDEX_INVALID)
			return;
		totalSuccessfulCount++;
		threadStatus[ix].SuccessfulCount++;
		if(proxyID==DIRECT_CONNECT)
		{
			directSuccessfulCount++;
			PRINT_CONSOLE_MSG("\r\nthread:%u=%u success count:%u , direct connect success count:%u , total success count:%u",ix,threadStatus[ix].ThreadID,threadStatus[ix].SuccessfulCount,directSuccessfulCount,totalSuccessfulCount);
		}
		else
		{
			DWORD proxyIndex=GetProxyIndex(proxyID);
			if(proxyIndex==PROXYINDEX_INVALID)
				return;
			proxyStatus[proxyIndex].SuccessfulCount++;
			in_addr addr;
			addr.S_un.S_addr=proxyStatus[proxyIndex].IP;
			PRINT_CONSOLE_MSG("\r\nthread:%u=%u success count:%u , proxy %u=%s,%u success count:%u , total success count:%u",ix,threadStatus[ix].ThreadID,threadStatus[ix].SuccessfulCount,proxyIndex,::inet_ntoa(addr),proxyStatus[proxyIndex].Port,proxyStatus[proxyIndex].SuccessfulCount,totalSuccessfulCount);
			/*::EnterCriticalSection(&proxySuccessCS);
			successSet.insert(proxyID);
			::LeaveCriticalSection(&proxySuccessCS);*/
		}
	}
	unsigned __int64 GetAUsableProxy(DWORD id)
	{
		if(proxyNum<=0)
		{
			//���Ƿ����ֱ������
			if(directConnStatus<directConnNum)
				return DIRECT_CONNECT;
		}
		else
		{
			DWORD tindex=GetThreadIndex(id);
			if(tindex==THREADINDEX_INVALID)
				return NOUSABLE_PROXY;
			DWORD delta=tindex*connPerThread/connPerProxy;			
			//�������б���ָ��ƫ�ƿ�ʼ���ҿ��ô���
			DWORD size=usedProxyQueue.size();
			if(size>delta)
			{			
				::EnterCriticalSection(&proxyReadCS);	
				unsigned __int64 rid=NOUSABLE_PROXY;
				for(DWORD i=delta;i<size+delta;i++)
				{
					unsigned __int64 id=usedProxyQueue[i%size];
					DWORD index=GetProxyIndex(id);
					if(index==PROXYINDEX_INVALID)
						continue;
					if(proxyStatus[index].ConnectionCount<connPerProxy)
					{
						rid=id;
						break;
					}
				}
				::LeaveCriticalSection(&proxyReadCS);
				if(rid!=NOUSABLE_PROXY)
					return rid;
				else
					return NOUSABLE_PROXY;
			}
		}
		return NOUSABLE_PROXY;
	}
	void IncreaseProxyConn(const unsigned __int64& id)
	{
		if(id==DIRECT_CONNECT && directConnStatus<directConnNum)
			directConnStatus++;
		DWORD index=GetProxyIndex(id);
		if(index==PROXYINDEX_INVALID)
			return;
		if(proxyStatus[index].ConnectionCount>=connPerProxy)
			return;
		proxyStatus[index].ConnectionCount++;
	}
	void DecreaseProxyConn(const unsigned __int64& id)
	{
		if(id==DIRECT_CONNECT && directConnStatus>0)
			directConnStatus--;
		DWORD index=GetProxyIndex(id);
		if(index==PROXYINDEX_INVALID)
			return;
		if(proxyStatus[index].ConnectionCount<=0)
			return;
		proxyStatus[index].ConnectionCount--;
	}
	void ClearProxyConn(const unsigned __int64& id)
	{
		if(id==DIRECT_CONNECT)
			directConnStatus=0;
		DWORD index=GetProxyIndex(id);
		if(index==PROXYINDEX_INVALID)
			return;
		proxyStatus[index].ConnectionCount=0;
	}
	bool IsWantedStatus(LPCSTR url)
	{
		StatusInfos::iterator it=statusInfos.begin();
		while(it!=statusInfos.end())
		{
			if(it->Status>0 && it->Url.CompareNoCase(url)==0)
			{
				return true;
			}
			it++;
		}
		return false;
	}
public:
	void PushAThreadInstance(HANDLE* pEvents);
	void EraseAThreadInstance(HANDLE* pEvents);
	void SignalThreadEvents(void);
	void SetIPCEvent(HANDLE pEvent);
	void SignalIPCEvent(void);
	void Suspend(void);
	void Resume(void);
public:
	char* inputBuffer;
	DWORD inputPtr,inputSize;
	char* outputBuffer;
	DWORD outputPtr,outputSize;
	ErrorQueue errorQueue;

	CRITICAL_SECTION proxyReadCS,proxyWriteCS,proxySuccessCS;
	//UsedProxyQueue��UnusedProxyQueue�Ǵ�����Ȳ��Ե�ʵ�֣�ǰ���ڵ���ʱ��һ��������ȶ��У���ִ��ʱ�䣩��
	//������һ����ͨ���У�����Э����ɴ�����ȣ������еĴ���֮������ǰ���ô���
	DWORD usedProxyNum;
	UsedProxyQueue usedProxyQueue;
	UnusedProxyQueue unusedProxyQueue;
	//�ɹ����ϼ�¼�������������гɹ����Ĵ���������ʵ�ֵ��Ȳ���
	UINT64Set successSet;
	//ProxyID_Index���Խ�����ID��ip/port��ӳ�䵽����״̬����������Ϊ��ǰ���ڴ����뵱ǰ���ô�����Ҫ���ڴ���
	//�������װ�룬���ǵ���װʱ�����д�������ʹ�ã���˵�ǰ���ô���ʵ�����ǿ��ô���ֱ����һ����װʱ���ô�
	//�����ȫ��������������Ȼ����ȫ[��ҪӰ��״̬��¼����ȷ��]��ȡ������װ�ļ���������������֮�����ɵ�ǰ
	//���ô���
	ProxyID_Index proxyInfo[2];
	DWORD existIndex;
	//���ô���������δ�ô���������¼����״̬�ռ��ʹ�������Ϊ�˸�Ч����UI����״̬��Ϣ����װʱ��Ҫ����״̬�ռ�
	//���������ȶ������ڴ�Ŀ�ģ�ǰ����������ȣ���������С����
	UnusedProxyIndex unusedProxyIndex;

	ProxyStatus* proxyStatus;
	ThreadStatus* threadStatus;
	ThreadID_Index threadid_index;
	HANDLE* threadHandles;
	ThreadInstances instances;
	CRITICAL_SECTION threadHandleCS;

	HANDLE ipcEvent;

	DWORD fileNum;
	DWORD* fileSizes;
	DWORD* readPoses;
	CRITICAL_SECTION readCS,writeCS,errorCS;

	InputFiles inputFiles;
	StatusInfos statusInfos;
	CString targetUrl;
	CString outputFile;
	CString errorFile;
	DWORD connPerProxy,connPerThread;
	DWORD threadNum,proxyNum;
	int addPostfix,postfixLen;
	CString postfixStr;

	DWORD directConnNum,maxOutputLines;
	DWORD sleepPerLink;
	__int64 countFrequency;
	DWORD averPerformance;
	volatile DWORD directConnStatus;
	volatile DWORD directFailedCount;
	volatile DWORD directSuccessfulCount;
	volatile DWORD totalSuccessfulCount;
	volatile DWORD totalOutputCount;
	volatile DWORD totalBadNameCount;
	bool isTerminate,isSuspend;

	DWORD uiUpdateInterval,outputInterval;
	DWORD checkNum,checkInterval;
};

class AutoCloseHandle
{
public:
	AutoCloseHandle(HANDLE h)
	{
		handle=h;
	}
	~AutoCloseHandle()
	{
		DWORD flag;
		if(::GetHandleInformation(handle,&flag) && flag!=HANDLE_FLAG_PROTECT_FROM_CLOSE)
			::CloseHandle(handle);
	}
public:
	operator HANDLE(void)
	{
		return handle;
	}
private:
	HANDLE handle;
};