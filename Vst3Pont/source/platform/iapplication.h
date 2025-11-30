
#pragma once

#include <memory>
#include <string>
#include <vector>

//------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {
namespace EditorHost {

//------------------------------------------------------------------------
class IApplication
{
public:
	virtual ~IApplication () noexcept = default;

	virtual void init(const std::vector<std::string>& cmdArgs) = 0;
	virtual void terminate () = 0;
};

using ApplicationPtr = std::unique_ptr<IApplication>;

//------------------------------------------------------------------------
} // EditorHost
} // Vst
} // Steinberg
