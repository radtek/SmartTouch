
namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class Culture
    {
        string code;
        public string Code { get { return code; } set { code = !string.IsNullOrEmpty(value)?value.Trim():null;  } }

        string name;
        public string Name { get { return name; } set { name = !string.IsNullOrEmpty(value)?value.Trim():null;  } }
    }
}
