using System.Linq;
using System.Reflection;

namespace Tofu3D;

public class FieldOrPropertyInfo
{
	public bool canShowInEditor = true;
	private FieldInfo fieldInfo;
	private PropertyInfo propertyInfo;

	public FieldOrPropertyInfo(FieldInfo fi, object obj)
	{
		fieldInfo = fi;
		UpdateCanShowInEditor(obj);
	}

	public FieldOrPropertyInfo(PropertyInfo pi,object obj)
	{
		propertyInfo = pi;
		UpdateCanShowInEditor(obj);
	}

	public IEnumerable<CustomAttributeData> CustomAttributes
	{
		get
		{
			if (fieldInfo != null)
			{
				return fieldInfo.CustomAttributes;
			}

			if (propertyInfo != null)
			{
				return propertyInfo.CustomAttributes;
			}

			return null;
		}
	}
	public string Name
	{
		get
		{
			if (fieldInfo != null)
			{
				return fieldInfo.Name;
			}

			if (propertyInfo != null)
			{
				return propertyInfo.Name;
			}

			return null;
		}
	}
	public Type FieldOrPropertyType
	{
		get
		{
			if (fieldInfo != null)
			{
				return fieldInfo.FieldType;
			}

			if (propertyInfo != null)
			{
				return propertyInfo.PropertyType;
			}

			return null;
		}
	}

	private void UpdateCanShowInEditor(object obj)
	{
		if (fieldInfo != null && fieldInfo.DeclaringType == typeof(Component))
		{
			canShowInEditor = false;
		}

		if (propertyInfo != null && propertyInfo?.DeclaringType == typeof(Component))
		{
			canShowInEditor = false;
		}

		for (int i = 0; i < CustomAttributes.Count(); i++)
		{
			if (CustomAttributes.ElementAtOrDefault(i).AttributeType == typeof(Show))
			{
				canShowInEditor = true;
			}

			if (CustomAttributes.ElementAtOrDefault(i).AttributeType == typeof(ShowIf))
			{
				string name = CustomAttributes.ElementAtOrDefault(i).ConstructorArguments[0].Value.ToString();

				FieldInfo field = obj.GetType().GetField(name);
				PropertyInfo property = obj.GetType().GetProperty(name);
				if (field != null)
				{
					canShowInEditor = (bool)field.GetValue(obj);
				}
				if (property != null)
				{
					canShowInEditor = (bool)property.GetValue(obj);
				}
			}
			if (CustomAttributes.ElementAtOrDefault(i).AttributeType == typeof(ShowIfNot))
			{
				string name = CustomAttributes.ElementAtOrDefault(i).ConstructorArguments[0].Value.ToString();

				FieldInfo field = obj.GetType().GetField(name);
				PropertyInfo property = obj.GetType().GetProperty(name);
				if (field != null)
				{
					canShowInEditor = (bool)field.GetValue(obj)==false;
				}
				if (property != null)
				{
					canShowInEditor = (bool)property.GetValue(obj)==false;
				}
			}
		}

		for (int i = 0; i < CustomAttributes.Count(); i++)
		{
			if (CustomAttributes.ElementAtOrDefault(i).AttributeType == typeof(Hide))
			{
				canShowInEditor = false;
			}
		}
	}

	public object? GetValue(object? obj)
	{
		if (fieldInfo != null)
		{
			return fieldInfo.GetValue(obj);
		}

		if (propertyInfo != null)
		{
			return propertyInfo.GetValue(obj);
		}

		return null;
	}

	public void SetValue(object? obj, object? value)
	{
		if (fieldInfo != null)
		{
			fieldInfo.SetValue(obj, value);
		}

		if (propertyInfo != null)
		{
			if (propertyInfo.GetSetMethod() != null)
			{
				propertyInfo.SetValue(obj, value);
			}
		}
	}
}