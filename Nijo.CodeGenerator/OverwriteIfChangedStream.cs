using System;
using System.IO;

namespace Nijo.CodeGenerator;

/// <summary>
/// 書き込まれた内容と既存ファイルの内容を先頭から逐次比較し、内容が異なる場合のみ既存ファイルを上書きするストリーム。
/// 内容が全く同じ場合はファイルを書き込み用に開くことすらしない。
/// 比較は固定長のバッファ単位で行うので、巨大なファイルでもメモリを圧迫しない。
///
/// コード自動生成前後でファイル変更を監視するツール（ホットリロードなど）を無駄に反応させないためのもの。
/// </summary>
internal sealed class OverwriteIfChangedStream : Stream {
    internal OverwriteIfChangedStream(string path) {
        _path = path;
        if (File.Exists(path)) {
            _reader = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        } else {
            _writer = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        }
    }

    private readonly string _path;
    private readonly byte[] _readBuffer = new byte[81920];
    /// <summary>既存ファイルとの比較中のみ存在する</summary>
    private FileStream? _reader;
    /// <summary>既存ファイルとの差異が見つかった後、または既存ファイルが無い場合のみ存在する</summary>
    private FileStream? _writer;

    public override void Write(ReadOnlySpan<byte> buffer) {
        while (_reader != null && !buffer.IsEmpty) {
            var chunk = buffer[..Math.Min(buffer.Length, _readBuffer.Length)];
            var position = _reader.Position;
            var readLength = _reader.ReadAtLeast(_readBuffer.AsSpan(0, chunk.Length), chunk.Length, throwOnEndOfStream: false);
            var commonLength = chunk.CommonPrefixLength(_readBuffer.AsSpan(0, readLength));

            if (commonLength < chunk.Length) {
                StartOverwriting(position + commonLength);
                buffer = buffer[commonLength..];
                break;
            }
            buffer = buffer[chunk.Length..];
        }

        if (_writer != null) {
            _writer.Write(buffer);
        }
    }

    public override void Write(byte[] buffer, int offset, int count) {
        Write(buffer.AsSpan(offset, count));
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            // 書き込まれた内容が既存ファイルの先頭部分と一致したまま終わった場合、既存ファイルの方が長いならその分を切り詰める
            if (_reader != null && _reader.Position < _reader.Length) {
                StartOverwriting(_reader.Position);
            }
            if (_writer != null) {
                _writer.SetLength(_writer.Position);
                _writer.Dispose();
            }
            _reader?.Dispose();
        }
        base.Dispose(disposing);
    }

    /// <summary>
    /// 既存ファイルとの比較をやめ、指定の位置から先を上書きするモードに切り替えます。
    /// 指定の位置より前の内容は既存ファイルと同じなので書き込まない。
    /// </summary>
    private void StartOverwriting(long position) {
        _reader!.Dispose();
        _reader = null;
        _writer = new FileStream(_path, FileMode.Open, FileAccess.Write, FileShare.None) {
            Position = position,
        };
    }

    public override void Flush() {
        _writer?.Flush();
    }

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => throw new NotSupportedException();
    public override long Position {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
}
