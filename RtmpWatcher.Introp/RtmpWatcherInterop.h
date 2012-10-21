// RtmpWatcher.Introp.h

#pragma once

#include "RawSocketGrabber.h"

using namespace System;

namespace RtmpInterop {

	public ref class RtmpWatcherInterop
	{
		public:
			void Start(int port);

		private:
			RawSocketGrabber* socketGrabber;
	};
}
