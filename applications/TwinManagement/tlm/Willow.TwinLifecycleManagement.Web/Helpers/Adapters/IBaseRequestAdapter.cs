namespace Willow.TwinLifecycleManagement.Web.Helpers.Adapters
{
	public interface IBaseRequestAdapter<out TTarget, in TAdaptee>
	{
		TTarget AdaptData(TAdaptee input);
	}
}
