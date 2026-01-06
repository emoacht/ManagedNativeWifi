using System;
using System.Collections.Generic;

namespace ManagedNativeWifi.Common
{
	/// <summary>
	/// Container of disposable object
	/// </summary>
	/// <typeparam name="T">Disposable object type</typeparam>
	/// <remarks>
	/// If a disposable object is given as content when this container is instantiated,
	/// the content object will not be disposed when this container is disposed.
	/// In contrast, if no disposable object is given (if it is default, in the case of class, null)
	/// as content when this container is instantiated, a new disposable object is instantiated
	/// instead and the content object will be disposed when this container is disposed.
	///	</remarks>
	internal class DisposableContainer<T> : IDisposable where T : IDisposable, new()
	{
		private readonly bool _isDefault;
		public T Content { get; }

		public DisposableContainer(T content)
		{
			if (content is null || EqualityComparer<T>.Default.Equals(content, default))
			{
				Content = new T();
				_isDefault = true;
			}
			else
			{
				Content = content;
			}
		}

		#region Dispose

		private bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if (_isDefault)
					Content.Dispose();
			}

			_disposed = true;
		}

		#endregion
	}
}