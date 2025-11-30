//
//#pragma once
//
//#include "imediaserver.h"
//#include "iparameterclient.h"
//#include "public.sdk/source/vst/hosting/eventlist.h"
//#include "public.sdk/source/vst/hosting/parameterchanges.h"
//#include "public.sdk/source/vst/hosting/processdata.h"
//#include "pluginterfaces/vst/ivstaudioprocessor.h"
//#include <array>
//
////------------------------------------------------------------------------
//namespace Steinberg {
//namespace Vst {
//
////------------------------------------------------------------------------
//class IMidiMapping;
//class IComponent;
//
//enum
//{
//	kMaxMidiMappingBusses = 4,
//	kMaxMidiChannels = 16
//};
//using Controllers = std::vector<int32>;
//using Channels = std::array<Controllers, kMaxMidiChannels>;
//using Busses = std::array<Channels, kMaxMidiMappingBusses>;
//using MidiCCMapping = Busses;
//
////------------------------------------------------------------------------
//using AudioClientPtr = std::shared_ptr<class AudioClient>;
////------------------------------------------------------------------------
//class AudioClient : public IAudioClient, public IMidiClient, public IParameterClient
//{
//public:
////--------------------------------------------------------------------
//	using Name = std::string;
//
//	AudioClient ();
//	virtual ~AudioClient ();
//
//	static AudioClientPtr create (const Name& name, IComponent* component,
//	                              IMidiMapping* midiMapping);
//
//	// IAudioClient
//	bool process (Buffers& buffers, int64_t continousFrames) override;
//	bool setSamplerate (SampleRate value) override;
//	bool setBlockSize (int32 value) override;
//	IAudioClient::IOSetup getIOSetup () const override;
//
//	// IMidiClient
//	bool onEvent (const Event& event, int32_t port) override;
//	IMidiClient::IOSetup getMidiIOSetup () const override;
//
//	// IParameterClient
//	void setParameter (ParamID id, ParamValue value, int32 sampleOffset) override;
//
//	bool initialize (const Name& name, IComponent* component, IMidiMapping* midiMapping);
//
////--------------------------------------------------------------------
//private:
//	//void createLocalMediaServer (const Name& name);
//	void terminate ();
//	void updateBusBuffers (Buffers& buffers, HostProcessData& processData);
//	void initProcessData ();
//	void initProcessContext ();
//	bool updateProcessSetup ();
//	void preprocess (Buffers& buffers, int64_t continousFrames);
//	void postprocess (Buffers& buffers);
//	bool isPortInRange (int32 port, int32 channel) const;
//	bool processVstEvent (const IMidiClient::Event& event, int32 port);
//	bool processParamChange (const IMidiClient::Event& event, int32 port);
//
//	SampleRate sampleRate = 0;
//	int32 blockSize = 0;
//	HostProcessData processData;
//	ProcessContext processContext;
//	EventList eventList;
//	ParameterChanges inputParameterChanges;
//	IComponent* component = nullptr;
//	ParameterChangeTransfer paramTransferrer;
//
//	MidiCCMapping midiCCMapping;
//	//IMediaServerPtr mediaServer;
//	bool isProcessing = false;
//
//	Name name;
//};
//
////------------------------------------------------------------------------
//} // Vst
//} // Steinberg
