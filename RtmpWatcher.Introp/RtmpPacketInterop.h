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
			int GetLength();
			String ^ GetSourceIP();
			String ^ GetDestIP();
			array<unsigned char>^ GetBytes();

		private:
			array<unsigned char>^ _bytes;
			int _length;
			String ^ _sourceIP;
			String ^ _destIp;
	};
}
