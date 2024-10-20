using DTDLParser.Models;

namespace Willow.TwinLifecycleManagement.Web.Models
{
	public class TwinFileColumn
	{
		public string Name { get; set; }
		public DTContentInfo ContentInfo { get; set; }
		public DTFieldInfo PropertyFieldInfo { get; set; }
		public DTPropertyInfo PropertyInfo { get; set; }
		public DTPropertyInfo RelationshipPropertyInfo { get; set; }
		public int Index { get; set; }
	}
}
