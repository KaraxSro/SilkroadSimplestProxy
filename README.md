# SilkroadSimplestProxy

Creadits goes to Drew Benton (a.k.a. pushedx)

- I refactored the code from [this thread](https://www.elitepvpers.com/forum/sro-coding-corner/1193543-c-simplest-proxy.html) to make it easier to understand
- Made it restart on lost connection, so it is possible to use without a loader
- Also the AgentServer connection does not use a different process
- You can download the latest release, no need to mess with building

## Usage

Edit the **StartProxy.bat** file in the folder, usage:

```
SilkroadSimplestProxy.exe <local address> <local port> <remote address> <remote port>
```

Example to use with official servers

```
SilkroadSimplestProxy.exe 127.0.0.1 16000 gwgt1.joymax.com 15779
```

Make sure to edit the IP inside your client to point to your **local address**, or use a **loader**.

## v1.01
- Added logging
- Added option to show Name of the packets, and also to hide them, see appsettings.json
- Parsed the packet names from https://github.com/DummkopfOfHachtenduden/SilkroadDoc
- 0x2002 (CLIENT_GLOBAL_MODULE_KEEP_ALIVE) **is hidden now**
