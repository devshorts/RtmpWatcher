// RtmpWatcher.Introp.h

#pragma once

#include "RtmpPacket.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace RtmpInterop {

	public ref class RtmpPacketInterop
	{
		public:
			RtmpPacketInterop(RtmpPacket * packet);

		private:
			array<unsigned char>^ bytes;
			int length;
			String ^ sourceIP;
			String ^ destIp;
	};
}
