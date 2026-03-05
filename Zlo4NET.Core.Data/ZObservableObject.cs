using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Zlo4NET.Core.Helpers;

namespace Zlo4NET.Core.Data;

public abstract class ZObservableObject : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string propName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
	}

	protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
	{
		if (object.Equals(storage, value))
		{
			return false;
		}
		storage = value;
		OnPropertyChanged(propertyName);
		return true;
	}

	internal void UpdateAll()
	{
		Type type = GetType();
		foreach (PropertyInfo item in ZObservableHelper.GetObservableObjectPropertiesFromType(type))
		{
			((ZObservableObject)item.GetValue(this))?.UpdateAll();
		}
		IEnumerable<PropertyInfo> observablePropertiesFromType = ZObservableHelper.GetObservablePropertiesFromType(type);
		_RaisePropertyChangeEvent(observablePropertiesFromType.Select((PropertyInfo pi) => pi.Name));
	}

	internal void UpdateByName(string propertyName)
	{
		OnPropertyChanged(propertyName);
	}

	private void _RaisePropertyChangeEvent(IEnumerable<string> propertyNames)
	{
		foreach (string propertyName in propertyNames)
		{
			OnPropertyChanged(propertyName);
		}
	}
}
