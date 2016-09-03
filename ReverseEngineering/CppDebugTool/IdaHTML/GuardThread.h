#pragma once

//GuardThread����������Ϊ��̨�߳̽�����GUI����ת����ǰ̨�߳�ͳһ����
//ת������Ϊ��
//	  Event=>GuardThread=>MSG=>GUI Thread

class GuardThread
{
public:
	static GuardThread* Start(void);
	static UINT WINAPI Loop(LPVOID obj);
	static void Terminate(GuardThread*& pObj);
public:
	void Signal(void);
private:
	GuardThread(void);
	~GuardThread(void);
private:
	void TerminateSignal(void);
	inline void OnceWork(void);
private:
	HANDLE hThread;//�������ⲿ���䣬�ⲿ�������ڶ���ʵ����ʼ����ֵ������ʵ���ͷ�ǰȡ��ֵ��

	bool isTerm;
	HANDLE hEvent;
};
