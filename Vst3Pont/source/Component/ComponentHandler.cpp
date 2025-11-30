#include "ComponentHandler.h"

using namespace Steinberg;
using namespace Steinberg::Vst;

ComponentHandler::ComponentHandler() { }


tresult PLUGIN_API ComponentHandler::beginEdit(ParamID id)
{
	return kNotImplemented;
}

tresult PLUGIN_API ComponentHandler::performEdit(ParamID id, ParamValue valueNormalized)
{
//#ifdef _DEBUG
//		AppendToLogFile(("Performing edit with parameter: " + std::to_string(valueNormalized)).c_str());
//#endif

	if (procData) {
			int paramIndex = 0;
			Steinberg::Vst::IParamValueQueue* ipvq = procData->outputParameterChanges->addParameterData(id, paramIndex);
			if (ipvq) {
				int valueIndex = 0;
				ipvq->addPoint(0, valueNormalized, valueIndex);
				return 0;
			}
			else
				return -1;
		}

	return kNotImplemented;
}


tresult PLUGIN_API ComponentHandler::endEdit(ParamID id)
{
	if (onEndEdit_) 
	{
		onEndEdit_(); 
		//AppendToLogFile("called on END EDIT");
	}

	return true;
}

tresult PLUGIN_API ComponentHandler::restartComponent(int32 flags)
{
	// when new bus added or removed?

#ifdef _DEBUG
	AppendToLogFile(("component restarted from plugin: " + std::to_string(flags) + "\n").c_str());
#endif


	return true;
	
}

tresult PLUGIN_API ComponentHandler::queryInterface(const TUID iid, void** obj)
{
	if (FUnknownPrivate::iidEqual(iid, Vst::IComponentHandler::iid)) {
		*obj = static_cast<Vst::IComponentHandler*>(this);
		addRef();
	#ifdef _DEBUG
		AppendToLogFile("QI: IComponentHandler\n");
	#endif

		return kResultOk;
	}
	
	if (FUnknownPrivate::iidEqual(iid, Vst::IComponentHandlerBusActivation::iid)) {
		*obj = static_cast<Vst::IComponentHandlerBusActivation*>(this);
		addRef();
	#ifdef _DEBUG
		AppendToLogFile("QI: IComponentHandlerBusActivation\n");
	#endif
		return kResultOk;
	}

	*obj = nullptr;
	return kNoInterface;

}
uint32 PLUGIN_API ComponentHandler::addRef() { return 1000; }
uint32 PLUGIN_API ComponentHandler::release() { return 1000; }



tresult PLUGIN_API ComponentHandler::requestBusActivation(MediaType t, BusDirection d, int32 index, TBool state)
{
#ifdef _DEBUG
	AppendToLogFile(("component requested bus activation: " +  std::to_string(t) + ", " + std::to_string(index) + "\n").c_str());
#endif

	return 0;

}

