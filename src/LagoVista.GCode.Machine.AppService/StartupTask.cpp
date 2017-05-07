using namespace Windows::ApplicationModel::Background;
using namespace Windows::ApplicationModel::AppService;

#include <Axis.h>

// These functions should be defined in the sketch file
void setup();
void loop();

bool m_bKill = false;

Axis PasteAxis(4, 17);
Axis PlaceAxis(3, 22);
Axis CAxis(2, 6);;
Axis YAxis(1, 19);;
Axis XAxis(0, 21);;

AppServiceConnection ^m_appServiceConnection;


namespace LagoVistaGCodeMachineAppService
{
	[Windows::Foundation::Metadata::WebHostHidden]
	public ref class StartupTask sealed : public IBackgroundTask
	{
	private:
		BackgroundTaskDeferral ^m_Deferral;
		

	public:
		virtual void Run(Windows::ApplicationModel::Background::IBackgroundTaskInstance^ taskInstance)
		{
			auto deferral = taskInstance->GetDeferral();

			taskInstance->Canceled += ref new Windows::ApplicationModel::Background::BackgroundTaskCanceledEventHandler(this, &LagoVistaGCodeMachineAppService::StartupTask::OnCanceled);

			auto triggerDetails = taskInstance->TriggerDetails;
			AppServiceTriggerDetails^ trigger = ((AppServiceTriggerDetails ^)(triggerDetails));
			m_appServiceConnection = trigger->AppServiceConnection;
			m_appServiceConnection->RequestReceived += ref new Windows::Foundation::TypedEventHandler<Windows::ApplicationModel::AppService::AppServiceConnection ^, Windows::ApplicationModel::AppService::AppServiceRequestReceivedEventArgs ^>(this, &LagoVistaGCodeMachineAppService::StartupTask::OnRequestReceived);


			auto msg = ref new Windows::Foundation::Collections::ValueSet();
			msg->Insert("STATUS", "ONLINE");
			m_appServiceConnection->SendMessageAsync(msg);


			setup();
			while (true)
			{
				loop();
			}
		}

		void OnRequestReceived(Windows::ApplicationModel::AppService::AppServiceConnection ^sender, Windows::ApplicationModel::AppService::AppServiceRequestReceivedEventArgs ^args);
		void OnCanceled(Windows::ApplicationModel::Background::IBackgroundTaskInstance ^sender, Windows::ApplicationModel::Background::BackgroundTaskCancellationReason reason);
	};

	void LagoVistaGCodeMachineAppService::StartupTask::OnRequestReceived(Windows::ApplicationModel::AppService::AppServiceConnection ^sender, Windows::ApplicationModel::AppService::AppServiceRequestReceivedEventArgs ^args)
	{
		auto deferral = args->GetDeferral();
		
		auto msg = ref new Windows::Foundation::Collections::ValueSet();
		msg->Insert("STATUS", "HI");
		m_appServiceConnection->SendMessageAsync(msg);


		if (args->Request->Message->HasKey("KILL")) {
			m_bKill = true;
			XAxis.Clear();
			YAxis.Clear();
			CAxis.Clear();
			PlaceAxis.Clear();
			PasteAxis.Clear();
		}
		else if (args->Request->Message->HasKey("RESET")) {
			m_bKill = false;
		}
		else if (args->Request->Message->HasKey("AXIS")) {
			auto msg = ref new Windows::Foundation::Collections::ValueSet();
			msg->Insert("STATUS", "2");
			m_appServiceConnection->SendMessageAsync(msg);

			int axis = (int)args->Request->Message->Lookup("AXIS");
			int multiplier = (long)args->Request->Message->Lookup("MULTIPLIER");
			INT64 steps = (long)args->Request->Message->Lookup("STEPS");

			switch (axis)
			{
			case 0: XAxis.SetSteps(steps, multiplier); break;
			case 1: YAxis.SetSteps(steps, multiplier); break;
			case 2: CAxis.SetSteps(steps, multiplier); break;
			case 3: PlaceAxis.SetSteps(steps, multiplier); break;
			case 4: PasteAxis.SetSteps(steps, multiplier); break;
			}

			msg = ref new Windows::Foundation::Collections::ValueSet();
			msg->Insert("STATUS", "3");
			m_appServiceConnection->SendMessageAsync(msg);

		}

		deferral->Complete();
	}


	void LagoVistaGCodeMachineAppService::StartupTask::OnCanceled(Windows::ApplicationModel::Background::IBackgroundTaskInstance ^sender, Windows::ApplicationModel::Background::BackgroundTaskCancellationReason reason)
	{
		m_Deferral->Complete();
	}
}

void AxisCompleted(int axis) {
	auto msg = ref new Windows::Foundation::Collections::ValueSet();
	msg->Insert("STATUS", axis);
	m_appServiceConnection->SendMessageAsync(msg);
}
