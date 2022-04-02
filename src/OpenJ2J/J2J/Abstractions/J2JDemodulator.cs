using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenJ2J.J2J.Abstractions
{
    public abstract class J2JDemodulator : IDisposable
    {
        #region ::Variables::

        protected FileStream? _fileStream = null;

        public FileStream? FileStream
        {
            get => _fileStream;
            set => _fileStream = value;
        }

        #endregion

        #region ::Constructors::

        public J2JDemodulator(FileStream fileStream)
        {
            _fileStream = fileStream;
        }

        #endregion

        #region ::Methods::

        public abstract bool Demodulate(string outputPath, bool useForcer);

        public abstract bool Demodulate(string outputPath, bool useForcer, string password);

        #endregion

        #region ::IDisposable Members::

        private bool disposedValue;

        protected virtual void DisposeManagedComponents()
        {
            _fileStream?.Dispose();
        }

        protected virtual void DisposeUnmanagedComponents()
        {

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DisposeManagedComponents();
                }

                DisposeUnmanagedComponents();
                disposedValue = true;
            }
        }

        // // TODO: 비관리형 리소스를 해제하는 코드가 'Dispose(bool disposing)'에 포함된 경우에만 종료자를 재정의합니다.
        // ~J2JDemodulator()
        // {
        //     // 이 코드를 변경하지 마세요. 'Dispose(bool disposing)' 메서드에 정리 코드를 입력합니다.
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 이 코드를 변경하지 마세요. 'Dispose(bool disposing)' 메서드에 정리 코드를 입력합니다.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
