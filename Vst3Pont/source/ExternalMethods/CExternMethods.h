#pragma once

#include <string>

#include "../plugin.h"
#include <unordered_map>
#include <iostream>

#include "../helpermethods.h"
#include "../Component/component.h"

extern std::unordered_map<int, Plugin*> appInstances;
inline int appCounter = 0;

inline Plugin* GetPlugin(int pluginId) {
	auto it = appInstances.find(pluginId);
	if (it != appInstances.end()) {
		return it->second;
	}
	return nullptr;
}

inline IAudioProcessor* GetAudioProcessor(int pluginId) {
	auto it = appInstances.find(pluginId);
	if (it != appInstances.end()) {
		return it->second->audioProcessor;
	}
	return nullptr;
}

inline IEditController* GetEditController(int pluginId) {
	auto it = appInstances.find(pluginId);
	if (it != appInstances.end()) {
		return it->second->editController;
	}
	return nullptr;
}

inline IComponent* GetComponent(int pluginId) {
	auto it = appInstances.find(pluginId);
	if (it != appInstances.end()) {
		return it->second->component;
	}
	return nullptr;
}

inline IMidiMapping* GetMidiMapping(int pluginId) {
	auto it = appInstances.find(pluginId);
	if (it != appInstances.end()) {
		return it->second->midiMapping;
	}
	return nullptr;
}

