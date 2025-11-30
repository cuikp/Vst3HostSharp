#include "plugin.h"
#include "./Component/component.h"
#include "Component/ComponentHandler.h"


using namespace Steinberg;
using namespace Steinberg::Vst;
using namespace Steinberg::Vst::EditorHost;


LONG CALLBACK VehHandler(PEXCEPTION_POINTERS ep)
{
	if (ep->ExceptionRecord->ExceptionCode ==
		EXCEPTION_ACCESS_VIOLATION)
	{
		std::cerr << "Caught AV in createInstance()\n";
		return EXCEPTION_EXECUTE_HANDLER;
	}
	return EXCEPTION_CONTINUE_SEARCH;
}


void Plugin::LoadVst3(const std::string& path)
{
	//#if _DEBUG
	//	//_CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);
	//	//_CrtSetBreakAlloc(530); // from debugging dump to check callstack  
	//#endif
		
	const std::vector<std::string>& cmdArgs = { path };

	PluginContextFactory::instance().setPluginContext(IPlatform::instance().getPluginFactoryContext());
	initializePlugin(path);

}


void Plugin::AudioProcessorDispose() {

	if (this->procData == nullptr) return;
	int result = disposeAudioProcessor(this->procData);
	this->procData = nullptr;  
	this->compHandler->procData = nullptr;
}

std::string Plugin::SetupAudioProcessor(int bufferSize, int parameterCount, double sampleRate) {
	
	std::string returnString = setupAudioProcessor(this, bufferSize, parameterCount, sampleRate);
	return  returnString;
}


void Plugin::initializePlugin(const std::string& path)
{
	std::string error;

	//AppendToLogFile("Will create module: path = " + path);

	module = VST3::Hosting::Module::create (path, error);

	if (!module)
	{
		AppendToLogFile("Could not create Module for file:");
		std::string reason = "Could not create Module for file:" + path + "\nError: " + error;
		IPlatform::instance ().kill (-1, reason);
	}

	auto factory = module->getFactory ();
	if (auto factoryHostContext = IPlatform::instance().getPluginFactoryContext())
		factory.setHostContext (factoryHostContext);

	VST3::Optional<VST3::UID> effectID;

	for (auto& classInfo : factory.classInfos ())
	{
		if (classInfo.category() == kVstAudioEffectClass)  // = "AudioModuleClass"
		{	
			if (effectID)
			{
				if (*effectID != classInfo.ID())
					continue;
			}

			plugProvider = owned (new PlugProvider (factory, classInfo, true));
			//AppendToLogFile("created new PlugProvider ok");

		/*	if (plugProvider->initialize() == false)
				plugProvider = nullptr;*/
			break;	
		}
	}
	
	component = plugProvider->getComponent();
	editController = plugProvider->getController();


	if (!editController)
	{
		error = "No EditController found (needed for allowing editor) in file " + path;
		IPlatform::instance ().kill (-1, error);
	}
	
	editController->release(); // plugProvider does an addRef

	//if (flags & kSetComponentHandler)

	compHandler = new ComponentHandler();
	
	int handlerResult = editController->setComponentHandler (compHandler);


#ifdef _DEBUG
	AppendToLogFile(("component Handler set: " + TResultToString(handlerResult)).c_str());
#endif

	component->setActive(true);

	AppendToLogFile("set component active");

	int queryAudioProcessorResult = component->queryInterface(IAudioProcessor::iid, (void**)&audioProcessor);

	//AppendToLogFile(("queried audioprocessor: " + TResultToString(queryAudioProcessorResult)).c_str());
		
	int queryMidiMapping = editController->queryInterface(IMidiMapping_iid, (void**)&midiMapping);
	
#ifdef _DEBUG
	AppendToLogFile(("got AudioProcessorResult: " + TResultToString(queryAudioProcessorResult)).c_str());
	AppendToLogFile(("got MidiMappingResult: " + TResultToString(queryMidiMapping)).c_str());
#endif

}


int Plugin::getEditorSize(int& width, int& height) {

	ViewRect* r = new ViewRect();
	windowController->plugView->getSize(r);
	width = r->getWidth();
	height = r->getHeight();
	delete[] r;

	return 0;
}


int getDPI() {

	HDC hdc = GetDC(NULL);
	if (hdc == NULL) {
		//std::cerr << "Failed to get device context" << std::endl;
		return -1; // Return an error
	}

	int dpiX = GetDeviceCaps(hdc, LOGPIXELSX);
	int dpiY = GetDeviceCaps(hdc, LOGPIXELSY);

	ReleaseDC(NULL, hdc);
		
	return dpiX; // dpiX and dpiY usually match on most displays
}


int Plugin::createViewAndShow(void* passedHandle, const char* platform)
{
	auto view = owned(editController->createView (ViewType::kEditor));
	if (!view)
	{
		msg("No view found");
		return -1;
	}

	
	int result = -1;

	windowController = std::make_shared<WindowController>(view);
	
	//Get size before
	ViewRect* vr = new ViewRect();
	windowController.get()->plugView->getSize(vr);
	Size beforeSize; 
	beforeSize.width = vr->right;
	beforeSize.height = vr->bottom;
	
	vr->right = 300;
	vr->bottom = 200;

	///////////////////////////////	
	windowController->onShowAlt(passedHandle, platform);
	//////////////////////////////
		
	//Get size after
	windowController.get()->plugView->getSize(vr);
	Size afterSize;
	afterSize.width = vr->right;
	afterSize.height = vr->bottom;
	AppendToLogFile(("got width Af as: " + std::to_string(afterSize.width) + "\n").c_str());

	float fac = static_cast<float>(afterSize.width) / static_cast<float>(beforeSize.width);
	
	float roundedFac = round(fac * 10.0f) / 10.0f;
	float dpiX = static_cast<float>(getDPI());
	float dpiFac = dpiX / 96.0f;

	if (roundedFac == 1.0f) {
		roundedFac = round(dpiFac * 10.0f) / 10.0f;

		//hack to ensure at least mininum size 
		afterSize.width = max(afterSize.width, 200);
		afterSize.height = max(afterSize.height, 200);
	}
	else {
		//do nothing
		//AppendToLogFile(("\nNot 1.0f roundedFac = " + std::to_string(roundedFac) + " --> dpiFac =" + std::to_string(dpiFac) + "\n").c_str());
	}

	windowController.get()->onContentScaleFactorChanged(max(roundedFac, 1.0f));
	windowController.get()->onResize(afterSize);
	
	delete[] vr;
	result = 0;

	return result;

}

void Plugin::terminate()
{	
	if (windowController)
	{
		windowController->closePlugView();
		windowController.reset();
	}
}


Plugin::~Plugin() noexcept
{
	//terminate();
}

void Plugin::DisposeAll() {

	if (windowController) 
	{
		windowController->closePlugView();
		windowController.reset();
	}
	
	if (audioProcessor)
		audioProcessor->setProcessing(false);
	
	if (component)
		component->setActive(false);

	AudioProcessorDispose();

	if (audioProcessor) 
	{
		audioProcessor->release();
		audioProcessor = nullptr;
	}

	if (component) {
		component->release();
		component = nullptr;
	}
		

	if (midiMapping) 
	{
		midiMapping->release();
		midiMapping = nullptr;
	}


	PluginContextFactory::instance().setPluginContext(nullptr);

	this->~Plugin();

	


	if (compHandler) {
		delete compHandler;
		compHandler = nullptr;
	}
		
//#if _DEBUG
//	_CrtDumpMemoryLeaks();
//#endif
//


}
