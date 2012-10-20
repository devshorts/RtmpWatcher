#include <functional>
#include <vector>
#include "SocketData.h"

class RawSocketGrabber{
public:

	enum TcpPacketType{
		FIN,
		SYN,
		RST,
		PSH,
		ACK,
		URG
	};

	RawSocketGrabber(int targetPort);
	~RawSocketGrabber();
	void operator()();
	void RegisterHandler(std::function<void (SocketData *)> handler);
	void Complete();

private:
	void Start();
	void CleanupSocket();

	void GetMachineIP(char * ip);
	void TransalteIP(unsigned int _ip, char *_cip);
	void DecodeTcp(char *_packet);
	TcpPacketType DeterminePacketType(unsigned short flags);

	void InitSocket();
	void BindSocketToIp();
	void CreatePromisciousSocket();

	void ReadOffSocket();

	std::function<void (SocketData *)> _packetFoundHandler;
	int _targetPort;	
	SOCKET socketPtr;
	sockaddr_in socketDefinition;

	bool isRunning;
};