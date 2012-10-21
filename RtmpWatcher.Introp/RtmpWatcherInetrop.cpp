// This is the main DLL file.

#include "stdafx.h"

#include "RtmpWatcherInterop.h"

void RtmpInterop::RtmpWatcherInterop::Start(int port){
	socketGrabber = new RawSocketGrabber(port);
	socketGrabber->Start();
	/*socketGrabber.RegisterHandler([&] (SocketData * data) { 

		Console.WriteLine("Got data!");

		delete data;
	});*/
}