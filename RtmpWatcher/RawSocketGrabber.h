#include <functional>
#include <vector>
#include "SocketData.h"

using namespace std;

class RawSocketGrabber{
public:
	RawSocketGrabber(int targetPort);
	void operator()();
	void RegisterHandler(std::function<void (SocketData *)> handler);

private:
	void Start();
	std::function<void (SocketData *)> _packetFoundHandler;
	

	void GetMachineIP(char * ip);
	void TransalteIP(unsigned int _ip, char *_cip);
	void DecodeTcp(char *_packet);

	int _targetPort;	
	SOCKET socket;
};