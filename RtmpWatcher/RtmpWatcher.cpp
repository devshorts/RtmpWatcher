// RtmpWatcher.cpp : Defines the entry point for the console application.
//

#include <iostream>

#include "RawSocketGrabber.h"

#include <boost/thread/thread.hpp>

using namespace std;


int main(int argc, char* argv[])
{
	RawSocketGrabber socketGrabber(8935);

	socketGrabber.RegisterHandler([&] (SocketData * data) { 

		cout << "Got data " << data->GetLength() << endl;

		delete data;
	});

	boost::thread socketThread = boost::thread(boost::ref(socketGrabber));

	socketThread.join();

	cout << "done" << endl;

	return 0;
}

