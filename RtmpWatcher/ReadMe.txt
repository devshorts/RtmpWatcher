---
Basic Gist
---

RawSocketGrabber.cpp basically it takes a packet and sends it to the "orderer". The orderer (PacketOrderer.cpp) is responsible for organizing packets by destination ports. The aggregator (RtmpPacketAggregator.cpp) pieces together tcp packets and parsers raw rtmp packets out of. Every time you capture a packet in the grabber, the grabber then asks to see if there are any packets that are ready (via the orderer to the aggregator). When a packet is ready an RtmpPacket object. Once we have a packet, it gets marshalled through managed via a function pointer to an interop class (RtmpWatcherInterop.cpp) that invokes an Action<RtmpPacketInterop> delegate into C#. 

http://tech.blinemedical.com/dropped-packets-with-promiscuous-raw-sockets/