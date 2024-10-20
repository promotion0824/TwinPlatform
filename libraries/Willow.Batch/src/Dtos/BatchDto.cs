namespace Willow.Batch
{
    /// <summary>
    /// A batched result.
    /// </summary>
    /// <typeparam name="T">The type of BatchDTO.</typeparam>
    public class BatchDto<T>
    {
        /// <summary>
        /// Gets or sets the count before.
        /// </summary>
        public int Before { get; set; }

        /// <summary>
        /// Gets or sets the count after (including items if any).
        /// </summary>
        public int After { get; set; }

        /// <summary>
        /// Gets or sets the Total Count.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Gets or sets the collection of Items.
        /// </summary>
        public T[] Items { get; set; }
    }
}
