#pragma once

#include "windows.h"

class SocketData{
public:
	SocketData(void * data, int length);
	~SocketData();
	void * GetData();
	int GetLength();

private:
	void * _data;
	int _length;
};