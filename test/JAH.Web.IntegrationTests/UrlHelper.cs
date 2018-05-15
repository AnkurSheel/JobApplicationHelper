using System.Reflection;
using System.Text;

public static class UrlHelper
{
    public static string ToUrl(this object instance)
    {
        var urlBuilder = new StringBuilder();
        PropertyInfo[] properties = instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (PropertyInfo property in properties)
        {
            urlBuilder.AppendFormat("{0}={1}&", property.Name, property.GetValue(instance, null));
        }

        if (urlBuilder.Length > 1)
        {
            urlBuilder.Remove(urlBuilder.Length - 1, 1);
        }

        return urlBuilder.ToString();
    }
}
