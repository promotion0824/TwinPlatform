namespace Willow.Batch
{
	/// <summary>
	/// A batched result
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class BatchDto<T>
	{
		/// <summary>
		/// Count before
		/// </summary>
		public int Before { get; set; }

		/// <summary>
		/// Count after (including items if any)
		/// </summary>
		public int After { get; set; }

		/// <summary>
		/// Total Count
		/// </summary>
		public int Total { get; set; }

		/// <summary>
		/// Items
		/// </summary>
		public T[] Items { get; set; }
	}
}
