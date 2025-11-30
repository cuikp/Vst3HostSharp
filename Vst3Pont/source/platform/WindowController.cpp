#include "WindowController.h"

#include "iplatform.h"
#include "pluginterfaces/gui/iplugviewcontentscalesupport.h"
#include "../helpermethods.h"

inline bool operator== (const ViewRect& r1, const ViewRect& r2)
{
	return memcmp(&r1, &r2, sizeof(ViewRect)) == 0;
}

inline bool operator!= (const ViewRect& r1, const ViewRect& r2)
{
	return !(r1 == r2);
}



WindowController::WindowController(const IPtr<IPlugView>& plugView) : plugView(plugView)
{
}


WindowController::~WindowController() noexcept
{
}


void WindowController::onShowAlt(void* windowHandle, const char* platform) {

	
	if (plugView->isPlatformTypeSupported(platform) != kResultTrue)
	{
		msg("PlugView does not support platform type:");
		//IPlatform::instance().kill(-1, std::string("PlugView does not support platform type:") + platform);
	}

	plugView->setFrame(this);
	
	if (plugView->attached(windowHandle, platform) != kResultTrue)
	{
		msg("Attaching plugview failed");
		//IPlatform::instance().kill(-1, "Attaching PlugView failed");
	}
}

void WindowController::onShow(IWindow& w)
{
	//SMTG_DBPRT1 ("onShow called (%p)\n", (void*)&w);

	window = &w;
	if (!plugView)
		return;

	auto platformWindow = window->getNativePlatformWindow();
	if (plugView->isPlatformTypeSupported(platformWindow.type) != kResultTrue)
	{
		IPlatform::instance().kill(-1, std::string("PlugView does not support platform type:") +
			platformWindow.type);
	}

	plugView->setFrame(this);

	if (plugView->attached(platformWindow.ptr, platformWindow.type) != kResultTrue)
	{
		IPlatform::instance().kill(-1, "Attaching PlugView failed");
	}
}


static PVOID gVehHandle = nullptr;

void InitBreakpointHandler()
{
	// 1 = first handler; if you load this before CoInitialize, it still works.
	gVehHandle = AddVectoredExceptionHandler(1, [](PEXCEPTION_POINTERS info) -> LONG {
		if (info->ExceptionRecord->ExceptionCode == EXCEPTION_BREAKPOINT) {
			// Optionally inspect info->ContextRecord->Rip to limit to your plugin module
			return EXCEPTION_CONTINUE_EXECUTION;
		}
		return EXCEPTION_CONTINUE_SEARCH;
		});
}



void WindowController::closePlugView()
{
	if (!plugView) return;

	AppendToLogFile("closing plugview");

	//plugView->removed();
	//plugView->setFrame(nullptr);
	
	if (plugView->removed() != kResultTrue)
		IPlatform::instance().kill(-1, "Removing PlugView failed");

	plugView->setFrame(nullptr);

	plugView = nullptr;

	//window = nullptr;

}



void WindowController::onClose(IWindow& w)
{
	//SMTG_DBPRT1 ("onClose called (%p)\n", (void*)&w);

	closePlugView();

	// TODO maybe quit only when the last window is closed
	IPlatform::instance().quit();
}



void WindowController::onResize(Size newSize)
{
	if (plugView)
	{
		ViewRect r{};
		r.right = newSize.width;
		r.bottom = newSize.height;
		ViewRect r2{};
		//if (plugView->getSize(&r2) == kResultTrue && r != r2)
		if (plugView->getSize(&r2) == kResultTrue)
			plugView->onSize(&r);
	}
}


Size WindowController::constrainSize(IWindow& w, Size requestedSize)
{
	//SMTG_DBPRT1 ("constrainSize called (%p)\n", (void*)&w);

	ViewRect r{};
	r.right = requestedSize.width;
	r.bottom = requestedSize.height;
	if (plugView && plugView->checkSizeConstraint(&r) != kResultTrue)
	{
		plugView->getSize(&r);
	}
	requestedSize.width = r.right - r.left;
	requestedSize.height = r.bottom - r.top;
	return requestedSize;
}



//void WindowController::onContentScaleFactorChanged(IWindow& w, float newScaleFactor)
void WindowController::onContentScaleFactorChanged(float newScaleFactor)

{
	FUnknownPtr<IPlugViewContentScaleSupport> css(plugView);
	if (css)
	{
		css->setContentScaleFactor(newScaleFactor);
	}
}



tresult PLUGIN_API WindowController::resizeView(IPlugView* view, ViewRect* newSize)
{
	//SMTG_DBPRT1 ("resizeView called (%p)\n", (void*)view);

	if (newSize == nullptr || view == nullptr || view != plugView)
		return kInvalidArgument;
	//if (!window)
	//	return kInternalError;
	if (resizeViewRecursionGard)
		return kResultFalse;
	ViewRect r;
	if (plugView->getSize(&r) != kResultTrue)
		return kInternalError;
	if (r == *newSize)
		return kResultTrue;

	resizeViewRecursionGard = true;
	Size size{ newSize->right - newSize->left, newSize->bottom - newSize->top };
	//window->resize(size);
	resizeViewRecursionGard = false;
	if (plugView->getSize(&r) != kResultTrue)
		return kInternalError;
	if (r != *newSize)
		plugView->onSize(newSize);
	return kResultTrue;
}




