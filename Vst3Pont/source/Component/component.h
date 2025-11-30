#pragma once

//#include <fstream>
//#include "base/source/fbuffer.h"
#include "pluginterfaces/base/ibstream.h"
#include "../helpermethods.h"
#include "../plugin.h"

using namespace Steinberg;

int GetState(Plugin* plugin, const char** stream, size_t* streamSize);
int SetState(Plugin* plugin, const char* str, int strsize);

std::string setupAudioProcessor(Plugin* plugin, int bufferSize, int parameterCount, double sampleRate);
int disposeAudioProcessor(ProcessData* procData);