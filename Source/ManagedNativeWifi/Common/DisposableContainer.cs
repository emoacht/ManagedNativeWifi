using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			_isDefault = EqualityComparer<T>.Default.Equals(content, default(T));
			this.Content = _isDefault ? new T() : content;
		}

		#region Dispose

		bool _disposed = false;

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