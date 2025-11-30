#pragma once

#include "CExternMethods.h"
#include <pluginterfaces/vst/ivstaudioprocessor.h>
#include <string>
#include <eventlist.h>
#include <parameterchanges.h>
#include <pluginterfaces/vst/vsttypes.h>
#include <pluginterfaces/vst/ivstcomponent.h>
#include <pluginterfaces/vst/ivstparameterchanges.h>
#include <pluginterfaces/base/ftypes.h>
#include <pluginterfaces/vst/ivstevents.h>
#include <plugin.h>

using namespace Steinberg;


extern "C" {

	EXPORTED_FUNCTION int SetupAudioProcessor(int pluginId, int bufferSize, ProcessData** processDataPtr, int parameterCount, double sampleRate) {

#ifdef _DEBUG
		AppendToLogFile(("pluginId = " + std::to_string(pluginId) + "\n").c_str());
#endif

		Plugin* thisplugin = GetPlugin(pluginId);
		if (thisplugin) {
			std::string audioProcessorResult = thisplugin->SetupAudioProcessor(bufferSize, parameterCount, sampleRate);
		
//#ifdef _DEBUG
//			AppendToLogFile((audioProcessorResult + "\n").c_str());
//#endif

			*processDataPtr = thisplugin->procData;
			return 0;
		}

		return -1;
	}

	void postprocess(ProcessData* procData)
	{
		(static_cast<EventList*>(procData->inputEvents))->clear();
		(static_cast<ParameterChanges*>(procData->inputParameterChanges))->clearQueue();

	}

	EXPORTED_FUNCTION int Process(int pluginId) {
	//EXPORTED_FUNCTION int Process(int pluginId, ProcessData * procData) {

		Plugin* thisPlugin = GetPlugin(pluginId);
		if (thisPlugin) {
			int processResult = thisPlugin->audioProcessor->process(*(thisPlugin->procData)); ///CHANGE TO PASS procData pointer maybe?????????
			//int processResult = thisPlugin->audioProcessor->process(*procData); 

			postprocess(thisPlugin->procData);

			return processResult;
		}
		return -1;

	}


	EXPORTED_FUNCTION int GetBusArrangement(int pluginId) {

		IAudioProcessor* thisAP = GetAudioProcessor(pluginId);
		SpeakerArrangement speakArr;
		int returnInt = thisAP->getBusArrangement(BusDirections::kOutput, 0, speakArr);
		return speakArr;
	}

	EXPORTED_FUNCTION int SetProcessing(int pluginId, int state) {

		IAudioProcessor* thisAP = GetAudioProcessor(pluginId);
		return thisAP->setProcessing(state != 0);
	}


	EXPORTED_FUNCTION int GetOutputParameterChangesCount(int pluginId) {
		Plugin* thisPlugin = GetPlugin(pluginId);
		return thisPlugin->procData->outputParameterChanges->getParameterCount();
	}

	EXPORTED_FUNCTION int GetParameterData(int pluginId, int index, ParamID& paramID, ParamValue& value) {
		
		Plugin* thisPlugin = GetPlugin(pluginId);

		IParamValueQueue* iParamValQ = thisPlugin->procData->outputParameterChanges->getParameterData(index);
		if (iParamValQ) {
			int32 offset = 0;
			paramID = iParamValQ->getParameterId();
			return iParamValQ->getPoint(index, offset, value);
		}
		return -1;

	}

	EXPORTED_FUNCTION int AddParameterData(int pluginId, ParamID paramID, double addValue) {

		Plugin* thisPlugin = GetPlugin(pluginId);
		
		int paramIndex = 0;
		IParamValueQueue* ipvq = thisPlugin->procData->inputParameterChanges->addParameterData(paramID, paramIndex);
		if (ipvq) {
			int valueIndex = 0;
			ipvq->addPoint(0, addValue, valueIndex);
			return 0;
		}
		else
			return -1;

	}

	EXPORTED_FUNCTION int GetEventCount(int pluginId) {
		Plugin* thisPlugin = GetPlugin(pluginId);
		if (thisPlugin) {
			thisPlugin->procData->inputEvents->getEventCount();
		}
		return -1;
	}

	EXPORTED_FUNCTION int GetEvent(int pluginId, int index, Event& event) {
		Plugin* thisPlugin = GetPlugin(pluginId);
		if (thisPlugin) {
			thisPlugin->procData->inputEvents->getEvent(index, event);
		}
		return -1;
	}

	EXPORTED_FUNCTION int AddEvent(int pluginId, Event& event) {

		Plugin* thisPlugin = GetPlugin(pluginId);
		if (thisPlugin) {
			
			if (thisPlugin->procData->inputEvents != nullptr) {
				int result = thisPlugin->procData->inputEvents->addEvent(event);
				/*if (result == 0) {
					AppendToLogFile(
						"Success in externC AddEvent!\neventtype= " +
						std::to_string(event.type) +
						"\nbusindex= " + std::to_string(event.busIndex) +
						"\nflags= " + std::to_string(event.flags) +
						"\nsampleoffset= " + std::to_string(event.sampleOffset) +
						"\nppq= " + std::to_string(event.ppqPosition) +
						"\npitch= " + std::to_string(event.noteOn.pitch) +
						"\nvelocity= " + std::to_string(event.noteOn.velocity) +
						"\nchannel= " + std::to_string(event.noteOn.channel)
					);
				}
				else
					AppendToLogFile("Event failed to add in cExtern:\n");*/
				return result;
			}
			//else
			//{
			//	AppendToLogFile("procdata-inputeevents was null pointer:\n");
			//	return -5;
			//}
		}
#ifdef _DEBUG
		AppendToLogFile("add event failed in cextern:\n");
#endif
		return -1;
	}



	EXPORTED_FUNCTION int ClearEvents(int pluginId) {
		Plugin* thisPlugin = GetPlugin(pluginId);
		if (thisPlugin) {
			int eventCount = thisPlugin->procData->inputEvents->getEventCount();
			(static_cast<EventList*>(thisPlugin->procData->inputEvents))->clear();
			(static_cast<EventList*>(thisPlugin->procData->outputEvents))->clear();

			for (int i = 0; i < 128; i++) {
				Event ev{};
				//ev.type = 1;
				NoteOffEvent nev{};
				nev.pitch = i;
				nev.velocity = 0;
				ev.noteOff = nev;
				thisPlugin->procData->inputEvents->addEvent(ev);
			}
				
			thisPlugin->audioProcessor->setProcessing(false);
			thisPlugin->component->setActive(false);
			thisPlugin->component->setActive(true);
			thisPlugin->audioProcessor->setProcessing(true);

			return eventCount;
		}
		return -1;
	}

}