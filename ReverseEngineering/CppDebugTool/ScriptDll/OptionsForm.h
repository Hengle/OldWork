#pragma once

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;

namespace ScriptDll {

	/// <summary>
	/// OptionsForm ժҪ
	///
	/// ����: ������Ĵ�������ƣ�����Ҫ����
	///          ����������������� .resx �ļ��������й���Դ���������ߵ�
	///          ����Դ�ļ��������ԡ�����
	///          �������������˴���Ĺ���
	///          ���ػ���Դ��ȷ������
	/// </summary>
	public ref class OptionsForm : public System::Windows::Forms::Form
	{
	public:
		OptionsForm(void)
		{
			InitializeComponent();
			//
			//TODO: �ڴ˴���ӹ��캯������
			//
		}
	public:
		property Object^ OptionObject
		{
			Object^ get(void)
			{
				return optionsGrid->SelectedObject;
			}
			void set(Object^ obj)
			{
				optionsGrid->SelectedObject=obj;
			}
		}
	protected:
		/// <summary>
		/// ������������ʹ�õ���Դ��
		/// </summary>
		~OptionsForm()
		{
			if (components)
			{
				delete components;
			}
		}
	private: 
		System::Windows::Forms::PropertyGrid^  optionsGrid;
	private:
		/// <summary>
		/// ����������������
		/// </summary>
		System::ComponentModel::Container ^components;

#pragma region Windows Form Designer generated code
		/// <summary>
		/// �����֧������ķ��� - ��Ҫ
		/// ʹ�ô���༭���޸Ĵ˷��������ݡ�
		/// </summary>
		void InitializeComponent(void)
		{
			this->optionsGrid = (gcnew System::Windows::Forms::PropertyGrid());
			this->SuspendLayout();
			// 
			// optionsGrid
			// 
			this->optionsGrid->Dock = System::Windows::Forms::DockStyle::Fill;
			this->optionsGrid->Location = System::Drawing::Point(0, 0);
			this->optionsGrid->Name = L"optionsGrid";
			this->optionsGrid->Size = System::Drawing::Size(292, 262);
			this->optionsGrid->TabIndex = 0;
			// 
			// OptionsForm
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(6, 12);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(292, 262);
			this->Controls->Add(this->optionsGrid);
			this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::SizableToolWindow;
			this->Name = L"OptionsForm";
			this->ShowIcon = false;
			this->ShowInTaskbar = false;
			this->Text = L"OptionsForm";
			this->FormClosing += gcnew System::Windows::Forms::FormClosingEventHandler(this, &OptionsForm::OptionsForm_FormClosing);
			this->ResumeLayout(false);

		}
#pragma endregion
	private: 
		System::Void OptionsForm_FormClosing(System::Object^  sender, System::Windows::Forms::FormClosingEventArgs^  e) 
		{
			if (e->CloseReason == System::Windows::Forms::CloseReason::UserClosing)
            {
                e->Cancel = true;
                Hide();
            }
		}
	};
}
