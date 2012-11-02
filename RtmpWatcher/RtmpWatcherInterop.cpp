// This is the main DLL file.

#include "stdafx.h"

#include "RtmpWatcherInterop.h"

#include "stdafx.h"
#include <iostream>
#include <string>

using namespace std;
using namespace System;
using System::Runtime::InteropServices::Marshal;

public delegate void DataSentDelegate(RtmpPacket * packet);

void RtmpInterop::RtmpWatcherInterop::ConvertManagedStringToStdString(std::string &outStr, System::String ^str) 
{
	IntPtr ansiStr = System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(str); 
	outStr = (const char*)ansiStr.ToPointer(); System::Runtime::InteropServices::Marshal::FreeHGlobal(ansiStr); 
}

void RtmpInterop::RtmpWatcherInterop::Start(System::String ^ nicDescription, int port, System::Action<RtmpPacketInterop^>^ onPacketFound){
	_onPacketFound = onPacketFound;

	string nativeNicString; 

	ConvertManagedStringToStdString(nativeNicString, nicDescription);

	socketGrabber = new RawSocketGrabber(nativeNicString, port);

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


