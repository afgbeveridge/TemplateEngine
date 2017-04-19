namespace Infra.BTA.Templates {

	public interface IContextObject {
		bool CanNavigate(string property);
		System.Collections.IEnumerable GetEnumerable(string property);
		object GetObject(string property);
		string GetValue(string property);
        string Identifier { get; }
	}
}
