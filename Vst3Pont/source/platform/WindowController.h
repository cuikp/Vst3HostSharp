#pragma once

#include "appinit.h"
#include "iwindow.h"
#include "public.sdk/source/vst/hosting/hostclasses.h"
#include "public.sdk/source/vst/hosting/module.h"
#include "public.sdk/source/vst/hosting/plugprovider.h"
#include "public.sdk/source/vst/utility/optional.h"


using namespace Steinberg;
using namespace Steinberg::Vst;
using namespace Steinberg::Vst::EditorHost;

class WindowController;

class WindowController : public IWindowController, public IPlugFrame
{
public:
	WindowController(const IPtr<IPlugView>& plugView);
	~WindowController() noexcept override;

	IPtr<IPlugView> plugView;

	void onShow(IWindow& w) override;
	
	void onShowAlt(void* windowHandle, const char* platform);
	void onClose(IWindow& w) override;
	void onResize(Size newSize) override;
	Size constrainSize(IWindow& w, Size requestedSize) override;
	void onContentScaleFactorChanged(float newScaleFactor) override;

	// IPlugFrame
	tresult PLUGIN_API resizeView(IPlugView* view, ViewRect* newSize) override;

	void closePlugView();

private:
	tresult PLUGIN_API queryInterface(const TUID _iid, void** obj) override
	{
		if (FUnknownPrivate::iidEqual(_iid, IPlugFrame::iid) ||
			FUnknownPrivate::iidEqual(_iid, FUnknown::iid))
		{
			*obj = this;
			addRef();
			return kResultTrue;
		}
		/*if (window)
			return window->queryInterface(_iid, obj);*/
		return kNoInterface;
	}
	// we do not care here of the ref-counting. A plug-in call of release should not destroy this
	// class!
	uint32 PLUGIN_API addRef() override { return 1000; }
	uint32 PLUGIN_API release() override { return 1000; }

	//IPtr<IPlugView> plugView;
	IWindow* window{ nullptr };
	bool resizeViewRecursionGard{ false };
};
