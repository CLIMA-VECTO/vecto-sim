using Newtonsoft.Json;

namespace TUGraz.VectoCore.FileIO
{
	public class JsonDataHeader
	{
		[JsonProperty(Required = Required.Always)] public string AppVersion;
		[JsonProperty(Required = Required.Always)] public string CreatedBy;
		[JsonProperty(Required = Required.Always)] public string Date;
		[JsonProperty(Required = Required.Always)] public uint FileVersion;

		#region Equality members

		protected bool Equals(JsonDataHeader other)
		{
			return string.Equals(CreatedBy, other.CreatedBy) && Date.Equals(other.Date) &&
					string.Equals(AppVersion, other.AppVersion) && FileVersion.Equals(other.FileVersion);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			if (ReferenceEquals(this, obj)) {
				return true;
			}
			if (obj.GetType() != GetType()) {
				return false;
			}
			return Equals((JsonDataHeader)obj);
		}

		public override int GetHashCode()
		{
			unchecked {
				var hashCode = (CreatedBy != null ? CreatedBy.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ Date.GetHashCode();
				hashCode = (hashCode * 397) ^ (AppVersion != null ? AppVersion.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ FileVersion.GetHashCode();
				return hashCode;
			}
		}

		#endregion
	}
}