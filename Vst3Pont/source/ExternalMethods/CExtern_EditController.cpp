#include "CExternMethods.h"

using namespace Steinberg;

extern "C" {


	EXPORTED_FUNCTION int GetParameterCount(int pluginId) {

		IEditController* thisEdCont = GetEditController(pluginId);
		if (thisEdCont) {
			int paramCount = thisEdCont->getParameterCount();
			return paramCount;

		}
		return -1;
	}

	EXPORTED_FUNCTION int GetParameterInfo(int pluginId, int index, ParameterInfo* paramInfo) {
		IEditController* thisEdCont = GetEditController(pluginId);
		return thisEdCont->getParameterInfo(index, *paramInfo);
	}

	EXPORTED_FUNCTION double GetParameterNormalized(int pluginId, uint32 id) {
		IEditController* thisEdCont = GetEditController(pluginId);
		return thisEdCont->getParamNormalized(id);
	}

	EXPORTED_FUNCTION int SetParameterNormalized(int pluginId, uint32 id, double paramValue) {
		IEditController* thisEdCont = GetEditController(pluginId);
		return thisEdCont->setParamNormalized(id, paramValue);

	}
	
	EXPORTED_FUNCTION int GetMidiControllerAssignment(int pluginId, int32 busIndex, int16 channel, int ctrlNumber, ParamID& id) {
      IMidiMapping* thisMidiMapping = GetMidiMapping(pluginId);
		uint32 paramID = 0;
		int tresult = thisMidiMapping->getMidiControllerAssignment(busIndex, channel, ctrlNumber, paramID);
		id = paramID;
		return tresult;
		//from SDK: getMidiControllerAssignment(int32 busIndex, int16 channel, CtrlNumber midiControllerNumber, ParamID & id/*out*/)
	}

}