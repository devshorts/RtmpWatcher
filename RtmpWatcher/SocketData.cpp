#include "SocketData.h"
#include <windows.h>

SocketData::SocketData(void * data, int length){
	_data = data;
	_length = length;
}

void * SocketData::GetData(){
	return _data;
}

int SocketData::GetLength(){
	return _length;
}

SocketData::~SocketData(){
	delete _data;
	_data = NULL;
}