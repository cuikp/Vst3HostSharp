
#pragma once

#include "iapplication.h"
#include "iwindow.h"
#include <functional>

//------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {
namespace EditorHost {

//------------------------------------------------------------------------
class IPlatform
{
public:
	virtual ~IPlatform () noexcept = default;

	virtual void setApplication (ApplicationPtr&& app) = 0;

	virtual WindowPtr createWindow (const std::string& title, Size size, bool resizeable,
	                                const WindowControllerPtr& controller) = 0;
	virtual void quit () = 0;
	virtual void kill (int resultCode, const std::string& reason) = 0;

	virtual FUnknown* getPluginFactoryContext () = 0;

	static IPlatform& instance ();
};

//------------------------------------------------------------------------
} // EditorHost
} // Vst
} // Steinberg

extern void ApplicationInit (int argc, const char* argv[]);
