#pragma once

#include "stdafx.h"
#include "resource.h"
#include <map>

#include <ida.hpp>
#include <idp.hpp>
#include <loader.hpp>
#include <expr.hpp>

#define _Module _AtlBaseModule
#include "Disasm.h"
#include "HdSerialLib.h"
#include "MyWindowProc.h"
#include "WinForIE.h"

#pragma comment(lib,"../../debuger_include/ida.lib")

#define WM_MY_RUN_REQUESTS	WINFORIE_LAST_MSG+1

typedef WinForIE<true,WS_POPUP|WS_BORDER|WS_CAPTION|WS_SYSMENU|WS_MAXIMIZEBOX|WS_MINIMIZEBOX| WS_SIZEBOX
		,WS_EX_LEFT> Dlg;//WS_EX_TOPMOST
typedef std::map<int,BOOL> HitInfo;

extern Dlg dlg;
extern bool isNT;
extern HWND mainHwnd;

class IdcApi : public IDispatch
{
	typedef IdcApi ComObj;
	typedef std::map<CString,DISPID> NAME_IDS;	
	IdcApi():m_cRef(0)
	{
		ResetBuildIn();
	}
public:
	static inline ComObj* CreateDispatch()
	{
		ComObj* p=new ComObj();
		p->AddRef();
		return p;
	}

	STDMETHODIMP QueryInterface(REFIID riid, void** ppv)
	{
		if (NULL == ppv) return E_POINTER;
		*ppv = NULL;
		
		HRESULT hr = S_OK;
		
		if ((IID_IUnknown == riid) || (IID_IDispatch == riid))
			*ppv = static_cast<IDispatch*>(this);		

		if(*ppv!=NULL)
		{
			reinterpret_cast<IUnknown*>(*ppv)->AddRef();
			return hr;
		}
		return E_NOINTERFACE;		
	}
	STDMETHODIMP_(ULONG) AddRef(void)
	{
		return ++m_cRef;
	}
	STDMETHODIMP_(ULONG) Release(void)
	{
		if (0 != --m_cRef) return m_cRef;
		delete this;
		return 0;
	}
	STDMETHODIMP GetTypeInfoCount(UINT* pctinfo)
	{
		if (NULL == pctinfo) return E_POINTER;
		*pctinfo = 0;
		return S_OK;
	}
	STDMETHODIMP GetTypeInfo(UINT iTInfo, LCID lcid, ITypeInfo** ppTInfo)
	{
		if (NULL == ppTInfo) return E_POINTER;
		*ppTInfo = NULL;
		return E_FAIL;
	}
	STDMETHODIMP GetIDsOfNames(REFIID riid, LPOLESTR* rgszNames, UINT cNames, LCID lcid, DISPID* rgDispId)
	{
		if (IID_NULL != riid)
			return DISP_E_UNKNOWNINTERFACE;
		
		if (NULL == rgDispId) return E_POINTER;
		if (NULL == rgszNames) return E_POINTER;
		if (cNames != 1) return E_INVALIDARG;
		CString name(*rgszNames);
		NAME_IDS::iterator it=name_ids.find(name);
		if(it!=name_ids.end())
		{
			*rgDispId=it->second;
			return S_OK;
		}
		else
		{
			*rgDispId = 0;
			return DISP_E_MEMBERNOTFOUND;
		}
	}
	STDMETHODIMP Invoke(DISPID dispIdMember, REFIID riid, LCID lcid, WORD wFlags, DISPPARAMS* pDispParams, VARIANT* pVarResult, EXCEPINFO* pExcepInfo, UINT* puArgErr)
	{
		if (IID_NULL != riid)
			return DISP_E_UNKNOWNINTERFACE;		
		if (NULL == pDispParams) return E_POINTER;
		
		extfun_t* pfun=(extfun_t*)dispIdMember;
		if(pfun)
		{
			//ȷ�����������������з�ɱ����
			bool haveWildArg=false;
			int ai=0;
			for(;pfun->args[ai];ai++)
			{
				if(pfun->args[ai]==VT_WILD)
				{
					haveWildArg=true;
					break;
				}
			}
			//��ʼ������ֵ����
			value_t res;
			res.vtype=VT_LONG;
			if(haveWildArg)
				res.num=pDispParams->cArgs;
			else
				res.num=0;
			//��������ռ䲢��ʼ��������
			value_t* args=new value_t[pDispParams->cArgs];
			for(int i=pDispParams->cArgs-1;i>=0;i--)
			{
				VARIANT& var=pDispParams->rgvarg[i];
				int ix=pDispParams->cArgs-i-1;
				if(ix<ai)//����֪���͵Ĳ�����IDC�ڲ�������������ת����ֵ
				{
					switch(pfun->args[ix])
					{
					case VT_STR:
						{
							args[ix].vtype=VT_STR;
							if(var.vt==VT_BSTR)
							{
								CString temp(var.bstrVal);
								args[ix].str=(char*)qalloc(temp.GetLength());
								strcpy(args[ix].str,temp);
							}
							else
							{
								CComVariant v=var;
								v.ChangeType(VT_BSTR);
								CString temp(v.bstrVal);
								args[ix].str=(char*)qalloc(temp.GetLength());
								strcpy(args[ix].str,temp);
							}
						}
						break;
					case VT_FLOAT:
						{
							//��Ϊ��֪��IDC����α�ʾ�������ģ����ｫ��������תΪ�ַ����ٵ�IDC APIתΪ������
							CComVariant v=var;
							v.ChangeType(VT_BSTR);
							CString temp(v.bstrVal);
							args[ix].vtype=VT_STR;
							args[ix].str=(char*)qalloc(temp.GetLength());
							strcpy(args[ix].str,temp);
							VarFloat(&args[ix]);
						}
						break;
					default:
						{
							CComVariant v=var;
							v.ChangeType(VT_I4);
							args[ix].vtype=VT_LONG;
							args[ix].num=(sval_t)v.lVal;
						}
					}
				}
				else if(haveWildArg)//�ɱ�������������͸�ֵ
				{
					switch(var.vt)
					{
					case VT_BSTR:
						{
							args[ix].vtype=VT_STR;
							CString temp(var.bstrVal);
							args[ix].str=(char*)qalloc(temp.GetLength());
							strcpy(args[ix].str,temp);
						}
						break;
					case VT_R4:
					case VT_R8:
						{
							//��Ϊ��֪��IDC����α�ʾ�������ģ����ｫ��������תΪ�ַ����ٵ�IDC APIתΪ������
							CComVariant v=var;
							v.ChangeType(VT_BSTR);
							CString temp(v.bstrVal);
							args[ix].vtype=VT_STR;
							args[ix].str=(char*)qalloc(temp.GetLength());
							strcpy(args[ix].str,temp);
							VarFloat(&args[ix]);
						}
						break;
					default:
						{
							CComVariant v=var;
							v.ChangeType(VT_I4);
							args[ix].vtype=VT_LONG;
							args[ix].num=(sval_t)v.lVal;
						}
					}
				}
				else//����Ĳ�������
				{
					break;
				}
			}
			//����IDC�ڲ�����
			error_t r=eOS;
			try
			{
				r=(*(pfun->fp))(args,&res);
			}
			catch(...)
			{}
			//�ͷŲ����ռ�
			delete[] args;
			//����������ֵ
			switch(res.vtype)
			{
			case VT_STR:
				{
					CComVariant(res.str).Detach(pVarResult);
					VarFree(&res);//�ͷ��ַ���
				}
				break;
			case VT_FLOAT:
				{					
					VarString(&res);//��תΪ�ַ���
					double v=::atof(res.str);//��תΪdouble
					CComVariant(v).Detach(pVarResult);
					VarFree(&res);//�ͷ�ǰ��ת��Ϊ�ַ���ʱidaproΪ�ַ�������Ŀռ�
				}
				break;
			default:
				{
					VarLong(&res);
					CComVariant((int)res.num).Detach(pVarResult);
				}
			}
			if(r==eOk)
			{
				return S_OK;
			}
			else
			{
				return E_INVALIDARG;
			}
		}
		return DISP_E_MEMBERNOTFOUND;
	}
private:
	void ResetBuildIn(void)
	{
		name_ids.clear();
		for(int fi=0;fi<IDCFuncs.qnty;fi++)
		{
			name_ids[CString(IDCFuncs.f[fi].name)]=(DISPID)&IDCFuncs.f[fi];
		}
	}
private:
	unsigned int m_cRef;
	NAME_IDS name_ids;
};


class IdaApi : public IDispatch
{
public:
	virtual DWORD __stdcall get_Hwnd(void)
	{
		return (DWORD)mainHwnd;
	}
	virtual IDispatch* __stdcall get_IdcApi(void)
	{
		IDispatch* p=NULL;
		pIDC.QueryInterface(&p);
		return p;
	}
public:
	BEGIN_INTF(IdaApi)
		PROPERTYGET(Hwnd,true)
		PROPERTYGET(IdcApi,true)
	END_INTF()
public:
	bool OnTranslateMsg(HWND,LPMSG pMsg,UINT)
	{
		if(pMsg->message==WM_KEYDOWN && (pMsg->wParam>=VK_LEFT && pMsg->wParam<=VK_DOWN || pMsg->wParam==VK_BACK || pMsg->wParam==VK_RETURN))
			return false;
		return (bool)dlg.PreTranslateMessage(pMsg);
	}
	bool OnIEMsg(HWND hWnd,UINT uMsg, WPARAM wParam, LPARAM lParam, LRESULT& lResult, BOOL& bHandled)
	{
		if(uMsg==WM_MY_RUN_REQUESTS)//�����GUI������Ҫ��ǰ̨GUI�߳�������ִ�У��˴�ͳһ����������������GuardThreadת������
		{
			return true;
		}
		else if(uMsg==WM_SYSCOMMAND)
		{			
			if(wParam==SC_CLOSE)
			{
				if(dlg.IsWindowVisible())
				{
					dlg.ShowWindow(SW_HIDE);
					return true;
				}
			}
		}
		return false;
	}
protected:
	IdaApi()
	{
		IdcApi* pApi=IdcApi::CreateDispatch();
		pIDC.Attach(pApi);

		::ZeroMemory(&ver,sizeof(OSVERSIONINFO));
        ver.dwOSVersionInfoSize=sizeof(OSVERSIONINFO);
        ::GetVersionEx(&ver);
        if(ver.dwMajorVersion>=5)
        {
			isNT=true;
			MyMessageHook<0>::TranslateMessageProcsRef()+=EventFactory::Produce(this,&IdaApi::OnTranslateMsg);
            MyMessageHook<0>::SetHook();
        }
		else
		{
			isNT=false;
		}
	}
	~IdaApi()
	{
		if(ver.dwMajorVersion>=5)
        {
            MyMessageHook<0>::UnHook();
        }
	}
private:
	CComPtr<IDispatch> pIDC;
	OSVERSIONINFO ver;
};