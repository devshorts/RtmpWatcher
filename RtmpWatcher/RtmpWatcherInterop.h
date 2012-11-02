// RtmpWatcher.Introp.h

#pragma once

#include "RtmpPacketInterop.h"
#include "RawSocketGrabber.h"
#include <string>

using namespace System;
using namespace System::Runtime::InteropServices;

namespace RtmpInterop {

	public ref class RtmpWatcherInterop
	{
		public:
			!RtmpWatcherInterop() { delete socketGrabber; };
			void Start(System::String ^ nicDescription, int port,  System::Action<RtmpPacketInterop^>^ onPacketFound);
			void Complete();

		private:
			System::Action<RtmpPacketInterop^>^ _onPacketFound;
			RawSocketGrabber* socketGrabber;
			void DataSent(RtmpPacket *);
			GCHandle delegateHandle;
			void ConvertManagedStringToStdString(std::string &outStr, System::String ^str);
	};
}
