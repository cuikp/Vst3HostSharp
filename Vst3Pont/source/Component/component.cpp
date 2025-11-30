#include "component.h"
#include "public.sdk/source/common/memorystream.cpp"
//#include <signal.h>

using namespace Steinberg;

int GetState(Plugin* plugin, const char** stream, size_t* streamSize)  {

	//int result0 = plugin->component->getState(&componentState);
	////result0 = plugin->editController->setComponentState(&componentState);
	////int result2 = plugin->editController->getState(&controllerState);
	//return result0;  xxx


	int result = -1;

	Steinberg::MemoryStream compState;

	result = plugin->component->getState(&compState);


	if (result == 0) {

		int64 size;
		compState.tell(&size);

		compState.seek(0, Steinberg::IBStream::IStreamSeekMode::kIBSeekSet, 0);
        const char* data = compState.getData();

        *stream = new char[size]; 
        std::memcpy((void*)(*stream), data, size); 
        *streamSize = size; 

		//compState.release();  ?????not necessary

		result = 0;
	}
	
	return result;

}


int SetState(Plugin* plugin, const char* stream, int streamSize) {


	//componentState.seek(0, Steinberg::IBStream::kIBSeekSet, nullptr);
	//int setStateResult0 = plugin->component->setState(&componentState);
	//componentState.seek(0, Steinberg::IBStream::kIBSeekSet, nullptr);
	//int setcompStateResult = plugin->editController->setComponentState(&componentState);
	////int setcontStateResult = plugin->editController->setState(&componentState);
	//return 0;


	Steinberg::MemoryStream compState;

	std::vector<char> buffer(streamSize);
	std::memcpy(buffer.data(), stream, streamSize);


	if (buffer[0] == 0)  //&& buffer[1] == 0) 
		return -1;


	int32* numwritten = 0;
	compState.write(buffer.data(), streamSize, numwritten);
	compState.seek(0, Steinberg::IBStream::kIBSeekSet, nullptr);

	//must set both
	int setStateResult = plugin->component->setState(&compState);
	int setStateResult2 = plugin->editController->setComponentState(&compState);

	return 0;

}


std::string setupAudioProcessor(Plugin* plugin, int bufferSize, int parameterCount, double sampleRate) {

	std::string returnString = "";

	plugin->AudioProcessorDispose(); // clean up to allow multiple calls to this method

	/*SpeakerArrangement speakArr;
	int returnInt = plugin->audioProcessor->getBusArrangement(BusDirections::kOutput, 0, speakArr);*/
	//plugin->audioProcessor->se
	

	std::string canProc32 = plugin->audioProcessor->canProcessSampleSize(0) == 0 ? "true" : "false";
	std::string canProc64 = plugin->audioProcessor->canProcessSampleSize(1) == 0 ? "true" : "false";
	returnString += "CanProcess32= " + canProc32 + "\nCanProcess64= " + canProc64 + "\n";

	int symbolicSampleSize = Steinberg::Vst::kSample32;
	//int symbolicSampleSize = Steinberg::Vst::kSample64;

	ProcessSetup processSetup;
	processSetup.processMode = ProcessModes::kRealtime;
	processSetup.maxSamplesPerBlock = bufferSize;
	processSetup.sampleRate = sampleRate;
	processSetup.symbolicSampleSize = symbolicSampleSize;

	int BusCountInEvent = plugin->component->getBusCount(MediaTypes::kEvent, BusDirections::kInput);
	int BusCountOutEvent = plugin->component->getBusCount(MediaTypes::kEvent, BusDirections::kOutput);
	//returnString += ("BusCounts in/out (event) = " + std::to_string(BusCountInEvent) + ", " + std::to_string(BusCountOutEvent) + "\n");
	returnString += ("Event BusCounts in = " + std::to_string(BusCountInEvent) + ", out = " + std::to_string(BusCountOutEvent) + "\n");

	/////////////////////// Activate EVENT busses
	int actEventBusResultIn = -1;
	for (int in = 0; in < BusCountInEvent; in++) {
		BusInfo binfo;
		plugin->component->getBusInfo(MediaTypes::kEvent, BusDirections::kInput, in, binfo);
		actEventBusResultIn = plugin->component->activateBus(MediaTypes::kEvent, BusDirections::kInput, in, true);
		returnString += ("EventBusInActivated? (" + std::to_string(in) + ") = " + TResultToString(actEventBusResultIn) + "\n");
	}
	int actEventBusResultOut = -1;
	for (int out = 0; out < BusCountOutEvent; out++) {
		actEventBusResultOut = plugin->component->activateBus(MediaTypes::kEvent, BusDirections::kOutput, out, true);
		returnString += ("EventBusOutActivated? (" + std::to_string(out) + ")= " + TResultToString(actEventBusResultOut) + "\n");
	}
	////////////////////////////////////////////////

	int BusCountInAudio = plugin->component->getBusCount(MediaTypes::kAudio, BusDirections::kInput);
	int BusCountOutAudio = plugin->component->getBusCount(MediaTypes::kAudio, BusDirections::kOutput);
	//returnString += ("BusCounts in/out (audio) = " + std::to_string(BusCountInAudio) + ", " + std::to_string(BusCountOutAudio) + "\n");
	returnString += ("Audio BusCounts in = " + std::to_string(BusCountInAudio) + ", out = " + std::to_string(BusCountOutAudio) + "\n");
	
	plugin->procData = new ProcessData();
	plugin->procData->numSamples = bufferSize;
	plugin->procData->numInputs = BusCountInAudio;
	plugin->procData->numOutputs = BusCountOutAudio;
	plugin->procData->symbolicSampleSize = symbolicSampleSize;
	plugin->procData->inputs = new AudioBusBuffers[plugin->procData->numInputs];
	plugin->procData->outputs = new AudioBusBuffers[plugin->procData->numOutputs];

	//Initialize input/output event and parameterchanges
	int eventCount = 128;
	plugin->procData->inputEvents = new EventList(eventCount);
	plugin->procData->outputEvents = new EventList(eventCount);

	plugin->procData->inputParameterChanges = new ParameterChanges(parameterCount);
	plugin->procData->outputParameterChanges = new ParameterChanges(parameterCount);

	plugin->procData->processContext = new ProcessContext();

	BusInfo businfo;

	/////////////////////// create all AUDIO buffers
	for (int inno = 0; inno < plugin->procData->numInputs; inno++) {
		plugin->component->getBusInfo(MediaTypes::kAudio, BusDirections::kInput, inno, businfo);
				
		int numChannelsThisIn = businfo.channelCount;

		returnString += ("audio buffer in - (" + std::to_string(inno) + "), bustype: " + std::to_string(businfo.busType) + ", channelsThisBus = " + std::to_string(numChannelsThisIn) + "\n");

		plugin->procData->inputs[inno].numChannels = numChannelsThisIn;
		plugin->procData->inputs[inno].channelBuffers32 = new Sample32 * [numChannelsThisIn];
		//for (int i = 0; i < numChannelsThisIn; ++i) {
		//	plugin->procData->inputs[inno].channelBuffers32[i] = new Sample32[bufferSize];  //allocated by C#
		//}
	}

	for (int outno = 0; outno < plugin->procData->numOutputs; outno++) {
		plugin->component->getBusInfo(MediaTypes::kAudio, BusDirections::kOutput, outno, businfo);
		int numChannelsThisOut = businfo.channelCount;
		plugin->procData->outputs[outno].numChannels = numChannelsThisOut;
		plugin->procData->outputs[outno].channelBuffers32 = new Sample32 * [numChannelsThisOut];
		//for (int i = 0; i < numChannelsThisOut; ++i) {
		//	plugin->procData->outputs[outno].channelBuffers32[i] = new Sample32[bufferSize]; //allocated by C#
		//}
	}


	/////////////////////// Activate in/out AUDIO busses  -- but ONLY first one since it is main 
	std::string successBussesStringIn = "";
	//for (int in = 0; in < BusCountInAudio; in++) {
	//	int activateBusInAudioResult = plugin->component->activateBus(MediaTypes::kAudio, BusDirections::kInput, in, true);
	//	if (activateBusInAudioResult == 0)
	//		successBussesStringIn += std::to_string(in) + ", ";
	//}
	for (int in = 0; in < min(1, BusCountInAudio); in++) {
		int activateBusInAudioResult = plugin->component->activateBus(MediaTypes::kAudio, BusDirections::kInput, in, true);
		if (activateBusInAudioResult == 0)
			successBussesStringIn += std::to_string(in) + ", ";
	}
	returnString += ("activated busses in (audio) - (" + successBussesStringIn + ")\n");

	std::string successBussesStringOut = "";
	//for (int out = 0; out < BusCountOutAudio; out++) {
	//	int activateBusOutAudioResult = plugin->component->activateBus(MediaTypes::kAudio, BusDirections::kOutput, out, true);
	//	if (activateBusOutAudioResult == 0)
	//		successBussesStringOut += std::to_string(out) + ", ";
	//}
	for (int out = 0; out < min(1, BusCountOutAudio); out++) {
		int activateBusOutAudioResult = plugin->component->activateBus(MediaTypes::kAudio, BusDirections::kOutput, out, true);
		if (activateBusOutAudioResult == 0)
			successBussesStringOut += std::to_string(out) + ", ";
	}
	returnString += ("activated busses out (audio) - (" + successBussesStringOut + ")\n");
	////////////////////////////////////////////////

	returnString += "Created and activated AudioBusBuffers & Event busses\nnum outputs: " + std::to_string(plugin->procData->numOutputs);

	plugin->audioProcessor->setupProcessing(processSetup);
	plugin->audioProcessor->setProcessing(true);

	plugin->compHandler->procData = plugin->procData;
	

	
#ifdef _DEBUG
	AppendToLogFile(returnString.c_str());
#endif
	
	return returnString;

}


int disposeAudioProcessor(ProcessData* procData) {
		
	if (procData->inputs)
	{
		for (int inno = 0; inno < procData->numInputs; inno++)
		{
			auto& in = procData->inputs[inno];

			if (in.channelBuffers32)
			{
				delete[] in.channelBuffers32;
				in.channelBuffers32 = nullptr;
			}

			if (in.channelBuffers64)
			{
				delete[] in.channelBuffers64;
				in.channelBuffers64 = nullptr;
			}
		}

		delete[] procData->inputs;
		procData->inputs = nullptr;
	}


	if (procData->outputs)
	{
		for (int outno = 0; outno < procData->numOutputs; outno++)
		{
			auto& out = procData->outputs[outno];

			if (out.channelBuffers32) 
			{
				delete[] out.channelBuffers32;
				out.channelBuffers32 = nullptr;
			}

			if (out.channelBuffers64) 
			{
				delete[] out.channelBuffers64;
				out.channelBuffers64 = nullptr;
			}
		}

		delete[] procData->outputs;
		procData->outputs = nullptr;

	}


	if (procData->inputEvents) {

		int eventCountIn = procData->inputEvents->getEventCount();
		procData->inputEvents->release();
		procData->inputEvents = nullptr;
	}

	if (procData->outputEvents) {
		int eventCountOut = procData->outputEvents->getEventCount();
		procData->outputEvents->release();
		procData->outputEvents = nullptr;
	}

	if (procData->inputParameterChanges) {

		procData->inputParameterChanges->release();
		procData->inputParameterChanges = nullptr;
	}

	if (procData->outputParameterChanges) {
		procData->outputParameterChanges->release();
		procData->outputParameterChanges = nullptr;
	}

	delete procData->processContext;
	procData->processContext = nullptr;

	delete procData;
	procData = nullptr;

	return 0;
}