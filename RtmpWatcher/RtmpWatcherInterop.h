// RtmpWatcher.Introp.h

#pragma once

#include "RtmpPacketInterop.h"
#include "RawSocketGrabber.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace RtmpInterop {

	public ref class RtmpWatcherInterop
	{
		public:
			!RtmpWatcherInterop() { delete socketGrabber; };
			void Start(int deviceIndex, int port,  System::Action<RtmpPacketInterop^>^ onPacketFound);
			void Complete();

		private:
			System::Action<RtmpPacketInterop^>^ _onPacketFound;
			RawSocketGrabber* socketGrabber;
			void DataSent(RtmpPacket *);
			GCHandle delegateHandle;
	};
}
