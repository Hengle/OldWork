#pragma once

class DebugThread
{
public:
	static DebugThread* Start(void);
	static UINT WINAPI Loop(LPVOID obj);
	static void Terminate(DebugThread*& pObj);
public:
	void Signal(void);
private:
	DebugThread(void);
	~DebugThread(void);
private:
	void TerminateSignal(void);
	inline void OnceWork(void);
private:
	HANDLE hThread;//�������ⲿ���䣬�ⲿ�������ڶ���ʵ����ʼ����ֵ������ʵ���ͷ�ǰȡ��ֵ��

	bool isTerm;
	HANDLE hEvent;
};
