
#include <engextcpp.hpp>

using namespace System::Management::Automation;

ref class PowerShellHostRawUI : Host::PSHostRawUserInterface{

public:
	property System::ConsoleColor BackgroundColor {
		System::ConsoleColor get() override{
			return System::ConsoleColor::White;
		}
		void set(System::ConsoleColor) override {			
		}
	}

	property System::ConsoleColor ForegroundColor {
		System::ConsoleColor get() override{
			return System::ConsoleColor::Black;
		}
		void set(System::ConsoleColor) override {
			
		}
	}


	property Host::Size BufferSize{
		Host::Size get() override{
			return Host::Size(g_ExtInstancePtr->m_OutputWidth, 1);
		}
		void set(Host::Size) override {			
		}
	}


	property int CursorSize{
		int get() override{
			return 1;
		}
		void set(int) override {			
		}
	}


	property bool KeyAvailable{
		bool get() override{
			return false;
		}
	}

	property Host::Coordinates CursorPosition  {
		Host::Coordinates get() override{
			return Host::Coordinates(1, 10);
		}
		void set(Host::Coordinates) override {
			
		}
	}
	property Host::Coordinates WindowPosition  {
		Host::Coordinates get() override{
			return Host::Coordinates(0, 0);
		}
		void set(Host::Coordinates) override {
			
		}
	}

	property Host::Size WindowSize{
		Host::Size get() override{
			return Host::Size(80, 50);
		}
		void set(Host::Size) override {			
		}
	}

	property System::String^ WindowTitle{
		System::String^ get() override{
			return "PSDbgExtension";
		}
		void set(System::String^) override {
			
		}
	}

	property Host::Size MaxWindowSize{
		Host::Size get() override{
			return Host::Size(System::Console::LargestWindowWidth, System::Console::LargestWindowHeight);
		}
	}

	property Host::Size MaxPhysicalWindowSize{
		Host::Size get() override{
			return Host::Size(System::Console::LargestWindowWidth, System::Console::LargestWindowHeight);
		}
	}

	Host::KeyInfo ReadKey(Host::ReadKeyOptions) override {
		throw gcnew  PSNotImplementedException();
	}

	void FlushInputBuffer(void) override {		
	}

	void SetBufferContents(Host::Coordinates, cli::array<Host::BufferCell, 2> ^) override {
		throw gcnew  PSNotImplementedException();
	}
	void SetBufferContents(Host::Rectangle,Host::BufferCell) override {
		throw gcnew  PSNotImplementedException();
	}
	cli::array<Host::BufferCell,2> ^ GetBufferContents(Host::Rectangle) override {
		throw gcnew  PSNotImplementedException();
	}
	void ScrollBufferContents(Host::Rectangle,Host::Coordinates,Host::Rectangle,Host::BufferCell) override {
		throw gcnew  PSNotImplementedException();
	}

};