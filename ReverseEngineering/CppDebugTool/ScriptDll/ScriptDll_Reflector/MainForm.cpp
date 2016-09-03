#include "StdAfx.h"

using namespace System;
using namespace System::IO;
using namespace System::Reflection;
using namespace System::Windows::Forms;

void ShowMainForm(void)
{
	String^ filePath=nullptr;
	array<Assembly^>^ assems=AppDomain::CurrentDomain->GetAssemblies();
	for each(Assembly^ a in assems)
	{
		if(a->FullName->ToLower()->IndexOf("injectforreflector")>=0)
			filePath=Path::GetDirectoryName(a->Location);
	}
	if(filePath==nullptr)
	{
		::MessageBox(NULL,L"�Ҳ���InjectForReflectorģ�飬�벻Ҫ�ı䱻ע��DLL�����֣�",L"����",MB_OK);
		return;
	}
	Assembly^ assem = Assembly::LoadFrom(Path::Combine(filePath,"injectReflector.dll"));
	Object^ obj=assem->CreateInstance("injectReflector.injectForm");
	Form^ form=dynamic_cast<Form^>(obj);
	form->Show();
	Application::Run();
}
