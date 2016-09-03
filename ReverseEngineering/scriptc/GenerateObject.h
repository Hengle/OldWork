#pragma once
#include <map>
#include <hash_map>
#include "Disasm.h"
using namespace std;

//IObject�ӿ������������ڽ������Զ�����󶼷��ϵĽӿڡ�
// {CDE21B1C-2CC4-4236-8C1F-2E15297762CC}
extern "C" const GUID IID_IObject;

//IString�ӿ��������ַ�������Ľӿڡ�
// {F58171C4-1B72-4dd8-AB4F-29F03B7E1CBB}
extern "C" const GUID IID_IString;
//�ڽ��ַ�����������ݲ���
class StringBase
{
protected:
	CString str;
	CComBSTR bstr;
	CComVariant variant;	
public:
	inline void Init(const char* s)
	{
		str=s;
	}
	inline void Init(const BSTR s)
	{
		str=s;
	}
	inline void Init(const VARIANT& s)
	{
		if(s.vt==VT_BSTR)
			str=s.bstrVal;
		else
		{
			CComVariant v=s;
			v.ChangeType(VT_BSTR);
			str=v.bstrVal;
		}
	}
	inline CString& Ref(void)
	{
		return str;
	}
	inline CComBSTR& UnicodeRef(void)
	{
		if(bstr.Length()==0)
			bstr=str;
		return bstr;
	}
	inline CComVariant& VariantRef(void)
	{
		if(variant.vt==VT_EMPTY)
			variant=str;
		return variant;
	}
};

class IObject : public IDispatch
{
public:
	virtual void __stdcall insert(VARIANT key,VARIANT val)=0;	//��ӳ�Ա�����������ԣ���ʹ������ʱ����һͬ���ַ������ԡ�
	virtual void __stdcall erase(VARIANT key)=0;				//ɾ�������Ա	
	virtual void __stdcall put_length(int size)=0;				
	virtual int __stdcall get_length(void)=0;					//�������Ե����DISPIDֵ���������ַ������Զ�Ӧ��DISPIDֵ
};
//void __stdcall set(int pos,VARIANT v);						//��������д�����ǹ�Invoke���õģ��������麯��
//VARIANT __stdcall get(int pos);								//�������Զ������ǹ�Invoke���õģ��������麯��

//�����ڽ��ַ�������ӿڣ����ö�̳������IObject��DynamicDispatch��IStringʵ��������
class IString : public IObject,public StringBase
{
public:
	//���һ�������Ƿ��ڲ��ַ������󲢷�����ָ��
	static inline IString* GetIString(const CComVariant& v)
	{
		if(v.vt==VT_DISPATCH && v.pdispVal)
		{
			CComPtr<IDispatch> dispEx;
			v.pdispVal->QueryInterface(IID_IString,(void**)&dispEx);
			if(!dispEx)
			{
				return NULL;
			}
			else
			{
				return static_cast<IString*>(v.pdispVal);
			}
		}
		return NULL;
	}
	static inline CString ToString(const CComVariant& v)
	{
		IString* pstr=GetIString(v);
		if(pstr)
		{
			return pstr->Ref();
		}
		else
		{
			CComVariant ret=v;
			ret.ChangeType(VT_BSTR);
			return CString(ret.bstrVal);
		}
	}
};

//��̬�Ľ���IDispatch�ĳ�Ա��DISPID����Ϊ�����ڽ��ַ���������һ��IDispatch��������ʱ�ַ���������
//IDispatch*����ʾ��ͨ����̬�Ľ���IDispatch*��DISPID��ӳ�䣬�����ȴ�BSTR��DISPID��ӳ���һЩ��
//��̬������ӳ���ϵ��Ҫ�ڶ����ͷ�ʱ�����Ϊ����Ҫ��IDispatch�����Release����ʵʩHook,����COM��
//�󷽷�ʵʩHook�Ƚϼ򵥣��滻���麯����Ķ�Ӧ����ɡ�
//����Ķ�̬ӳ�����ȫ�־�̬���ݣ��ڶ���̹������ݵ�DLL�п��ܲ����쳣��
class DynamicDispatch
{
	typedef unsigned long (__stdcall*DispatchReleasePtr)(IDispatch*);
	typedef stdext::hash_map<DWORD,DispatchReleasePtr> Vtbl_OldPtr;
	typedef stdext::hash_map<IDispatch*,DISPID> Member_Dispid;
	typedef stdext::hash_map<IDispatch*,Member_Dispid> Obj_Disps;
public:
	static DISPID GetDispID(IDispatch* obj,IDispatch* member)
	{
		//��IObject������IDispatchʵ���У�DISPID����ʵ����صģ�������Ƕ�ÿ��ʵ������ӳ�䣬����ͨ����COM����
		//DISPID��ʵ��������ض���ʵ����أ�����ЩCOM������ԣ������ʵ�ַ�ʽ�е��˷ѿռ䡣
		Obj_Disps& objdisps=ObjDispsRef();
		Obj_Disps::iterator it=objdisps.find(obj);
		//û�иö������ʱ�����ö������
		if(it==objdisps.end())
		{
			//����HOOK,��ΪVTBL�����������Ƕ���ģ�HOOK��VTBL��ַ����ʶ�����ö���ʵ��ָ��
			DWORD pOBJ=(DWORD)obj;
			DWORD pVTBL=*(DWORD*)pOBJ;
			Vtbl_OldPtr& vop=VtblOldPtrRef();
			Vtbl_OldPtr::iterator vit=vop.find(pVTBL);
			if(vit==vop.end())
			{
				MemoryProtect mp((LPVOID)pVTBL);
				DWORD pFUNC=((DWORD*)pVTBL)[2];//Release������IDispatch�麯����ĵ��������
				((DWORD*)pVTBL)[2]=(DWORD)DispatchRelease;
				DispatchReleasePtr ptr=(DispatchReleasePtr)pFUNC;
				VtblOldPtrRef()[pVTBL]=ptr;
			}
			//�������
			Member_Dispid md;
			objdisps[obj]=md;
			it=objdisps.find(obj);
		}
		//����ӳ��ʱ����ȡ��DISPID������������ñ�׼IDispatch�ӿڷ���ȡDISPID��������ӳ��
		Member_Dispid& dispids=it->second;
		Member_Dispid::iterator mit=dispids.find(member);
		if(mit!=dispids.end())
		{
			return mit->second;
		}
		else
		{
			//����ת�ͣ�����û�н����жϣ�ͨ���ӿڲ�ѯ����ȷ����һת���ǰ�ȫ�ġ�
			/*
			CComPtr<IDispatch> dispEx;
			member->QueryInterface(IID_IString,(void**)&dispEx);
			*/
			IString* pmember=static_cast<IString*>(member);
			CComBSTR& bstr=pmember->UnicodeRef();
			CComDispatchDriver disp(obj);
			DISPID dispid=0xffffffff;
			HRESULT hr=disp.GetIDOfName(bstr,&dispid);
			if(SUCCEEDED(hr))
			{
				dispids[member]=dispid;
			}
			return dispid;
		}
	}
	//�Զ�̬��ӵĳ�Ա��ͬһ�����ͬһ��Ա�ڲ�ͬʱ��DISPID���ܷ����仯��
	//��һ���������ڱ仯ǰ�����ǰ��¼��DISPID
	static void Erase(IDispatch* obj,IDispatch* member)
	{
		Obj_Disps& objdisps=ObjDispsRef();
		Obj_Disps::iterator it=objdisps.find(obj);
		if(it!=objdisps.end())
		{
			it->second.erase(member);
		}
	}
	static void ClearAll(void)
	{
		ObjDispsRef().clear();
	}
private:
	static unsigned long __stdcall DispatchRelease(IDispatch* obj)
	{
		DWORD pOBJ=(DWORD)obj;
		DWORD pVTBL=*(DWORD*)pOBJ;
		Vtbl_OldPtr& oldptr=VtblOldPtrRef();
		Vtbl_OldPtr::iterator it=oldptr.find(pVTBL);
		if(it!=oldptr.end())
		{
			DispatchReleasePtr drPtr=it->second;
			unsigned long ct=(*drPtr)(obj);
			if(ct<=0)//���ü���Ϊ0ʱ�����Ѿ��ͷţ���ʱ�����ö����Ӧ��ӳ��
			{
				ObjDispsRef().erase(obj);
			}
			return ct;
		}		
		//������Ӵ˴����ر�Ȼ�����ڴ�й©����������������Ҫ�������ǰ�����ơ�
		throw 0;
	}
private:
	//�����VTBL��IDispatch::Release��ӳ���������������ǵ����ģ�ռ�ÿռ�ֻ�ڽ����˳�ʱ�ͷš�
	static Vtbl_OldPtr& VtblOldPtrRef(void)
	{
		static Vtbl_OldPtr vtbloldptr;
		return vtbloldptr;
	}
	static Obj_Disps& ObjDispsRef(void)
	{
		static Obj_Disps objdisps;
		return objdisps;
	}
};

//����ģ���Ƶ��������ж���Ŀ�ת���Ե�ģ�塣
template <class T, class U>
	class Conversion
{
    typedef char Small;
    struct Big { char dummy[2]; };
    static Small Test(U*);
    static Big Test(...);
    static T* MakeT();
public:
    enum { exists =
        sizeof(Test(MakeT())) == sizeof(Small) };
};
//�Զ���Ӧ�����ģ��̳У����Դ������������������
template<typename VM>
	class ObjectBaseT : public IObject
{	
	VM* vmachine;	
public:
	inline void SetVM(VM* p)
	{
		vmachine=p;
	}
	inline VM* GetVM(void)
	{
		return vmachine;
	}
	inline CComVariant ExecFunction(int index,CComVariant* externArgs,int argnum,CComVariant obj)
	{
		return vmachine->ExecFunction(index,externArgs,argnum,obj);
	}
};
//�����ڽ��ַ�����������ģ��̳У����Դ������������������
template<typename VM>
	class StringBaseT : public IString
{	
	VM* vmachine;	
public:
	inline void SetVM(VM* p)
	{
		vmachine=p;
	}
	inline VM* GetVM(void)
	{
		return vmachine;
	}
	inline CComVariant ExecFunction(int index,CComVariant* externArgs,int argnum,CComVariant obj)
	{
		return vmachine->ExecFunction(index,externArgs,argnum,obj);
	}
};

#define MEMBERID_FIRST		0x70000000
//����ģ����ȡ��������Ϣ��ʵ��IObject��֧��ģ�塣
template<typename T>
	class CDispatchExForScriptBaseT0 : public T
{
	typedef CDispatchExForScriptBaseT0<T> ComObj;	
	unsigned long m_refs;
    ITypeInfo * typeInfo;
protected:	
	typedef std::map<CStringW,DISPID> DispIDMap;
	typedef stdext::hash_map<DISPID,CComVariant> Members;

	DispIDMap dispIDMap;	//��Ա����DISPID�Ķ�Ӧ��ϵ
	Members members;		//��Ա�б�
	DISPID nextDispId;
protected:
	CDispatchExForScriptBaseT0():m_refs(0),typeInfo(NULL),nextDispId(MEMBERID_FIRST)
	{}
	static inline void CreateObject(ITypeInfo* pTypeInfo,ComObj* p)
	{
		p->AddRef();
		pTypeInfo->AddRef();
		p->typeInfo=pTypeInfo;
	}
	inline int StrToInt(const CStringW& ws)
	{
		CString ss((LPCWSTR)ws);
		LPCSTR s=(LPCSTR)ss;
		if(::strlen(s)==1 && s[0]>='0' && s[0]<='9')
		{
			return int(s[0])-int('0');
		}
		int r=::atoi(s);
		if(r)return r;
		return -1;
	}
	inline CStringW IntToStr(int d)
	{
		char buf[256];
		::itoa(d,buf,10);
		return CStringW(buf);
	}
public:
    /* IUnknown methods */
    STDMETHOD(QueryInterface)(REFIID riid, void** ppv)
	{
		if(IsEqualIID(riid, IID_IUnknown)
			|| IsEqualIID(riid, IID_IDispatch)
			|| IsEqualIID(riid, IID_IObject))
		{
			*ppv = this;
			AddRef();
			return NOERROR;
		}
		*ppv = NULL;
		return E_NOINTERFACE;
	}
    STDMETHOD_(unsigned long, AddRef)(void)
	{
		//ATLTRACE("retcount: %d\n",m_refs+1);
		return ++m_refs;
	}
    STDMETHOD_(unsigned long, Release)(void)
	{
		//ATLTRACE("retcount: %d\n",m_refs-1);
		if(--m_refs == 0)
		{
			if(typeInfo != NULL)
			{
				typeInfo->Release();
			}
			delete this;
			return 0;
		}
		return m_refs;
	}

    /* IDispatch methods */
    STDMETHOD(GetTypeInfoCount)(unsigned int * pcTypeInfo)
	{
		*pcTypeInfo=1;
		return NOERROR;
	}
    STDMETHOD(GetTypeInfo)(
      unsigned int iTypeInfo,
      LCID lcid,
      ITypeInfo ** ppTypeInfo)
	{
		if(iTypeInfo != 0)
			return DISP_E_BADINDEX;
		
		typeInfo->AddRef();
		*ppTypeInfo = typeInfo;
		
		return NOERROR;
	}
    STDMETHOD(GetIDsOfNames)(
      REFIID riid,
      OLECHAR ** rgszNames,
      unsigned int cNames,
      LCID lcid,
      DISPID * rgdispid)
	{		
		//��ȡ�Զ���ĳ�Ա��ʵ�ֶ�ϵͳ�ڽ�����ʵ���������滻��
		BSTR name=*rgszNames;					
		int temp=StrToInt(name);//����������ַ��������Ӧ��ֵΪ��DISPIDֵ
		if(temp>=0 && temp<MEMBERID_FIRST)
		{
			*rgdispid=(DISPID)temp;
			return S_OK;
		}
		DispIDMap::iterator it=dispIDMap.find(name);
		if(it!=dispIDMap.end())
		{
			*rgdispid=it->second;
			return S_OK;
		}
		else
		{
			return DispGetIDsOfNames(typeInfo,rgszNames,1,rgdispid);			
		}	
	}
    STDMETHOD(Invoke)(
      DISPID id,
      REFIID riid,
      LCID lcid,
      unsigned short wFlags,
      DISPPARAMS * pdp,
      VARIANT * pvarRes,
      EXCEPINFO * pei,
      unsigned int * pwArgErr)
	{
		HRESULT hr=S_OK;
		//ԭIDispatch�ӿ�֧��
		hr=DispInvoke(this,typeInfo,id,wFlags,pdp,pvarRes,pei,pwArgErr);
		if(hr==DISP_E_MEMBERNOTFOUND)//��̬�����뷽����֧��
		{			
			switch(wFlags)
			{
			case DISPATCH_METHOD:
				{
					Members::iterator it=members.find(id);
					CComVariant& obj=it->second;
					if(obj.vt==VT_I4)
					{
						CComVariant* parg=new CComVariant[pdp->cArgs];
						int ix=0;
						for(int i=pdp->cArgs-1;i>=0;i--)//���������򴫵ݵ�
						{				
							parg[ix++]=pdp->rgvarg[i];
						}
						CComVariant ret=T::ExecFunction(obj.lVal-1,parg,ix,CComVariant((IDispatch*)this));
						ret.Detach(pvarRes);
						hr=S_OK;
						delete[] parg;
					}
					else
					{
						hr=DISP_E_MEMBERNOTFOUND;
					}
				}
				break;
			}
		}
		return hr;
	}
public:
	/*IObject method*/
	virtual void __stdcall insert(VARIANT key,VARIANT val)
	{
		BSTR bstrName=NULL;
		if(key.vt==VT_DISPATCH)
		{
			IString* pstr=IString::GetIString(key);
			if(pstr)
				bstrName=pstr->UnicodeRef();
		}
		else if(key.vt==VT_BSTR)
		{
			bstrName=key.bstrVal;
		}
		if(bstrName)
		{
			DISPID dispid=-1;
			HRESULT hr=GetIDsOfNames(IID_NULL,&bstrName,1,LOCALE_SYSTEM_DEFAULT,&dispid);
			if(!FAILED(hr))
			{
				CComDispatchDriver disp(this);
				disp.PutProperty(dispid,&val);
			}
			if(hr==S_FALSE)
				hr=DISP_E_UNKNOWNNAME;
			if(hr==DISP_E_UNKNOWNNAME)
			{
				dispid=nextDispId;
				members[nextDispId]=val;
				dispIDMap[bstrName]=nextDispId;
				nextDispId++;
			}
		}
	}    
    void __stdcall erase(VARIANT key)
	{	
		BSTR name=NULL;
		if(key.vt==VT_DISPATCH)
		{
			IString* pstr=IString::GetIString(key);
			if(pstr)
				name=pstr->UnicodeRef();
		}
		else if(key.vt==VT_BSTR)
		{
			name=key.bstrVal;
		}
		if(name)
		{
			DispIDMap::iterator it=dispIDMap.find(name);
			if(it!=dispIDMap.end())
			{			
				Members::iterator it2=members.find(it->second);
				if(it2!=members.end())
					members.erase(it2);
				dispIDMap.erase(name);
			}
		}
	}
};
//�ṩ�Զ���ӿڵĲ�ѯ֧��
template<typename T,const IID* piid>
	class CDispatchExForScriptBaseT : public CDispatchExForScriptBaseT0<T>
{
protected:
	CDispatchExForScriptBaseT()
	{}
public:
    /* IUnknown methods */
    STDMETHOD(QueryInterface)(REFIID riid, void** ppv)
	{
		if(IsEqualIID(riid, IID_IUnknown)
			|| IsEqualIID(riid, IID_IDispatch)
			|| IsEqualIID(riid, IID_IObject)
			|| IsEqualIID(riid, *piid))
		{
			*ppv = this;
			AddRef();
			return NOERROR;
		}
		*ppv = NULL;
		return E_NOINTERFACE;
	}
};
//�ֲ��ػ�ʵ������ɲ�ѯ����ӿڵĶ���
template<typename T>
	class CDispatchExForScriptBaseT<T,&IID_NULL> : public CDispatchExForScriptBaseT0<T>
{
};
//һ���ڲ�����ʵ��
template<typename T,const IID* piid,int container=0>
	class CDispatchExForScriptT : public CDispatchExForScriptBaseT<T,piid>
{	
	typedef CDispatchExForScriptBaseT<T,piid> BaseClass;
	typedef std::vector<DISPID> DISPIDS;
	int length;		//��ʶ��ǰ������Ϊ���ĳ�Ա�����ֵ
private:
	CDispatchExForScriptT():length(0)
	{}
public:
	typedef CDispatchExForScriptT<T,piid,container> ComObj;
	static inline ComObj* CreateObject(ITypeInfo* pTypeInfo)
	{
		ComObj* p=new ComObj();
		BaseClass::CreateObject(pTypeInfo,p);
		return p;
	}
public:
    STDMETHOD(Invoke)(
      DISPID id,
      REFIID riid,
      LCID lcid,
      unsigned short wFlags,
      DISPPARAMS * pdp,
      VARIANT * pvarRes,
      EXCEPINFO * pei,
      unsigned int * pwArgErr)
	{
		HRESULT hr=S_OK;
		hr=BaseClass::Invoke(id,riid,lcid,wFlags,pdp,pvarRes,pei,pwArgErr);
		if(hr==DISP_E_MEMBERNOTFOUND)//��̬���Ե�֧��
		{			
			Members::iterator it=members.find(id);
			if(id>=0 && id<MEMBERID_FIRST && it==members.end() && (wFlags==DISPATCH_PROPERTYPUT || wFlags==DISPATCH_PROPERTYPUTREF))
			{
				if(length<=id)length=id+1;
				members[id]=CComVariant();
				it=members.find(id);
			}
			if(it!=members.end())
			{
				CComVariant& obj=it->second;
				switch(wFlags)
				{
				case DISPATCH_PROPERTYGET:
					{
						CComVariant ret=obj;
						ret.Detach(pvarRes);
						hr=S_OK;
					}
					break;
				case DISPATCH_PROPERTYPUT:
					obj=pdp->rgvarg[0];
					hr=S_OK;
					break;
				case DISPATCH_PROPERTYPUTREF:
					obj=pdp->rgvarg[0];
					hr=S_OK;
					break;
				}
			}
		}
		return hr;
	}
public:
	/*IObject method*/
	virtual void __stdcall insert(VARIANT key,VARIANT val)
	{
		if(key.vt==VT_BSTR || key.vt==VT_DISPATCH)
		{
			BaseClass::insert(key,val);
		}
		else if(key.lVal>=0 && key.lVal<MEMBERID_FIRST)
		{
			int v=key.lVal;
			CComDispatchDriver disp(this);
			for(int i=length;i>v;i++)
			{
				CComVariant r;
				disp.GetProperty((DISPID)(i-1),&r);
				disp.PutProperty((DISPID)i,&r);
			}
			disp.PutProperty(key.lVal,&val);
		}
	}    
    virtual void __stdcall erase(VARIANT key)
	{		
		if(key.vt==VT_BSTR || key.vt==VT_DISPATCH)
		{
			BaseClass::erase(key);
		}
		else if(key.lVal>=0 && key.lVal<MEMBERID_FIRST)
		{
			members.erase(key.lVal);
			if(length=key.lVal+1)length--;
		}
	}
	virtual void __stdcall put_length(int size)
	{
		if(size<0 || size>MEMBERID_FIRST)
			return;
		if(size<length)
		{
			DISPIDS disps;
			Members::iterator it=members.begin();
			while(it!=members.end())
			{
				if(it->first>=size)
				{
					disps.push_back(it->first);
				}
				it++;
			}
			DISPIDS::iterator it2=disps.begin();
			while(it2!=disps.end())
			{
				members.erase(*it2);
				it2++;
			}
		}		
		length=size;
	}
	virtual int __stdcall get_length(void)
	{
		return length;
	}
};
//��������ʵ��
template<typename T,const IID* piid>
	class CDispatchExForScriptT<T,piid,1> : public CDispatchExForScriptBaseT<T,piid>
{	
	typedef CDispatchExForScriptBaseT<T,piid> BaseClass;
private:
	CDispatchExForScriptT()
	{}
public:
	typedef CDispatchExForScriptT<T,piid,1> ComObj;
	static inline ComObj* CreateObject(ITypeInfo* pTypeInfo)
	{
		ComObj* p=new ComObj();
		BaseClass::CreateObject(pTypeInfo,p);
		return p;
	}
public:
    STDMETHOD(Invoke)(
      DISPID id,
      REFIID riid,
      LCID lcid,
      unsigned short wFlags,
      DISPPARAMS * pdp,
      VARIANT * pvarRes,
      EXCEPINFO * pei,
      unsigned int * pwArgErr)
	{
		HRESULT hr=S_OK;
		hr=BaseClass::Invoke(id,riid,lcid,wFlags,pdp,pvarRes,pei,pwArgErr);
		if(hr==DISP_E_MEMBERNOTFOUND)//��̬���Ե�֧��
		{			
			Members::iterator it=members.find(id);
			if(it!=members.end())
			{
				CComVariant& obj=it->second;
				switch(wFlags)
				{
				case DISPATCH_PROPERTYGET:
					{
						CComVariant ret=obj;
						ret.Detach(pvarRes);
						hr=S_OK;
					}
					break;
				case DISPATCH_PROPERTYPUT:
					obj=pdp->rgvarg[0];
					hr=S_OK;
					break;
				case DISPATCH_PROPERTYPUTREF:
					obj=pdp->rgvarg[0];
					hr=S_OK;
					break;
				}
			}
			else if(id>=0 && id<MEMBERID_FIRST)//����Ԫ������
			{
				switch(wFlags)
				{
				case DISPATCH_PROPERTYGET:
					{
						*pvarRes=T::get(id);
						hr=S_OK;
					}
					break;
				case DISPATCH_PROPERTYPUT:
					T::set(id,pdp->rgvarg[0]);
					hr=S_OK;
					break;
				case DISPATCH_PROPERTYPUTREF:
					T::set(id,pdp->rgvarg[0]);
					hr=S_OK;
					break;
				}
			}
		}
		return hr;
	}
public:
	/*IObject method*/
	virtual void __stdcall insert(VARIANT key,VARIANT val)
	{
		if(key.vt==VT_BSTR || key.vt==VT_DISPATCH)
		{
			BaseClass::insert(key,val);
		}
		else if(key.lVal>=0 && key.lVal<MEMBERID_FIRST)//����Ԫ������
		{
			T::insert(key,val);
		}
	}    
    virtual void __stdcall erase(VARIANT key)
	{		
		if(key.vt==VT_BSTR || key.vt==VT_DISPATCH)
		{
			BaseClass::erase(key);
		}
		else if(key.lVal>=0 && key.lVal<MEMBERID_FIRST)//����Ԫ������
		{
			T::erase(key);
		}
	}
	virtual void __stdcall put_length(int size)
	{
		if(size<0 || size>MEMBERID_FIRST)
			return;
		T::put_length(size);
	}
	virtual int __stdcall get_length(void)
	{
		return T::get_length();
	}
};
template<typename T,const IID* piid,int container> inline 
	CDispatchExForScriptT<T,piid,container>* CreateDispExForScript(INTERFACEDATA& interfaceData)
{
	static CComPtr<ITypeInfo> typeInfo;
	if(!typeInfo)
	{
		HRESULT hr=::CreateDispTypeInfo(&interfaceData,LOCALE_SYSTEM_DEFAULT,&typeInfo);
		if(FAILED(hr))return NULL;
	}
	CDispatchExForScriptT<T,piid,container>* p=CDispatchExForScriptT<T,piid,container>::CreateObject(typeInfo);
	return p;
}
//����������Ϊ�����ɴ������IDispatchEx�ӿڵľ�̬����
//CreateDispatchEx�����з���˳��Ӧ�����������鷽������
//˳��һ�£��Ҹ��鷽��������__stdcall��������
#define BEGIN_INTF_EX_T(theClass,piid,container) \
	static inline theClass* CreateDispatchEx(void)\
	{\
		typedef theClass implClass;\
		typedef CDispatchExForScriptT<theClass,piid,container> subClass;\
		GenerateDisp gd;\
		static MethodData methodData[]=\
		{\
			gd.PlaceMethod(L"insert",&subClass::insert),\
			gd.PlaceMethod(L"erase",&subClass::erase),\
			gd.PlaceMethod(L"length",&subClass::put_length,DISPATCH_PROPERTYPUT,false),\
			gd.PlaceMethod(L"length",&subClass::get_length,DISPATCH_PROPERTYGET,true),
			
#define END_INTF_EX_T(piid,container) \
				gd.PlaceMethod()\
		};\
		static INTERFACEDATA interfaceData=\
		{\
			methodData,\
			gd.GetMethodNum()\
		};\
		return CreateDispExForScript<implClass,piid,container>(interfaceData);\
	}

#define BEGIN_INTF_EX(theClass) BEGIN_INTF_EX_T(theClass,&IID_NULL,0)
#define BEGIN_INTF_EX2(theClass) BEGIN_INTF_EX_T(theClass,&IID_NULL,1)
#define BEGIN_INTF_EX3(theClass) BEGIN_INTF_EX_T(theClass,&IID_IString,1)
#define END_INTF_EX() END_INTF_EX_T(&IID_NULL,0)
#define END_INTF_EX2() END_INTF_EX_T(&IID_NULL,1)
#define END_INTF_EX3() END_INTF_EX_T(&IID_IString,1)
