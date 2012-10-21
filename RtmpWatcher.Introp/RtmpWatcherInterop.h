// RtmpWatcher.Introp.h

#pragma once

#include "RawSocketGrabber.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace RtmpInterop {

	public ref class RtmpWatcherInterop
	{
		public:
			!RtmpWatcherInterop() { delete socketGrabber; };
			void Start(int port);
			void Complete();

		private:
			RawSocketGrabber* socketGrabber;
			void DataSent(RtmpPacket *);
			GCHandle delegateHandle;
	};
}
