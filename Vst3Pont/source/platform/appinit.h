#pragma once

#include "iapplication.h"
#include "iplatform.h"

//------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {
namespace EditorHost {

//------------------------------------------------------------------------
struct AppInit
{
	explicit AppInit (ApplicationPtr&& app)
	{
		IPlatform::instance ().setApplication (std::move (app));
	}
};

//------------------------------------------------------------------------
} // EditorHost
} // Vst
} // Steinberg
