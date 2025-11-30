#include "CExternMethods.h"

using namespace Steinberg;

VST3::Hosting::Module::Ptr module{ nullptr };
IPtr<PlugProvider> plugProvider{ nullptr };


extern "C" {

	EXPORTED_FUNCTION void SetLogFilePath(const char* path) {

		g_logFilePath = path;
	}

	EXPORTED_FUNCTION void FreeString(const char* str) {
		free((void*)str);
	}

	EXPORTED_FUNCTION int LoadVst3Ext(const char* path, int* hasEditor) {

		Plugin* pluginInstance = new Plugin();
		
#ifdef _DEBUG
		std::string pathStr(path);
		std::string mess = "Loading Path= " + pathStr + "\n";
		AppendToLogFile(mess.c_str());
#endif			 

		pluginInstance->LoadVst3(path);
		appInstances[appCounter] = pluginInstance;
		int returnId = appCounter;
		appCounter += 1;

		if (hasEditor) {
			*hasEditor = pluginInstance->editController == nullptr ? 0 : 1;
		}

		return returnId;
	}

	EXPORTED_FUNCTION int CountClassesNew(IPluginFactory* pluginFactory) {
		return pluginFactory->countClasses();

	}

	EXPORTED_FUNCTION int GetClassInfosNew(IPluginFactory* pluginFactory, int index, PClassInfoW& classInfoW, PClassInfo2& classInfo2, PClassInfo& classInfo) {

		int result = -1;

		if (!pluginFactory) { std::cerr << "Plugin factory is null." << std::endl; return -1; }


		auto fac3 = Steinberg::FUnknownPtr<Steinberg::IPluginFactory3>(pluginFactory);
		auto fac2 = Steinberg::FUnknownPtr<Steinberg::IPluginFactory2>(pluginFactory);

		Steinberg::PClassInfo ci;
		Steinberg::PClassInfo2 ci2;
		Steinberg::PClassInfoW ci3;

		if (fac3 && fac3->getClassInfoUnicode(index, &ci3) == Steinberg::kResultTrue) {
			classInfoW = ci3;
			result = 3;
		}
		else if (fac2 && fac2->getClassInfo2(index, &ci2) == Steinberg::kResultTrue) {
			classInfo2 = ci2;
			result = 2;
		}
		else if (pluginFactory->getClassInfo(index, &ci) == Steinberg::kResultTrue) {
			std::cerr << "Plugin factory" << std::endl;
			classInfo = ci;
			result = 1;
		}

		return result;
	}
	

	EXPORTED_FUNCTION void ReleasePluginFactory(IPluginFactory* pluginFactory) {
		pluginFactory->release();
	}


	EXPORTED_FUNCTION int DisposePlugin(int pluginId) {

		auto it = appInstances.find(pluginId);
		if (it == appInstances.end())
			return -1;

		Plugin* thisPlugin = it->second;

#ifdef _DEBUG
		AppendToLogFile(("\nDisposing of plugin: " + std::to_string(pluginId)).c_str());
#endif

		try
		{
			thisPlugin->DisposeAll();

#ifdef _DEBUG
			AppendToLogFile(("\nDisposed plugin: " + std::to_string(pluginId)).c_str());
#endif
		}
		catch (...) { return -1; }

		delete thisPlugin;
		appInstances.erase(it);

		return 0;
	}


	typedef void (*CallbackFunction)();
	CallbackFunction endEditCallback = nullptr;
	//CallbackFunction resizeViewHandle = nullptr;

	EXPORTED_FUNCTION int CreateAndShowEditor(int pluginId, void* passedHandle, int platform, void* resizeViewHandle, void* endEditHandle) {
	//EXPORTED_FUNCTION int CreateAndShowEditor(int pluginId, void* passedHandle, int platform) {

		const char* platformType = "HWND";
		if (platform == 1)
			platformType = "NSView"; // MACOS

		endEditCallback = reinterpret_cast<CallbackFunction>(endEditHandle);
		GetPlugin(pluginId)->compHandler->setOnEndEdit(endEditCallback);
				

		return GetPlugin(pluginId)->createViewAndShow(passedHandle, platformType);
		
	}


	EXPORTED_FUNCTION int GetEditorSize(int pluginId, int& width, int& height) {
				
		return GetPlugin(pluginId)->getEditorSize(width, height);
		
	}


	
}


