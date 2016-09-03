#pragma once

#pragma push_macro("GetObject")
#undef GetObject

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;
using namespace Microsoft::CSharp;

#include "ScriptInterpreter.h"
#include "OptionsForm.h"

namespace ScriptDllOld
{
	/// <summary> 
	/// MainForm ժҪ
	///
	/// ����: ��������ĸ�������ƣ�����Ҫ���� 
	///          ����������������� .resx �ļ��������й���Դ���������ߵ� 
	///          ����Դ�ļ��������ԡ�  ����
	///          �������������˴��������
	///          ���ػ���Դ��ȷ������
	/// </summary>
	public __gc class MainForm : public System::Windows::Forms::Form
	{
		Options* options;
		OptionsForm* optionsForm;
	public: 
		MainForm(void)
		{
			InitializeComponent();
			options=new Options();
			optionsForm=new OptionsForm();

			scriptCtrl->Encoding = System::Text::Encoding::Default;
			resultCtrl->Encoding = System::Text::Encoding::Default;
			scriptCtrl->Document->HighlightingStrategy = ICSharpCode::TextEditor::Document::HighlightingStrategyFactory::CreateHighlightingStrategy("C#");
		}
        
	protected: 
		void Dispose(Boolean disposing)
		{
			if (disposing && components)
			{
				components->Dispose();
			}
			__super::Dispose(disposing);
		}
	private: System::Windows::Forms::ToolBar *  toolBar1;
	private: ICSharpCode::TextEditor::TextEditorControl *  scriptCtrl;
	private: ICSharpCode::TextEditor::TextEditorControl *  resultCtrl;
	private: System::Windows::Forms::ToolBarButton *  loadBtn;
	private: System::Windows::Forms::ToolBarButton *  saveBtn;
	private: System::Windows::Forms::ToolBarButton *  runBtn;
	private: System::Windows::Forms::ToolBarButton *  optionBtn;
	private: System::Windows::Forms::ImageList *  toolbarImageList;
	private: System::ComponentModel::IContainer *  components;
	private: System::Windows::Forms::ToolBarButton *  toolBarButton1;
	private:
		/// <summary>
		/// ����������������
		/// </summary>


		/// <summary>
		/// �����֧������ķ��� - ��Ҫʹ�ô���༭���޸�
		/// �˷��������ݡ�
		/// </summary>
		void InitializeComponent(void)
		{
			this->components = new System::ComponentModel::Container();
			System::Resources::ResourceManager *  resources = new System::Resources::ResourceManager(__typeof(ScriptDllOld::MainForm));
			this->toolBar1 = new System::Windows::Forms::ToolBar();
			this->loadBtn = new System::Windows::Forms::ToolBarButton();
			this->saveBtn = new System::Windows::Forms::ToolBarButton();
			this->runBtn = new System::Windows::Forms::ToolBarButton();
			this->toolBarButton1 = new System::Windows::Forms::ToolBarButton();
			this->optionBtn = new System::Windows::Forms::ToolBarButton();
			this->toolbarImageList = new System::Windows::Forms::ImageList(this->components);
			this->scriptCtrl = new ICSharpCode::TextEditor::TextEditorControl();
			this->resultCtrl = new ICSharpCode::TextEditor::TextEditorControl();
			this->SuspendLayout();
			// 
			// toolBar1
			// 
			System::Windows::Forms::ToolBarButton* __mcTemp__1[] = new System::Windows::Forms::ToolBarButton*[5];
			__mcTemp__1[0] = this->loadBtn;
			__mcTemp__1[1] = this->saveBtn;
			__mcTemp__1[2] = this->runBtn;
			__mcTemp__1[3] = this->toolBarButton1;
			__mcTemp__1[4] = this->optionBtn;
			this->toolBar1->Buttons->AddRange(__mcTemp__1);
			this->toolBar1->ButtonSize = System::Drawing::Size(31, 35);
			this->toolBar1->Divider = false;
			this->toolBar1->DropDownArrows = true;
			this->toolBar1->ImageList = this->toolbarImageList;
			this->toolBar1->Location = System::Drawing::Point(0, 0);
			this->toolBar1->Name = S"toolBar1";
			this->toolBar1->ShowToolTips = true;
			this->toolBar1->Size = System::Drawing::Size(824, 39);
			this->toolBar1->TabIndex = 0;
			this->toolBar1->ButtonClick += new System::Windows::Forms::ToolBarButtonClickEventHandler(this, toolBar1_ButtonClick);
			// 
			// loadBtn
			// 
			this->loadBtn->Text = S"װ��";
			// 
			// saveBtn
			// 
			this->saveBtn->Text = S"����";
			// 
			// runBtn
			// 
			this->runBtn->Text = S"����";
			// 
			// toolBarButton1
			// 
			this->toolBarButton1->Style = System::Windows::Forms::ToolBarButtonStyle::Separator;
			// 
			// optionBtn
			// 
			this->optionBtn->Text = S"ѡ��";
			// 
			// toolbarImageList
			// 
			this->toolbarImageList->ImageSize = System::Drawing::Size(16, 16);
			this->toolbarImageList->TransparentColor = System::Drawing::Color::Transparent;
			// 
			// scriptCtrl
			// 
			this->scriptCtrl->Dock = System::Windows::Forms::DockStyle::Top;
			this->scriptCtrl->Location = System::Drawing::Point(0, 39);
			this->scriptCtrl->Name = S"scriptCtrl";
			this->scriptCtrl->ShowEOLMarkers = true;
			this->scriptCtrl->ShowSpaces = true;
			this->scriptCtrl->ShowTabs = true;
			this->scriptCtrl->ShowVRuler = true;
			this->scriptCtrl->Size = System::Drawing::Size(824, 382);
			this->scriptCtrl->TabIndex = 2;
			// 
			// resultCtrl
			// 
			this->resultCtrl->Dock = System::Windows::Forms::DockStyle::Fill;
			this->resultCtrl->Location = System::Drawing::Point(0, 421);
			this->resultCtrl->Name = S"resultCtrl";
			this->resultCtrl->ShowEOLMarkers = true;
			this->resultCtrl->ShowHRuler = true;
			this->resultCtrl->ShowInvalidLines = false;
			this->resultCtrl->ShowSpaces = true;
			this->resultCtrl->ShowTabs = true;
			this->resultCtrl->ShowVRuler = true;
			this->resultCtrl->Size = System::Drawing::Size(824, 165);
			this->resultCtrl->TabIndex = 3;
			// 
			// MainForm
			// 
			this->AutoScaleBaseSize = System::Drawing::Size(6, 14);
			this->ClientSize = System::Drawing::Size(824, 586);
			this->Controls->Add(this->resultCtrl);
			this->Controls->Add(this->scriptCtrl);
			this->Controls->Add(this->toolBar1);
			this->Name = S"MainForm";
			this->Text = S"MainForm";
			this->ResumeLayout(false);

		}		
	private: 
		System::Void toolBar1_ButtonClick(System::Object *  sender, System::Windows::Forms::ToolBarButtonClickEventArgs *  e)
		{
			if(e->Button==loadBtn)
			{
				OpenFileDialog* ofd = new OpenFileDialog();
				ofd->DefaultExt = "cs";
				ofd->Filter = "CSharp�ļ�|*.cs|�����ļ�|*.*";
				ofd->CheckPathExists = true;
				ofd->CheckFileExists = true;
				ofd->Title = "��ָ��Ҫ�򿪵Ľű��ļ�/ģ���ļ���һ��Ҫ��ScriptNS���ռ估Script�࣬������һ������public void Run(Control);��";
				if (System::Windows::Forms::DialogResult::OK == ofd->ShowDialog())
				{
					scriptCtrl->Text = OpenScript(ofd->FileName);
				}			
			}
			else if(e->Button==saveBtn)
			{
				SaveFileDialog* sfd = new SaveFileDialog();
				sfd->DefaultExt = "cs";
				sfd->Filter = "CSharp�ļ�|*.cs|�����ļ�|*.*";			
				sfd->CheckPathExists = true;
				sfd->OverwritePrompt = true;
				sfd->Title = "��ָ��Ҫ����Ľű��ļ�";
				if (System::Windows::Forms::DialogResult::OK == sfd->ShowDialog())
				{
					SaveScript(sfd->FileName, scriptCtrl->Text);
				}
			}
			else if(e->Button==runBtn)
			{
				try
				{
					resultCtrl->Text = "";
					ScriptInterpreter* si = new ScriptInterpreter();
					for(int i=0;i<options->References->Count;i++)
					{
						String* s=options->References[i];
						si->ReferencedAssemblies->Add(s);
					}
					si->CompilerOptions=options->Option;
					Object* scriptCode = si->LoadCode(new CSharpCodeProvider(),scriptCtrl->Text, "ScriptNS", "Script");
					if (scriptCode != NULL)
					{
						Type* t=scriptCode->GetType();
						Object* objs[]={resultCtrl};
						t->InvokeMember("Run",System::Reflection::BindingFlags::InvokeMethod,NULL,scriptCode,objs);
					}
					else
					{
						String* ss = "�������";
						for(int i=0;i<si->Results->Count;i++)
						{
							String* s=si->Results->Item[i];
							ss = String::Concat(ss , "\r\n" , s);
						}
						resultCtrl->Text = ss;
					}
				}
				catch (Exception* eee)
				{
					resultCtrl->Text = String::Concat("�����쳣:" , eee->Message);
				}
				__finally
				{
					resultCtrl->Refresh();
				}
			}
			else if(e->Button==optionBtn)
			{
				optionsForm->OptionObject=options;
				if (!optionsForm->Visible)
				{
					optionsForm->Owner = this;
					optionsForm->Show();
					optionsForm->Location = Point(Left + (Width - optionsForm->Width) / 2, Top + (Height - optionsForm->Height) / 2);
				}
				else if (optionsForm->WindowState == System::Windows::Forms::FormWindowState::Minimized)
				{   
					optionsForm->WindowState = System::Windows::Forms::FormWindowState::Normal;
				}   
			}
		}
	private:		
        String* OpenScript(String* file)
        {
            try
            {
                StreamReader* sr = new StreamReader(file);
                String* code = sr->ReadToEnd();
                sr->Close();
                return code;
            }
            catch(Exception*)
            {
                return "";
            }
        }
        void SaveScript(String* file,String* code)
        {
            StreamWriter* sw=new StreamWriter(file);
            sw->Write(code);
            sw->Close();
        }	
	};
}
#pragma pop_macro("GetObject")

extern void ShowMainForm(void);
