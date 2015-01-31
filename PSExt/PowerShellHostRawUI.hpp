#include "PowerShellHostRawUI.hpp"

ref class PowerShellHostUI : System::Management::Automation::Host::PSHostUserInterface{

public:
	property System::Management::Automation::Host::PSHostRawUserInterface^ RawUI{
		System::Management::Automation::Host::PSHostRawUserInterface^ get() override{

		}
	}
};