using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Engine;

public class FieldOrPropertyInfo
{
	public bool canShowInEditor = true;
	private FieldInfo fieldInfo;
	private PropertyInfo propertyInfo;

	public FieldOrPropertyInfo(FieldInfo fi)
	{
		fieldInfo = fi;
		UpdateCanShowInEditor();
	}

	public FieldOrPropertyInfo(PropertyInfo pi)
	{
		propertyInfo = pi;
		UpdateCanShowInEditor();
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

	private void UpdateCanShowInEditor()
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
			if (CustomAttributes.ElementAtOrDefault(i).AttributeType == typeof(Show))
			{
				canShowInEditor = true;
			}

		for (int i = 0; i < CustomAttributes.Count(); i++)
			if (CustomAttributes.ElementAtOrDefault(i).AttributeType == typeof(Hide))
			{
				canShowInEditor = false;
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