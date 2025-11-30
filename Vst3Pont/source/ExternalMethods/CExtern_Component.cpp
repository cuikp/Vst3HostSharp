#include "CExternMethods.h"

using namespace Steinberg;

extern "C" {
		

	EXPORTED_FUNCTION int ActivateBus(int pluginId, int32 mediaType, int32 busDir, int32 index, uint8 state)
	{
		IComponent* thisComp = GetComponent(pluginId);
		int returnInt = thisComp->activateBus(mediaType, busDir, index, state);
		return returnInt;
	}

	EXPORTED_FUNCTION int GetBusInfo(int pluginId, MediaType mediaType, BusDirection busdirection, int index, BusInfo& businfo) {

		IComponent* thisComp = GetComponent(pluginId);
		if (thisComp) {
			int processResult = thisComp->getBusInfo(mediaType, busdirection, index, businfo);
			return 0;
		}
		return -1;

	}

	EXPORTED_FUNCTION int GetBusCount(int pluginId, MediaType mediaType, BusDirection busdirection) {

		IComponent* thisComp = GetComponent(pluginId);
		if (thisComp)
			return thisComp->getBusCount(mediaType, busdirection);
			return -1;
	}

	
	EXPORTED_FUNCTION int GetStateOfPlugin(int pluginId, const char** str, size_t* strsize) {

		return GetState(GetPlugin(pluginId), str, strsize);
	}


	EXPORTED_FUNCTION void ReleaseStateStream(const char* stream) {
		delete[] stream; 
	}


	EXPORTED_FUNCTION int SetStateOfPlugin(int pluginId, const char* str, int strsize) {

		return SetState(GetPlugin(pluginId), str, strsize);

	}
}