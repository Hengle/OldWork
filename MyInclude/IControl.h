#pragma once

class IControl
{
public:
	virtual HWND __stdcall GetHwnd(void)=0;
	virtual UINT __stdcall GetID(void)=0;
	//��ȡ�ؼ�ռλHTMLԪ��ID�����ؼ�ʹ�ø�Ԫ�صĿռ�
	virtual BSTR __stdcall GetPlaceHolderID(void)=0;
	virtual LRESULT __stdcall OnNotify(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)=0;
};