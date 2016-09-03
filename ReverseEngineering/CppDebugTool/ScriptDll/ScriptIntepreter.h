#pragma once
using namespace System;
using namespace System::Collections::Generic;
using namespace System::CodeDom::Compiler;
using namespace System::IO;

ref class ScriptInterpreter
{
public:
	ScriptInterpreter(void)
	{
		referencedAssemblies = gcnew List<String^>();
		compilerOptions = nullptr;
		compilerResults = gcnew List<String^>();
	}
    /// <summary>
    /// ����һ��ģ���ļ�������һ������һ��װ����һ�����scriptԴ����,ģ���ļ��б����ж������滻����:#ASSEMBLYNAME#,#CLASSNAME#,�ֱ��Ӧװ������������
    /// </summary>
    static String^ ReadTemplate(String^ templateFile, String^ assemblyName, String^ className)
    {
        try
        {
            StreamReader^ sr = gcnew StreamReader(templateFile);
            String^ templateCode = sr->ReadToEnd();
            sr->Close();
            return templateCode->Replace("#ASSEMBLYNAME#", assemblyName)->Replace("#CLASSNAME#", className);
        }
        catch(...)
        {
            return "";
        }
    }
    property List<String^>^ ReferencedAssemblies
    {
        List<String^>^ get(void)
        {
            return referencedAssemblies;
        }
    }
    property String^ CompilerOptions
    {
        String^ get(void)
        {
            return compilerOptions;
        }
        void set(String^ value)
        {
            compilerOptions = value;
        }
    }
    property List<String^>^ Results
    {
        List<String^>^ get(void)
        {
            return compilerResults;
        }
    }
	generic<typename T>
		where T : CodeDomProvider,gcnew()
	Object^ LoadCode(String^ scriptCode, String^ assemblyName, String^ className)
    {
        try
        {
            T cdp = gcnew T();
            CompilerParameters^ args = gcnew CompilerParameters();
            args->GenerateExecutable = false;
            args->GenerateInMemory = true;
            for each(String^ s in this->ReferencedAssemblies)
            {
                args->ReferencedAssemblies->Add(s);
            }
            args->CompilerOptions = CompilerOptions;
            CompilerResults^ res = cdp->CompileAssemblyFromSource(args, scriptCode);
            Results->Clear();
            for each(String^s in res->Output)
            {
                Results->Add(s);
            }
            if (res->Output->Count > 0)
                return nullptr;
            return res->CompiledAssembly->CreateInstance(assemblyName + "." + className);
        }
        catch(Exception^ eee)
        {
            Results->Add(eee->Message);
            return nullptr;
        }
    }
	generic<typename T>
		where T : CodeDomProvider,gcnew()
	Object^ LoadFile(String^ file, String^ assemblyName, String^ className)
    {
        String^ content = ReadTemplate(file, assemblyName, className);
        if (content->Length <= 0)
            return nullptr;
        return LoadCode<T>(content, assemblyName, className);
    }
private:
	List<String^>^ referencedAssemblies;
    String^ compilerOptions;
    List<String^>^ compilerResults;
};
	
ref class Options
{
public:
	[Category("C#��������")]
	[DisplayName("����ѡ��")]
	[Description("C#����ѡ��μ�MSDN������")]
	property String^ Option
	{
		String^ get(void)
		{
			return option;
		}
		void set(String^ value)
		{
			option=value;
		}
	}
	[Category("C#��������")]
	[DisplayName("����ģ��")]
	[Description("ϵͳDLL����Ҫָ��·��������DLL��Ҫָ��ȫ·��")]
	property array<String^>^ References
	{
		array<String^>^ get(void)
		{
			return references;
		}
		void set(array<String^>^ value)
		{
			references=value;
		}
	}
public:
	Options()
	{
		option="/unsafe /optimize";
		references=gcnew array<String^>{"System.dll","System.Data.Dll","System.Drawing.dll","System.Web.dll","System.Xml.dll","System.Windows.Forms.dll"};
	}
private:
	String^ option;
	array<String^>^ references;
};
