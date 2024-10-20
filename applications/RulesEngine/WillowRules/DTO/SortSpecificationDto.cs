namespace Willow.Rules.DTO
{
	/// <summary>
	/// Sort specification component used by MUI grid
	/// </summary>
	public class SortSpecificationDto
    {
        /// <summary>
		/// field name
		/// </summary>
        public string field { get; set; }

        /// <summary>
		/// "asc", "desc" or empty
		/// </summary>
        public string sort { get; set; }

        /// <summary>
		/// Create a <see cref="SortSpecificationDto" />
		/// </summary>
        public SortSpecificationDto(string field, string sort)
        {
			this.field = field;
			this.sort = sort;
		}

		public SortSpecificationDto() { this.field = ""; this.sort = ""; }
	}
}
