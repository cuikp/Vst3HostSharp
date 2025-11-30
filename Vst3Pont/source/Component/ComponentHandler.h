#pragma once
//#include "../plugin.h"
#include "pluginterfaces/vst/vsttypes.h"
#include "../Component/ComponentHandler.h"
#include "pluginterfaces/vst/ivsteditcontroller.h"
#include "public.sdk/source/vst/hosting/processdata.h"
#include "public.sdk/source/vst/hosting/parameterchanges.h"
#include "public.sdk/source/vst/hosting/processdata.h"

#include "../helpermethods.h"
#include <functional>

using namespace Steinberg;
using namespace Steinberg::Vst;

class ComponentHandler : public IComponentHandler, IComponentHandlerBusActivation
{
public:
	ProcessData* procData{ nullptr };

	ComponentHandler();  

	tresult PLUGIN_API beginEdit(ParamID id); 
	tresult PLUGIN_API performEdit(ParamID id, ParamValue valueNormalized);
	tresult PLUGIN_API endEdit(ParamID id);
	tresult PLUGIN_API restartComponent(int32 flags);
	tresult PLUGIN_API IComponentHandlerBusActivation::requestBusActivation(MediaType type, BusDirection dir, int32 index, TBool state);

	tresult PLUGIN_API queryInterface(const TUID /*_iid*/, void** /*obj*/);
	uint32 PLUGIN_API addRef();
	uint32 PLUGIN_API release();

	
	void setOnEndEdit(std::function<void()> f) { onEndEdit_ = std::move(f); }

private:
	//Plugin* plugin_;
	std::function<void()> onEndEdit_;


};
