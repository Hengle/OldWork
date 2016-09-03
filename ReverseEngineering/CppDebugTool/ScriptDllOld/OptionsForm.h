#pragma once

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;


namespace ScriptDllOld
{
	/// <summary> 
	/// OptionsForm ժҪ
	///
	/// ����: ��������ĸ�������ƣ�����Ҫ���� 
	///          ����������������� .resx �ļ��������й���Դ���������ߵ� 
	///          ����Դ�ļ��������ԡ�  ����
	///          �������������˴��������
	///          ���ػ���Դ��ȷ������
	/// </summary>
	public __gc class OptionsForm : public System::Windows::Forms::Form
	{
	public: 
		OptionsForm(void)
		{
			InitializeComponent();
		}
        __property Object* get_OptionObject(void)
		{
			return optionsGrid->SelectedObject;
		}
		__property void set_OptionObject(Object* obj)
		{
			optionsGrid->SelectedObject=obj;
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
	private: 
		System::Windows::Forms::PropertyGrid *  optionsGrid;
	private:
		/// <summary>
		/// ����������������
		/// </summary>
		System::ComponentModel::Container* components;

		/// <summary>
		/// �����֧������ķ��� - ��Ҫʹ�ô���༭���޸�
		/// �˷��������ݡ�
		/// </summary>
		void InitializeComponent(void)
		{
			this->optionsGrid = new System::Windows::Forms::PropertyGrid();
			this->SuspendLayout();
			// 
			// optionsGrid
			// 
			this->optionsGrid->CommandsVisibleIfAvailable = true;
			this->optionsGrid->Dock = System::Windows::Forms::DockStyle::Fill;
			this->optionsGrid->LargeButtons = false;
			this->optionsGrid->LineColor = System::Drawing::SystemColors::ScrollBar;
			this->optionsGrid->Location = System::Drawing::Point(0, 0);
			this->optionsGrid->Name = S"optionsGrid";
			this->optionsGrid->Size = System::Drawing::Size(292, 262);
			this->optionsGrid->TabIndex = 0;
			this->optionsGrid->Text = S"propertyGrid1";
			this->optionsGrid->ViewBackColor = System::Drawing::SystemColors::Window;
			this->optionsGrid->ViewForeColor = System::Drawing::SystemColors::WindowText;
			// 
			// OptionsForm
			// 
			this->AutoScaleBaseSize = System::Drawing::Size(6, 14);
			this->ClientSize = System::Drawing::Size(292, 262);
			this->Controls->Add(this->optionsGrid);
			this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::SizableToolWindow;
			this->Name = S"OptionsForm";
			this->ShowInTaskbar = false;
			this->Text = S"OptionsForm";
			this->Closing += new System::ComponentModel::CancelEventHandler(this, OptionsForm_Closing);
			this->ResumeLayout(false);

		}		

	private: 
		System::Void OptionsForm_Closing(System::Object *  sender, System::ComponentModel::CancelEventArgs *  e)
		{
            e->Cancel = true;
            Hide();
		}
	};
}
