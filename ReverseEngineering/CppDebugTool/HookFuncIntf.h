#pragma once

interface HookFunc : public IDispatch 
{
public:
	virtual void __stdcall DirectReturnInt32(unsigned int int32Val, unsigned int popn);
	virtual void __stdcall DirectReturnInt64(unsigned int low32Val,unsigned int high32Val, unsigned int popn);
	virtual void __stdcall CallBeforeExit(UINT script);
	virtual void __stdcall BeforeExitReturnInt32(unsigned int int32Val);
	virtual void __stdcall BeforeExitReturnInt64(unsigned int low32Val,unsigned int high32Val);
	virtual unsigned int __stdcall get_Edx(void);
	virtual unsigned int __stdcall get_Ebx(void);
	virtual unsigned int __stdcall get_Ebp(void);
	virtual unsigned int __stdcall get_Esi(void);
	virtual unsigned int __stdcall get_Edi(void);
	virtual unsigned int __stdcall get_Eax(void);
	virtual unsigned int __stdcall get_Esp(void);
	virtual unsigned int __stdcall get_EFlags(void);
	virtual unsigned int __stdcall get_SegCs(void);
	virtual unsigned int __stdcall get_SegDs(void);
	virtual unsigned int __stdcall get_SegEs(void);
	virtual unsigned int __stdcall get_SegSs(void);
	virtual unsigned int __stdcall get_SegFs(void);
	virtual unsigned int __stdcall get_SegGs(void);
	virtual unsigned int __stdcall get_Eip(void);
	virtual unsigned int __stdcall get_ParamPtr(void);
	virtual unsigned int __stdcall get_ThisPtr(void);
	virtual unsigned int __stdcall get_OriginalAPI(void);
	virtual unsigned int __stdcall get_DirectReturn(void);
	virtual unsigned int __stdcall get_ReturnLowInt32(void);
	virtual unsigned int __stdcall get_ReturnHighInt32(void);
	virtual unsigned int __stdcall get_PopNBytes(void);
	virtual UINT __stdcall get_BeforeExitScript(void);
	virtual void __stdcall Log(const char* mess);
	virtual void __stdcall LogStdInfo(const char* name);
	virtual void __stdcall EnableRecursion(unsigned int val);
	virtual const char* __stdcall IntToHex(unsigned int val);
	virtual unsigned int __stdcall HexToInt(const char* val);
	virtual void __stdcall ShowUI(void);
	virtual void __stdcall CloseUI(void);
};
/*
class HookUI : public IDispatch
{
	public:
		virtual BSTR __stdcall CompileScript(BSTR scpFile);//SC�ű���app=HookUI,window=window
		virtual VARIANT __stdcall CallScript(VARIANT vals);
		virtual int __stdcall CreateHook(VARIANT func,VARIANT api,VARIANT module);
		virtual void __stdcall RemoveHook(int index);
		virtual IDispatch* __stdcall get_ExtObject(void);
		virtual BSTR __stdcall get_HookPath(void);
};
*/
/*UI�ű�����
//compileScript();//����SC�ű�
//var a=createHook(1,0x000449d8,0);//ָ���ű����ò���1Ϊ1�¹���
//var b=createHook("test",0x00044000,0);//ʹ��C++ģ����ָ�������¹��ӣ��б��ڽű�������HookScript.cfg���õĹ��ӣ�
//���ֹ���ֱ�Ӵ�Ŀ�꺯���������Ӻ�����HookScript.cpp�е�SetHookInfo���������˶�Ӧ���ӵ�ԭ�������õ�ַ(ʵ��ʵʩ��
//�õĵ�ַ,���Ǳ���ס�ĺ�����ԭʼ��ַ,�ű�������HookScript.cfg���õĹ���ͨ������HookFunc*Ҳ�ɵõ���Ӧ���õ�ַ) 
//...
//removeHook(a);
//removeHook(b);
*/