#pragma once

#include "platform/WindowController.h"
#include "pluginterfaces/vst/ivstaudioprocessor.h"
#include "pluginterfaces/vst/vsttypes.h"
#include "pluginterfaces/base/funknown.h"
#include "pluginterfaces/gui/iplugview.h"
#include "pluginterfaces/vst/ivsteditcontroller.h"

#include "public.sdk/source/vst/hosting/eventlist.h"
#include "public.sdk/source/vst/hosting/parameterchanges.h"
#include "public.sdk/source/vst/hosting/processdata.h"

#include "Component/ComponentHandler.h"


#include "helpermethods.h"

#if defined(_WIN32) || defined(_WIN64)
#define EXPORTED_FUNCTION __declspec(dllexport)
#else
#define EXPORTED_FUNCTION __attribute__((visibility("default")))
#endif

using namespace Steinberg;
using namespace Steinberg::Vst;


class Plugin : public IApplication
{
public:
	Plugin() {}
	~Plugin() noexcept override;
	
	void init(const std::vector<std::string>& cmdArgs) override{}
	void terminate() override;
	void LoadVst3(const std::string& path);
	

private:
	enum OpenFlags
	{
		kSetComponentHandler = 1 << 0,
		kSecondWindow = 1 << 1,
	};

	void initializePlugin(const std::string& path);
	
	VST3::Hosting::Module::Ptr module{ nullptr };
	IPtr<PlugProvider> plugProvider{ nullptr };
	Vst::HostApplication pluginContext;
	//WindowPtr window;
	std::shared_ptr<WindowController> windowController;
public:
	IEditController* editController{ nullptr };
	IComponent* component{ nullptr };
	IAudioProcessor* audioProcessor { nullptr };
	ProcessData* procData { nullptr };
	ComponentHandler* compHandler{ nullptr };
	IMidiMapping* midiMapping{ nullptr };

	int getEditorSize(int& width, int& height);
	//const char* SetupAudioProcessor(int bufferSize, int parameterCount);
	std::string SetupAudioProcessor(int bufferSize, int parameterCount, double sampleRate);
	int createViewAndShow(void* passedHandle, const char* platform);
	void AudioProcessorDispose();
	void DisposeAll();
};

