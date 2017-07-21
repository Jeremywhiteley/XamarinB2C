using System;
using Newtonsoft.Json;

namespace XamarinB2C.Core.Model
{
    /// <summary>
    /// Todo item.
    /// </summary>
	public class TodoItem
	{
		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }

		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "notes")]
		public string Notes { get; set; }

		[JsonProperty(PropertyName = "done")]
		public bool Done { get; set; }
	}
}
