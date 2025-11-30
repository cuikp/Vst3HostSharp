#pragma once

#include "public.sdk/source/vst/hosting/module.h"
#include "pluginterfaces/vst/ivsttestplugprovider.h"
#include "pluginterfaces/base/funknownimpl.h"

#include <ostream>

namespace Steinberg {
	namespace Vst {

		class IComponent;
		class IEditController;
		class ConnectionProxy;

		class PlugProvider
			: public U::Implements<U::Directly<ITestPlugProvider2>, U::Indirectly<ITestPlugProvider>>
		{
		public:
		public:
			using ClassInfo = VST3::Hosting::ClassInfo;
			using PluginFactory = VST3::Hosting::PluginFactory;

			//--- ---------------------------------------------------------------------
			PlugProvider(const PluginFactory& factory, ClassInfo info, bool plugIsGlobal = true);
			~PlugProvider() override;

			bool initialize();

			IComponent* PLUGIN_API getComponent() SMTG_OVERRIDE;
			IEditController* PLUGIN_API getController() SMTG_OVERRIDE;
			tresult PLUGIN_API releasePlugIn(IComponent* component, IEditController* controller) SMTG_OVERRIDE;
			tresult PLUGIN_API getSubCategories(IStringResult& result) const SMTG_OVERRIDE
			{
				result.setText(classInfo.subCategoriesString().data());
				return kResultTrue;
			}
			tresult PLUGIN_API getComponentUID(FUID& uid) const SMTG_OVERRIDE;

			//--- from ITestPlugProvider2 ------------------
			IPluginFactory* PLUGIN_API getPluginFactory() SMTG_OVERRIDE;

			static void setErrorStream(std::ostream* stream);

		protected:
			bool setupPlugin(FUnknown* hostContext);
			bool connectComponents();
			bool disconnectComponents();
			void terminatePlugin();

			template<typename Proc>
			void printError(Proc p) const;

			PluginFactory factory;
			IPtr<IComponent> component;
			IPtr<IEditController> controller;
			ClassInfo classInfo;

			IPtr<ConnectionProxy> componentCP;
			IPtr<ConnectionProxy> controllerCP;

			bool plugIsGlobal;
		};


		class PluginContextFactory
		{
		public:
			static PluginContextFactory& instance()
			{
				static PluginContextFactory factory;
				return factory;
			}

			void setPluginContext(FUnknown* obj) { context = obj; }
			FUnknown* getPluginContext() const { return context; }
		private:
			FUnknown* context;
		};


	} // Vst
} // Steinberg
