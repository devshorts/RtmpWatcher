// This is the main DLL file.

#include "stdafx.h"

#include "RtmpWatcherInterop.h"

using namespace System;

public delegate void DataSentDelegate(RtmpPacket * packet);

void RtmpInterop::RtmpWatcherInterop::Start(int port, System::Action<RtmpPacketInterop^>^ onPacketFound){
	_onPacketFound = onPacketFound;

	socketGrabber = new RawSocketGrabber(port);

	DataSentDelegate^ dg = gcnew DataSentDelegate(this, &RtmpInterop::RtmpWatcherInterop::DataSent);
	delegateHandle = GCHandle::Alloc(dg);

	IntPtr ip = Marshal::GetFunctionPointerForDelegate(dg);
	RtmpPacketFoundFuncPtr cb = static_cast<RtmpPacketFoundFuncPtr>(ip.ToPointer());
	
	socketGrabber->RegisterHandler(cb);

	socketGrabber->Start();
}

void RtmpInterop::RtmpWatcherInterop::Complete(){
	socketGrabber->Complete();
}

void RtmpInterop::RtmpWatcherInterop::DataSent(RtmpPacket * data){
	//printf("Data sent\n");

	RtmpPacketInterop ^ interopPacket = gcnew RtmpPacketInterop(data);

	_onPacketFound(interopPacket);
}


